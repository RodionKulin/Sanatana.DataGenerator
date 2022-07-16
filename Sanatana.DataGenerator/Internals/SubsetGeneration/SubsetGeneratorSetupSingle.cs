using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Supervisors.Subset;
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


        #region Configure services
        public virtual SubsetGeneratorSetupSingle SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            base.SetTargetCount(entitiesSelection, 1);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            base.SetTargetCount<TEntity>(1);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            base.SetTargetCount(entitiesSelection, targetCount);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetTargetCount<TEntity>(long targetCount)
            where TEntity : class
        {
            base.SetTargetCount<TEntity>(targetCount);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetInMemoryStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            var memoryStorage = new InMemoryStorage();
            base.SetStorage(entitiesSelection, removeOtherStorages, memoryStorage);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetInMemoryStorage<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            var memoryStorage = new InMemoryStorage();
            base.SetStorage<TEntity>(removeOtherStorages, memoryStorage);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            base.SetStorage(entitiesSelection, removeOtherStorages, storage);
            return this;
        }

        public virtual SubsetGeneratorSetupSingle SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
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
        public virtual SubsetGeneratorSetupSingle ModifyEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
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
        public virtual SubsetGeneratorSetupSingle ModifyEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
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
        public virtual SubsetGeneratorSetupSingle ModifyEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.ModifyEntity(entityDescriptionSetup);
            return this;
        }
        #endregion


        #region Generate
        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return all generated entity instances for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use ReturnAll method.
        /// </summary>
        /// <returns></returns>
        public virtual object[] GetMultipleTargets()
        {
            return base.GetMultipleTargetsImp();
        }

        /// <summary>
        /// Generate target entity with it's required entities.
        /// Will return first generated entity instance for target entity.
        /// Use this method if target entity is inserted to InMemoryStorage, otherwise use Generate method that only inserts to database.
        /// Required entities will not be returned. If required have database PersistentStorage configured, they will be inserted to database.
        /// To also return required entities use ReturnAll method.
        /// </summary>
        /// <returns></returns>
        public virtual object GetSingleTarget()
        {
            object[] instances = base.GetMultipleTargetsImp();
            return instances[0];
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
        /// Use this method if entities inserted to InMemoryStorage. Can optionally also insert to database PersistentStorage.
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<Type, object[]> GetAll()
        {
            return base.GetAllImp();
        }
        #endregion
    }
}
