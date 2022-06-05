using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Strategies
{
    /// <summary>
    /// Entity persistent storage write trigger. It measures after what number instances should start persistent storage insert.
    /// </summary>
    public interface IFlushStrategy
    {
        bool CheckIsFlushRequired(EntityContext entityContext, FlushRange flushRange);
        void UpdateFlushRangeCapacity(EntityContext entityContext, FlushRange flushRange, int requestCapacity);
    }
}
