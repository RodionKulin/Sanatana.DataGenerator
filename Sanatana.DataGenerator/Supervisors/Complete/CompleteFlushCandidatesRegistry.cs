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
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.Objects;

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
        ///  Entities that are stored at temporary storage and reached a point when have enough count to flush to persistent storage. 
        ///  They are kept until required to generate other entities and will be flushed when no longer required.
        /// </summary>
        protected ReverseOrderedSet<EntityContext> _flushCandidates;
        /// <summary>
        /// List of entities that need to be generated and already completed.
        /// </summary>
        protected IProgressState _progressState;


        //init
        public CompleteFlushCandidatesRegistry(GeneratorSetup generatorSetup,
            Dictionary<Type, EntityContext> entityContexts, IProgressState progressState)
        {
            _flushCandidates = new ReverseOrderedSet<EntityContext>();
            _generatorSetup = generatorSetup;
            _entityContexts = entityContexts;
            _progressState = progressState;
        }
        

        //Update flush ranges
        public virtual void UpdateRequestCapacity(EntityContext entityContext)
        {
            //NextReleaseCount will be used by child entities to check if they still can generate from this parent
            IRequestCapacityProvider requestCapacityProvider = _generatorSetup.Defaults.GetRequestCapacityProvider(entityContext.Description);
            IFlushStrategy flushStrategy = _generatorSetup.Defaults.GetFlushStrategy(entityContext.Description);

            //update capacity for latest range
            FlushRange latestRange = entityContext.EntityProgress.GetLatestRangeNoThrow();
            if(latestRange != null)
            {
                long capacity = requestCapacityProvider.GetCapacity(entityContext, latestRange);
                flushStrategy.UpdateFlushRangeCapacity(entityContext, latestRange, capacity);
            }

            //create new ranges
            FlushRange newRange = entityContext.EntityProgress.CreateNewRangeIfRequired();
            while (newRange != null)
            {
                long capacity = requestCapacityProvider.GetCapacity(entityContext, newRange);
                flushStrategy.UpdateFlushRangeCapacity(entityContext, newRange, capacity);

                newRange = entityContext.EntityProgress.CreateNewRangeIfRequired();
            }
        }

        public virtual bool UpdateFlushRequired(EntityContext entityContext)
        {
            //should support starting next FlushCommand (for next range) for same entity before previous flush command has ended.
            IFlushStrategy flushStrategy = _generatorSetup.Defaults.GetFlushStrategy(entityContext.Description);

            List<FlushRange> flushRequiredRanges = entityContext.EntityProgress.FlushRanges
                .Where(flushRange => flushStrategy.CheckIsFlushRequired(entityContext, flushRange))
                .ToList();
            flushRequiredRanges.ForEach(x => x.SetFlushStatus(FlushStatus.FlushRequired));

            return flushRequiredRanges.Count > 0;
        }


        //Flush commands
        public virtual List<ICommand> GetNextFlushCommands(EntityContext entityContext)
        {
            var flushCommands = new List<ICommand>();

            IEnumerable<FlushRange> flushRequiredRanges = entityContext.EntityProgress.FlushRanges
               .Where(flushRange => flushRange.FlushStatus == FlushStatus.FlushRequired);

            foreach (FlushRange flushRange in flushRequiredRanges)
            {
                //Need to create ids by database before using as required by children.
                //Will write to persitent storage, but won't remove from temporary storage.
                if (entityContext.Description.InsertToPersistentStorageBeforeUse)
                {
                    flushCommands.Add(new GenerateStorageIdsCommand(entityContext, flushRange, _generatorSetup));
                }

                //also check parent entities, if they are no longer required and can be flushed too
                AppendParentsFlushCommands(entityContext.ParentEntities, flushCommands, entityContext.Type.Name);

                //don't flush and release instances if child entities can still use it to generate
                bool hasChildThatCanGenerate = FindChildThatCanGenerate(entityContext, flushRange, true) != null;
                if (hasChildThatCanGenerate)
                {
                    _flushCandidates.Add(entityContext);
                }
                else
                {
                    ICommand command = entityContext.Description.InsertToPersistentStorageBeforeUse
                        ? new ReleaseFromTempStorageCommand(entityContext, flushRange, _generatorSetup, this, entityContext.Type.Name)
                        : (ICommand)new FlushCommand(entityContext, flushRange, _generatorSetup, this);
                    flushCommands.Add(command);
                }
            }

            return flushCommands;
        }

        protected virtual void AppendParentsFlushCommands(List<IEntityDescription> parents, 
            List<ICommand> flushCommands, string invokedBy)
        {
            foreach (IEntityDescription parent in parents)
            {
                EntityContext parentContext = _entityContexts[parent.Type];
                if (!_flushCandidates.Contains(parentContext))
                {
                    //parent is not a flush candidate
                    return; 
                }

                FlushRange[] flushRequiredRanges = parentContext.EntityProgress.FlushRanges
                   .Where(flushRange => flushRange.FlushStatus == FlushStatus.FlushRequired)
                   .ToArray();

                FlushRange[] noDependentChildRanges = flushRequiredRanges
                   .Where(flushRange => FindChildThatCanGenerate(parentContext, flushRange, true) == null)
                   .ToArray();

                if (flushRequiredRanges.Length == noDependentChildRanges.Length)
                { 
                    //all flush candidate ranges flushed, then entity no longer is a flush candidate
                    _flushCandidates.Remove(parentContext);
                }

                foreach (FlushRange flushRange in noDependentChildRanges)
                {
                    flushCommands.Add(parentContext.Description.InsertToPersistentStorageBeforeUse
                        ? new ReleaseFromTempStorageCommand(parentContext, flushRange, _generatorSetup, this, invokedBy)
                        : (ICommand)new FlushCommand(parentContext, flushRange, _generatorSetup, this));

                    AppendParentsFlushCommands(parentContext.ParentEntities, flushCommands, invokedBy);
                }
            }
        }

        /// <summary>  
        /// FlushCandidates are Entities in temporary storage, that still have child entities requiring em.
        /// Will generate all children that require FlushCandidates as a parent before flushing em.
        /// After all direct (1 level) children are generated they also might become candidates to flush into persistent storage if 2 level children exist.
        /// </summary>
        /// <param name="parentContext">Parent entity that is a candidate to flush generated entities to persistent storage</param>
        /// <param name="parentRange">Parent entity range of instances to check</param>
        /// <param name="onlyCheckCanGenerate">When onlyCheckCanGenerate=true, is used to decide if parent entity if flush ready. 
        /// If false then parent entity won't be ready to flush, but skip generating children that should generage their children first. 
        /// Leaf nodes go first, then their parents.</param>
        /// <returns></returns>
        protected virtual EntityContext FindChildThatCanGenerate(
            EntityContext parentContext, FlushRange parentRange, bool onlyCheckCanGenerate)
        {
            List<EntityContext> notCompletedChildren = parentContext.ChildEntities
               .Where(child => !_progressState.CompletedEntityTypes.Contains(child.Type))
               .Select(x => _entityContexts[x.Type])
               .ToList();
            if (notCompletedChildren.Count == 0)
            {
                return null;
            }

            return notCompletedChildren.Find(childContext =>
                CheckChildCanGenerate(childContext, parentContext, parentRange, onlyCheckCanGenerate));
        }

        protected virtual bool CheckChildCanGenerate(EntityContext child, EntityContext parent, 
            FlushRange parentRange, bool onlyCheckCanGenerate)
        {
            Type typeToFlush = parent.Type;
            RequiredEntity requiredParent = child.Description.Required.First(x => x.Type == typeToFlush);
            ISpreadStrategy spreadStrategy = _generatorSetup.Defaults.GetSpreadStrategy(child.Description, requiredParent);

            //check if child can still use parent entities from temporary storage to generate
            bool canGenerateMore = spreadStrategy.CanGenerateFromParentRange(parent, parentRange, child);
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
            //ReverseOrderedSet preserve order of flush candidates and check them in reverse order as candidates were inserted.
            //So latest flush candidate will receive new children first.
            foreach (EntityContext entityContext in _flushCandidates)
            {
                IEnumerable<FlushRange> flushRequiredRanges = entityContext.EntityProgress.FlushRanges
                   .Where(flushRange => flushRange.FlushStatus == FlushStatus.FlushRequired);

                foreach (FlushRange parentRange in flushRequiredRanges)
                {
                    EntityContext childCanGenerate = FindChildThatCanGenerate(entityContext, parentRange, false);
                    if (childCanGenerate != null)
                    {
                        return childCanGenerate;
                    }
                }
            }

            return null;
        }
    }
}
