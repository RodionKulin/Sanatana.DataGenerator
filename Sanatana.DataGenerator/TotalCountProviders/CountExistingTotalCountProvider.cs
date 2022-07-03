using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
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
        protected Expression<Func<TEntity, bool>> _filter;


        //init
        public CountExistingTotalCountProvider(Expression<Func<TEntity, bool>> filter = null)
        {
            if (filter == null)
            {
                filter = (entity) => true;
            }
            _filter = filter;
        }


        //methods
        public virtual long GetTargetCount(IEntityDescription description, DefaultSettings defaults)
        {
            //DefaultSettings defaults
            IPersistentStorageSelector _persistentStorageSelector = defaults.GetPersistentStorageSelector(description);
            long totalCount  = _persistentStorageSelector.Count(_filter);

            return totalCount;
        }
    }
}
