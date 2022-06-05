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
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Internals.Reflection;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Debugging;

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
        protected decimal _lastPercents;
        protected Dictionary<Type, EntityContext> _entityContexts;
        protected ReflectionInvoker _reflectionInvoker;


        //event
        /// <summary>
        /// Progress change event that will report overall completion percent in range from 0 to 100.
        /// </summary>
        public event Action<GeneratorSetup, decimal> ProgressChanged;


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
            TemporaryStorage = new TemporaryStorage();
            CommandsHistory = new CommandsHistory();

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
            Validator.CheckEntitySettingsForGenerators(EntityDescriptions);
        }

        protected virtual void Setup()
        {
            _lastPercents = -1;
            CommandsHistory.Clear();
            TemporaryStorage.GeneratorSetup = this;
            _entityContexts = SetupEntityContexts(EntityDescriptions);
            SetupSpreadStrategies();
            Supervisor.Setup(this, _entityContexts);
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

                ITotalCountProvider totalCountProvider = Defaults.GetTotalCountProvider(description);
                long targetTotalCount = totalCountProvider.GetTargetCount();

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
            foreach (ICommand command in Supervisor.GetNextCommand())
            {
                CommandsHistory.TrackCommand(command);
                command.Execute();
                UpdateProgress(forceUpdate: false);
            }

            TemporaryStorage.WaitAllTasks();
        }
        
        protected virtual void UpdateProgress(bool forceUpdate)
        {
            long actionCalls = IdIterator.GetNextId<IProgressState>();

            //trigger event only every N generated instances
            long invokeOnEveryNCall = 1000;
            bool invoke = actionCalls % invokeOnEveryNCall == 0;
            if (!invoke && forceUpdate == false)
            {
                return;
            }

            //invoke handler
            decimal percents = Supervisor.ProgressState.GetCompletionPercents();
            if(_lastPercents == percents)
            {
                return;
            }
            _lastPercents = percents;

            Action<GeneratorSetup, decimal> progressChanged = ProgressChanged;
            if (progressChanged != null)
            {
                progressChanged(this, percents);
            }
        }


        //IDisposalbe
        /// <summary>
        /// Will call Dispose() on ReaderWriterLockSlim for entities and IPersistentStorage storages
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Dispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                List<IPersistentStorage> storages = Defaults.GetPersistentStorages(entityContext.Description);
                storages.ForEach(storage => storage.Dispose());

                entityContext.Dispose();
            }
        }
    }
}
