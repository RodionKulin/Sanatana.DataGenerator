using Sanatana.DataGenerator.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Text;

namespace Sanatana.DataGenerator.Entities
{
    public static class EntityDescriptionExtensions
    {
        /// <summary>
        /// Add EntityFrameworkPersistentStorage to store entity instances in SQL.
        /// And set EF RequestCapacityProvider, that will maximise number of instances inserted per single request.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="entityDescription"></param>
        /// <param name="dbContextFactory"></param>
        /// <returns></returns>
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
