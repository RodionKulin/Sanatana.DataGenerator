using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sanatana.DataGenerator.Internals.Commands;
using Sanatana.DataGenerator.Internals.Reflection;
using Sanatana.DataGenerator.Internals.Debugging;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using Sanatana.DataGenerator.Internals.Validators;
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.TargetCountProviders;

[assembly: InternalsVisibleTo("Sanatana.DataGeneratorSpecs")]
[assembly: InternalsVisibleTo("Sanatana.DataGenerator.EntityFrameworkCoreSpecs")]
namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Setup class to register all the entities and settings for generation.
    /// </summary>
    public class GeneratorSetup
    {
        //fields
        protected DefaultSettings _defaults;
        protected ValidatorsSetup _validators;
        protected ISupervisor _supervisor;
        protected ReflectionInvoker _reflectionInvoker;
        protected CommandsHistory _commandsHistory;
        protected ProgressEventTrigger _progress;
        protected GeneratorServices _generatorServices;
        /// <summary>
        /// All entity types configured that will be used to generate new instances.
        /// </summary>
        protected Dictionary<Type, IEntityDescription> _entityDescriptions;
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
            _validators = new ValidatorsSetup();
        }

        public GeneratorSetup(Dictionary<Type, IEntityDescription> entityDescriptions, ISupervisor supervisor,
            DefaultSettings defaults, ProgressEventTrigger progress, TemporaryStorage temporaryStorage, ValidatorsSetup validators)
        {
            _reflectionInvoker = new ReflectionInvoker();
            _commandsHistory = new CommandsHistory();
            _entityDescriptions = entityDescriptions;

            _temporaryStorage = new TemporaryStorage();
            _defaults = defaults;
            _supervisor = supervisor;
            _progress = progress;
            _temporaryStorage = temporaryStorage;
            _validators = validators;
        }

        protected virtual GeneratorSetup Clone(Dictionary<Type, IEntityDescription> entityDescriptions = null,
            ISupervisor supervisor = null, DefaultSettings defaults = null, ProgressEventTrigger progress = null,
            TemporaryStorage temporaryStorage = null, ValidatorsSetup validators = null)
        {
            entityDescriptions = entityDescriptions ?? _entityDescriptions.ToDictionary(x => x.Key, x => x.Value);
            supervisor = supervisor ?? _supervisor.Clone();
            defaults = defaults ?? _defaults.Clone();
            progress = progress ?? _progress.Clone();
            temporaryStorage = temporaryStorage ?? _temporaryStorage.Clone();
            validators = validators ?? _validators.Clone();

            return new GeneratorSetup(entityDescriptions, supervisor, defaults, progress, temporaryStorage, validators);
        }


        #region Register entity
        /// <summary>
        /// Add new IEntityDescription.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual GeneratorSetup RegisterEntity<TEntity>()
            where TEntity : class
        {
            var entityDescription = new EntityDescription<TEntity>();
            return RegisterEntity(entityDescription);
        }

        /// <summary>
        /// Add new IEntityDescription and setup additional IEntityDescription properties.
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
            entityDescription = entityDescription ?? throw new ArgumentNullException(nameof(entityDescription));
            if (_entityDescriptions.ContainsKey(entityDescription.Type))
            {
                throw new ArgumentException($"Entity type {entityDescription.Type} already registered. To modify existing {nameof(IEntityDescription)} use {nameof(EditEntity)} method.");
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
            entityDescriptions = entityDescriptions ?? throw new ArgumentNullException(nameof(entityDescriptions));

            List<Type> duplicateEntityTypes = _entityDescriptions.Keys
                .Intersect(entityDescriptions.Select(x => x.Type))
                .ToList();
            if (duplicateEntityTypes.Count > 0)
            {
                string duplicateTypesJoined = string.Join(",", duplicateEntityTypes.Select(x => x.FullName));
                throw new ArgumentException($"Entity type(s) {duplicateTypesJoined} already registered. To modify existing {nameof(IEntityDescription)} use {nameof(EditEntity)} method.");
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
        #endregion


        #region Edit entity
        /// <summary>
        /// Get existing EntityDescription&lt;TEntity&gt; to modify.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public virtual GeneratorSetup EditEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            entityDescriptionSetup = entityDescriptionSetup ?? throw new ArgumentNullException(nameof(entityDescriptionSetup));

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
                throw new TypeAccessException($"Entity type [{entityType.FullName}] was registered with IEntityDescription of type [{descriptionActualType.FullName}]. Not able to cast description to type [{descriptionBaseType.FullName}]. Use another {nameof(EditEntity)} method instead.");
            }

            entityDescription = entityDescription.Clone();
            entityDescription = entityDescriptionSetup((EntityDescription<TEntity>)entityDescription);

            entityDescription = entityDescription ?? throw new ArgumentNullException(nameof(entityDescription));
            if (entityDescription.Type != entityType)
            {
                throw new ArgumentException($"Entity type {entityDescription.Type.FullName} returned from {nameof(EditEntity)}. Not allowed to change type of existing entity {entityType.FullName}.");
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
        public virtual GeneratorSetup EditEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
        {
            entityDescriptionSetup = entityDescriptionSetup ?? throw new ArgumentNullException(nameof(entityDescriptionSetup));

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
        public virtual GeneratorSetup EditEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            entityDescriptionSetup = entityDescriptionSetup ?? throw new ArgumentNullException(nameof(entityDescriptionSetup));

            IEntityDescription[] newEntityDescriptions = _entityDescriptions.Values
                .Select(x => x.Clone())
                .ToArray();
            newEntityDescriptions = entityDescriptionSetup(newEntityDescriptions)
                ?? throw new ArgumentNullException(nameof(newEntityDescriptions));

            Dictionary<Type, IEntityDescription> allEntityDescriptions = newEntityDescriptions.ToDictionary(x => x.Type, x => x);
            return Clone(entityDescriptions: allEntityDescriptions);
        }

        /// <summary>
        /// Set TargetCount to 1 for all entities.
        /// </summary>
        /// <returns></returns>
        public virtual GeneratorSetup SetTargetCountSingle()
        {
            return SetTargetCount(1);
        }

        /// <summary>
        /// Set TargetCount for all entities.
        /// </summary>
        /// <param name="targetCount"></param>
        /// <returns></returns>
        public virtual GeneratorSetup SetTargetCount(long targetCount)
        {
            return EditEntity(descriptions =>
            {
                foreach (IEntityDescription entity in descriptions)
                {
                    entity.TargetCountProvider = new StrictTargetCountProvider(targetCount);
                }
                return descriptions;
            });
        }
        #endregion


        #region Select entities
        /// <summary>
        /// Check if entity type is already configured.
        /// </summary>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public virtual bool ContainsEntity(Type entityType)
        {
            return _entityDescriptions.ContainsKey(entityType);
        }

        /// <summary>
        /// Get entity types that are already registered in GeneratorSetup.
        /// </summary>
        /// <returns></returns>
        public virtual Type[] GetRegistegedEntityTypes()
        {
            return _entityDescriptions.Keys.ToArray();
        }

        /// <summary>
        /// Get entity types that are already registered in GeneratorSetup
        /// and do not include IDelegateParameterizedGenerator as Generator 
        /// and do not include IDelegateParameterizedModifier among Modifiers.
        /// Default settings are not considered.
        /// </summary>
        /// <returns></returns>
        public virtual Type[] GetRegistegedNotParameterizedEntityTypes()
        {
            return _entityDescriptions
                .Where(x => x.Value.Generator.IsNotParameterizedGenerator()
                    && x.Value.Modifiers.IsNotParameterizedModifiers())
                .Select(x => x.Key)
                .ToArray();
        }
        #endregion


        #region Configure services
        /// <summary>
        /// Configure validators by removing default ones or adding new custom validators.
        /// Validators should implement one of interfaces:
        /// IBeforeSetupValidator - runs before getting TargetCount and list of parent and child entities, based on Required settings;
        /// IAfterSetupValidator - runs after getting TargetCount and list of parent and child entities, based on Required settings;
        /// IGenerateValidator - runs after entity instances were generated;
        /// IModifyValidator - runs after entity instances were modified.
        /// </summary>
        public virtual GeneratorSetup SetValidators(Func<ValidatorsSetup, ValidatorsSetup> validatorsSetup)
        {
            ValidatorsSetup validators = _validators.Clone();
            validatorsSetup.Invoke(validators);
            return Clone(validators: validators);
        }

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
        /// Configure existing ISupervisor or provide new.
        /// Supervisor produces generation and flush commands. Determines the order in which entity instances are be generated.
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
        /// ISupervisor produces generation and flush commands, determines the order in which entity instances will be generated and inserted to persistent storages.
        /// Default is CompleteSupervisor that will produce commands to generate complete set of entities configured.
        /// </summary>
        public virtual GeneratorSetup SetSupervisor(ISupervisor supervisor)
        {
            return Clone(supervisor: supervisor);
        }

        /// <summary>
        /// Add handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// </summary>
        public virtual GeneratorSetup SetProgressHandler(Action<ProgressEventTrigger> progressSetup)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressSetup.Invoke(progressEventTrigger);
            return Clone(progress: progressEventTrigger);
        }

        /// <summary>
        /// Add handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// </summary>
        public virtual GeneratorSetup SetProgressHandler(Action<decimal> progressHandler)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressEventTrigger.Subscribe(progressHandler);
            return Clone(progress: progressEventTrigger);
        }

        /// <summary>
        /// Remove handler for progress change event. Progress is reported as percent in range from 0 to 100.
        /// </summary>
        public virtual GeneratorSetup RemoveProgressHandler(Action<decimal> progressHandler)
        {
            ProgressEventTrigger progressEventTrigger = _progress.Clone();
            progressEventTrigger.Unsubscribe(progressHandler);
            return Clone(progress: progressEventTrigger);
        }

        /// <summary>
        /// Set maximum running parallel tasks to insert entities into persistent storage.
        /// Default value equals to number of processor cores count.
        /// </summary>
        public virtual GeneratorSetup SetMaxParallelInserts(int maxTasksRunning)
        {
            TemporaryStorage temporaryStorage = _temporaryStorage.Clone();
            temporaryStorage.MaxTasksRunning = maxTasksRunning;
            return Clone(temporaryStorage: temporaryStorage);
        }

        #endregion


        #region Generation
        public virtual void Generate()
        {
            _generatorServices = null;
            GeneratorServices generatorServices = GetGeneratorServices();

            generatorServices.Validators.ValidateBeforeSetup(generatorServices);

            Setup(generatorServices);

            generatorServices.Validators.ValidateAfterSetup(generatorServices);

            ExecuteGenerationLoop();
        }

        internal GeneratorServices GetGeneratorServices()
        {
            _generatorServices = _generatorServices ?? new GeneratorServices()
            {
                TemporaryStorage = _temporaryStorage,
                Defaults = _defaults,
                EntityDescriptions = _entityDescriptions,
                Supervisor = _supervisor,
                Validators = _validators,
            };
            return _generatorServices;
        }

        protected virtual void Setup(GeneratorServices generatorServices)
        {
            //reset inner variables in services
            _progress.Setup(_supervisor);
            _commandsHistory.Setup();
            _temporaryStorage.Setup();

            //Should be called first to setup Parent and Child entities for each entity.
            generatorServices.SetupEntityContexts();
            //Should be called before SetupTargetCount to support CombinatoricsSpreadStrategy that returns ITargetCountProvider.GetTargetCount based on parent entities TargetCount.
            generatorServices.SetupSpreadStrategies();
            generatorServices.SetupTargetCount();
            generatorServices.SetupPersistentStorages();

            //reset inner variables in generators and modifiers
            _entityDescriptions.Values.Where(x => x.Generator != null)
                .ToList()
                .ForEach(x => x.Generator.Setup(generatorServices));
            _entityDescriptions.Values.Where(x => x.Modifiers != null)
                .SelectMany(x => x.Modifiers)
                .ToList()
                .ForEach(x => x.Setup(generatorServices));
            if(_defaults.Generator != null)
            {
                _defaults.Generator.Setup(generatorServices);
            }
            if (_defaults.Modifiers != null)
            {
                _defaults.Modifiers.ForEach(x => x.Setup(generatorServices));
            }

            _supervisor.Setup(generatorServices);
        }

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
        #endregion


        #region Subset generation
        /// <summary>
        /// Convert to SubsetGeneratorSetup class, that has methods to configure and generate subset of all entities configured.
        /// Will generate only entity types provided as parameters and their required entities.
        /// </summary>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany ToSubsetSetup(List<Type> targetEntities)
        {
            return new SubsetGeneratorSetupMany(this, targetEntities);
        }

        /// <summary>
        /// Convert to SubsetGeneratorSetup class, that has methods to configure and generate subset of all entities configured.
        /// Will generate only entity types provided as parameters and their required entities.
        /// </summary>
        /// <param name="targetEntities"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual SubsetGeneratorSetupMany ToSubsetSetup(params Type[] targetEntities)
        {
            targetEntities = targetEntities ?? throw new ArgumentNullException(nameof(targetEntities));
            return new SubsetGeneratorSetupMany(this, targetEntities.ToList());
        }

        /// <summary>
        /// Convert to SubsetGeneratorSetup class, that has methods to configure and generate subset of all entities configured.
        /// Will generate only entity type provided as parameter and it's required entities.
        /// </summary>
        /// <param name="targetEntity"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle ToSubsetSetup(Type targetEntity)
        {
            return new SubsetGeneratorSetupSingle(this, targetEntity);
        }

        /// <summary>
        /// Convert to SubsetGeneratorSetup class, that has methods to configure and generate subset of all entities configured.
        /// Will generate only entity type provided as parameter and it's required entities.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<TEntity> ToSubsetSetup<TEntity>()
        {
            return new SubsetGeneratorSetupSingle<TEntity>(this);
        }
        #endregion
    }
}
