using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sanatana.DataGenerator.Internals.Progress
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
        protected long _flushState;


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
        public int FlushRequestCapacity
        {
            get { return (int)Interlocked.Read(ref _flushRequestCapacity); }
            protected set { Interlocked.Exchange(ref _flushRequestCapacity, value); }
        }     
        /// <summary>
        /// Insert to persistent storage state of instances in this range.
        /// </summary>
        public FlushStatus FlushStatus
        {
            get { return (FlushStatus)Interlocked.Read(ref _flushState); }
            protected set { Interlocked.Exchange(ref _flushState, (long)value); }
        }


        //init
        public FlushRange(long previousRangeFlushedCount, int capacity)
        {
            PreviousRangeFlushedCount = previousRangeFlushedCount;
            FlushRequestCapacity = capacity;
        }


        //methods
        public virtual void UpdateCapacity(int capacity)
        {
            FlushRequestCapacity = capacity;
            ThisRangeFlushCount = PreviousRangeFlushedCount + capacity;
        }

        public virtual void SetFlushStatus(FlushStatus flushStatus)
        {
            FlushStatus = flushStatus;
        }
    }
}
