﻿using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Internals;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGenerator.EntityFramework
{
    public class EntityFrameworkCoreRequestCapacityProvider : IRequestCapacityProvider
    {
        //fields
        protected Func<DbContext> _dbContextFactory;
        protected Dictionary<Type, int> _entityMaxCount;


        //init
        public EntityFrameworkCoreRequestCapacityProvider(Func<DbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _entityMaxCount = new Dictionary<Type, int>();
        }


        //methods
        public virtual void TrackEntityGeneration(EntityContext entityContext, IList instances)
        {
        }

        public virtual int GetCapacity(EntityContext entityContext, FlushRange flushRange)
        {
            if (!_entityMaxCount.ContainsKey(entityContext.Type))
            {
                using (DbContext db = _dbContextFactory())
                {
                    List<string> mappedProps = db.GetAllMappedProperties(entityContext.Type);
                    List<string> generatedProps = db.GetDatabaseGeneratedProperties(entityContext.Type);
                    int paramsPerEntity = mappedProps.Count - generatedProps.Count;
                    int maxEntitiesInBatch = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS / paramsPerEntity;
                    _entityMaxCount.Add(entityContext.Type, maxEntitiesInBatch);
                }
            }

            return _entityMaxCount[entityContext.Type];
        }
    }
}
