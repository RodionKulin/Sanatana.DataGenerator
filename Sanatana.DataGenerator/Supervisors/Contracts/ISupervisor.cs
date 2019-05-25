using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Commands;

namespace Sanatana.DataGenerator.Supervisors.Contracts
{
    public interface ISupervisor
    {
        IProgressState ProgressState { get; }
        void Setup(GeneratorSetup generatorSetup, Dictionary<Type, EntityContext> entityContexts);
        ICommand GetNextCommand();
        void HandleGenerateCompleted(EntityContext entityContext, IList generatedEntities);
        void EnqueueCommand(ICommand command);
    }
}