using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using System;

namespace Sanatana.DataGenerator.Entities
{
    public static class EntityDescriptionExtensions
    {
        /// <summary>
        /// Add EfCorePersistentStorage to store entity instances in SQL Server.
        /// And set EfCoreRequestCapacityProvider, that will maximise number of instances inserted per single request.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityDescription"></param>
        /// <param name="dbContextFactory"></param>
        /// <returns></returns>
        public static EntityDescription<TEntity> AddEfCorePersistentStorageSqlServer<TEntity>(
            this EntityDescription<TEntity> entityDescription, Func<DbContext> dbContextFactory)
            where TEntity : class
        {
            IRepositoryFactory repositoryFactory = new SqlRepositoryFactory(dbContextFactory);
            var storage = new EfCorePersistentStorage(repositoryFactory);
            entityDescription.AddPersistentStorage(storage);
            entityDescription.SetRequestCapacityProvider(storage);

            return entityDescription;
        }

    }
}
