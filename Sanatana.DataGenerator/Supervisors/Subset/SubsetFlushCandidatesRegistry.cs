using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    public class SubsetFlushCandidatesRegistry : CompleteFlushCandidatesRegistry
    {
        //fields
        protected List<Type> _entitiesSubset;


        //init
        public SubsetFlushCandidatesRegistry(List<Type> entitiesSubset, GeneratorSetup generatorSetup,
            Dictionary<Type, EntityContext> entityContexts, IProgressState orderProgress)
            : base(generatorSetup, entityContexts, orderProgress)
        {
            if (entitiesSubset == null)
            {
                throw new ArgumentNullException(nameof(entitiesSubset));
            }

            _entitiesSubset = entitiesSubset;
        }


        //methods
        protected override EntityContext FindChildThatCanGenerate(
            EntityContext parentContext, FlushRange parentRange, bool includeChildrenThatAreFlushCandidates)
        {
            IEnumerable<EntityContext> notCompletedChildren = parentContext.ChildEntities
                .Where(x => _entitiesSubset.Contains(x.Type))   //only subset of types
                .Where(child => !_progressState.CompletedEntityTypes.Contains(child.Type))
                .Select(x => _entityContexts[x.Type]);

            if (includeChildrenThatAreFlushCandidates == false)
            {
                notCompletedChildren = notCompletedChildren
                    .Where(childContext => !_flushCandidates.Contains(childContext));
            }

            return notCompletedChildren
                .ToList()
                .Find(childContext => CheckChildCanGenerate(childContext, parentContext, parentRange));
        }
    }
}
