using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.Internals.Collections;
using System.Collections;

namespace Sanatana.DataGenerator.Storages
{
    public class InMemoryStorage : IPersistentStorage
    {
        //fields
        protected ConcurrentDictionary<Type, ILockedList> _storages;
        

        //init
        public InMemoryStorage()
        {
            _storages = new ConcurrentDictionary<Type, ILockedList>();
        }

        public virtual void Setup()
        {
            _storages.Clear();
        }


        //methods
        public virtual Task Insert<TEntity>(List<TEntity> instances)
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            LockedList<TEntity> storage = _storages.GetOrAdd(entityType, (t) => new LockedList<TEntity>())
                as LockedList<TEntity>;

            storage.AddRange(instances);
          
            return Task.CompletedTask;
        }

        public virtual List<TEntity> Select<TEntity>()
        {
            Type entityType = typeof(TEntity);
            LockedList<TEntity> storage = _storages[entityType] as LockedList<TEntity>;
            return storage.SelectGeneric();
        }

        public virtual IList Select(Type entityType)
        {
            ILockedList storage = _storages[entityType];
            return storage.SelectNonGeneric();
        }

    }
}
