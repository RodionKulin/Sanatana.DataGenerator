using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Strategies
{
    /// <summary>
    /// Entity persistent storage write trigger.
    /// </summary>
    public interface IFlushStrategy
    {
        bool IsFlushRequired(EntityContext entityContext, long requestCapacity);
        void SetNextFlushCount(EntityContext entityContext, long requestCapacity);
    }
}
