using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public interface IPersistentStorageSelector
    {
        List<TEntity> Select<TEntity, TOrderByKey>(Expression<Func<TEntity, bool>> filter, 
            Expression<Func<TEntity, TOrderByKey>> orderBy, bool isAscOrder, int skip, int take)
            where TEntity : class;

        long Count<TEntity>(Expression<Func<TEntity, bool>> filter)
            where TEntity : class;
    }
}
