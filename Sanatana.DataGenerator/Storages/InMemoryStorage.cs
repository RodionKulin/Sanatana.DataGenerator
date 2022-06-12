using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace Sanatana.DataGenerator.Storages
{
    public class InMemoryStorage : IPersistentStorage
    {
        //fields
        protected ConcurrentDictionary<Type, ConcurrentBag<object>> _storages;
        

        //init
        public InMemoryStorage()
        {
            _storages = new ConcurrentDictionary<Type, ConcurrentBag<object>>();
        }
        

        //methods
        public virtual Task Insert<TEntity>(List<TEntity> instances)
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            ConcurrentBag<object> storage = _storages.GetOrAdd(entityType, (t) => new ConcurrentBag<object>());
            foreach (TEntity instance in instances)
            {
                storage.Add(instance);
            }
            return Task.FromResult(0);
        }

        public virtual TEntity[] Select<TEntity>()
        {
            Type entityType = typeof(TEntity);
            return _storages[entityType].Cast<TEntity>().ToArray();
        }

        public virtual object[] Select(Type entityType)
        {
            return _storages[entityType].ToArray();
        }

        public virtual void Dispose()
        {
        }
    }
}
