using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sanatana.DataGenerator.Internals.Progress
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
        public virtual FlushRange CreateNewRangeIfRequired()
        {
            FlushRange latestRange = FlushRanges.LastOrDefault();
            if (latestRange == null)
            {
                var newRange = new FlushRange(0, int.MaxValue);
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

        public virtual void UpdateCapacity(long requestCapacity)
        {
            FlushRange flushRange = FlushRanges.LastOrDefault();
            if (flushRange == null)
            {
                FlushRanges.Add(new FlushRange(0, requestCapacity));
            }

            flushRange.UpdateCapacity(requestCapacity);
            if (CurrentCount > flushRange.ThisRangeFlushCount)
            {
                FlushRanges.Add(new FlushRange(flushRange.ThisRangeFlushCount, requestCapacity));
            }
        }

        public virtual FlushRange GetLatestRange()
        {
            FlushRange flushRange = FlushRanges.LastOrDefault();
            if (flushRange == null)
            {
                string data = GetRangesDump();
                string message = $"No last {nameof(FlushRange)} found. {data}";
                throw new DataMisalignedException(message);
            }

            return flushRange;
        }

        public virtual FlushRange GetLatestRangeNoThrow()
        {
            return FlushRanges.LastOrDefault();
        }

        public virtual bool CheckIsNewFlushRequired(FlushRange flushRange)
        {
            if(flushRange.FlushStatus != FlushStatus.FlushNotRequired)
            {
                //other statuses already started flushing
                return false;
            }

            if(CurrentCount >= flushRange.ThisRangeFlushCount)
            {
                //reached number of instances for full capacity db insert request 
                return true;
            }

            if (CurrentCount >= TargetCount)
            {
                //final flush may be required
                //if still have some instances in Temp storage and
                //if not have enough instances to reach full capacity db insert request 
                return true;
            }

            return false;
        }

        public virtual bool HasNotReleasedRange()
        {
            return FlushRanges.Any(x => x.FlushStatus != FlushStatus.FlushedAndReleased);
        }

        public virtual long GetReleasedCount()
        {
            FlushRange flushRange = FlushRanges.LastOrDefault(x => x.FlushStatus == FlushStatus.FlushedAndReleased);
            if(flushRange == null)
            {
                return 0;
            }

            return flushRange.ThisRangeFlushCount;
        }

        public virtual long GetNextReleaseCount()
        {
            FlushRange flushRange = FlushRanges.FirstOrDefault(x => x.FlushStatus != FlushStatus.FlushedAndReleased);
            if (flushRange == null)
            {
                string data = GetRangesDump();
                string message = $"No {nameof(FlushRange)} found with IsReleased=false to find NextReleaseCount. {data}";
                throw new DataMisalignedException(message);
            }

            return flushRange.ThisRangeFlushCount;
        }

        public virtual long GetFlushedCount()
        {
            FlushRange flushRange = FlushRanges.LastOrDefault(x => x.FlushStatus == FlushStatus.Flushed ||
                x.FlushStatus == FlushStatus.FlushedAndReleased);
            if (flushRange == null)
            {
                return 0;
            }

            return flushRange.ThisRangeFlushCount;
        }

        public virtual string GetRangesDump()
        {
            return JsonConvert.SerializeObject(FlushRanges);
        }
    }
}
