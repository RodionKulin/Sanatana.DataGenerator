using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Linq;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGenerator.GenerationOrder.Complete
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
            IFlushTrigger flushTrigger = _generatorSetup.GetFlushTrigger(entityContext.Description);
            bool isFlushRequired = flushTrigger.IsFlushRequired(entityContext);

            if (isFlushRequired)
            {
                flushTrigger.SetNextFlushCount(entityContext);
            }

            return isFlushRequired;
        }


        //Flush actions
        public virtual List<EntityAction> GetFlushActions(EntityContext entityContext, IList generatedEntities)
        {
            var flushActions = new List<EntityAction>();

            bool hasDependentChild = FindChildThatCanGenerate(entityContext, true) != null;
            if (hasDependentChild)
            {
                _flushCandidates.Add(entityContext);

                if (entityContext.Description.InsertToPersistentStorageBeforeUse)
                {
                    flushActions.Add(new EntityAction
                    {
                        ActionType = ActionType.GenerateStorageIds,
                        EntityContext = entityContext
                    });
                }
                return flushActions;
            }

            //has no dependent children, so can flush to persistent storage
            flushActions.Add(new EntityAction
            {
                ActionType = ActionType.FlushToPersistentStorare,
                EntityContext = entityContext
            });
            
            //also check parent entities, if they are no longer required and can be flushed too
            CheckParentFlushCandidatesAreFlushReady(entityContext, flushActions);
            return flushActions;
        }

        protected virtual void CheckParentFlushCandidatesAreFlushReady(
            EntityContext childContext, List<EntityAction> flushActions)
        {
            foreach (IEntityDescription parent in childContext.ParentEntities)
            {
                EntityContext parentEntityContext = _entityContexts[parent.Type];
                bool isFlushCandidate = _flushCandidates.Contains(parentEntityContext);
                if (isFlushCandidate == false)
                {
                    continue;
                }

                bool hasDependentChild = FindChildThatCanGenerate(parentEntityContext, true) != null;
                if (hasDependentChild == false)
                {
                    _flushCandidates.Remove(parentEntityContext);
                    flushActions.Add(new EntityAction
                    {
                        ActionType = ActionType.FlushToPersistentStorare,
                        EntityContext = parentEntityContext
                    });
                    CheckParentFlushCandidatesAreFlushReady(parentEntityContext, flushActions);
                }
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
        protected virtual EntityContext FindChildThatCanGenerate(EntityContext parentContext, bool onlyCheckCanGenerate)
        {
            List<IEntityDescription> notCompletedChildren = parentContext.ChildEntities
               .Where(child => !_progressState.CompletedEntityTypes.Contains(child.Type))
               .ToList();
            if (notCompletedChildren.Count == 0)
            {
                return null;
            }

            EntityContext childContext = FindChildThatCanGenerate(notCompletedChildren,
                parentContext, onlyCheckCanGenerate);
            return childContext;
        }

        protected virtual EntityContext FindChildThatCanGenerate(List<IEntityDescription> notCompletedChildren,
            EntityContext parentContext, bool onlyCheckCanGenerate)
        {
            Type typeToFlush = parentContext.Type;
            EntityContext childContext = null;

            IEntityDescription canGenerateChild = notCompletedChildren.FirstOrDefault(child =>
            {
                childContext = _entityContexts[child.Type];
                RequiredEntity parent = child.Required.First(x => x.Type == typeToFlush);
                ISpreadStrategy spreadStrategy = _generatorSetup.GetSpreadStrategy(child, parent);

                //check if child can still use parent entities from temporary storage to generate
                bool canGenerateMore = spreadStrategy.CheckIfMoreChildrenCanBeGeneratedFromParentsNextFlushCount(
                    parentContext, childContext);
                if (onlyCheckCanGenerate)
                {
                    return canGenerateMore;
                }

                //check if child is not a flush candidate itself
                bool childIsFlushCandidate = _flushCandidates.Contains(childContext);
                return !childIsFlushCandidate && canGenerateMore;
            });

            return childContext;
        }


        //Find next node to generate among flush candidates children
        public virtual EntityContext FindChildOfFlushCandidates()
        {
            //find child nodes of flush candidates
            EntityContext childCanGenerate = null;
            bool hasChildOfFlushCandidate = _flushCandidates.Any(parentContext =>
            {
                childCanGenerate = FindChildThatCanGenerate(parentContext, false);
                return childCanGenerate != null;
            });

            return childCanGenerate;
        }
    }
}
