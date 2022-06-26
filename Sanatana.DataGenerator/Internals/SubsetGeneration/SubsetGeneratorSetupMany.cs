using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public class SubsetGeneratorSetupMany : SubsetGeneratorSetup
    {

        //init
        public SubsetGeneratorSetupMany(GeneratorSetup generatorSetup, List<Type> targetEntities)
            : base(generatorSetup, targetEntities)
        {
        }



        //setup methods
        public new virtual SubsetGeneratorSetupMany SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            return (SubsetGeneratorSetupMany)base.SetTargetCount(entitiesSelection, 1);
        }

        public new virtual SubsetGeneratorSetupMany SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            return (SubsetGeneratorSetupMany)base.SetTargetCount<TEntity>(1);
        }

        public new virtual SubsetGeneratorSetupMany SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            return (SubsetGeneratorSetupMany)base.SetTargetCount(entitiesSelection, targetCount);
        }

        public new virtual SubsetGeneratorSetupMany SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            return (SubsetGeneratorSetupMany)base.SetTargetCount<TEntity>(targetCount);
        }

        public new virtual SubsetGeneratorSetupMany SetStorageInMemory(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            return (SubsetGeneratorSetupMany)base.SetStorageInMemory(entitiesSelection, removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupMany SetStorageInMemory<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            return (SubsetGeneratorSetupMany)base.SetStorageInMemory<TEntity>(removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupMany SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            return (SubsetGeneratorSetupMany)base.SetStorage(entitiesSelection, removeOtherStorages, storage);
        }

        public new virtual SubsetGeneratorSetupMany SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            return (SubsetGeneratorSetupMany)base.SetStorage<TEntity>(removeOtherStorages, storage);
        }



        //generate methods
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
                List<IPersistentStorage> storages = services.Defaults.GetPersistentStorages(services.EntityDescriptions[entityType]);
                InMemoryStorage inMemoryStorage = storages.OfType<InMemoryStorage>().FirstOrDefault();
                if(inMemoryStorage != null)
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
