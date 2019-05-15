using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals;
using System.Collections;

namespace Sanatana.DataGenerator.GenerationOrder.Contracts
{
    public interface IFlushCandidatesRegistry
    {
        bool CheckIsFlushRequired(EntityContext entityContext);
        List<EntityAction> GetFlushActions(EntityContext entityContext, IList generatedEntities);
        EntityContext FindChildOfFlushCandidates();
    }
}