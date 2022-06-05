using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGenerator.Strategies
{
    public class DefaultFlushStrategy : IFlushStrategy
    {
        public virtual bool CheckIsFlushRequired(EntityContext entityContext, FlushRange flushRange)
        {
            EntityProgress progress = entityContext.EntityProgress;
            return progress.CheckIsNewFlushRequired(flushRange);
        }

        public void UpdateFlushRangeCapacity(EntityContext entityContext, FlushRange flushRange, int requestCapacity)
        {
            flushRange.UpdateCapacity(requestCapacity);
        }
    }
}
