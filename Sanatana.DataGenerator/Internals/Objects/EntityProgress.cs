using Newtonsoft.Json;
using Sanatana.DataGenerator.Internals.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sanatana.DataGenerator.Internals
{
    public class EntityProgress
    {
        //properties
        /// <summary>
        /// Total number of entities that will be created in the end by generator.
        /// </summary>
        public long TargetCount { get; set; }
        /// <summary>
        /// Total number of entity instances generated during this program run. 
        /// This includes all inserted to persistent storage and all accumulated in temporary storage before inserting to persistent.
        /// Incremented after instances were generated.
        /// </summary>
        public long CurrentCount { get; set; }
        /// <summary>
        /// Number of entities required to generate during planing of next iteration. 
        /// Usually CurrentCount + 1, but depends on SpreadStrategy of child entities and Required relations..
        /// </summary>
        public long NextIterationCount { get; set; }
        /// <summary>
        /// Ranges of entity instances prepared to flush into persistent storage in single batch.
        /// </summary>
        public List<FlushRange> FlushRanges { get; protected set; }


        //init
        public EntityProgress()
        {
            FlushRanges = new List<FlushRange>();
        }

        
        //methods
        public virtual FlushRange GetNextFlushRange()
        {
            //not checking x.IsFlushRequired, that can be false for last range
            FlushRange flushRange = FlushRanges.FirstOrDefault(x => !x.IsFlushed && !x.IsFlushInProgress);
            if(flushRange == null)
            {
                string data = JsonConvert.SerializeObject(FlushRanges);
                string message = $"No {nameof(FlushRange)} found to start flush. {data}";
                throw new DataMisalignedException(message);
            }

            return flushRange;
        }

        public virtual FlushRange GetNextReleaseRange()
        {
            FlushRange flushRange = FlushRanges.FirstOrDefault(x => x.IsFlushed && !x.IsReleased);
            if (flushRange == null)
            {
                string data = JsonConvert.SerializeObject(FlushRanges);
                string message = $"No {nameof(FlushRange)} found to start release. {data}";
                throw new DataMisalignedException(message);
            }

            return flushRange;
        }

        public virtual FlushRange GetLatestRange()
        {
            FlushRange flushRange = FlushRanges.LastOrDefault();
            if (flushRange == null)
            {
                string data = JsonConvert.SerializeObject(FlushRanges);
                string message = $"No last {nameof(FlushRange)} found. {data}";
                throw new DataMisalignedException(message);
            }

            return flushRange;
        }

        public virtual bool HasNotReleased()
        {
            FlushRange flushRange = FlushRanges.FirstOrDefault(x => x.IsFlushed && !x.IsReleased);
            x.EntityProgress.ReleasedCount < x.EntityProgress.CurrentCount
        }
    }
}
