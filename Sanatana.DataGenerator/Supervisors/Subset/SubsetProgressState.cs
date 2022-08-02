using Sanatana.DataGenerator.Supervisors.Complete;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    public class SubsetProgressState : CompleteProgressState
    {
        //fields
        protected List<Type> _entitiesSubset;


        //init
        public SubsetProgressState(List<Type> entitiesSubset, Dictionary<Type, EntityContext> entityContexts)
            : base(entityContexts)
        {
            if (entitiesSubset == null)
            {
                throw new ArgumentNullException(nameof(entitiesSubset));
            }
            _entitiesSubset = entitiesSubset;
        }


        //methods
        public override bool GetIsFinished()
        {
            return NotCompletedEntities
                .Where(x => _entitiesSubset.Contains(x.Type))   //only subset of types
                .Count() == 0;
        }

        public override decimal GetCompletionPercents()
        {
            IEnumerable<EntityContext> entityContexts = _entityContexts
                .Select(x => x.Value)
                .Where(x => _entitiesSubset.Contains(x.Type));   //only subset of types

            return ConvertEntitiesCompletionPercents(entityContexts);
        }

    }
}
