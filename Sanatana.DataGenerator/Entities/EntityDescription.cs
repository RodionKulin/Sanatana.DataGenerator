using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Entities
{
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
        /// Entities generator. Can return entities one by one or in small batches. 
        /// Usually better to return single entity not to store extra entities in memory.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// A method to make adjustments to entity after generated.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Database storage for generated entities.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Provider of total number of entities that needs to be generated.
        /// </summary>
        public IQuantityProvider QuantityProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entities to database.
        /// </summary>
        public IFlushStrategy FlushTrigger { get; set; }
        /// <summary>
        /// Get database generated columns like Id after inserting entities first. 
        /// Than only pass entities as required.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }


    }
}
