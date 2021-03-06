﻿using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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
            EntityContext parentContext, bool onlyCheckCanGenerate)
        {
            List<EntityContext> notCompletedChildren = parentContext.ChildEntities
                .Where(x => _entitiesSubset.Contains(x.Type))   //only subset of types
                .Where(child => !_progressState.CompletedEntityTypes.Contains(child.Type))
                .Select(x => _entityContexts[x.Type])
                .ToList();
            if (notCompletedChildren.Count == 0)
            {
                return null;
            }

            EntityContext childContext = notCompletedChildren.Find(child =>
                CheckChildCanGenerate(child, parentContext, onlyCheckCanGenerate));
            return childContext;
        }
    }
}
