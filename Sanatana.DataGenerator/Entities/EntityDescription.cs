using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.RequestCapacityProviders;

namespace Sanatana.DataGenerator.Entities
{
    /// <summary>
    /// Entity configuration for generation process
    /// </summary>
    public class EntityDescription : IEntityDescription
    {
        //properties
        /// <summary>
        /// Type of entity to generate
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Entities that are needed for generator as foreign keys or other type of dependency.
        /// </summary>
        public List<RequiredEntity> Required { get; set; }
        /// <summary>
        /// Entity instances generator. Can return instances one by one or in small batches. 
        /// Usually better to return single instance not to store extra instances in memory.
        /// Be default will use DefaultGenerator from GeneratorSetup.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// List of methods to make adjustments to entity instance after generation.
        /// Be default will use DefaultModifiers from GeneratorSetup.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Database storages for generated entities.
        /// Be default will use DefaultPersistentStorages from GeneratorSetup.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Provider of total number of entity instances that need to be generated.
        /// Be default will use DefaultTotalCountProvider from GeneratorSetup.
        /// </summary>
        public ITotalCountProvider TotalCountProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entities to database.
        /// Be default will use DefaultFlushStrategy from GeneratorSetup.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Provider of number of entity instances that can be inserted with next request to persistent storage.
        /// Be default will use DefaultRequestCapacityProvider from GeneratorSetup.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Checker of entity instances to be inserted into database. 
        /// Excludes unwanted instances, like the ones that already exist in database for EnsureExistGenerator.
        /// By default is not used.
        /// </summary>
        public IStorageInsertGuard StorageInsertGuard { get; set; }
        /// <summary>
        /// Get database generated columns after inserting entities first (for example Id).
        /// Only after receiving such columns pass entity instances as required for generation.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }



        //init
        public IEntityDescription Clone()
        {
            return new EntityDescription
            {
                Type = Type,
                Required = new List<RequiredEntity>(Required),
                Generator = Generator,
                Modifiers = new List<IModifier>(Modifiers),
                PersistentStorages = new List<IPersistentStorage>(PersistentStorages),
                TotalCountProvider = TotalCountProvider,
                FlushStrategy = FlushStrategy,
                RequestCapacityProvider = RequestCapacityProvider,
                StorageInsertGuard = StorageInsertGuard,
                InsertToPersistentStorageBeforeUse = InsertToPersistentStorageBeforeUse
            };
        }
    }
}
