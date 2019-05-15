using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder.Complete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.GenerationOrder.Contracts;

namespace Sanatana.DataGenerator.GenerationOrder.Subset
{
    public class SubsetNodeFinder : CompleteNextNodeFinder
    {
        //fields
        protected List<Type> _entitiesSubset;


        //init
        public SubsetNodeFinder(List<Type> entitiesSubset, GeneratorSetup generatorSetup,
            IFlushCandidatesRegistry flushCandidatesRegistry,
            IProgressState orderProgress)
            : base(generatorSetup, flushCandidatesRegistry, orderProgress)
        {
            if (entitiesSubset == null)
            {
                throw new ArgumentNullException(nameof(entitiesSubset));
            }
            _entitiesSubset = entitiesSubset;
        }


        //methods
        protected override EntityContext FindNextLeaf()
        {
            EntityContext leafNode = _progressState.NotCompletedEntities
                .Where(x => _entitiesSubset.Contains(x.Type))   //only subset of types
               .Where(x => x.ChildEntities
                   .Select(req => req.Type)
                   .Where(c => _entitiesSubset.Contains(c))   //only subset of child types
                   .Except(_progressState.CompletedEntityTypes)
                   .Count() == 0)
               .FirstOrDefault();

            return leafNode;
        }

    }
}
