using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    /// <summary>
    /// VoidStorage does not store provided entity instances anywhere.
    /// Intended to be used with ReuseExistingGenerator that provides entity instances already stored into database, that we dont want to insert again.
    /// </summary>
    public class VoidStorage : IPersistentStorage
    {
        public virtual Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            return Task.FromResult(0);
        }

        public virtual void Dispose()
        {
        }
    }
}
