using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sanatana.DataGenerator.Internals.Objects
{
    /// <summary>
    /// Range of entity instances prepared to flush into persistent storage in single batch.
    /// </summary>
    public class FlushRange
    {
        //fields
        protected long _previousRangeFlushedCount;
        protected long _thisRangeFlushCount;
        protected long _flushRequestCapacity;
        protected long _isFlushed;
        protected long _isReleased;
        protected long _isFlushInProgress;
        protected long _isFlushRequired;


        //properties
        /// <summary>
        /// Number of entity instances created that were already inserted into persistent storage.
        /// </summary>
        public long PreviousRangeFlushedCount
        {
            get { return Interlocked.Read(ref _previousRangeFlushedCount); }
            protected set { Interlocked.Exchange(ref _previousRangeFlushedCount, value); }
        }
        /// <summary>
        /// Number or instances that will be removed from temp storage during next flush to persistent storage. 
        /// This includes previously removed from TempStorage count & next instances batch count that will be removed next flush.
        /// Used in TempStorage to remove instances.
        /// Used in EvenSpreadStrategy to check if child entity can generate from instances in TempStorage before flush.
        public long ThisRangeFlushCount
        {
            get { return Interlocked.Read(ref _thisRangeFlushCount); }
            protected set { Interlocked.Exchange(ref _thisRangeFlushCount, value); }
        }
        /// <summary>
        /// Number of entity instances that can be inserted to persistent storage in single request.
        /// </summary>
        public long FlushRequestCapacity
        {
            get { return Interlocked.Read(ref _flushRequestCapacity); }
            protected set { Interlocked.Exchange(ref _flushRequestCapacity, value); }
        }
        /// <summary>
        /// If instances are flushed, they were inserted to persistent storage. And database generated columns were set on instances.
        /// This happens 1. When all child entities were generated from this range or 2. If InsertToPersistentStorageBeforeUse=true and capacity for flush request is reached.
        /// </summary>
        public bool IsFlushed
        {
            get { return Interlocked.Read(ref _isFlushed) == 1; }
            protected set { Interlocked.Exchange(ref _isFlushed, Convert.ToInt64(value)); }
        }
        /// <summary>
        /// Release is removing instances from TempStorage.
        /// If instanced are released, they no longer can be used by child entities to generate. 
        /// </summary>
        public bool IsReleased
        {
            get { return Interlocked.Read(ref _isReleased) == 1; }
            protected set { Interlocked.Exchange(ref _isReleased, Convert.ToInt64(value)); }
        }
        /// <summary>
        /// Indicates if async FlushCommand started for this range
        /// </summary>
        public bool IsFlushInProgress
        {
            get { return Interlocked.Read(ref _isFlushInProgress) == 1; }
            protected set { Interlocked.Exchange(ref _isFlushInProgress, Convert.ToInt64(value)); }
        }
        /// <summary>
        /// Should flush to persistent storage instances from TempStorage. 
        /// If child entity can be generated from this parent in TempStorage, then IsFlushRequired won't be check untill flushed.
        /// </summary>
        public bool IsFlushRequired
        {
            /* Interlocked.Read() is only available for int64,
			 * so we have to represent the bool as a long with 0's and 1's
			 */
            get { return Interlocked.Read(ref _isFlushRequired) == 1; }
            protected set { Interlocked.Exchange(ref _isFlushRequired, Convert.ToInt64(value)); }
        }


        //init
        public FlushRange(long previousRangeFlushedCount, long capacity)
        {
            PreviousRangeFlushedCount = previousRangeFlushedCount;
            FlushRequestCapacity = capacity;
        }


        //methods
        public virtual void UpdateCapacity(long capacity)
        {
            FlushRequestCapacity = capacity;
            ThisRangeFlushCount = PreviousRangeFlushedCount + capacity;
        }

        public virtual void SetFlushedAndReleased()
        {
            IsFlushed = true;
            IsReleased = true;
            IsFlushRequired = false;
        }

        public virtual void SetFlushed()
        {
            IsFlushed = true;
            IsFlushRequired = false;
        }

        public virtual void SetReleased()
        {
            IsReleased = true;
        }

    }
}
