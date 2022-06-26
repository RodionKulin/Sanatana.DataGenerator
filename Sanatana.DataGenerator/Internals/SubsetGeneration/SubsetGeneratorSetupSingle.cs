using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public class SubsetGeneratorSetupSingle : SubsetGeneratorSetup
    {

        //init
        public SubsetGeneratorSetupSingle(GeneratorSetup generatorSetup, Type targetEntity)
            : base(generatorSetup, new List<Type> { targetEntity })
        {  
        }



        //setup methods
        public new virtual SubsetGeneratorSetupSingle SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            return (SubsetGeneratorSetupSingle)base.SetTargetCount(entitiesSelection, 1);
        }

        public new virtual SubsetGeneratorSetupSingle SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle)base.SetTargetCount<TEntity>(1);
        }

        public new virtual SubsetGeneratorSetupSingle SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            return (SubsetGeneratorSetupSingle)base.SetTargetCount(entitiesSelection, targetCount);
        }

        public new virtual SubsetGeneratorSetupSingle SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle)base.SetTargetCount<TEntity>(targetCount);
        }

        public new virtual SubsetGeneratorSetupSingle SetStorageInMemory(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            return (SubsetGeneratorSetupSingle)base.SetStorageInMemory(entitiesSelection, removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupSingle SetStorageInMemory<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle)base.SetStorageInMemory<TEntity>(removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupSingle SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            return (SubsetGeneratorSetupSingle)base.SetStorage(entitiesSelection, removeOtherStorages, storage);
        }

        public new virtual SubsetGeneratorSetupSingle SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle)base.SetStorage<TEntity>(removeOtherStorages, storage);
        }



        //generate methods
        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return all generated entity instances for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use ReturnAll method.
        /// </summary>
        /// <returns></returns>
        public virtual object[] GetTargetMany()
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
            object[] instancesGenerated = storage.Select(targetEntityType);
            if (instancesGenerated.Length == 0)
            {
                throw new ArgumentException($"No instances of type {targetEntityType.FullName} were found in {nameof(InMemoryStorage)}.");
            }
            return instancesGenerated;
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return first generated entity instance for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use ReturnAll method.
        /// </summary>
        /// <returns></returns>
        public virtual object GetTarget()
        {
            object[] instancesGenerated = GetTargetMany();
            return instancesGenerated[0];
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Use this method if entities are only inserted to database and not stored into InMemoryStorage.
        /// </summary>
        public virtual void Generate()
        {
            //validate
            GeneratorServices services = _generatorSetup.GetGeneratorServices();
            Type targetEntityType = _subsetSettings.TargetEntities.First();
            services.ValidateEntitiesConfigured(targetEntityType);

            //generate
            _generatorSetup.Generate();
        }

        /// <summary>
        /// Generate subset of target entities with their required entities.
        /// Will return generated entity instances for target and required entities.
        /// Entities that did not have InMemoryStorage in PersistentStorages will not be returned.
        /// Use SetStorageInMemory method to set InMemoryStorage for multiple entities.
        /// Use this method if entities inserted to InMemoryStorage. Can optionally also insert to database PersistentStorage.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<Type, object[]> GetAll()
        {
            //validate
            GeneratorServices services = _generatorSetup.GetGeneratorServices();
            List<Type> entityTypes = _subsetSettings.TargetAndRequiredEntities;
            services.ValidateEntitiesConfigured(entityTypes);
            services.ValidateNoEntityDuplicates(entityTypes);

            //generate
            _generatorSetup.Generate();

            //get instances from InMemoryStorage
            var entitiesInstances = new Dictionary<Type, object[]>();
            foreach (Type entityType in entityTypes)
            {
                IEntityDescription description = services.EntityDescriptions[entityType];
                List<IPersistentStorage> storages = services.Defaults.GetPersistentStorages(description);
                InMemoryStorage inMemoryStorage = storages.OfType<InMemoryStorage>().FirstOrDefault();
                if (inMemoryStorage != null)
                {
                    //If no InMemoryStorage was added then entity will not be returned.
                    //It's required to supports inserting entity to db without keeping in memory
                    object[] entityInstances = inMemoryStorage.Select(entityType);
                    entitiesInstances.Add(entityType, entityInstances);
                }
            }

            return entitiesInstances;
        }
    }
}
