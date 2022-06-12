using Sanatana.DataGenerator.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace Sanatana.DataGenerator.Entities
{
    public static class EntityDescriptionExtensions
    {
        public static EntityDescription<TEntity> AddPersistentStorageEf<TEntity>(
            this EntityDescription<TEntity> entityDescription, Func<DbContext> dbContextFactory)
            where TEntity : class
        {
            var storage = new EntityFrameworkPersistentStorage(dbContextFactory);
            entityDescription.AddPersistentStorage(storage);
            entityDescription.SetRequestCapacityProvider(storage);

            return entityDescription;
        }
    }
}
