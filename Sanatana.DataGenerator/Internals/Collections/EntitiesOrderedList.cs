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
        private readonly List<Type> _list = new List<Type>();


        //methods
        public void Add(EntityContext next)
        {
            //insert before smallest index of child entity
            int? minChildEntityIndex = next.ChildEntities
                .Select(ent => _list.IndexOf(ent.Type))
                .Where(ind => ind != -1)
                .Select(ind => (int?)ind)
                .Min();
            minChildEntityIndex = minChildEntityIndex ?? int.MaxValue;

            //insert after largest index of parent entity
            int? maxParentEntityIndex = next.ParentEntities
                .Select(ent => _list.IndexOf(ent.Type))
                .Where(ind => ind != -1)
                .Select(ind => (int?)ind)
                .Max();
            maxParentEntityIndex = maxParentEntityIndex ?? -1;

            if(minChildEntityIndex <= maxParentEntityIndex)
            {
                throw new NotSupportedException($"Not able to build an ordered list of entities for {next.Type.FullName} minChildEntityIndex={minChildEntityIndex} maxParentEntityIndex={maxParentEntityIndex}");
            }

            _list.Insert(maxParentEntityIndex.Value + 1, next.Type);
        }

        public IEnumerator<Type> GetEnumerator()
        {
            foreach (Type type in _list)
            {
                yield return type;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
