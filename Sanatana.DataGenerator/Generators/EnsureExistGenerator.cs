using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// Generator that provides new instance only if it does not exist in PersistentStorage yet.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TOrderByKey"></typeparam>
    public class EnsureExistGenerator<TEntity, TOrderByKey> : IGenerator, IStorageInsertGuard, IFlushStrategy
        where TEntity : class
    {
        //fields
        protected IGenerator _newInstancesGenerator;
        protected IEqualityComparer<TEntity> _comparer;
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected Dictionary<TEntity, TEntity> _existingInstancesCache;
        protected NewInstanceCounter _newInstanceCounter;
        protected int _maxInstancesInTempStorage = 10000;



        //init
        public EnsureExistGenerator(IPersistentStorageSelector persistentStorageSelector, 
            IGenerator newInstancesGenerator, IEqualityComparer<TEntity> comparer)
        {
            _persistentStorageSelector = persistentStorageSelector ?? throw new ArgumentNullException(nameof(persistentStorageSelector));
            _newInstancesGenerator = newInstancesGenerator ?? throw new ArgumentNullException(nameof(newInstancesGenerator));
            _comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            _newInstanceCounter = new NewInstanceCounter();
        }


        //setup
        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="storageSelectorFilter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetFilter(
            Expression<Func<TEntity, bool>> storageSelectorFilter)
        {
            if (storageSelectorFilter == null)
            {
                throw new ArgumentNullException(nameof(storageSelectorFilter));
            }
            _storageSelectorFilter = storageSelectorFilter;
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
                throw new ArgumentNullException(nameof(storageSelectorOrderBy));
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
                    _storageSelectorFilter, _storageSelectorOrderBy, 0, int.MaxValue);
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
                bool exist = existingInstances.TryGetValue(nextInstance, out TEntity existingInstance);
                combination.Add(exist ? existingInstance : nextInstance);
                exists.Add(exist);
            }

            _newInstanceCounter.TrackNewInstances(context, exists);

            return combination;
        }


        //validation
        public virtual void ValidateEntitySettings(IEntityDescription entity)
        {
            //check generic type of _newInstancesGenerator Generator
            Type newGenType = _newInstancesGenerator.GetType();
            if (typeof(DelegateParameterizedGenerator<>).IsAssignableFrom(newGenType))
            {
                //not a perfect solution to check only first generic argument, better check all
                Type[] typeArguments = newGenType.GetGenericArguments();
                if(typeof(TEntity) != typeArguments[0])
                {
                    throw new NotSupportedException($"newInstancesGenerator with generic argument {typeArguments[0].FullName} should produce same entity type {typeof(TEntity).FullName}");
                }
            }

            //_newInstancesGenerator Generator should only generate new instances. Other scenarios may be supported, but not tested yet.
            Type[] notSupportedInnerTypes = new[]
            {
                typeof(EnsureExistGenerator<,>),
                typeof(EnsureExistRangeGenerator<,>),
                typeof(ReuseExistingGenerator<,>),
            };
            bool hasNotSupportedInnerTypes = notSupportedInnerTypes
                .Select(type => type.IsAssignableFrom(newGenType))
                .Any(x => x == true);
            if (hasNotSupportedInnerTypes)
            {
                string notSupportedJoined = string.Join(", ", notSupportedInnerTypes.Select(x => x.FullName));
                throw new NotSupportedException($"newInstancesGenerator should produce new instances and should not be assignable from {notSupportedJoined}");
            }

            //check count of instances in PersistentStorage to prevent selecting to large number
            long storageCount = _persistentStorageSelector.Count(_storageSelectorFilter);
            int maxCacheSize = 100000;
            if (storageCount > maxCacheSize)
            {
                throw new NotSupportedException($"Number of selectable instances of type {typeof(TEntity)} in persistent storage {storageCount} is larger then max cap of {maxCacheSize} instances. " +
                    $"This is a measure to prevent selecting too large datasets into inmemory cache. " +
                    $"Optionally can override {nameof(ValidateEntitySettings)} method to disable this check.");
            }
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
