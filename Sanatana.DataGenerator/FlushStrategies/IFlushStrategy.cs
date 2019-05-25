using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Strategies
{
    /// <summary>
    /// Entity persistent storage write trigger, that signals a required flush.
    /// </summary>
    public interface IFlushStrategy
    {
        bool IsFlushRequired(EntityContext entityContext);
        void SetNextFlushCount(EntityContext entityContext);
        void SetNextReleaseCount(EntityContext entityContext);
    }
}
