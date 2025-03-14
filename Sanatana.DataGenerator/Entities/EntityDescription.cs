using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.TargetCountProviders;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Reflection;

namespace Sanatana.DataGenerator.Entities
{
    /// <summary>
    /// Entity configuration for generation process
    /// </summary>
    public class EntityDescription : IEntityDescription
    {
        //properties
        /// <summary>
        /// Type of entity to generate.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Entities that are needed for generator as foreign keys or other type of dependency.
        /// Required entity intstances are generated first, then will be passed as parameters to generator.
        /// </summary>
        public List<RequiredEntity> Required { get; set; }
        /// <summary>
        /// Entity instances generator. Can return instances one by one or in small batches. 
        /// Usually better to return single instance not to store extra instances in memory.
        /// If not specified will use Generator from DefaultSettings.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// List of methods to make adjustments to entity instance after generation.
        /// If not specified will use Modifiers from DefaultSettings.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// If true, will execute default Modifiers before executing Entity specific modifiers.
        /// If false and if Entity specific modifiers provided, than will execute only Entity specific modifiers.
        /// Default is false.
        /// </summary>
        public bool KeepDefaultModifiers { get; set; }
        /// <summary>
        /// Database storages for generated entity instances.
        /// If not specified will use PersistentStorages from DefaultSettings.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Provider of total number of entity instances that need to be generated.
        /// If not specified will use TargetCountProvider from DefaultSettings.
        /// </summary>
        public ITargetCountProvider TargetCountProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entity instances to persistent storage.
        /// If not specified will use FlushStrategy from DefaultSettings.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Provider of number of entity instances that can be inserted with next request to persistent storage.
        /// If not specified will use RequestCapacityProvider from DefaultSettings.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Checker of entity instances to be inserted into database. 
        /// Excludes unwanted instances, like the ones that already exist in database for EnsureExistGenerator.
        /// If not specified is not used.
        /// </summary>
        public IStorageInsertGuard StorageInsertGuard { get; set; }
        /// <summary>
        /// Get database generated columns after inserting entities instances to persistent storage. For example Id.
        /// Only after receiving such columns pass entity instances as required to generator.
        /// Also makes insert requests to persistent storage sync for this entity. Multiple parallel inserts wont be possible.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }
        /// <summary>
        /// Selector from persistent storage, that will provide existing instances.
        /// Only required if EnsureExistGenerator or EnsureExistRangeGenerator is used.
        /// If not specified will use PersistentStorageSelector from DefaultSettings.
        /// </summary>
        public IPersistentStorageSelector PersistentStorageSelector { get; set; }



        //init
        public EntityDescription()
        {
            Required = new List<RequiredEntity>();
            Modifiers = new List<IModifier>();
            PersistentStorages = new List<IPersistentStorage>();
        }

        public virtual IEntityDescription Clone()
        {
            return new EntityDescription
            {
                Type = Type,
                Required = new List<RequiredEntity>(Required),
                Generator = Generator,
                Modifiers = new List<IModifier>(Modifiers),
                KeepDefaultModifiers = KeepDefaultModifiers,
                PersistentStorages = new List<IPersistentStorage>(PersistentStorages),
                TargetCountProvider = TargetCountProvider,
                FlushStrategy = FlushStrategy,
                RequestCapacityProvider = RequestCapacityProvider,
                StorageInsertGuard = StorageInsertGuard,
                InsertToPersistentStorageBeforeUse = InsertToPersistentStorageBeforeUse,
                PersistentStorageSelector = PersistentStorageSelector
            };
        }

        /// <summary>
        /// Convert to EntityDescription&lt;TEntity&gt;
        /// </summary>
        /// <returns></returns>
        public virtual IEntityDescription ToGenericEntityDescription()
        {
            IEntityDescription entityDescription = new ReflectionInvoker().CreateGenericEntityDescription(Type);

            //Type is set as generic parameter
            entityDescription.Required = new List<RequiredEntity>(Required);
            entityDescription.Generator = Generator;
            entityDescription.Modifiers = new List<IModifier>(Modifiers);
            entityDescription.PersistentStorages = new List<IPersistentStorage>(PersistentStorages);
            entityDescription.TargetCountProvider = TargetCountProvider;
            entityDescription.FlushStrategy = FlushStrategy;
            entityDescription.RequestCapacityProvider = RequestCapacityProvider;
            entityDescription.StorageInsertGuard = StorageInsertGuard;
            entityDescription.InsertToPersistentStorageBeforeUse = InsertToPersistentStorageBeforeUse;
            entityDescription.PersistentStorageSelector = PersistentStorageSelector;

            return entityDescription;
        }
    }
}
