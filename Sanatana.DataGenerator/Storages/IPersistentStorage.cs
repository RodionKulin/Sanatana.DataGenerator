using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public interface IPersistentStorage : IDisposable
    {
        Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class;
    }
}
