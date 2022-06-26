using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public class EntityProgress
    {
        protected long _releasedCount = 0;


        //properties
        protected long ReleasedCount
        {
            get { return _releasedCount; }
            set { _releasedCount = value; }
        }
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
        public virtual FlushRange CreateNewRangeIfRequired()
        {
            FlushRange latestRange = FlushRanges.LastOrDefault();
            if (latestRange == null)
            {
                var newRange = new FlushRange(ReleasedCount, int.MaxValue);
                FlushRanges.Add(newRange);
                return newRange;
            }

            long currentCount = Math.Min(CurrentCount, TargetCount); //do not need to insert into db instances above TargetCount
            if (currentCount > latestRange.ThisRangeFlushCount)
            {
                var newRange = new FlushRange(latestRange.ThisRangeFlushCount, int.MaxValue);
                FlushRanges.Add(newRange);
                return newRange;
            }

            return null;
        }

        public virtual void RemoveRange(FlushRange flushRange)
        {
            FlushRanges.Remove(flushRange);
            ReleasedCount = Math.Max(ReleasedCount, flushRange.ThisRangeFlushCount);
        }

        public virtual bool CheckIsNewFlushRequired(FlushRange flushRange)
        {
            if(CurrentCount >= flushRange.ThisRangeFlushCount)
            {
                //reached number of instances for full capacity db insert request 
                return true;
            }

            if (CurrentCount >= TargetCount)
            {
                //final flush may be required
                //if all target number of instances generated
                //if still have some instances in temp storage and
                //if not have enough entity instances to reach full capacity db insert request 
                return true;
            }

            return false;
        }

        public virtual long GetReleasedCount()
        {
            return ReleasedCount;
        }

        public virtual string GetRangesDump()
        {
            return JsonConvert.SerializeObject(FlushRanges);
        }

    }
}
