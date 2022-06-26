using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    /// <summary>
    /// Provides commands to generate a subset list of all entities configured.
    /// Will include target entities and all their Required entities.
    /// </summary>
    public class SubsetSupervisor : CompleteSupervisor
    {
        //fields
        protected List<Type> _targetEntitiesSubset;


        //init
        public SubsetSupervisor(List<Type> targetEntitiesSubset)
        {
            _targetEntitiesSubset = targetEntitiesSubset;
        }

        public override void Setup(GeneratorServices generatorServices)
        {
            base.Setup(generatorServices);

            SubsetSettings subsetSettings = new SubsetSettings(_targetEntitiesSubset);
            subsetSettings.Setup(generatorServices);

            ProgressState = new SubsetProgressState(subsetSettings.TargetAndRequiredEntities, generatorServices.EntityContexts);
            _flushCandidatesRegistry = new SubsetFlushCandidatesRegistry(
                subsetSettings.TargetAndRequiredEntities, generatorServices, ProgressState);
            _nextNodeFinder = new SubsetNodeFinder(subsetSettings.TargetAndRequiredEntities, generatorServices,
                _flushCandidatesRegistry, ProgressState);
            _requiredQueueBuilder = new CompleteRequiredQueueBuilder(
                generatorServices, _nextNodeFinder);
        }


        //clone
        public override ISupervisor Clone()
        {
            return new SubsetSupervisor(new List<Type>(_targetEntitiesSubset));
        }


    }
}
