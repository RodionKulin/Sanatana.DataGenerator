using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Internals.Progress;


namespace Sanatana.DataGenerator.Generators
{
    public class EnsureExistGenerator<TEntity, TOrderByKey> : IGenerator, IStorageInsertGuard, IFlushStrategy
        where TEntity : class
    {
        protected IGenerator _newInstancesGenerator;
        protected IEqualityComparer<TEntity> _comparer;
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected Expression<Func<TEntity, bool>> _storageSelectorfilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected Dictionary<TEntity, TEntity> _existingInstancesCache;
        protected NewInstanceCounter _newInstanceCounter;
        protected int _maxInstancesInTempStorage = 10000;


        //init
        public EnsureExistGenerator(IPersistentStorageSelector persistentStorageSelector, 
            IGenerator newInstancesGenerator, IEqualityComparer<TEntity> comparer)
        {
            _persistentStorageSelector = persistentStorageSelector;
            _newInstancesGenerator = newInstancesGenerator;
            _comparer = comparer;
            _newInstanceCounter = new NewInstanceCounter();
        }


        //setup
        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="storageSelectorfilter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetFilter(Expression<Func<TEntity, bool>> storageSelectorfilter)
        {
            if (storageSelectorfilter == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorfilter)} can not be null");
            }
            _storageSelectorfilter = storageSelectorfilter;
            return this;
        }

        /// <summary>
        /// Set optional orderBy expression to select existing instances with expected order.
        /// By default will select unordered instances.
        /// </summary>
        /// <param name="storageSelectorOrderBy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetOrderBy(
            Expression<Func<TEntity, TOrderByKey>> storageSelectorOrderBy)
        {
            if (storageSelectorOrderBy == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorOrderBy)} can not be null");
            }
            _storageSelectorOrderBy = storageSelectorOrderBy;
            return this;
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            var nextInstances = (List<TEntity>)_newInstancesGenerator.Generate(context);
            Dictionary<TEntity, TEntity> existingInstancesCache = GetExistingInstancesCache(context, nextInstances);
            return CombineInstances(context, existingInstancesCache, nextInstances);
        }

        protected virtual Dictionary<TEntity, TEntity> GetExistingInstancesCache(GeneratorContext context, List<TEntity> nextInstances)
        {
            if (_existingInstancesCache == null)
            {
                var storageInstances = _persistentStorageSelector.Select(
                    _storageSelectorfilter, _storageSelectorOrderBy, 0, int.MaxValue);
                _existingInstancesCache = storageInstances.ToDictionary(x => x, _comparer);
            }

            return _existingInstancesCache;
        }

        protected virtual IList CombineInstances(GeneratorContext context, Dictionary<TEntity, TEntity> existingInstances, List<TEntity> nextInstances)
        {
            var combination = new List<TEntity>();
            var exists = new List<bool>();

            foreach (TEntity nextInstance in nextInstances)
            {
                TEntity existingInstance = null;
                bool exist = existingInstances.TryGetValue(nextInstance, out existingInstance);
                combination.Add(exist ? existingInstance : nextInstance);
                exists.Add(exist);
            }

            _newInstanceCounter.TrackNewInstances(context, exists);

            return combination;
        }


        //validation
        public virtual void ValidateEntitySettings(IEntityDescription entity)
        {
        }


        //IStorageInsertGuard methods
        public virtual void PreventInsertion(EntityContext entityContext, IList nextItems)
        {
            Dictionary<TEntity, TEntity> existingInstancesCache = _existingInstancesCache ?? throw new NullReferenceException(
                $"Method {nameof(PreventInsertion)} called before {nameof(Generate)} so {nameof(_existingInstancesCache)} was not initialized yet");

            for (int i = nextItems.Count - 1; i >= 0; i--)
            {
                TEntity nextItem = (TEntity)nextItems[i];
                bool drop = existingInstancesCache.ContainsKey(nextItem);
                if (drop)
                {
                    nextItems.RemoveAt(i);
                }
            }
        }


        //IFlushStrategy methods
        public virtual bool CheckIsFlushRequired(EntityContext entityContext, FlushRange flushRange)
        {
            EntityProgress progress = entityContext.EntityProgress;
            bool isFlushRequired = progress.CheckIsNewFlushRequired(flushRange);

            //clear cache after it is used in flush
            if (isFlushRequired)
            {
                //Improving memory usage here by removing previous history records.
                //But should not call this CheckIsFlushRequired method again untill actually will flush instances for this entity.
                _newInstanceCounter.RemoveHistoryRecords(flushRange.PreviousRangeFlushedCount);
            }

            return isFlushRequired;
        }

        public virtual void UpdateFlushRangeCapacity(EntityContext entityContext, FlushRange flushRange, int requestCapacity)
        {
            long newInstanceCount = _newInstanceCounter.GetNewInstanceCount(flushRange.PreviousRangeFlushedCount);
            bool isNewInstanceCountExceeded = newInstanceCount >= requestCapacity;

            //1. Compare to _maxInstancesInTempStorage capacity
            //Total instances count, including new and existing instances.
            //Even if it is not enough new instances to make full capacity insert, then still perform insert to get rid of existing instances in TempStorage.
            //2. Compare to requestCapacity
            //Check if new instances generated count is enough for full capacity insert request.
            flushRange.UpdateCapacity(isNewInstanceCountExceeded ? requestCapacity : _maxInstancesInTempStorage);
        }

    }
}
