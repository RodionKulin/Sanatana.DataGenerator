using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Commands;

namespace Sanatana.DataGenerator.Supervisors.Contracts
{
    /// <summary>
    /// Queue builder that finds next best entity to generate and build a queue of required entities for it.
    /// </summary>
    public interface IRequiredQueueBuilder
    {
        ICommand GetNextCommand();
        void UpdateCounters(EntityContext entityContext, IList generatedEntities, bool flushRequired);
    }
}