using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Strategies
{
    public class DefaultFlushStrategy : IFlushStrategy
    {
        //methods
        public virtual bool IsFlushRequired(EntityContext entityContext, long requestCapacity)
        {
            EntityProgress progress = entityContext.EntityProgress;
            FlushRange flushRange = progress.GetLatestRange();
            
            long tempStorageCount = progress.CurrentCount - flushRange.PreviousRangeFlushedCount;
            return tempStorageCount >= requestCapacity;
        }

        public virtual void SetNextFlushCount(EntityContext entityContext, long requestCapacity)
        {
            EntityProgress progress = entityContext.EntityProgress;
            FlushRange flushRange = progress.GetLatestRange();
            flushRange.UpdateCapacity(requestCapacity);
        }

    }
}
