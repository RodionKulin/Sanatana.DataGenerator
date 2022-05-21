using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public interface IPersistentStorageSelector : IDisposable
    {
        List<TEntity> Select<TEntity>(Func<TEntity, bool> filter, int skip, int take)
            where TEntity : class;
        long Count<TEntity>(Func<TEntity, bool> filter)
            where TEntity : class;
    }
}
