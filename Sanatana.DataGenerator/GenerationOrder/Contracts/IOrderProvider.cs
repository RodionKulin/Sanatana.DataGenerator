using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Sanatana.DataGenerator.GenerationOrder.Contracts
{
    public interface IOrderProvider
    {
        IProgressState ProgressState { get; }
        void Setup(GeneratorSetup generatorSetup, Dictionary<Type, EntityContext> entityContexts);
        EntityAction GetNextAction();
        void UpdateCounters(Type type, IList generatedEntities);
    }
}