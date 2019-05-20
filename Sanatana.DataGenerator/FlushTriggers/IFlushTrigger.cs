using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.FlushTriggers
{
    /// <summary>
    /// Entity persistent storage write trigger, that signals a required flush.
    /// </summary>
    public interface IFlushTrigger
    {
        bool IsFlushRequired(EntityContext entityContext);
        void SetNextFlushCount(EntityContext entityContext);
        void SetNextReleaseCount(EntityContext entityContext);
    }
}
