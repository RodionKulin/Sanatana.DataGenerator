﻿using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Internals.Commands;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Contracts
{
    public interface ISupervisor
    {
        IProgressState ProgressState { get; }
        void Setup(GeneratorServices generatorServices);
        IEnumerable<ICommand> IterateCommands();
        void HandleGenerateCompleted(EntityContext entityContext, IList generatedEntities);
        void EnqueueCommand(ICommand command);
        ISupervisor Clone();
    }
}