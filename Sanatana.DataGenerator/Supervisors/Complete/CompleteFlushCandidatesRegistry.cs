using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Commands;

namespace Sanatana.DataGenerator.Supervisors.Complete
{
    public class CompleteFlushCandidatesRegistry : IFlushCandidatesRegistry
    {
        /// <summary>
        /// Default services for entities.
        /// </summary>
        protected GeneratorSetup _generatorSetup;
        /// <summary>
        /// All entities and their related extra properties and current generated count.
        /// </summary>
        protected Dictionary<Type, EntityContext> _entityContexts;
        /// <summary>
        ///  Entities that are stored at temporary storage and reached some threshold number to store in temporary storage. 
        ///  They are kept until required to generate other entities and will be flushed when no longer required.
        /// </summary>
        protected HashSet<EntityContext> _flushCandidates;
        /// <summary>
        /// List of entities that need to be generated and already completed.
        /// </summary>
        protected IProgressState _progressState;


        //init
        public CompleteFlushCandidatesRegistry(GeneratorSetup generatorSetup,
            Dictionary<Type, EntityContext> entityContexts, IProgressState progressState)
        {
            _flushCandidates = new HashSet<EntityContext>();
            _generatorSetup = generatorSetup;
            _entityContexts = entityContexts;
            _progressState = progressState;
        }
        

        //Flush required checks
        public virtual bool CheckIsFlushRequired(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            bool isFlushInProgress = progress.IsFlushInProgress();
            if (isFlushInProgress)
            {
                return false;
            }

            //NextReleaseCount will be used by child entities to check if they still can generate from this parent
            IFlushStrategy flushTrigger = _generatorSetup.GetFlushTrigger(entityContext.Description);
            flushTrigger.SetNextReleaseCount(entityContext);

            //when all entities are generated also should flush
            bool isFlushRequired = flushTrigger.IsFlushRequired(entityContext);
            bool isCompleted = progress.CurrentCount >= progress.TargetCount;
            if (isCompleted)
            {
                isFlushRequired = true;
            }
            if (!isFlushRequired)
            {
                //if flush is not required NextFlushCount should not be updated
                //isFlushInProgress is using NextFlushCount 
                return isFlushRequired;
            }

            //update next flush count
            flushTrigger.SetNextFlushCount(entityContext);

            //add to flush candidates if will be used by dependent children
            bool hasDependentChild = FindChildThatCanGenerate(entityContext, true) != null;
            if (hasDependentChild)
            {
                _flushCandidates.Add(entityContext);
            }

            return isFlushRequired;
        }


        //Flush actions
        public virtual List<ICommand> GetNextFlushCommands(EntityContext entityContext)
        {
            var flushCommands = new List<ICommand>();

            //needs to create ids by database before using as required by children
            //will write to persitent storage, but won't remove from temporary storage
            if (entityContext.Description.InsertToPersistentStorageBeforeUse)
            {
                flushCommands.Add(new GenerateStorageIdsCommand(entityContext, _generatorSetup));
            }

            //also check parent entities, if they are no longer required and can be flushed too
            AppendParentsFlushActions(entityContext.ParentEntities, flushCommands);

            bool hasDependentChild = FindChildThatCanGenerate(entityContext, true) != null;
            if (hasDependentChild)
            {
                return flushCommands;
            }

            //has no dependent children, so can flush to persistent storage
            ICommand command = entityContext.Description.InsertToPersistentStorageBeforeUse
                ? new ReleaseFromTempStorageCommand(entityContext, _generatorSetup, this)
                : (ICommand)new FlushCommand(entityContext, _generatorSetup, this);
            flushCommands.Add(command);

            return flushCommands;
        }

        protected virtual void AppendParentsFlushActions(
            List<IEntityDescription> parents, List<ICommand> flushCommands)
        {
            foreach (IEntityDescription parent in parents)
            {
                EntityContext parentContext = _entityContexts[parent.Type];
                bool isFlushCandidate = _flushCandidates.Contains(parentContext);
                if (isFlushCandidate == false)
                {
                    return;
                }

                bool hasDependentChild = FindChildThatCanGenerate(parentContext, true) != null;
                if (hasDependentChild)
                {
                    return;
                }

                _flushCandidates.Remove(parentContext);

                ICommand command = parentContext.Description.InsertToPersistentStorageBeforeUse
                    ? new ReleaseFromTempStorageCommand(parentContext, _generatorSetup, this)
                    : (ICommand)new FlushCommand(parentContext, _generatorSetup, this);
                flushCommands.Add(command);

                AppendParentsFlushActions(parentContext.ParentEntities, flushCommands);
            }
        }

        /// <summary>  
        /// FlushCandidates are Entities in temporary storage, that still have child entities requiring em.
        /// Will generate all children that require FlushCandidates as a parent before flushing em.
        /// After all direct (1 level) children are generated they also might become candidates to flush into permanent storage.
        /// </summary>
        /// <param name="parentContext">Parent entity that is a candidate to flush generated entities to permanent storage</param>
        /// <param name="onlyCheckCanGenerate">When onlyCheckCanGenerate=true, is used to deside if parent entity if flush ready. If false then parent entity won't be ready to flush, but skip generating children that should generage their children first. Leaf nodes go first, then their parents.</param>
        /// <returns></returns>
        protected virtual EntityContext FindChildThatCanGenerate(
            EntityContext parentContext, bool onlyCheckCanGenerate)
        {
            List<EntityContext> notCompletedChildren = parentContext.ChildEntities
               .Where(child => !_progressState.CompletedEntityTypes.Contains(child.Type))
               .Select(x => _entityContexts[x.Type])
               .ToList();
            if (notCompletedChildren.Count == 0)
            {
                return null;
            }

            EntityContext childContext = notCompletedChildren.Find(child => 
                CheckChildCanGenerate(child, parentContext, onlyCheckCanGenerate));
            return childContext;
        }

        protected virtual bool CheckChildCanGenerate(EntityContext child,
            EntityContext parent, bool onlyCheckCanGenerate)
        {
            Type typeToFlush = parent.Type;
            RequiredEntity requiredParent = child.Description.Required.First(x => x.Type == typeToFlush);
            ISpreadStrategy spreadStrategy = _generatorSetup.GetSpreadStrategy(child.Description, requiredParent);

            //check if child can still use parent entities from temporary storage to generate
            bool canGenerateMore = spreadStrategy.CanGenerateFromParentNextReleaseCount(parent, child);
            if (onlyCheckCanGenerate)
            {
                return canGenerateMore;
            }

            //check if child is not a flush candidate itself
            bool childIsFlushCandidate = _flushCandidates.Contains(child);
            return !childIsFlushCandidate && canGenerateMore;
        }


        //Find next node to generate among flush candidates children
        public virtual EntityContext FindChildOfFlushCandidates()
        {
            //find child nodes of flush candidates
            EntityContext childCanGenerate = null;

            foreach (EntityContext entity in _flushCandidates)
            {
                childCanGenerate = FindChildThatCanGenerate(entity, false);
                if(childCanGenerate != null){
                    break;
                }
            }

            return childCanGenerate;
        }
    }
}
