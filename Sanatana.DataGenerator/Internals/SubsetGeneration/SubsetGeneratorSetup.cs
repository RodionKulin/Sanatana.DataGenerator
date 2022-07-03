using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Supervisors.Subset;
using Sanatana.DataGenerator.TotalCountProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public class SubsetGeneratorSetup
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
        public virtual SubsetGeneratorSetup SetTargetCountSingle(EntitiesSelection entitiesSelection)
        {
            return SetTargetCount(entitiesSelection, 1);
        }

        public virtual SubsetGeneratorSetup SetTargetCountSingle<TEntity>()
            where TEntity : class
        {
            return SetTargetCount<TEntity>(1);
        }

        public virtual SubsetGeneratorSetup SetTargetCount(EntitiesSelection entitiesSelection, long targetCount)
        {
            _generatorSetup = _generatorSetup.ModifyEntity((IEntityDescription[] all) =>
            {
                all.SelectEntities(entitiesSelection, _subsetSettings)
                    .ToList()
                    .ForEach(description => description.TotalCountProvider = new StrictTotalCountProvider(targetCount));
                return all;
            });
            return this;
        }

        public virtual SubsetGeneratorSetup SetTargetCount<TEntity>(long targetCount) 
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.ModifyEntity((EntityDescription<TEntity> description) =>
            {
                return description.SetTargetCount(targetCount);
            });
            return this;
        }

        public virtual SubsetGeneratorSetup SetStorageInMemory(EntitiesSelection entitiesSelection, bool removeOtherStorages)
        {
            var memoryStorage = new InMemoryStorage();
            return SetStorage(entitiesSelection, removeOtherStorages, memoryStorage);
        }

        public virtual SubsetGeneratorSetup SetStorageInMemory<TEntity>(bool removeOtherStorages)
            where TEntity : class
        {
            var memoryStorage = new InMemoryStorage();
            return SetStorage<TEntity>(removeOtherStorages, memoryStorage);
        }

        public virtual SubsetGeneratorSetup SetStorage(EntitiesSelection entitiesSelection, bool removeOtherStorages, IPersistentStorage storage)
        {
            _generatorSetup = _generatorSetup.ModifyEntity((IEntityDescription[] all) =>
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
            return this;
        }

        public virtual SubsetGeneratorSetup SetStorage<TEntity>(bool removeOtherStorages, IPersistentStorage storage)
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.ModifyEntity((EntityDescription<TEntity> description) =>
            {if (removeOtherStorages)
                {
                    description.PersistentStorages.Clear();
                }
                if (!description.PersistentStorages.Where(x => x.GetType() == storage.GetType()).Any())
                {
                    description.PersistentStorages.Add(storage);
                }
                return description;
            });
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
        public virtual SubsetGeneratorSetup ModifyEntity<TEntity>(Func<EntityDescription<TEntity>, EntityDescription<TEntity>> entityDescriptionSetup)
            where TEntity : class
        {
            _generatorSetup = _generatorSetup.ModifyEntity<TEntity>(entityDescriptionSetup);
            return this;
        }

        /// <summary>
        /// Get existing IEntityDescription to modify.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="entityDescriptionSetup"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public virtual SubsetGeneratorSetup ModifyEntity(Type entityType, Func<IEntityDescription, IEntityDescription> entityDescriptionSetup)
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
        public virtual SubsetGeneratorSetup ModifyEntity(Func<IEntityDescription[], IEntityDescription[]> entityDescriptionSetup)
        {
            _generatorSetup = _generatorSetup.ModifyEntity(entityDescriptionSetup);
            return this;
        }
        #endregion
    }
}
