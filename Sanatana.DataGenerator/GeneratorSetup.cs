using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.Commands;

[assembly: InternalsVisibleTo("Sanatana.DataGeneratorSpecs")]
namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Setup class to register all the entities with their generators and start to generate
    /// </summary>
    public class GeneratorSetup
    {
        //fields
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected ReflectionInvoker _reflectionInvoker;


        //event
        /// <summary>
        /// Progress change event that will report overall completion percent in range from 0 to 100.
        /// </summary>
        public event Action<GeneratorSetup, decimal> ProgressChanged;


        //properties
        /// <summary>
        /// Configuration validator that will throw errors on missing or inconsistent setup
        /// </summary>
        internal Validator Validator { get; set; }
        /// <summary>
        /// In memory storage for generated entities to accumulate some descent batches before inserting to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }
        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }
        /// <summary>
        /// Default strategy to trigger entity persistent storage writes.
        /// Will be used for entity types that does not have a FlushStrategy specified.
        /// LimitedCapacityFlushStrategy with a limit of 100 is set by default.
        /// </summary>
        public IFlushStrategy DefaultFlushStrategy { get; set; }
        /// <summary>
        /// Default entities generator. 
        /// Will be used for entity types that does not have a Generator specified.
        /// By default is not set.
        /// </summary>
        public IGenerator DefaultGenerator { get; set; }
        /// <summary>
        /// Default modifiers to make adjustments to entity after generation.
        /// Will be used for entity types that does not have Modifers specified.
        /// </summary>
        public List<IModifier> DefaultModifiers { get; set; }
        /// <summary>
        /// Default provider of total number of entities that needs to be generated.
        /// Will be used for entity types that does not have a QuantityProvider specified.
        /// By default is not set.
        /// </summary>
        public IQuantityProvider DefaultQuantityProvider { get; set; }
        /// <summary>
        /// Default strategy to reuse same parent entities among multiple child entities.
        /// Will be used for Required entity types that does not have a SpreadStrategy specified.
        /// EvenSpreadStrategy is set by default
        /// </summary>
        public ISpreadStrategy DefaultSpreadStrategy { get; set; }
        /// <summary>
        /// Default persistent storage for generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// By default is not set.
        /// </summary>
        public List<IPersistentStorage> DefaultPersistentStorages { get; set; }
        /// <summary>
        /// Producer of generation and flush commands.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public ISupervisor Supervisor { get; set; }



        //init
        public GeneratorSetup()
        {
            _reflectionInvoker = new ReflectionInvoker();
            EntityDescriptions = new Dictionary<Type, IEntityDescription>();
            TemporaryStorage = new TemporaryStorage();

            DefaultSpreadStrategy = new EvenSpreadStrategy();
            DefaultFlushStrategy = new LimitedCapacityFlushStrategy(100);
            DefaultPersistentStorages = new List<IPersistentStorage>();
            Supervisor = new CompleteSupervisor();
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
                long targetTotalCount = quantityProvider.GetTargetQuantity();

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

        public virtual GeneratorSetup RegisterEntities(IEnumerable<IEntityDescription> entityDescriptions)
        {
            foreach (IEntityDescription item in entityDescriptions)
            {
                EntityDescriptions.Add(item.Type, item);
            }
            return this;
        }
        

        //Get registered entity
        public virtual EntityDescription<TEntity> GetEntityDescription<TEntity>()
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            bool isEntityRegistered = EntityDescriptions.ContainsKey(entityType);
            if (!isEntityRegistered || EntityDescriptions[entityType] == null)
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered. Use {nameof(RegisterEntity)} method first.");
            }

            IEntityDescription entityDescription = EntityDescriptions[entityType];
            Type descriptionActualType = entityDescription.GetType();
            Type descriptionBaseType = typeof(EntityDescription<>);
            if (!(entityDescription is EntityDescription<TEntity>))
            {
                throw new TypeAccessException($"Entity type [{entityType.FullName}] was registered with description of type [{descriptionActualType.FullName}]. Not able to cast description to type [{descriptionBaseType.FullName}]. Use {nameof(EntityDescriptions)} property instead.");
            }

            return (EntityDescription<TEntity>)entityDescription;
        }


        //Generation main steps
        public virtual void Generate()
        {
            Validate();

            Setup();

            ExecuteGenerationLoop();

            UpdateProgress(forceUpdate: true);
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
            Supervisor.Setup(this, _entityContexts);
        }
        
        

        //Execution loop
        protected virtual void ExecuteGenerationLoop()
        {
            while (true)
            {
                UpdateProgress(forceUpdate: false);

                ICommand command = Supervisor.GetNextCommand();
                bool continueCommands = command.Execute();
                if (!continueCommands)
                {
                    break;
                }
            }
        }
        
        protected virtual void UpdateProgress(bool forceUpdate)
        {
            long actionCalls = IdIterator.GetNextId<IProgressState>();

            //trigger handler only every N generated entities
            long onEveryNCall = 100;
            bool invoke = actionCalls % onEveryNCall == 0;
            if (!invoke && forceUpdate == false)
            {
                return;
            }

            //invoke handler
            decimal percents = Supervisor.ProgressState.GetCompletionPercents();
            var progressChanged = ProgressChanged;
            if (progressChanged != null)
            {
                progressChanged(this, percents);
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

        internal virtual List<IPersistentStorage> GetPersistentStorages(IEntityDescription entityDescription)
        {
            if (entityDescription.PersistentStorages != null && entityDescription.PersistentStorages.Count > 0)
            {
                return entityDescription.PersistentStorages;
            }

            if (DefaultPersistentStorages != null && DefaultPersistentStorages.Count > 0)
            {
                return DefaultPersistentStorages;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IPersistentStorage)} configured and {nameof(DefaultPersistentStorages)} also was not provided.");
        }

        internal virtual IFlushStrategy GetFlushTrigger(IEntityDescription entityDescription)
        {
            if (entityDescription.FlushTrigger != null)
            {
                return entityDescription.FlushTrigger;
            }

            if (DefaultFlushStrategy != null)
            {
                return DefaultFlushStrategy;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IFlushStrategy)} configured and {nameof(DefaultFlushStrategy)} also was not provided.");
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
