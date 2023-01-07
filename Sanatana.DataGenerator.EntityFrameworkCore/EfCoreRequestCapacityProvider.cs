using Microsoft.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch;
using System;
using System.Collections;
using System.Collections.Generic;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.EntityFrameworkCore.Batch.Repositories;
using Sanatana.EntityFrameworkCore.Batch.Commands;

namespace Sanatana.DataGenerator.EntityFrameworkCore
{
    public class EfCoreRequestCapacityProvider : IRequestCapacityProvider
    {
        //fields
        protected Dictionary<Type, int> _entityMaxCount;
        protected IRepositoryFactory _repositoryFactory;


        //init
        public EfCoreRequestCapacityProvider(IRepositoryFactory repositoryFactory)
        {
            _entityMaxCount = new Dictionary<Type, int>();
            _repositoryFactory = repositoryFactory;
        }


        //methods
        public virtual void TrackEntityGeneration(EntityContext entityContext, IList instances)
        {
        }

        public virtual int GetCapacity(EntityContext entityContext, FlushRange flushRange)
        {
            if (!_entityMaxCount.ContainsKey(entityContext.Type))
            {
                using (IRepository repository = _repositoryFactory.CreateRepository())
                {
                    List<string> mappedProps = repository.DbContext.GetAllMappedProperties(entityContext.Type);
                    string[] generatedProps = repository.DbContext.GetDatabaseGeneratedColumns(entityContext.Type);
                    int paramsPerEntity = mappedProps.Count - generatedProps.Length;
                    paramsPerEntity = Math.Max(paramsPerEntity, 1); //if paramsPerEntity=0 and all props are db generated, then count as 1
                    int maxEntitiesInBatch = repository.DbParametersService.MaxParametersPerCommand / paramsPerEntity;
                    _entityMaxCount.Add(entityContext.Type, maxEntitiesInBatch);
                }
            }

            return _entityMaxCount[entityContext.Type];
        }
    }
}
