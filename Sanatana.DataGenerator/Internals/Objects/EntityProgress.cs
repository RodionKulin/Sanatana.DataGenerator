using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals
{
    public class EntityProgress
    {
        /// <summary>
        /// Total number of entities that will be created in the end by generator.
        /// </summary>
        public long TargetCount { get; set; }
        /// <summary>
        /// Number of entities already created by generator during this program run. 
        /// This includes all inserted into permanent storage and all kept in temporary storage.
        /// </summary>
        public long CurrentCount { get; set; }       
        /// <summary>
        /// Number of entities required to generate during planing of next ineration. 
        /// Usually CurrentCount + 1, but depends on SpreadStrategy of child entities.
        /// </summary>
        public long NextIterationCount { get; set; }
        /// <summary>
        /// Number of entities created that were already inserted into permanent storage.
        /// </summary>
        public long FlushedCount { get; set; }
        /// <summary>
        /// Number or items that will be flushed during next flush to permanent storage. 
        /// This includes FlushedCount & next batch of items count that will be flushed.
        /// </summary>
        public long NextFlushCount { get; set; }
    }
}
