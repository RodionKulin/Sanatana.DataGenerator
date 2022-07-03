using Sanatana.DataGenerator.Comparers;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Providers
{
    internal class SimpleEqualityComparerFactory : IEqualityComparerFactory
    {
        public IEqualityComparer<TEntity> GetEqualityComparer<TEntity>(IEntityDescription description)
        {
            var comparers = new Dictionary<Type, object>()
            {
                { typeof(Category), new KeyEqualityComparer<Category, int>(x => x.Id) },
                { typeof(Post), new KeyEqualityComparer<Post, int>(x => x.Id) },
                { typeof(Comment), new KeyEqualityComparer<Comment, int>(x => x.Id) }
            };

            return (IEqualityComparer<TEntity>)comparers[typeof(TEntity)];
        }
    }
}
