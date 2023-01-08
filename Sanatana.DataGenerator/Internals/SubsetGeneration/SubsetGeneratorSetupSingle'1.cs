using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Supervisors.Subset;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public class SubsetGeneratorSetupSingle<T> : SubsetGeneratorSetup
    {
        //init
        public SubsetGeneratorSetupSingle(GeneratorSetup generatorSetup)
            : base(generatorSetup, new List<Type> { typeof(T) })
        {
        }


        #region Configure services
        /// <summary>
        /// Set TargetCount to 1 for all entities matching entitiesSelection.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            base.SetTargetCountImp(entitiesSelection, 1);
            return this;
        }
        
        /// <summary>
        /// Set TargetCount to 1 for entity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            base.SetTargetCountImp<TEntity>(1);
            return this;
        }

        /// <summary>
        /// Set TargetCount for all entities matching entitiesSelection.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="targetCount"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            base.SetTargetCountImp(entitiesSelection, targetCount);
            return this;
        }

        /// <summary>
        /// Set TargetCount for entity.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="targetCount"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            base.SetTargetCountImp<TEntity>(targetCount);
            return this;
        }

        /// <summary>
        /// Add InMemoryStorage as persistent storage for all entities matching entitiesSelection.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="removeOtherStorages"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> AddInMemoryStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages = false)
        {
            var memoryStorage = new InMemoryStorage();
            base.AddStorageImp(entitiesSelection, memoryStorage, removeOtherStorages);
            return this;
        }

        /// <summary>
        /// Add InMemoryStorage as persistent storage for entity.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="removeOtherStorages"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> AddInMemoryStorage<TEntity>(bool removeOtherStorages = false)
            where TEntity : class
        {
            var memoryStorage = new InMemoryStorage();
            base.AddStorageImp<TEntity>(memoryStorage, removeOtherStorages);
            return this;
        }

        /// <summary>
        /// Add persistent storage for all entities matching entitiesSelection.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <param name="entitiesSelection"></param>
        /// <param name="removeOtherStorages"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> AddStorage(EntitiesSelection entitiesSelection, IPersistentStorage storage, bool removeOtherStorages = false)
        {
            base.AddStorageImp(entitiesSelection, storage, removeOtherStorages);
            return this;
        }

        /// <summary>
        /// Add persistent storage for entity.
        /// Optionally remove previously added persistent storages.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="removeOtherStorages"></param>
        /// <param name="storage"></param>
        /// <returns></returns>
        public virtual SubsetGeneratorSetupSingle<T> AddStorage<TEntity>(IPersistentStorage storage, bool removeOtherStorages = false)
            where TEntity : class
        {
            base.AddStorageImp<TEntity>(storage, removeOtherStorages);
            return this;
        }
        #endregion


        #region Edit entity
        /// <summary>
        /// Get existing EntityDescription&lt;TEntity&gt; to modify.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        /// <exception cref="TypeAccessException"></exception>
        public virtual SubsetGeneratorSetupSingle<T> EditEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.EditEntity(entityDescriptionSetup);
            return this;
        }

        /// <summary>
        /// Get existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual SubsetGeneratorSetupSingle<T> EditEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.EditEntity(entityType, entityDescriptionSetup);
            return this;
        }

        /// <summary>
        /// Get all existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual SubsetGeneratorSetupSingle<T> EditEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.EditEntity(entityDescriptionSetup);
            return this;
        }
        #endregion


        #region Generate
        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return all generated entity instances for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use GetAll method.
        /// </summary>
        /// <returns></returns>
        public virtual T[] GetMultipleTargets()
        {
            IList instances = base.GetMultipleTargetsImp();
            return instances.Cast<T>().ToArray();
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return first generated entity instance for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use GetAll method.
        /// </summary>
        /// <returns></returns>
        public virtual T GetSingleTarget()
        {
            IList instances = base.GetMultipleTargetsImp();
            return instances.Cast<T>().FirstOrDefault();
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Use this method if entities are only inserted to database and not stored into InMemoryStorage.
        /// </summary>
        public virtual void Generate()
        {
            base.GenerateImp();
        }

        /// <summary>
        /// Generate subset of target entities with their required entities.
        /// Will return generated entity instances for target and required entities.
        /// Entities that did not have InMemoryStorage in PersistentStorages will not be returned.
        /// Use SetInMemoryStorage method to set InMemoryStorage for multiple entities.
        /// Use GetAll method if entities inserted to InMemoryStorage. Can optionally also insert to database PersistentStorage.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<Type, IList> GetAll()
        {
            return base.GetAllImp();
        }
        #endregion
    }
}
