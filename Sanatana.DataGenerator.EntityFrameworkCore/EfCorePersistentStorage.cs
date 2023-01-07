﻿using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.Storages;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Internals.PropertyMapping;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.EntityFrameworkCore
{
    
    public class EfCorePersistentStorage : 
        EfCoreRequestCapacityProvider, IPersistentStorage, IPersistentStorageSelector
    {
        //init
        public EfCorePersistentStorage(IRepositoryFactory repositoryFactory)
            : base(repositoryFactory)
        {
        }

        public virtual void Setup()
        {
        }


        //IPersistentStorage methods
        public virtual async Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            //will set ids generated by database on entities
            using (IRepository repository = _repositoryFactory.CreateRepository())
            {
                InsertCommand<TEntity> insertCommand = repository.InsertManyCommand(entities);
                insertCommand.Insert.SetIncludeAllByDefault(ColumnSetEnum.DbGenerated);
                insertCommand.Output.SetExcludeAllByDefault(ColumnSetEnum.DbGenerated);
                int insertedCount = await insertCommand.ExecuteAsync().ConfigureAwait(false);
            }
        }


        //IPersistentStorageSelector methods
        public virtual List<TEntity> Select<TEntity, TOrderByKey>(Expression<Func<TEntity, bool>> filter,
            Expression<Func<TEntity, TOrderByKey>> orderBy, bool isAscOrder, int skip, int take)
            where TEntity : class
        {
            using (IRepository repository = _repositoryFactory.CreateRepository())
            {
                IQueryable<TEntity> query = repository.DbContext.Set<TEntity>()
                    .Where(filter);

                if (orderBy != null)
                {
                    query = isAscOrder
                        ? query.OrderBy(orderBy)
                        : query.OrderByDescending(orderBy);
                }

                List<TEntity> entities = query
                    .Skip(skip)
                    .Take(take)
                    .ToList();
                return entities;
            }
        }

        public virtual long Count<TEntity>(Expression<Func<TEntity, bool>> filter)
            where TEntity : class
        {
            using (IRepository repository = _repositoryFactory.CreateRepository())
            {
                long count = repository.DbContext.Set<TEntity>()
                    .Where(filter)
                    .LongCount();
                return count;
            }
        }

    }
}
