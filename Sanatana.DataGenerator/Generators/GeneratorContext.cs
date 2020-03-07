using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Generators
{
    public class GeneratorContext
    {
        /// <summary>
        /// Description with verious settings how this entity should be generated.
        /// </summary>
        public IEntityDescription Description { get; set; }
        /// <summary>
        /// All entities configured for generation.
        /// </summary>
        public Dictionary<Type, EntityContext> EntityContexts { get; set; }
        /// <summary>
        /// Number of items that will be generated in the end.
        /// </summary>
        public long TargetCount { get; set; }
        /// <summary>
        /// Number of items already generated.
        /// </summary>
        public long CurrentCount { get; set; }
        /// <summary>
        /// Entities that will be generated before and passed as arguments.
        /// </summary>
        public Dictionary<Type, object> RequiredEntities { get; set; }
    }
}
