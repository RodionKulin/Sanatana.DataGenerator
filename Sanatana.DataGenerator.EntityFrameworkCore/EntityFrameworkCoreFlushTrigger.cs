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
        protected DbContext _db;
        protected Dictionary<Type, int> _entityMaxCount;


        //init
        public EntityFrameworkCoreFlushTrigger(DbContext db)
        {
            _db = db;
            _entityMaxCount = new Dictionary<Type, int>();
        }


        //methods
        protected override long GetCapacity(EntityContext entityContext)
        {
            int maxEntitiesInBatch = 0;

            if (!_entityMaxCount.ContainsKey(entityContext.Type))
            {
                List<string> mappedProps = _db.GetAllMappedProperties(entityContext.Type);
                List<string> generatedProps = _db.GetDatabaseGeneratedProperties(entityContext.Type);
                int paramsPerEntity = mappedProps.Count - generatedProps.Count;
                maxEntitiesInBatch = EntityFrameworkConstants.MAX_NUMBER_OF_SQL_COMMAND_PARAMETERS / paramsPerEntity;
                _entityMaxCount.Add(entityContext.Type, maxEntitiesInBatch);
            }

            return maxEntitiesInBatch;
        }
    }
}
