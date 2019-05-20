using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using System.Collections;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGenerator.GenerationOrder.Complete
{
    public class CompleteRequiredQueueBuilder : IRequiredQueueBuilder
    {
        //fields
        protected GeneratorSetup _generatorSetup;
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected INextNodeFinder _nextNodeFinder;
        /// <summary>
        /// Queue of next entities to generate
        /// </summary>
        protected Stack<OrderIterationType> _queue;


        //init
        public CompleteRequiredQueueBuilder(GeneratorSetup generatorSetup, 
            Dictionary<Type, EntityContext> entityContexts, INextNodeFinder nextNodeFinder)
        {
            _generatorSetup = generatorSetup;
            _entityContexts = entityContexts;
            _nextNodeFinder = nextNodeFinder;
        }


        //Get next action from queue
        public virtual EntityAction GetNextAction()
        {
            if (_queue == null ||
                _queue.Count == 0)
            {
                EntityContext nextNode = _nextNodeFinder.FindNextNode();
                if (nextNode == null)
                {
                    return new EntityAction
                    {
                        ActionType = ActionType.Finish
                    };
                }

                _queue = CreateNextQueue(nextNode);
            }

            OrderIterationType next = _queue.Peek();
            return new EntityAction
            {
                ActionType = ActionType.Generate,
                EntityContext = _entityContexts[next.EntityType]
            };
        }

        /// <summary>
        /// Build a queue of required entities recursively
        /// </summary>
        /// <returns></returns>
        protected virtual Stack<OrderIterationType> CreateNextQueue(EntityContext next)
        {
            var generationOrder = new Stack<OrderIterationType>();
            PushQueueItem(generationOrder, next.Type, 1);
            AddRequiredParentsRecursive(next, generationOrder);

            return generationOrder;
        }

        protected virtual bool PushQueueItem(Stack<OrderIterationType> generationOrder, 
            Type type, long newItemsCount)
        {
            if (newItemsCount < 1)
            {
                return false;
            }

            EntityContext entityContext = _entityContexts[type];
            entityContext.EntityProgress.NextIterationCount =
                entityContext.EntityProgress.CurrentCount + newItemsCount;

            OrderIterationType order = generationOrder
                .FirstOrDefault(x => x.EntityType == type);

            if (order == null)
            {
                generationOrder.Push(new OrderIterationType
                {
                    EntityType = type,
                    GenerateCount = newItemsCount
                });
                return true;
            }

            if (order.GenerateCount < newItemsCount)
            {
                order.GenerateCount = newItemsCount;
                return true;
            }

            return false;
        }

        protected virtual void AddRequiredParentsRecursive(EntityContext child, Stack<OrderIterationType> generationOrder)
        {
            if (child.Description.Required == null)
            {
                return;
            }

            foreach (RequiredEntity requiredEntity in child.Description.Required)
            {
                EntityContext parent = _entityContexts[requiredEntity.Type];
                ISpreadStrategy spreadStrategy = _generatorSetup.GetSpreadStrategy(child.Description, requiredEntity);

                long parentRequiredCount = spreadStrategy.GetNextIterationParentsCount(parent, child);
                long parentNewItemsCount = parentRequiredCount - parent.EntityProgress.CurrentCount;

                bool added = PushQueueItem(generationOrder, requiredEntity.Type, parentNewItemsCount);
                if (added)
                {
                    AddRequiredParentsRecursive(parent, generationOrder);
                }
            }
        }


        //Update counters
        public virtual void UpdateCounters(
            EntityContext entityContext, IList generatedEntities, bool flushRequired)
        {
            OrderIterationType next = _queue.Peek();
            if (entityContext.Type != next.EntityType)
            {
                throw new ArgumentException($"Type {entityContext.Type.Name} provided in {nameof(UpdateCounters)} does not match the latest action Type {next.EntityType}");
            }

            next.GenerateCount -= generatedEntities.Count;

            if (entityContext.Description.InsertToPersistentStorageBeforeUse)
            {
                UpdateCountersForInsertToPersistentStorageBeforeUse(
                    entityContext, next, flushRequired);
                return;
            }

            if (next.GenerateCount <= 0)
            {
                _queue.Pop();
            }
        }

        private void UpdateCountersForInsertToPersistentStorageBeforeUse(
            EntityContext entityContext, OrderIterationType next, bool flushRequired)
        {
            EntityProgress progress = entityContext.EntityProgress;

            //if still flushing previous entities of that type, than do not generate too much
            bool flushInProgress = progress.IsFlushInProgress();
            if (flushInProgress)
            {
                if (next.GenerateCount <= 0)
                {
                    _queue.Pop();
                }
                return;
            }

            //for entities that generate Id on database, will generate as much as possible before flushing.
            //the number to generate will be determined by IFlushTrigger.
            bool completed = progress.TargetCount <= progress.CurrentCount;
            if (flushRequired || completed)
            {
                _queue.Pop();
            }
        }
    }
}
