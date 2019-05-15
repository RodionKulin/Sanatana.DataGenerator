using System;
using System.Collections;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals;

namespace Sanatana.DataGenerator.GenerationOrder.Contracts
{
    /// <summary>
    /// Registry of entities that need to be generated
    /// </summary>
    public interface IProgressState
    {
        List<Type> CompletedEntityTypes { get; }
        List<EntityContext> NotCompletedEntities { get;  }
        void UpdateCounters(EntityContext updateEntityContext, IList generatedEntities);
        bool GetIsFinished();
        decimal GetCompletionPercents();
    }
}