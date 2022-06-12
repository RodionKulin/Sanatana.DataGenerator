using System;
using System.Collections;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Contracts
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