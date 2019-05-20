using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder;
using Sanatana.DataGenerator.GenerationOrder.Complete;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Modifiers;

[assembly: InternalsVisibleTo("Sanatana.DataGeneratorSpecs")]
namespace Sanatana.DataGenerator
{
    public class GeneratorSetup
    {
        //fields
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected ReflectionInvoker _reflectionInvoker;


        //event
        public event Action<GeneratorSetup, decimal> ProgressChanged;


        //properties
        /// <summary>
        /// Configuration validator
        /// </summary>
        internal Validator Validator { get; set; }
        /// <summary>
        /// In memory storage for generated entities with they are batch inserted to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }
        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }
        /// <summary>
        /// Provider of entity types to generate using entity configuration
        /// </summary>
        public IOrderProvider OrderProvider { get; set; }
        /// <summary>
        /// Default entities generator. 
        /// Will be used for entity types that does not have a Generator specified.
        /// </summary>
        public IGenerator DefaultGenerator { get; set; }
        /// <summary>
        /// Default post processing method to make adjustments to entity after generated.
        /// Will be used for entity types that does not have a PostProcessor specified.
        /// </summary>
        public List<IModifier> DefaultModifiers { get; set; }
        /// <summary>
        /// Default persistent storage for generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// </summary>
        public IPersistentStorage DefaultPersistentStorage { get; set; }
        /// <summary>
        /// Default strategy to reuse same parent entities among multiple child entities.
        /// Will be used for Required entity types that does not have a SpreadStrategy specified.
        /// </summary>
        public ISpreadStrategy DefaultSpreadStrategy { get; set; }
        /// <summary>
        /// Default provider of total number of entities that needs to be generated.
        /// Will be used for entity types that does not have a QuantityProvider specified.
        /// </summary>
        public IQuantityProvider DefaultQuantityProvider { get; set; }
        /// <summary>
        /// Default entity persistent storage write trigger.
        /// Will be used for entity types that does not have a FlushTrigger specified.
        /// </summary>
        public IFlushTrigger DefaultFlushTrigger { get; set; }

        

        //init
        public GeneratorSetup()
        {
            _reflectionInvoker = new ReflectionInvoker();
            EntityDescriptions = new Dictionary<Type, IEntityDescription>();
            TemporaryStorage = new TemporaryStorage();

            DefaultSpreadStrategy = new EvenSpreadStrategy();
            DefaultFlushTrigger = new LimitedCapacityFlushTrigger(100);
            OrderProvider = new CompleteOrderProvider();
        }

        internal virtual Dictionary<Type, EntityContext> SetupEntityContexts(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            var entityContexts = new Dictionary<Type, EntityContext>();

            foreach (IEntityDescription description in entityDescriptions.Values)
            {
                List<IEntityDescription> children = entityDescriptions.Values
                    .Where(x => x.Required != null
                        && x.Required.Select(req => req.Type).Contains(description.Type))
                    .ToList();

                List<IEntityDescription> parents = new List<IEntityDescription>();
                if (description.Required != null)
                {
                    IEnumerable<Type> parentTypes = description.Required.Select(x => x.Type);
                    parents = entityDescriptions.Values
                       .Where(x => parentTypes.Contains(x.Type))
                       .ToList();
                }

                IQuantityProvider quantityProvider = GetQuantityProvider(description);
                long targetTotalCount = quantityProvider.GetTargetTotalQuantity();

                entityContexts.Add(description.Type, new EntityContext
                {
                    Type = description.Type,
                    Description = description,
                    ChildEntities = children,
                    ParentEntities = parents,
                    EntityProgress = new EntityProgress
                    {
                        TargetCount = targetTotalCount
                    }
                });
            }

            return entityContexts;
        }

        protected virtual void SetupSpreadStrategies()
        {
            foreach (EntityContext entity in _entityContexts.Values)
            {
                if(entity.Description.Required == null)
                {
                    continue;
                }

                foreach (RequiredEntity required in entity.Description.Required)
                {
                    required.SpreadStrategy.Setup(entity, _entityContexts);
                }
            }
        }
        


