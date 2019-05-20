using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.Internals;
using Sanatana.EntityFramework.Batch;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.EntityFramework
{
    public class EntityFrameworkFlushTrigger : FlushTriggerBase
    {
        //fields
        protected DbContext _db;
        protected Dictionary<Type, long> _entityMaxCount;


        //init
        public EntityFrameworkFlushTrigger(DbContext db)
        {
            _db = db;
            _entityMaxCount = new Dictionary<Type, long>();
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
