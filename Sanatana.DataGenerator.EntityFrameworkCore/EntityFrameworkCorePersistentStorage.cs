﻿using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using Sanatana.EntityFrameworkCore.Batch.ColumnMapping;
using Sanatana.EntityFrameworkCore.Batch.Commands;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.EntityFramework
{
    public class EntityFrameworkCorePersistentStorage : 
        EntityFrameworkCoreRequestCapacityProvider, IPersistentStorage, IPersistentStorageSelector
    {
        //init
        public EntityFrameworkCorePersistentStorage(Func<DbContext> dbContextFactory)
            : base(dbContextFactory)
        {
        }


        //methods
        public virtual async Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            //will set ids generated by database on entities
            using (DbContext dbContext = _dbContextFactory())
            {
                var merge = new MergeCommand<TEntity>(dbContext, entities);
                merge.Output.SetExcludeAllByDefault(true)
                    .SetExcludeDbGeneratedByDefault(ExcludeOptions.Include);
                int insertedCount = await merge.ExecuteAsync(MergeType.Insert)
                    .ConfigureAwait(false);
            }
        }


        //selectors
        public virtual List<TEntity> Select<TEntity, TOrderByKey>(Expression<Func<TEntity, bool>> filter,
            Expression<Func<TEntity, TOrderByKey>> orderBy, int skip, int take)
            where TEntity : class
        {
            using (DbContext dbContext = _dbContextFactory())
            {
                IQueryable<TEntity> query = dbContext.Set<TEntity>()
                    .Where(filter);

                if (orderBy != null)
                {
                    query = query.OrderBy(orderBy);
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
            using (DbContext dbContext = _dbContextFactory())
            {
                long count = dbContext.Set<TEntity>()
                    .Where(filter)
                    .LongCount();
                return count;
            }
        }
    }
}
