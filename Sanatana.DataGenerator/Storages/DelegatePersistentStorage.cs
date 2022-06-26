using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    /// <summary>
    /// Insert generated entity instances with lambda expression
    /// </summary>
    public class DelegatePersistentStorage : IPersistentStorage
    {
        //properties
        public object InsertFunc { get; set; }

        //init
        public DelegatePersistentStorage(object insertFunc)
        {
            InsertFunc = insertFunc;
        }

        //methods
        public virtual Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            var insertFunc = (Func<List<TEntity>, Task>)InsertFunc;
            return insertFunc.Invoke(entities);
        }

        public virtual void Setup()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
