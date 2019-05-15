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
    public class EntityFrameworkFlushTrigger : IFlushTrigger
    {
        //fields
        protected DbContext _db;
        protected Dictionary<Type, int> _entityMaxCount;


        //init
        public EntityFrameworkFlushTrigger(DbContext db)
        {
            _db = db;
            _entityMaxCount = new Dictionary<Type, int>();
        }


        //methods
        public virtual bool IsFlushRequired(EntityContext entityContext)
        {
            int maxEntitiesInBatch = GetMaxEntitiesInBatch(entityContext);

            EntityProgress progress = entityContext.EntityProgress;
            long tempStorageCount = progress.CurrentCount - progress.FlushedCount;
            return tempStorageCount >= maxEntitiesInBatch;
        }

        public virtual void SetNextFlushCount(EntityContext entityContext)
        {
            int maxEntitiesInBatch = GetMaxEntitiesInBatch(entityContext);

            EntityProgress progress = entityContext.EntityProgress;
            long tempStorageCount = progress.CurrentCount - progress.FlushedCount;
            if (tempStorageCount >= maxEntitiesInBatch)
            {
                progress.NextFlushCount = progress.FlushedCount + maxEntitiesInBatch;
            }
        }

        protected virtual int GetMaxEntitiesInBatch(EntityContext entityContext)
        {
            int maxEntitiesInBatch = 0;

            if (_entityMaxCount.ContainsKey(entityContext.Type))
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
