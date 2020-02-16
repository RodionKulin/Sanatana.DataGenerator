using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sanatana.DataGenerator.Internals
{
    public class EntityProgress
    {
        //fields
        protected long _flushedCount;
        protected long _nextFlushCount;
        protected long _releasedCount;
        protected long _nextReleaseCount;


        //properties
        /// <summary>
        /// Total number of entities that will be created in the end by generator.
        /// </summary>
        public long TargetCount { get; set; }
        /// <summary>
        /// Number of entities already created by generator during this program run. 
        /// This includes all inserted into persistent storage and all kept in temporary storage.
        /// </summary>
        public long CurrentCount { get; set; }       
        /// <summary>
        /// Number of entities required to generate during planing of next iteration. 
        /// Usually CurrentCount + 1, but depends on SpreadStrategy of child entities and Required relations..
        /// </summary>
        public long NextIterationCount { get; set; }
        /// <summary>
        /// Number of entities created that were already inserted into persistent storage.
        /// </summary>
        public long FlushedCount
        {
            get { return Interlocked.Read(ref _flushedCount); }
            protected set { Interlocked.Exchange(ref _flushedCount, value); }
        }
        /// <summary>
        /// Number or entities that will be flushed during next flush to persistent storage. 
        /// This includes previously FlushedCount & next entities batch count that will be flushed.
        /// </summary>
        public long NextFlushCount
        {
            get { return Interlocked.Read(ref _nextFlushCount); }
            set { Interlocked.Exchange(ref _nextFlushCount, value); }
        }
        /// <summary>
        /// Number of items that no longer stored in temporary storage.
        /// Should be same as FlushedCount except for InsertToPersistentStorageBeforeUse entities.
        /// In that case FlushedCount is updated after inserting and ReleasedCount
        /// is updated when entities no longer used by dependent children.
        /// </summary>
        public long ReleasedCount
        {
            get { return Interlocked.Read(ref _releasedCount); }
            protected set { Interlocked.Exchange(ref _releasedCount, value); }
        }
        /// <summary>
        /// Number or entities that will be removed from temp storage during next flush to persistent storage. 
        /// This includes previously RemovedFromTempStorageCount & next entities batch count that will be removed.
        /// </summary>
        public long NextReleaseCount
        {
            get { return Interlocked.Read(ref _nextReleaseCount); }
            set { Interlocked.Exchange(ref _nextReleaseCount, value); }
        }


        //methods
        public virtual void AddFlushedCount(long numberOfFlushed)
        {
            Interlocked.Add(ref _flushedCount, numberOfFlushed);
        }

        public virtual void AddReleasedCount(long numberOfRemoved)
        {
            Interlocked.Add(ref _releasedCount, numberOfRemoved);
        }

        public virtual bool IsFlushInProgress()
        {
            return NextFlushCount > FlushedCount;
        }
    }
}
