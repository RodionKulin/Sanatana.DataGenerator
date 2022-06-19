using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    internal class CompleteSupervisorProvider
    {
        //properties
        public ISupervisor Supervisor { get; protected set; }
        public IProgressState ProgressState { get; protected set; }
        public IFlushCandidatesRegistry FlushCandidatesRegistry { get; private set; }
        public IRequiredQueueBuilder RequiredQueueBuilder { get; private set; }
        public INextNodeFinder NextNodeFinder { get; private set; }
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; private set; }
        public Dictionary<Type, EntityContext> EntityContexts { get; private set; }


        //init
        public CompleteSupervisorProvider(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            EntityDescriptions = entityDescriptions;
            SetupCompleteSupervisor();
        }


        //individual setup methods
        private void SetupCompleteSupervisor()
        {
            var generatorSetup = new GeneratorSetup();

            GeneratorServices generatorServices = generatorSetup.GetGeneratorServices();
            generatorServices.SetupEntityContexts(EntityDescriptions);
            generatorServices.SetupSpreadStrategies();
            generatorServices.SetupTargetCount();
            EntityContexts = generatorServices.EntityContexts;

            Supervisor = new CompleteSupervisor();
            ProgressState = new CompleteProgressState(generatorServices.EntityContexts);
            FlushCandidatesRegistry = new CompleteFlushCandidatesRegistry(
                generatorServices, ProgressState);
            NextNodeFinder = new CompleteNextNodeFinder(
                generatorServices, FlushCandidatesRegistry, ProgressState);
            RequiredQueueBuilder = new CompleteRequiredQueueBuilder(
                generatorServices, NextNodeFinder);

            Supervisor.Setup(generatorServices);
        }

        public void SetEntityRequestCapacity(int capacity)
        {
            foreach (EntityContext entityContext in EntityContexts.Values)
            {
                FlushRange firstFlushRange = entityContext.EntityProgress.CreateNewRangeIfRequired();
                firstFlushRange.UpdateCapacity(capacity);
            }
        }

        public void SetEntityCurrentCount(Dictionary<Type, long> entityCurrentCount)
        {
            foreach (KeyValuePair<Type, long> entity in entityCurrentCount)
            {
                EntityContexts[entity.Key].EntityProgress.CurrentCount = entity.Value;
            }
        }

        public void UpdateFlushCandidates()
        {
            foreach (Type entityType in EntityContexts.Keys)
            {
                FlushCandidatesRegistry.UpdateRequestCapacity(EntityContexts[entityType]);
                FlushCandidatesRegistry.UpdateFlushRequired(EntityContexts[entityType]);
            }
        }

        public void UpdateFlushCandidates(params Type[] typesToUpdate)
        {
            typesToUpdate = EntityContexts.Keys.Intersect(typesToUpdate).ToArray();

            foreach (Type entityType in typesToUpdate)
            {
                FlushCandidatesRegistry.UpdateRequestCapacity(EntityContexts[entityType]);
                FlushCandidatesRegistry.UpdateFlushRequired(EntityContexts[entityType]);
            }
        }



        //combination of setup methods
        public static Dictionary<Type, EntityContext> ToEntityContexts(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            var provider = new CompleteSupervisorProvider(entityDescriptions);
            provider.SetEntityRequestCapacity(100);
            provider.UpdateFlushCandidates();
            return provider.EntityContexts;
        }

        public static CompleteSupervisor GetCompleteSupervisor(IEnumerable<IEntityDescription> entityDescriptions)
        {
            Dictionary<Type, IEntityDescription> dictDescriptions = entityDescriptions.ToDictionary(x => x.Type, x => x);
            var provider = new CompleteSupervisorProvider(dictDescriptions);
            return (CompleteSupervisor)provider.Supervisor;
        }
    }
}
