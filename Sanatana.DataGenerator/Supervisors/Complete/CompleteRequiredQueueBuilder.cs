using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System.Collections;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Internals.Commands;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Extensions;

namespace Sanatana.DataGenerator.Supervisors.Complete
{
    public class CompleteRequiredQueueBuilder : IRequiredQueueBuilder
    {
        //fields
        protected GeneratorServices _generatorServices;
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected INextNodeFinder _nextNodeFinder;
        /// <summary>
        /// List of next entities to generate
        /// </summary>
        protected List<OrderedType> _entitiesOrder;


        //init
        public CompleteRequiredQueueBuilder(GeneratorServices generatorServices, INextNodeFinder nextNodeFinder)
        {
            _generatorServices = generatorServices;
            _entityContexts = generatorServices.EntityContexts;
            _nextNodeFinder = nextNodeFinder;
            _entitiesOrder = new List<OrderedType>();
        }


        //Get next command from stack
        public virtual ICommand GetNextCommand()
        {
            if (_entitiesOrder.Count == 0)
            {
                EntityContext nextNode = _nextNodeFinder.FindNextNode();
                if (nextNode == null)
                {
                    return null;
                }

                PushEntity(nextNode.Type, 1, _entitiesOrder);
                PushRequiredEntitiesRecursive(nextNode, _entitiesOrder);
            }

            OrderedType next = _entitiesOrder.Peek();
            return new GenerateCommand(_entityContexts[next.EntityType], _generatorServices);
        }

        protected virtual bool PushEntity(Type type, long newItemsCount, List<OrderedType> entitiesOrder)
        {
            if (newItemsCount < 1)
            {
                return false;
            }

            EntityContext entityContext = _entityContexts[type];
            entityContext.EntityProgress.NextIterationCount =
                entityContext.EntityProgress.CurrentCount + newItemsCount;

            OrderedType orderedType = entitiesOrder.LastOrDefault(x => x.EntityType == type);
            if (orderedType == null)
            {
                entitiesOrder.Add(new OrderedType
                {
                    EntityType = type,
                    GenerateCount = newItemsCount
                });
                return true;
            }

            //make sure order of Required entities is respected recursively
            entitiesOrder.Remove(orderedType);
            entitiesOrder.Add(orderedType);

            //update required count
            if (orderedType.GenerateCount < newItemsCount)
            {
                orderedType.GenerateCount = newItemsCount;
                return true;
            }

            return false;
        }

        protected virtual void PushRequiredEntitiesRecursive(EntityContext child, List<OrderedType> entitiesOrder)
        {
            if (child.Description.Required == null)
            {
                return;
            }

            foreach (RequiredEntity requiredEntity in child.Description.Required)
            {
                EntityContext parent = _entityContexts[requiredEntity.Type];
                ISpreadStrategy spreadStrategy = _generatorServices.Defaults.GetSpreadStrategy(child.Description, requiredEntity);

                long parentRequiredCount = spreadStrategy.GetNextIterationParentCount(parent, child);
                long parentNewItemsCount = parentRequiredCount - parent.EntityProgress.CurrentCount;

                bool added = PushEntity(requiredEntity.Type, parentNewItemsCount, entitiesOrder);
                if (added)
                {
                    PushRequiredEntitiesRecursive(parent, entitiesOrder);
                }
            }
        }


        //Update counters
        public virtual void UpdateCounters(EntityContext entityContext, IList generatedEntities, bool isFlushRequired)
        {
            OrderedType next = _entitiesOrder.Peek();
            if (entityContext.Type != next.EntityType)
            {
                throw new ArgumentException($"Type {entityContext.Type.FullName} provided in {nameof(UpdateCounters)} does not match the latest action Type {next.EntityType}");
            }

            next.GenerateCount -= generatedEntities.Count;

            if (entityContext.Description.InsertToPersistentStorageBeforeUse)
            {
                //for entities that generate Id on database, will generate as much as possible before flushing.
                //the number to generate will be determined by IFlushTrigger.
                if (isFlushRequired)
                {
                    _entitiesOrder.Pop();
                }
                return;
            }

            if (next.GenerateCount <= 0)
            {
                _entitiesOrder.Pop();
            }
        }
    }
}
