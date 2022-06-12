using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals;
using System.Collections;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Contracts
{
    public interface IFlushCandidatesRegistry
    {
        void UpdateRequestCapacity(EntityContext entityContext);
        bool UpdateFlushRequired(EntityContext entityContext);
        List<ICommand> GetNextFlushCommands(EntityContext entityContext);
        EntityContext FindChildOfFlushCandidate();
    }
}