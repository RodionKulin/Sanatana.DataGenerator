using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    /// <summary>
    /// Provides commands to generate a subset list of all entities configured.
    /// </summary>
    public class SubsetSupervisor : CompleteSupervisor
    {
        //fields
        protected List<Type> _entitiesSubset;


        //init
        public SubsetSupervisor(List<Type> entitiesSubset)
        {
            if(entitiesSubset == null)
            {
                throw new ArgumentNullException(nameof(entitiesSubset));
            }
            _entitiesSubset = entitiesSubset;
        }

        public override void Setup(GeneratorServices generatorServices)
        {
            base.Setup(generatorServices);

            ProgressState = new SubsetProgressState(_entitiesSubset, generatorServices.EntityContexts);
            _flushCandidatesRegistry = new SubsetFlushCandidatesRegistry(
                _entitiesSubset, generatorServices, ProgressState);
            _nextNodeFinder = new SubsetNodeFinder(_entitiesSubset, generatorServices,
                _flushCandidatesRegistry, ProgressState);
            _requiredQueueBuilder = new CompleteRequiredQueueBuilder(
                generatorServices, _nextNodeFinder);
        }


        //Clone
        public ISupervisor Clone()
        {
            return new SubsetSupervisor(new List<Type>(_entitiesSubset));
        }
    }
}
