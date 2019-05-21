using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.Internals;
using Sanatana.EntityFrameworkCore.Batch;
using Sanatana.EntityFrameworkCore.Batch.Commands.Merge;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.EntityFramework
{
    public class EntityFrameworkCoreFlushTrigger : FlushTriggerBase
    {
        //fields
        protected Func<DbContext> _dbContextFactory;
        protected Dictionary<Type, long> _entityMaxCount;


        //init
        public EntityFrameworkCoreFlushTrigger(Func<DbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
            _entityMaxCount = new Dictionary<Type, long>();
        }


        //methods
        protected override long GetCapacity(EntityContext entityContext)
        {
            int maxEntitiesInBatch = 0;

            if (!_entityMaxCount.ContainsKey(entityContext.Type))
            {
                using (DbContext db = _dbContextFactory())
                {
                    List<string> mappedProps = db.GetAllMappedProperties(entityContext.Type);
                    List<string> generatedProps = db.GetDatabaseGeneratedProperties(entityContext.Type);
                    int paramsPerEntity = mappedProps.Count - generatedProps.Count;
                    maxEntitiesInBatch = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS / paramsPerEntity;
                    _entityMaxCount.Add(entityContext.Type, maxEntitiesInBatch);
                }
            }

            return maxEntitiesInBatch;
        }
    }
}
