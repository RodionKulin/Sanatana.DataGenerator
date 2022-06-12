using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Context object holding required data and configuration to generate new entity instance.
    /// </summary>
    public class GeneratorContext
    {
        /// <summary>
        /// Description with various settings how this entity should be generated.
        /// </summary>
        public IEntityDescription Description { get; set; }
        /// <summary>
        /// All entities configured for generation.
        /// </summary>
        public Dictionary<Type, EntityContext> EntityContexts { get; set; }
        /// <summary>
        /// Number of instances that will be generated in the end.
        /// </summary>
        public long TargetCount { get; set; }
        /// <summary>
        /// Number of instances already generated.
        /// </summary>
        public long CurrentCount { get; set; }
        /// <summary>
        /// Entity instances that will be passed as arguments to generator.
        /// </summary>
        public Dictionary<Type, object> RequiredEntities { get; set; }
    }
}
