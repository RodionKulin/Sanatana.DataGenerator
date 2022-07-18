using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Supervisors.Subset;
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


        #region Configure services
        /// <summary>
        /// Set TargetCount to 1 for all entities matching entitiesSelection.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            base.SetTargetCount(entitiesSelection, 1);
            return this;
        }

        /// <summary>
        /// Set TargetCount to 1 for entity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            base.SetTargetCount<TEntity>(1);
            return this;
        }

        /// <summary>
        /// Set TargetCount for all entities matching entitiesSelection.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="targetCount"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            base.SetTargetCount(entitiesSelection, targetCount);
            return this;
        }

        /// <summary>
        /// Set TargetCount for entity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="targetCount"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            base.SetTargetCount<TEntity>(targetCount);
            return this;
        }

        /// <summary>
        /// Set InMemoryStorage as persistent storage for all entities matching entitiesSelection.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="removeOtherStorages"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetInMemoryStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            var memoryStorage = new InMemoryStorage();
            base.SetStorage(entitiesSelection, removeOtherStorages, memoryStorage);
            return this;
        }

        /// <summary>
        /// Set InMemoryStorage as persistent storage for entity.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="removeOtherStorages"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetInMemoryStorage<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            var memoryStorage = new InMemoryStorage();
            base.SetStorage<TEntity>(removeOtherStorages, memoryStorage);
            return this;
        }

        /// <summary>
        /// Set persistent storage for all entities matching entitiesSelection.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="removeOtherStorages"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            base.SetStorage(entitiesSelection, removeOtherStorages, storage);
            return this;
        }

        /// <summary>
        /// Set persistent storage for entity.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="removeOtherStorages"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupMany SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            base.SetStorage<TEntity>(removeOtherStorages, storage);
            return this;
        }
        #endregion


        #region Modify entity
        /// <summary>
        /// Get existing EntityDescription&lt;TEntity&gt; to modify.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public virtual SubsetGeneratorSetupMany ModifyEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.ModifyEntity(entityDescriptionSetup);
            return this;
        }

        /// <summary>
        /// Get existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual SubsetGeneratorSetupMany ModifyEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.ModifyEntity(entityType, entityDescriptionSetup);
            return this;
        }

        /// <summary>
        /// Get all existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual SubsetGeneratorSetupMany ModifyEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.ModifyEntity(entityDescriptionSetup);
            return this;
        }
        #endregion


        #region Generate
        /// <summary>
        /// Generate subset of target entities with their required entities.
        /// Will return generated entity instances for target and required entities.
        /// Entities that did not have InMemoryStorage in PersistentStorages will not be returned.
        /// Use SetInMemoryStorage method to set InMemoryStorage for multiple entities.
        /// Use GetAll method if entities inserted to InMemoryStorage. Can optionally also insert to database persistent storage.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<Type, object[]> GetAll()
        {
            return base.GetAllImp();
        }
        #endregion
    }
}
