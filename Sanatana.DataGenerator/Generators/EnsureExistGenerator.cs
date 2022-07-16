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
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.Comparers;

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
        //config fields
        protected IGenerator _newInstancesGenerator;
        protected IEqualityComparer<TEntity> _comparer;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected bool _isAscOrder;
        protected int _maxSelectableInstances = 10000;

        //state fields
        protected Dictionary<TEntity, TEntity> _existingInstancesCache;
        protected NewInstanceCounter _newInstanceCounter;


        //init
        public EnsureExistGenerator(IGenerator newInstancesGenerator)
        {
            _newInstancesGenerator = newInstancesGenerator ?? throw new ArgumentNullException(nameof(newInstancesGenerator));
            _newInstanceCounter = new NewInstanceCounter();
        }


        //configure methods
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
        /// <param name="isAscOrder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetOrderBy(
            Expression<Func<TEntity, TOrderByKey>> storageSelectorOrderBy, bool isAscOrder = true)
        {
            if (storageSelectorOrderBy == null)
            {
                throw new ArgumentNullException(nameof(storageSelectorOrderBy));
            }
            _storageSelectorOrderBy = storageSelectorOrderBy;
            _isAscOrder = isAscOrder;
            return this;
        }

        /// <summary>
        /// Set comparer, that will be used to find existing entity instances in persistent storage.
        /// </summary>
        /// <param name="comparer"></param>
        /// <returns></returns>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetComparer(
            IEqualityComparer<TEntity> comparer)
        {
            _comparer = comparer;
            return this;
        }

        /// <summary>
        /// Set comparer by key, that will be used to find existing entity instances in persistent storage.
        /// </summary>
        /// <typeparam name="TCompareKey"></typeparam>
        /// <param name="keyExtractor"></param>
        /// <returns></returns>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetComparer<TCompareKey>(
            Func <TEntity, TCompareKey> keyExtractor)
        {
            _comparer = new KeyEqualityComparer<TEntity, TCompareKey>(keyExtractor);
            return this;
        }

        /// <summary>
        /// Set comparer by key, that will be used to find existing entity instances in persistent storage.
        /// Will use StrictKeyEqualityComparer that is slightly more performant than KeyEqualityComparer, because will not use unboxing to compare keys.
        /// </summary>
        /// <typeparam name="TCompareKey"></typeparam>
        /// <param name="keyExtractor"></param>
        /// <returns></returns>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetComparerEquatable<TCompareKey>(
            Func<TEntity, TCompareKey> keyExtractor)
            where TCompareKey : IEquatable<TCompareKey>
        {
            _comparer = new StrictKeyEqualityComparer<TEntity, TCompareKey>(keyExtractor);
            return this;
        }

        /// <summary>
        /// Set max number of instances that can be selected from persistent storage.
        /// There will be only single request to select all instances after filtering.
        /// If number of instances will be higher, then maxSelectableInstances, then will throw an error.
        /// This is a measure to prevent selecting too large datasets into inmemory cache.
        /// By default the limit is 10000 instances after filtering.
        /// </summary>
        /// <param name="maxSelectableInstances"></param>
        /// <returns></returns>
        public virtual EnsureExistGenerator<TEntity, TOrderByKey> SetMaxSelectableInstances(
           int maxSelectableInstances = 10000)
        {
            _maxSelectableInstances = maxSelectableInstances;
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
                IPersistentStorageSelector persistentStorageSelector = context.Defaults.GetPersistentStorageSelector(context.Description);
                List<TEntity> storageInstances = persistentStorageSelector.Select(
                    _storageSelectorFilter, _storageSelectorOrderBy, _isAscOrder, 0, int.MaxValue);

                IEqualityComparer<TEntity> comparer = _comparer 
                    ?? context.Defaults.GetDefaultEqualityComparer<TEntity>(context.Description);
                _existingInstancesCache = storageInstances.ToDictionary(x => x, comparer);
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
        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
            //check generic type of _newInstancesGenerator Generator
            Type newGenType = _newInstancesGenerator.GetType();
            if (newGenType is IDelegateParameterizedGenerator)
            {
                //not a perfect solution, better to check all generic arguments
                Type[] typeArguments = newGenType.GetGenericArguments();
                if(typeof(TEntity) != typeArguments[0])
                {
                    throw new NotSupportedException($"newInstancesGenerator with generic argument {typeArguments[0].FullName} should produce same entity type {typeof(TEntity).FullName}");
                }
            }

            //validate that comparer is set
            if(_comparer == null)
            {
                //this method throws exception if comparer not set
                defaults.GetDefaultEqualityComparer<TEntity>(entity);
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
            IPersistentStorageSelector persistentStorageSelector = defaults.GetPersistentStorageSelector(entity);
            long storageCount = persistentStorageSelector.Count(_storageSelectorFilter);
            if (storageCount > _maxSelectableInstances)
            {
                throw new NotSupportedException($"Number of selectable instances of type {typeof(TEntity)} in persistent storage {storageCount} is larger then max cap of {_maxSelectableInstances} instances to select. " +
                    $"This is a measure to prevent selecting too large datasets into inmemory cache. " +
                    $"Optionally can increase this cap in {nameof(SetMaxSelectableInstances)} method.");
            }
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults) { }


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

            if (isFlushRequired)
            {
                //Improving memory usage here by removing previous history records, already flushed.
                long[] previousRangesEnd = entityContext.EntityProgress.FlushRanges
                    .Select(x => x.PreviousRangeFlushedCount)
                    .ToArray();
                if(previousRangesEnd.Length > 0)
                {  
                    long lowestRangeBound = previousRangesEnd.Min();
                    _newInstanceCounter.RemoveHistoryRecords(lowestRangeBound);
                }
            }

            return isFlushRequired;
        }

        public virtual void UpdateFlushRangeCapacity(EntityContext entityContext, FlushRange flushRange, int requestCapacity)
        {
            long newInstanceCount = _newInstanceCounter.GetNewInstanceCount(flushRange.PreviousRangeFlushedCount);
            bool isNewInstanceCountExceeded = newInstanceCount >= requestCapacity;

            //1. Compare to requestCapacity
            //Check if new instances generated count is enough for full capacity insert request.
            //2. Compare to _maxSelectableInstances capacity
            //Total instances count, including new and existing instances.
            //Even if it is not enough new instances to make full capacity insert, then still perform insert to get rid of existing instances in TempStorage.

            flushRange.UpdateCapacity(isNewInstanceCountExceeded ? requestCapacity : _maxSelectableInstances);
        }

    }
}
