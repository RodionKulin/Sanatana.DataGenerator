using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    public class SubsetNodeFinder : CompleteNextNodeFinder
    {
        //fields
        protected List<Type> _entitiesSubset;


        //init
        public SubsetNodeFinder(List<Type> entitiesSubset, GeneratorServices generatorServices,
            IFlushCandidatesRegistry flushCandidatesRegistry,
            IProgressState progressState)
            : base(generatorServices, flushCandidatesRegistry, progressState)
        {
            _entitiesSubset = entitiesSubset ?? throw new ArgumentNullException(nameof(entitiesSubset));
        }


        //methods
        protected override EntityContext FindNextLeaf()
        {
            EntityContext leafNode = _progressState.NotCompletedEntities
                .Where(x => _entitiesSubset.Contains(x.Type))   //only subset of types
                .Where(x => x.ChildEntities
                    .Select(req => req.Type)
                    .Where(c => _entitiesSubset.Contains(c))    //only subset of child types
                    .Except(_progressState.CompletedEntityTypes)
                    .Count() == 0)
                .FirstOrDefault();

            return leafNode;
        }

    }
}
