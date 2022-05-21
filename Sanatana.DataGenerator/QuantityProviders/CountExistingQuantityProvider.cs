using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.QuantityProviders
{
    public class CountExistingQuantityProvider<TEntity> : IQuantityProvider
        where TEntity : class
    {
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected Func<TEntity, bool> _filter;


        //properties
        public long TotalQuantity { get; protected set; }


        //init
        public CountExistingQuantityProvider(IPersistentStorageSelector persistentStorageSelector, Func<TEntity, bool> filter = null)
        {
            if (persistentStorageSelector == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(persistentStorageSelector)} can not be null");
            }
            _persistentStorageSelector = persistentStorageSelector;

            if (filter == null)
            {
                filter = (entity) => true;
            }
            _filter = filter;
        }



        //methods
        public virtual long GetTargetQuantity()
        {
            TotalQuantity  = _persistentStorageSelector.Count<TEntity>(_filter);

            return TotalQuantity;
        }
    }
}
