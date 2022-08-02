using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore;
using System;

namespace Sanatana.DataGenerator.Entities
{
    public static class EntityDescriptionExtensions
    {
        /// <summary>
        /// Add EfCorePersistentStorage to store entity instances in SQL.
        /// And set EfCoreRequestCapacityProvider, that will maximise number of instances inserted per single request.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityDescription"></param>
        /// <param name="dbContextFactory"></param>
        /// <returns></returns>
        public static EntityDescription<TEntity> AddEfCorePersistentStorage<TEntity>(
            this EntityDescription<TEntity> entityDescription, Func<DbContext> dbContextFactory)
            where TEntity : class
        {
            var storage = new EfCorePersistentStorage(dbContextFactory);
            entityDescription.AddPersistentStorage(storage);
            entityDescription.SetRequestCapacityProvider(storage);

            return entityDescription;
        }

    }
}
