using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Storages;
using System;
using System.Linq.Expressions;

namespace Sanatana.DataGenerator.TargetCountProviders
{
    public class CountExistingTargetCountProvider<TEntity> : ITargetCountProvider
        where TEntity : class
    {
        protected Expression<Func<TEntity, bool>> _filter;


        //init
        public CountExistingTargetCountProvider(Expression<Func<TEntity, bool>> filter = null)
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
            long targetCount  = _persistentStorageSelector.Count(_filter);

            return targetCount;
        }
    }
}
