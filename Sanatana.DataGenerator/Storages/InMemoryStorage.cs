using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public class InMemoryStorage : IPersistentStorage
    {
        //fields
        protected Dictionary<Type, ArrayList> _entities;


        //init
        public InMemoryStorage()
        {
            _entities = new Dictionary<Type, ArrayList>();
        }
        

        //methods
        public virtual Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            Type entityType = typeof(TEntity);
            if (!_entities.ContainsKey(entityType))
            {
                _entities.Add(entityType, new ArrayList());
            }

            _entities[entityType].AddRange(entities);
            return Task.FromResult(0);
        }

        public virtual TEntity[] GetList<TEntity>()
        {
            Type entityType = typeof(TEntity);
            ArrayList itemsList = _entities[entityType];
            TEntity[] array = itemsList.ToArray(typeof(TEntity)) as TEntity[];
            return array;
        }

        public void Dispose()
        {
        }
    }
}
