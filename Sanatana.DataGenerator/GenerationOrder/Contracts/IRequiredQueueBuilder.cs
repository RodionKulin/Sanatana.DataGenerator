using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Internals;

namespace Sanatana.DataGenerator.GenerationOrder.Contracts
{
    /// <summary>
    /// Queue builder that finds next best entity to generate and build a queue of required entities for it.
    /// </summary>
    public interface IRequiredQueueBuilder
    {
        EntityAction GetNextAction();
        void UpdateCounters(EntityContext entityContext, IList generatedEntities, bool flushRequired);
    }
}