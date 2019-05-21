using MongoDB.Driver;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.MongoDb
{
    public class MongoDbPersistentStorage : IPersistentStorage
    {
        //fields
        protected IMongoDatabase _mongoDatabase;
        protected Dictionary<Type, string> _collectionNames;
        protected string _collectionName;


        //init
        public MongoDbPersistentStorage(IMongoDatabase mongoDatabase,
            Dictionary<Type, string> collectionNames)
        {
            _mongoDatabase = mongoDatabase;
            _collectionNames = collectionNames;
        }

        public MongoDbPersistentStorage(IMongoDatabase mongoDatabase,
            string collectionName)
        {
            _mongoDatabase = mongoDatabase;
            _collectionName = collectionName;
        }


        //methods
        public virtual async Task Insert<TEntity>(List<TEntity> entities) 
            where TEntity : class
        {
            string collectionName = _collectionName == null
                ? _collectionNames[typeof(TEntity)]
                : _collectionName;

            IMongoCollection <TEntity> collection = 
                _mongoDatabase.GetCollection<TEntity>(collectionName);
            await collection.InsertManyAsync(entities, new InsertManyOptions
            {
                IsOrdered = true
            }).ConfigureAwait(false);
        }

        public virtual void Dispose()
        {
        }
    }
}
