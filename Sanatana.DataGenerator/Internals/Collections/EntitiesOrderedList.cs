using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Collections
{
    /// <summary>
    /// Orders entity types according to their Required property. 
    /// Entities with no Required items will go first. 
    /// Other entities with Required set will go after thier Required items.
    /// </summary>
    public class EntitiesOrderedList : IEnumerable<Type>
    {
        private Dictionary<Type, EntityContext> _entityContexts;
        private List<EntityContext> _sortedEntities;


        //init
        public EntitiesOrderedList(Dictionary<Type, EntityContext> entityContexts)
        {
            _entityContexts = entityContexts;
            _sortedEntities = entityContexts.Values.ToList();
            _sortedEntities.Sort(CompareTypesOrder);
        }


        //IEnumerator
        public virtual IEnumerator<Type> GetEnumerator()
        {
            foreach (EntityContext entity in _sortedEntities)
            {
                yield return entity.Type;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        //Comparer
        protected virtual int CompareTypesOrder(EntityContext t1, EntityContext t2)
        {
            bool t2IsParentOft1 = ParentsContainEntity(t1.ParentEntities, t2.Type);
            if (t2IsParentOft1)
            {
                return 1;
            }

            bool t1IsParentOft2 = ParentsContainEntity(t2.ParentEntities, t1.Type);
            if (t1IsParentOft2)
            {
                return -1;
            }

            return 0;
        }

        protected virtual bool ParentsContainEntity(List<IEntityDescription> parents, Type entityToCheck)
        {
            if(parents == null || parents.Count == 0)
            {
                return false;
            }

            if (parents.Find(x => x.Type == entityToCheck) != null)
            {
                return true;
            }

            return parents.SelectMany(x => x.Required)
                .Select(required => _entityContexts[required.Type])
                .Any(parentEntity => ParentsContainEntity(parentEntity.ParentEntities, entityToCheck));
        }
    }
}
