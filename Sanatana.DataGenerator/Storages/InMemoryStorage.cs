using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public class InMemoryStorage : IPersistentStorage
    {
        //fields
        protected ConcurrentDictionary<Type, ArrayList> _storages;


        //init
        public InMemoryStorage()
        {
            _storages = new ConcurrentDictionary<Type, ArrayList>();
        }
        

        //methods
        public virtual Task Insert<TEntity>(List<TEntity> instances)
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            ArrayList storage = _storages.GetOrAdd(entityType, (t) => new ArrayList());
            storage.AddRange(instances);
            return Task.FromResult(0);
        }

        public virtual TEntity[] GetInstances<TEntity>()
        {
            Type entityType = typeof(TEntity);
            ArrayList storage = _storages[entityType];
            TEntity[] instances = storage.ToArray(typeof(TEntity)) as TEntity[];
            return instances;
        }

        public virtual void Dispose()
        {
        }
    }
}
