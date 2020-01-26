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
        /// <summary>
        /// MongoDb storage that will pick collection name depending from entity type
        /// </summary>
        /// <param name="mongoDatabase"></param>
        /// <param name="collectionNames"></param>
        public MongoDbPersistentStorage(IMongoDatabase mongoDatabase,
            Dictionary<Type, string> collectionNames)
        {
            if (collectionNames == null || collectionNames.Count == 0)
            {
                throw new NullReferenceException($"{nameof(collectionNames)} can not be null or empty");
            }
            _mongoDatabase = mongoDatabase;
            _collectionNames = collectionNames;
        }

        /// <summary>
        /// MongoDb storage that will write all incoming entities so single collection
        /// </summary>
        /// <param name="mongoDatabase"></param>
        /// <param name="collectionName"></param>
        public MongoDbPersistentStorage(IMongoDatabase mongoDatabase, string collectionName)
        {
            if (collectionName == null)
            {
                throw new NullReferenceException($"{nameof(collectionName)} can not be null");
            }
            _mongoDatabase = mongoDatabase;
            _collectionName = collectionName;
        }


        //methods
        public virtual Task Insert<TEntity>(List<TEntity> entities) 
            where TEntity : class
        {
            string collectionName = _collectionName == null
                ? _collectionNames[typeof(TEntity)]
                : _collectionName;

            IMongoCollection<TEntity> collection = _mongoDatabase.GetCollection<TEntity>(collectionName);
            return collection.InsertManyAsync(entities, new InsertManyOptions
            {
                IsOrdered = false
            });
        }

        public virtual void Dispose()
        {
        }
    }
}