        //Register entitities
        public virtual EntityDescription<TEntity> RegisterEntity<TEntity>()
            where TEntity : class
        {
            var entityDescription = new EntityDescription<TEntity>();
            EntityDescriptions.Add(entityDescription.Type, entityDescription);
            return entityDescription;
        }

        public virtual GeneratorSetup RegisterEntity(IEntityDescription entityDescription)
        {
            EntityDescriptions.Add(entityDescription.Type, entityDescription);
            return this;
        }

        public virtual GeneratorSetup RegisterEntities(
            IEnumerable<IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription item in entityDescriptions)
            {
                EntityDescriptions.Add(item.Type, item);
            }
            return this;
        }
        


        //Generation main steps
        public virtual void Generate()
        {
            Validate();

            Setup();

            ExecuteGenerationLoop();

            //finish
            FlushStorage();
            UpdateProgress();
            Dispose();
        }

        protected virtual void Validate()
        {
            Validator = new Validator(this);
            Validator.CheckGeneratorSetupComplete(EntityDescriptions);
            Validator.CheckRequiredEntitiesPresent(EntityDescriptions);
            Validator.CheckCircularDependencies(EntityDescriptions);
            Validator.CheckGeneratorsParams(EntityDescriptions);
            Validator.CheckModifiersParams(EntityDescriptions);
        }

        protected virtual void Setup()
        {
            _entityContexts = SetupEntityContexts(EntityDescriptions);
            SetupSpreadStrategies();
            OrderProvider.Setup(this, _entityContexts);
        }
        


        //Execution loop
        protected virtual void ExecuteGenerationLoop()
        {
            while (true)
            {
                UpdateProgress();

                //handle completed flush actions
                List<EntityAction> completedActions = TemporaryStorage.GetCompletedActions();
                foreach (EntityAction completedAction in completedActions)
                {
                    OrderProvider.HandleFlushCompleted(completedAction);
                }

                //get next action
                EntityAction action = OrderProvider.GetNextAction();
                if (action.ActionType == ActionType.Finish)
                {
                    break;
                }

                IPersistentStorage persistentStorage = null;
                switch (action.ActionType)
                {
                    case ActionType.Generate:
                        GenerateEntity(action);
                        break;
                    case ActionType.FlushToPersistentStorage:
                        persistentStorage = GetPersistentStorage(action.EntityContext.Description);
                        TemporaryStorage.FlushToPersistent(action, persistentStorage);
                        break;
                    case ActionType.GenerateStorageIds:
                        persistentStorage = GetPersistentStorage(action.EntityContext.Description);
                        TemporaryStorage.GenerateStorageIds(action, persistentStorage);

                        break;
                }
            }
        }
        
        protected virtual void GenerateEntity(EntityAction action)
        {
            IEntityDescription entityDescription = action.EntityContext.Description;
            IGenerator generator = GetGenerator(entityDescription);
            List<IModifier> modifiers = GetModifiers(entityDescription);
            
            var context = new GeneratorContext
            {
                Description = entityDescription,
                TargetCount = action.EntityContext.EntityProgress.TargetCount,
                CurrentCount = action.EntityContext.EntityProgress.CurrentCount,
                RequiredEntities = GetRequiredEntities(action),
            };

            
            IList entities = generator.Generate(context);
            Validator.CheckGeneratedCount(entities, entityDescription.Type, generator);

            foreach (IModifier modifier in modifiers)
            {
                entities = modifier.Modify(context, entities);
                Validator.CheckModifiedCount(entities, entityDescription.Type, modifier);
            }

            TemporaryStorage.InsertToTemporary(action.EntityContext, entities);
            OrderProvider.HandleGenerateCompleted(action.EntityContext, entities);
        }
        
        protected virtual Dictionary<Type, object> GetRequiredEntities(EntityAction action)
        {
            var result = new Dictionary<Type, object>();

            foreach (RequiredEntity requiredEntity in action.EntityContext.Description.Required)
            {
                Type parent = requiredEntity.Type;
                EntityContext parentEntityContext = _entityContexts[parent];

                ISpreadStrategy spreadStrategy = GetSpreadStrategy(action.EntityContext.Description, requiredEntity);
                long parentEntityIndex = spreadStrategy.GetParentIndex(parentEntityContext, action.EntityContext);

                object parentEntity = TemporaryStorage.Select(parentEntityContext, parentEntityIndex);
                result.Add(parent, parentEntity);
            }

            return result;
        }

        protected virtual void UpdateProgress()
        {
            long actionCalls = IdIterator.GetNextId<IProgressState>();

            //trigger handler only every N generated entities
            long onEveryNCall = 100;
            bool invoke = actionCalls % onEveryNCall == 0;
            if (!invoke)
            {
                return;
            }

            //invoke handler
            decimal percents = OrderProvider.ProgressState.GetCompletionPercents();
            var progressChanged = ProgressChanged;
            if (progressChanged != null)
            {
                progressChanged(this, percents);
            }
        }



        //Flush all and dispose
        protected virtual void FlushStorage()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                entityContext.EntityProgress.NextFlushCount = entityContext.EntityProgress.CurrentCount;
                entityContext.EntityProgress.NextReleaseCount = entityContext.EntityProgress.CurrentCount;

                IPersistentStorage storage = GetPersistentStorage(entityContext.Description);

                if (entityContext.Description.InsertToPersistentStorageBeforeUse)
                {
                    TemporaryStorage.GenerateStorageIds(new EntityAction
                    {
                        ActionType = ActionType.GenerateStorageIds,
                        EntityContext = entityContext
                    }, storage);
                }

                TemporaryStorage.FlushToPersistent(new EntityAction
                {
                    ActionType = ActionType.FlushToPersistentStorage,
                    EntityContext = entityContext
                }, storage);
            }

            TemporaryStorage.WaitAllTasks();
        }

        protected virtual void Dispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                IPersistentStorage storage = GetPersistentStorage(entityContext.Description);
                storage.Dispose();

                entityContext.Dispose();
            }
        }



        //Get entity specific or default service
        internal virtual IGenerator GetGenerator(IEntityDescription entityDescription)
        {
            if (entityDescription.Generator != null)
            {
                return entityDescription.Generator;
            }

            if (DefaultGenerator != null)
            {
                return DefaultGenerator;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IGenerator)} configured and {nameof(DefaultGenerator)} also was not provided.");
        }

        internal virtual List<IModifier> GetModifiers(IEntityDescription entityDescription)
        {
            if (entityDescription.Modifiers != null
                && entityDescription.Modifiers.Count > 0)
            {
                return entityDescription.Modifiers;
            }

            if (DefaultModifiers != null)
            {
                return DefaultModifiers;
            }

            return new List<IModifier>();
        }

        internal virtual IQuantityProvider GetQuantityProvider(IEntityDescription entityDescription)
        {
            if(entityDescription.QuantityProvider != null)
            {
                return entityDescription.QuantityProvider;
            }

            if(DefaultQuantityProvider != null)
            {
                return DefaultQuantityProvider;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IQuantityProvider)} configured and {nameof(DefaultQuantityProvider)} also was not provided.");
        }

        internal virtual IPersistentStorage GetPersistentStorage(IEntityDescription entityDescription)
        {
            if (entityDescription.PersistentStorage != null)
            {
                return entityDescription.PersistentStorage;
            }

            if (DefaultPersistentStorage != null)
            {
                return DefaultPersistentStorage;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IPersistentStorage)} configured and {nameof(DefaultPersistentStorage)} also was not provided.");
        }

        internal virtual IFlushTrigger GetFlushTrigger(IEntityDescription entityDescription)
        {
            if (entityDescription.FlushTrigger != null)
            {
                return entityDescription.FlushTrigger;
            }

            if (DefaultFlushTrigger != null)
            {
                return DefaultFlushTrigger;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IFlushTrigger)} configured and {nameof(DefaultFlushTrigger)} also was not provided.");
        }

        internal virtual ISpreadStrategy GetSpreadStrategy(IEntityDescription entityDescription, RequiredEntity requiredEntity)
        {
            if (requiredEntity.SpreadStrategy != null)
            {
                return requiredEntity.SpreadStrategy;
            }

            if (DefaultSpreadStrategy != null)
            {
                return DefaultSpreadStrategy;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} for required entity {requiredEntity.Type} did not have an {nameof(ISpreadStrategy)} configured and {nameof(DefaultSpreadStrategy)} also was not provided.");
        }

    }
}
