using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Sanatana.DataGenerator.TotalCountProviders
{
    public class CountExistingTotalCountProvider<TEntity> : ITotalCountProvider
        where TEntity : class
    {
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected Expression<Func<TEntity, bool>> _filter;


        //properties
        public long TargetCount { get; protected set; }


        //init
        public CountExistingTotalCountProvider(
            IPersistentStorageSelector persistentStorageSelector,
            Expression<Func<TEntity, bool>> filter = null)
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
        public virtual long GetTargetCount()
        {
            TargetCount  = _persistentStorageSelector.Count(_filter);

            return TargetCount;
        }
    }
}
