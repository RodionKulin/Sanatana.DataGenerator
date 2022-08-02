using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    public class InMemoryPersistentStorageSelector : IPersistentStorageSelector
    {
        protected IList _instances;


        //init
        public InMemoryPersistentStorageSelector(IList instances)
        {
            _instances = instances;
        }


        //methods
        public long Count<TEntity>(Expression<Func<TEntity, bool>> filter) where TEntity : class
        {
            return _instances.Count;
        }

        public List<TEntity> Select<TEntity, TOrderByKey>(Expression<Func<TEntity, bool>> filter, 
            Expression<Func<TEntity, TOrderByKey>> orderBy, bool isAsc, int skip, int take) 
            where TEntity : class
        {
            List<TEntity> instances = (List<TEntity>)_instances;

            IEnumerable<TEntity> query = instances
                .Where(filter.Compile());
            if(orderBy != null)
            {
                query = isAsc
                    ? query.OrderBy(orderBy.Compile())
                    : query.OrderByDescending(orderBy.Compile());
            }

            return query
                .Skip(skip)
                .Take(take)
                .ToList();
        }
    }
}
