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
        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        protected Dictionary<Type, IEntityDescription> _entityDescriptions;
        /// <summary>
        /// Configuration validator that will throw errors on missing or inconsistent setup
        /// </summary>
        protected Validator _validator;
        /// <summary>
        /// Inmemory storage for generated entities to accumulate batches before inserting to persistent storage.
        /// </summary>
        protected TemporaryStorage _temporaryStorage;


        //init
        public GeneratorSetup()
        {
            _reflectionInvoker = new ReflectionInvoker();
            _entityDescriptions = new Dictionary<Type, IEntityDescription>();
            _commandsHistory = new CommandsHistory();

            _temporaryStorage = new TemporaryStorage();
            _defaults = new DefaultSettings();
            _supervisor = new CompleteSupervisor();
            _progress = new ProgressEventTrigger();
        }

        public GeneratorSetup(Dictionary<Type, IEntityDescription> entityDescriptions, ISupervisor supervisor,
            DefaultSettings defaults, ProgressEventTrigger progress, TemporaryStorage temporaryStorage)
        {
            _reflectionInvoker = new ReflectionInvoker();
            _commandsHistory = new CommandsHistory();
            _entityDescriptions = entityDescriptions;

            _temporaryStorage = new TemporaryStorage();
            _defaults = defaults;
            _supervisor = supervisor;
            _progress = progress;
            _temporaryStorage = temporaryStorage;
        }

        protected virtual GeneratorSetup Clone(Dictionary<Type, IEntityDescription> entityDescriptions = null,
            ISupervisor supervisor = null, DefaultSettings defaults = null, ProgressEventTrigger progress = null,
            TemporaryStorage temporaryStorage = null)
        {
            entityDescriptions = entityDescriptions ?? _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            supervisor = supervisor ?? _supervisor.Clone();
            defaults = defaults ?? _defaults.Clone();
            progress = progress ?? _progress.Clone();
            temporaryStorage = temporaryStorage ?? _temporaryStorage.Clone();

            return new GeneratorSetup(entityDescriptions, supervisor, defaults, progress, temporaryStorage);
        }


        //Register entity
        /// <summary>
        /// Add new IEntityDescription.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual GeneratorSetup RegisterEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            var entityDescription = new EntityDescription<TEntity>();
            entityDescription = entityDescriptionSetup(entityDescription);
         
            return RegisterEntity(entityDescription);
        }

        /// <summary>
        /// Add new IEntityDescription.
        /// </summary>
        /// <param name="entityDescription"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual GeneratorSetup RegisterEntity(IEntityDescription entityDescription)
        {
            entityDescription = entityDescription ?? throw new ArgumentNullException($"Provided {nameof(entityDescription)} is null");
            if (_entityDescriptions.ContainsKey(entityDescription.Type))
            {
                throw new ArgumentException($"Entity type {entityDescription.Type} already registered. To modify existing {nameof(IEntityDescription)} use {nameof(ModifyEntity)} method.");
            }

            Dictionary<Type, IEntityDescription> allEntityDescriptions = _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            allEntityDescriptions.Add(entityDescription.Type, entityDescription);
            return Clone(entityDescriptions: allEntityDescriptions);
        }

        /// <summary>
        /// Add multiple IEntityDescription items.
        /// </summary>
        /// <param name="entityDescriptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual GeneratorSetup RegisterEntity(IEnumerable<IEntityDescription> entityDescriptions)
        {
            entityDescriptions = entityDescriptions ?? throw new ArgumentNullException($"Provided {nameof(entityDescriptions)} is null");

            List<Type> duplicateEntityTypes = _entityDescriptions.Keys
                .Intersect(entityDescriptions.Select(x => x.Type))
                .ToList();
            if (duplicateEntityTypes.Count > 0)
            {
                string duplicateTypesJoined = string.Join(",", duplicateEntityTypes.Select(x => x.Name));
                throw new ArgumentException($"Entity type(s) {duplicateTypesJoined} already registered. To modify existing {nameof(IEntityDescription)} use {nameof(ModifyEntity)} method.");
            }

            string[] duplicateTypes = entityDescriptions.Select(x => x.Type)
                .GroupBy(type => type)
                .Where(group => group.Count() > 1)
                .Select(group => $"Entity of type {group.Key.FullName} included multiple times in {nameof(entityDescriptions)} parameter. Duplicates are not allowed.")
                .ToArray();
            if (duplicateTypes.Length > 0)
            {
                throw new ArgumentException(string.Join(", ", duplicateTypes));
            }

            Dictionary<Type, IEntityDescription> allEntityDescriptions = _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            foreach (IEntityDescription entityDescription in entityDescriptions)
            {
                allEntityDescriptions.Add(entityDescription.Type, entityDescription);
            }
            return Clone(entityDescriptions: allEntityDescriptions);
        }

        /// <summary>
        /// Get existing EntityDescription&lt;TEntity&gt; to modify.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public virtual GeneratorSetup ModifyEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            if (!_entityDescriptions.ContainsKey(entityType))
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered. Use {nameof(RegisterEntity)} method first.");
            }

            IEntityDescription entityDescription = _entityDescriptions[entityType];
            if (!(entityDescription is EntityDescription<TEntity>))
            {
                Type descriptionActualType = entityDescription.GetType();
                Type descriptionBaseType = typeof(EntityDescription<>);
                throw new TypeAccessException($"Entity type [{entityType.FullName}] was registered with IEntityDescription of type [{descriptionActualType.FullName}]. Not able to cast description to type [{descriptionBaseType.FullName}]. Use another {nameof(ModifyEntity)} method instead.");
            }

            entityDescription = entityDescription.Clone();
            entityDescription = entityDescriptionSetup((EntityDescription<TEntity>)entityDescription);

            entityDescription = entityDescription ?? throw new ArgumentNullException($"Provided {nameof(entityDescription)} is null");
            if (entityDescription.Type != entityType)
            {
                throw new ArgumentException($"Entity type {entityDescription.Type.FullName} returned from {nameof(ModifyEntity)}. Not allowed to change type of existing entity {entityType.FullName}.");
            }

            Dictionary<Type, IEntityDescription> allEntityDescriptions = _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            allEntityDescriptions[entityDescription.Type] = entityDescription;
            return Clone(entityDescriptions: allEntityDescriptions);
        }

        /// <summary>
        /// Get existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual GeneratorSetup ModifyEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
        {
            if (!_entityDescriptions.ContainsKey(entityType))
            {
                throw new KeyNotFoundException($"Entity type [{entityType.FullName}] is not registered. Use {nameof(RegisterEntity)} method first.");
            }

            IEntityDescription entityDescription = _entityDescriptions[entityType];
            entityDescription = entityDescription.Clone();
            entityDescription = entityDescriptionSetup(entityDescription);

            Dictionary<Type, IEntityDescription> allEntityDescriptions = _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            allEntityDescriptions[entityDescription.Type] = entityDescription;
            return Clone(entityDescriptions: allEntityDescriptions);
        }

        /// <summary>
        /// Get all existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual GeneratorSetup ModifyEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            IEntityDescription[] newEntityDescriptions = _entityDescriptions.Values
                .Select(x => x.Clone())
                .ToArray();
            newEntityDescriptions = entityDescriptionSetup(newEntityDescriptions);

            Dictionary<Type, IEntityDescription> allEntityDescriptions = _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            return Clone(entityDescriptions: allEntityDescriptions);
        }


        //Configure services
        /// <summary>
        /// Configure existing DefaultSettings or provide new.
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public virtual GeneratorSetup SetDefaultSettings(Func<DefaultSettings, DefaultSettings> defaultsSetup)
        {
            DefaultSettings defaults = _defaults.Clone();
            defaultsSetup.Invoke(defaults);
            return Clone(defaults: defaults);
        }

        /// <summary>
        /// Provide new DefaultSettings.
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public virtual GeneratorSetup SetDefaultSettings(DefaultSettings defaults)
        {
            return Clone(defaults: defaults);
        }

        /// <summary>
        /// Configure existing ISupervisor or provide new.
        /// Producer of generation and flush commands. Determines the order in which entity instances will be generated.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public virtual GeneratorSetup SetSupervisor(Func<ISupervisor, ISupervisor> supervisorSetup)
        {
            ISupervisor supervisor = _supervisor.Clone();
            supervisorSetup.Invoke(supervisor);
            return Clone(supervisor: supervisor);
        }

        /// <summary>
        /// Provide new ISupervisor.
        /// Producer of generation and flush commands. Determines the order in which entity instances will be generated.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public virtual GeneratorSetup SetSupervisor(ISupervisor supervisor)
        {
            return Clone(supervisor: supervisor);
        }

        /// <summary>
        /// Add handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// Subsribe to Change event with single or multiple event handlers. 
        /// </summary>
        public virtual GeneratorSetup SetProgressHandler(Action<ProgressEventTrigger> progressSetup)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressSetup.Invoke(progressEventTrigger);
            return Clone(progress: progressEventTrigger);
        }

        /// <summary>
        /// Add handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// Only single event handler is used. If need multiple event handler, use SetProgressHandler method with Action&lt;ProgressEventTrigger&gt;. 
        /// </summary>
        public virtual GeneratorSetup SetProgressHandler(Action<decimal> progressHandler)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressEventTrigger.Changed += progressHandler;
            return Clone(progress: progressEventTrigger);
        }

        /// <summary>
        /// Add handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// Subsribe to Change event with single or multiple event handlers. 
        /// </summary>
        public virtual GeneratorSetup SetTemporaryStorage(Action<TemporaryStorage> temporaryStorageSetup)
        {
            TemporaryStorage temporaryStorage = _temporaryStorage.Clone();
            temporaryStorageSetup.Invoke(temporaryStorage);
            return Clone(temporaryStorage: temporaryStorage);
        }




        //Generation start
        internal GeneratorServices GetGeneratorServices()
        {
            return new GeneratorServices()
            {
                TemporaryStorage = _temporaryStorage,
                Defaults = _defaults,
                EntityDescriptions = _entityDescriptions,
                Validator = _validator,
                Supervisor = _supervisor
            };
        }

        public virtual void Generate()
        {
            GeneratorServices generatorServices = GetGeneratorServices();

            Validate(generatorServices);

            Setup(generatorServices);

            ExecuteGenerationLoop();
        }

        protected virtual void Validate(GeneratorServices generatorServices)
        {
            _validator = new Validator(generatorServices);
            _validator.ValidateOnStart(_entityDescriptions);
        }

        protected virtual void Setup(GeneratorServices generatorServices)
        {
            _progress.Setup(_supervisor);
            _progress.Clear();
            _commandsHistory.Clear();

            generatorServices.SetupSpreadStrategies();
            generatorServices.SetupEntityContexts(_entityDescriptions);

            _supervisor.Setup(generatorServices);
        }



        //Execution loop
        protected virtual void ExecuteGenerationLoop()
        {
            foreach (ICommand command in _supervisor.IterateCommands())
            {
                _commandsHistory.LogCommand(command);
                command.Execute();
                _progress.UpdateProgressInt(forceUpdate: false);
            }

            _temporaryStorage.WaitAllTasks();
            _progress.UpdateProgressInt(forceUpdate: true);
        }


        //Singular instances generation setup
        //public virtual SingularGeneratorSetup ToSingular()
        //{
        //    return new SingularGeneratorSetup(this);
        //}


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

            _progress.Dispose();
        }
    }
}
