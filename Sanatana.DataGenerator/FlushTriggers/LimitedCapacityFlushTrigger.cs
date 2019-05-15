using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.FlushTriggers
{
    public class LimitedCapacityFlushTrigger : IFlushTrigger
    {
        //proerpties
        public long Capacity { get; protected set; }


        //init
        public LimitedCapacityFlushTrigger(long capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than 0");
            }
            
            Capacity = capacity;
        }


        //methods
        public virtual bool IsFlushRequired(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            long tempStorageCount = progress.CurrentCount - progress.FlushedCount;
            return tempStorageCount >= Capacity;
        }

        public virtual void SetNextFlushCount(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            long tempStorageCount = progress.CurrentCount - progress.FlushedCount;
            if (tempStorageCount >= Capacity)
            {
                progress.NextFlushCount = progress.FlushedCount + Capacity;
            }
        }
    }
}
