using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Comparers
{
    public interface IEqualityComparerFactory
    {
        IEqualityComparer<TEntity> GetEqualityComparer<TEntity>(IEntityDescription description);
    }
}
