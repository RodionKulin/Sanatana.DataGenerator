using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Internals.Reflection;
using Sanatana.DataGenerator.Internals.Debugging;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

[assembly: InternalsVisibleTo("Sanatana.DataGeneratorSpecs")]
[assembly: InternalsVisibleTo("Sanatana.DataGenerator.EntityFrameworkCoreSpecs")]
namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Setup class to register all the entities with their generators and start to generate
    /// </summary>
    public class GeneratorSetup : IDisposable
    {
        //fields
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected ReflectionInvoker _reflectionInvoker;


        //public properties
        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }        
        /// <summary>
        /// Producer of generation and flush commands. Determines the order in which entity instances will be generated.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public ISupervisor Supervisor { get; set; }
        /// <summary>
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public DefaultSettings Defaults { get; set; }
        /// <summary>
        /// Progress change event holding class that will report overall completion percent in range from 0 to 100.
        /// </summary>
        public ProgressEventTrigger Progress { get; protected set; }


        //internal properties
        /// <summary>
        /// Configuration validator that will throw errors on missing or inconsistent setup
        /// </summary>
        internal Validator Validator { get; set; }
        /// <summary>
        /// Inmemory storage for generated entities to accumulate batches before inserting to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }
        internal CommandsHistory CommandsHistory { get; set; }


        //init
        public GeneratorSetup()
        {
            _reflectionInvoker = new ReflectionInvoker();
            EntityDescriptions = new Dictionary<Type, IEntityDescription>();
            TemporaryStorage = new TemporaryStorage(this);
            CommandsHistory = new CommandsHistory();
            Progress = new ProgressEventTrigger(this);

            Defaults = new DefaultSettings();
            Supervisor = new CompleteSupervisor();
        }


        //Register entity
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

        public virtual GeneratorSetup RegisterEntity(IEnumerable<IEntityDescription> entityDescriptions)
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

            IEntityDescription entityDescription = GetEntityDescription(entityType);
            if (!(entityDescription is EntityDescription<TEntity>))
            {
                Type descriptionActualType = entityDescription.GetType();
                Type descriptionBaseType = typeof(EntityDescription<>);
                throw new TypeAccessException($"Entity type [{entityType.FullName}] was registered with description of type [{descriptionActualType.FullName}]. Not able to cast description to type [{descriptionBaseType.FullName}]. Use {nameof(EntityDescriptions)} property instead.");
            }

            return (EntityDescription<TEntity>)entityDescription;
        }

        public virtual IEntityDescription GetEntityDescription(Type entityType)
        {
            bool isEntityRegistered = EntityDescriptions.ContainsKey(entityType);
            if (!isEntityRegistered || EntityDescriptions[entityType] == null)
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered. Use {nameof(RegisterEntity)} method first.");
            }

            IEntityDescription entityDescription = EntityDescriptions[entityType];
            return entityDescription;
        }


        //Generation start
        public virtual void Generate()
        {
            Validate();

            Setup();

            ExecuteGenerationLoop();

        }

        protected virtual void Validate()
        {
            Validator = new Validator(this);
            Validator.CheckGeneratorSetupComplete(EntityDescriptions);
            Validator.CheckRequiredEntitiesPresent(EntityDescriptions);
            Validator.CheckCircularDependencies(EntityDescriptions);
            Validator.CheckGeneratorsParams(EntityDescriptions);
            Validator.CheckModifiersParams(EntityDescriptions);
            Validator.CheckEntitySettingsForGenerators(EntityDescriptions);
        }

        protected virtual void Setup()
        {
            Progress.Clear();
            CommandsHistory.Clear();
            _entityContexts = SetupEntityContexts(EntityDescriptions);
            SetupSpreadStrategies();
            Supervisor.Setup(this, _entityContexts);
        }

        internal virtual Dictionary<Type, EntityContext> SetupEntityContexts(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            Dictionary<Type, EntityContext> entityContexts = entityDescriptions.Values
                .Select(description => EntityContext.Factory.Create(entityDescriptions, description, Defaults))
                .ToDictionary(entityContext => entityContext.Type, entityContext => entityContext);
            return entityContexts;
        }

        protected virtual void SetupSpreadStrategies()
        {
            foreach (EntityContext entity in _entityContexts.Values)
            {
                if (entity.Description.Required == null)
                {
                    continue;
                }

                foreach (RequiredEntity required in entity.Description.Required)
                {
                    required.SpreadStrategy.Setup(entity, _entityContexts);
                }
            }
        }


        //Execution loop
        protected virtual void ExecuteGenerationLoop()
        {
            foreach (ICommand command in Supervisor.IterateCommands())
            {
                CommandsHistory.LogCommand(command);
                command.Execute();
                Progress.UpdateProgressInt(forceUpdate: false);
            }

            TemporaryStorage.WaitAllTasks();
            Progress.UpdateProgressInt(forceUpdate: true);
        }
        


        //IDisposalbe
        /// <summary>
        /// Will 
        /// -call Dispose() on ReaderWriterLockSlim for entities 
        /// -call Dispose() on IPersistentStorage storages
        /// -unsubscribe all ProgressChanged event handlers
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void Dispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                List<IPersistentStorage> storages = Defaults.GetPersistentStorages(entityContext.Description);
                storages.ForEach(storage => storage.Dispose());

                entityContext.Dispose();
            }

            Progress.Dispose();
        }
    }
}
