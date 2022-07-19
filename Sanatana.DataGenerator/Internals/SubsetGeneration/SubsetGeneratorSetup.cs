using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Supervisors.Subset;
using Sanatana.DataGenerator.TargetCountProviders;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public abstract class SubsetGeneratorSetup
    {
        //fields
        protected GeneratorSetup _generatorSetup;
        protected SubsetSettings _subsetSettings;
        protected ISupervisor _previousSupervisor;


        //init
        public SubsetGeneratorSetup(GeneratorSetup generatorSetup, List<Type> targetEntities)
        {
            _generatorSetup = generatorSetup;

            //change supervisor
            _previousSupervisor = _generatorSetup.GetGeneratorServices().Supervisor;
            _generatorSetup.SetSupervisor(new SubsetSupervisor(targetEntities));

            //prepare subset settings
            _subsetSettings = new SubsetSettings(targetEntities);
            _subsetSettings.Setup(generatorSetup.GetGeneratorServices());
        }


        /// <summary>
        /// Convert back to GeneratorSetup to use it's configuration methods.
        /// And restore previous Supervisor with SetSupervisor method.
        /// </summary>
        /// <returns></returns>
        public virtual GeneratorSetup ToFullGeneratorSetup()
        {
            return _generatorSetup.SetSupervisor(_previousSupervisor);
        }


        #region Configure services
        protected virtual void SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            _generatorSetup = _generatorSetup.EditEntity((IEntityDescription[] all) =>
            {
                all.SelectEntities(entitiesSelection, _subsetSettings)
                    .ToList()
                    .ForEach(description => description.TargetCountProvider = new StrictTargetCountProvider(targetCount));
                return all;
            });
        }

        protected virtual void SetTargetCount<TEntity>(long targetCount) 
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.EditEntity((EntityDescription<TEntity> description) =>
            {
                return description.SetTargetCount(targetCount);
            });
        }

        protected virtual void SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            _generatorSetup = _generatorSetup.EditEntity((IEntityDescription[] all) =>
            {
                IEnumerable<IEntityDescription> selectedEntities = all.SelectEntities(entitiesSelection, _subsetSettings);
                foreach (IEntityDescription description in selectedEntities)
                {
                    if (removeOtherStorages)
                    {
                        description.PersistentStorages.Clear();
                    }
                    if (!description.PersistentStorages.Where(x => x.GetType() == storage.GetType()).Any())
                    {
                        description.PersistentStorages.Add(storage);
                    }
                }
                return all;
            });
        }

        protected virtual void SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.EditEntity((EntityDescription<TEntity> description) =>
            {
                if (removeOtherStorages)
                {
                    description.PersistentStorages.Clear();
                }
                if (!description.PersistentStorages.Where(x => x.GetType() == storage.GetType()).Any())
                {
                    description.PersistentStorages.Add(storage);
                }
                return description;
            });
        }
        #endregion


        #region Generate methods
        protected virtual IList GetMultipleTargetsImp()
        {
            //validate
            GeneratorServices services = _generatorSetup.GetGeneratorServices();
            Type targetEntityType = _subsetSettings.TargetEntities.First();
            services.ValidateEntitiesConfigured(targetEntityType);

            //generate
            _generatorSetup.Generate();

            //get InMemoryStorage
            IEntityDescription description = services.EntityDescriptions[targetEntityType];
            InMemoryStorage[] inMemoryStorages = services.Defaults.GetPersistentStorages(description)
                .OfType<InMemoryStorage>().ToArray();
            if (inMemoryStorages.Length == 0)
            {
                throw new ArgumentException($"Method can only be called when entity has {nameof(InMemoryStorage)} in {nameof(IEntityDescription)}.{nameof(IEntityDescription.PersistentStorages)}.");
            }
            InMemoryStorage storage = inMemoryStorages[0];

            //get instance generated
            IList instancesGenerated = storage.Select(targetEntityType);
            if (instancesGenerated.Count == 0)
            {
                throw new ArgumentException($"No instances of type {targetEntityType.FullName} were found in {nameof(InMemoryStorage)}.");
            }
            return instancesGenerated;
        }

        protected virtual void GenerateImp()
        {
            //validate
            GeneratorServices services = _generatorSetup.GetGeneratorServices();
            Type targetEntityType = _subsetSettings.TargetEntities.First();
            services.ValidateEntitiesConfigured(targetEntityType);

            //generate
            _generatorSetup.Generate();
        }

        protected virtual Dictionary<Type, IList> GetAllImp()
        {
            //validate
            GeneratorServices services = _generatorSetup.GetGeneratorServices();
            List<Type> entityTypes = _subsetSettings.TargetAndRequiredEntities;
            services.ValidateEntitiesConfigured(entityTypes);
            services.ValidateNoEntityDuplicates(entityTypes);

            //generate
            _generatorSetup.Generate();

            //get instances from InMemoryStorage
            var entitiesInstances = new Dictionary<Type, IList>();
            foreach (Type entityType in entityTypes)
            {
                IEntityDescription description = services.EntityDescriptions[entityType];
                List<IPersistentStorage> storages = services.Defaults.GetPersistentStorages(description);
                InMemoryStorage inMemoryStorage = storages.OfType<InMemoryStorage>().FirstOrDefault();
                if (inMemoryStorage != null)
                {
                    //If no InMemoryStorage was added then entity will not be returned.
                    //It's required to supports inserting entity to db without keeping in memory
                    IList entityInstances = inMemoryStorage.Select(entityType);
                    entitiesInstances.Add(entityType, entityInstances);
                }
            }

            return entitiesInstances;
        }
        #endregion
    }
}
