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
        protected DefaultSettings _defaults;
        protected ISupervisor _supervisor;
        protected ReflectionInvoker _reflectionInvoker;
        protected CommandsHistory _commandsHistory;
        protected ProgressEventTrigger _progress;


        //public properties
        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }


        //internal properties
        /// <summary>
        /// Configuration validator that will throw errors on missing or inconsistent setup
        /// </summary>
        internal Validator Validator { get; set; }
        /// <summary>
        /// Inmemory storage for generated entities to accumulate batches before inserting to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }


        //init
        public GeneratorSetup()
        {
            _reflectionInvoker = new ReflectionInvoker();
            EntityDescriptions = new Dictionary<Type, IEntityDescription>();
            _commandsHistory = new CommandsHistory();

            TemporaryStorage = new TemporaryStorage(this);
            _defaults = new DefaultSettings();
            _supervisor = new CompleteSupervisor();
            _progress = new ProgressEventTrigger(_supervisor);
        }

        public GeneratorSetup(IEnumerable<IEntityDescription> entityDescriptions,
            ISupervisor supervisor, DefaultSettings defaults, ProgressEventTrigger progress)
        {
            _reflectionInvoker = new ReflectionInvoker();
            _commandsHistory = new CommandsHistory();
            EntityDescriptions = entityDescriptions.ToDictionary(x => x.Type, x => x);

            TemporaryStorage = new TemporaryStorage(this);
            _defaults = defaults;
            _supervisor = supervisor;
            _progress = progress;
        }

        protected virtual GeneratorSetup Clone(IEnumerable<IEntityDescription> entityDescriptions = null,
            ISupervisor supervisor = null, DefaultSettings defaults = null, ProgressEventTrigger progress = null)
        {
            entityDescriptions = entityDescriptions ?? new List<IEntityDescription>(EntityDescriptions.Values);
            supervisor = supervisor ?? _supervisor.Clone();
            defaults = defaults ?? _defaults.Clone();
            progress = progress ?? _progress.Clone();   

            return new GeneratorSetup(entityDescriptions, supervisor, defaults, progress);
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

        public virtual EntityDescription<TEntity> GetEntity<TEntity>()
            where TEntity : class
        {
            Type entityType = typeof(TEntity);

            IEntityDescription entityDescription = GetEntity(entityType);
            if (!(entityDescription is EntityDescription<TEntity>))
            {
                Type descriptionActualType = entityDescription.GetType();
                Type descriptionBaseType = typeof(EntityDescription<>);
                throw new TypeAccessException($"Entity type [{entityType.FullName}] was registered with description of type [{descriptionActualType.FullName}]. Not able to cast description to type [{descriptionBaseType.FullName}]. Use {nameof(EntityDescriptions)} property instead.");
            }

            return (EntityDescription<TEntity>)entityDescription;
        }

        public virtual IEntityDescription GetEntity(Type entityType)
        {
            bool isEntityRegistered = EntityDescriptions.ContainsKey(entityType);
            if (!isEntityRegistered || EntityDescriptions[entityType] == null)
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered. Use {nameof(RegisterEntity)} method first.");
            }

            IEntityDescription entityDescription = EntityDescriptions[entityType];
            return entityDescription;
        }



        //Configure services
        /// <summary>
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public virtual GeneratorSetup ConfigureDefaultSettings(Func<DefaultSettings, DefaultSettings> defaultsSetup)
        {
            DefaultSettings defaults = _defaults.Clone();
            defaultsSetup.Invoke(defaults);
            return Clone(defaults: defaults);
        }

        /// <summary>
        /// Producer of generation and flush commands. Determines the order in which entity instances will be generated.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public virtual GeneratorSetup ConfigureSupervisor(Func<ISupervisor, ISupervisor> supervisorSetup)
        {
            ISupervisor supervisor = _supervisor.Clone();
            supervisorSetup.Invoke(supervisor);
            return Clone(supervisor: supervisor);
        }

        /// <summary>
        /// Producer of generation and flush commands. Determines the order in which entity instances will be generated.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public virtual GeneratorSetup SetSupervisor(ISupervisor supervisor)
        {
            return Clone(supervisor: supervisor);
        }

        /// <summary>
        /// Progress change event holding class that will report overall completion percent in range from 0 to 100.
        /// </summary>
        public virtual GeneratorSetup ConfigureProgressEventTrigger(Action<ProgressEventTrigger> progressSetup)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressSetup.Invoke(progressEventTrigger);
            return Clone(progress: progressEventTrigger);
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
            _progress.Setup(_supervisor);
            _progress.Clear();
            _commandsHistory.Clear();
            _entityContexts = SetupEntityContexts(EntityDescriptions);
            SetupSpreadStrategies();
            _supervisor.Setup(this, _entityContexts);
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
            foreach (ICommand command in _supervisor.IterateCommands())
            {
                _commandsHistory.LogCommand(command);
                command.Execute();
                Progress.UpdateProgressInt(forceUpdate: false);
            }

            TemporaryStorage.WaitAllTasks();
            Progress.UpdateProgressInt(forceUpdate: true);
        }


        //Singular instances generation setup
        public virtual SingularGeneratorSetup ToSingular()
        {
            return new SingularGeneratorSetup(this);
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
                List<IPersistentStorage> storages = _defaults.GetPersistentStorages(entityContext.Description);
                storages.ForEach(storage => storage.Dispose());

                entityContext.Dispose();
            }

            Progress.Dispose();
        }
    }
}
