using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Entities
{
    public static class EntityDescriptionExtensions
    {
        public static EntityDescription<TEntity> AddPersistentStorageEfCore<TEntity>(
            this EntityDescription<TEntity> entityDescription, Func<DbContext> dbContextFactory)
            where TEntity : class
        {
            var storage = new EntityFrameworkCorePersistentStorage(dbContextFactory);
            entityDescription.AddPersistentStorage(storage);
            entityDescription.SetRequestCapacityProvider(storage);

            return entityDescription;
        }
    }
}
