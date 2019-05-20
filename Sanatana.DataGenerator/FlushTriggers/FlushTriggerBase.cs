using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.FlushTriggers
{
    public abstract class FlushTriggerBase : IFlushTrigger
    {
        //properties
        protected long _capacity;
    


        //methods
        protected virtual long GetCapacity(EntityContext entityContext)
        {
            return _capacity;
        }

        public virtual bool IsFlushRequired(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            long capacity = GetCapacity(entityContext);

            long tempStorageCount = progress.CurrentCount - progress.FlushedCount;
            return tempStorageCount >= capacity;
        }

        public virtual void SetNextFlushCount(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            long capacity = GetCapacity(entityContext);

            progress.NextFlushCount = progress.FlushedCount + capacity;
        }

        public virtual void SetNextReleaseCount(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;
            long capacity = GetCapacity(entityContext);

            progress.NextReleaseCount = progress.ReleasedCount + capacity;
        }
    }
}
