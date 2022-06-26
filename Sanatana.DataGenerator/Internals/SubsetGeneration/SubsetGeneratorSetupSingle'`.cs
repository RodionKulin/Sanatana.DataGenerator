using Sanatana.DataGenerator.Storages;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public class SubsetGeneratorSetupSingle<TTEntity> : SubsetGeneratorSetupSingle
    {
        //init
        public SubsetGeneratorSetupSingle(GeneratorSetup generatorSetup)
            : base(generatorSetup, typeof(TTEntity))
        {
        }




        //setup methods
        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetTargetCount(entitiesSelection, 1);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetTargetCount<TEntity>(1);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetTargetCount(entitiesSelection, targetCount);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetTargetCount<TEntity>(targetCount);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetStorageInMemory(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetStorageInMemory(entitiesSelection, removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetStorageInMemory<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetStorageInMemory<TEntity>(removeOtherStorages);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetStorage(entitiesSelection, removeOtherStorages, storage);
        }

        public new virtual SubsetGeneratorSetupSingle<TTEntity> SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            return (SubsetGeneratorSetupSingle<TTEntity>)base.SetStorage<TEntity>(removeOtherStorages, storage);
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
        public new virtual TTEntity[] GetTargetMany()
        {
            object[] instances = base.GetTargetMany();
            return instances.Cast<TTEntity>().ToArray();
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return first generated entity instance for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use ReturnAll method.
        /// </summary>
        /// <returns></returns>
        public new virtual TTEntity GetTarget()
        {
            object instance = base.GetTarget();
            return (TTEntity)instance;
        }
    }
}
