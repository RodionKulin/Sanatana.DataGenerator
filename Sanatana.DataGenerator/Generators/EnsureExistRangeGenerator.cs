using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Collections;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGenerator.Generators
{
    public class EnsureExistRangeGenerator<TEntity, TOrderByKey> : IGenerator, IStorageInsertGuard, IFlushStrategy
        where TEntity : class
    {
        //config fields
        protected IGenerator _newInstancesGenerator;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected bool _isAscOrder = true;
        protected Func<TEntity, long> _idSelector = null;
        protected int _storageSelectorBatchSize = 1000;

        //state fields
        protected RangeSet<TEntity> _rangeSet;
        protected NewInstanceCounter _newInstanceCounter;


        //init
        public EnsureExistRangeGenerator(IGenerator newInstancesGenerator, Func<TEntity, long> idSelector)
        {
            _newInstancesGenerator = newInstancesGenerator ?? throw new ArgumentNullException(nameof(newInstancesGenerator));
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
            _rangeSet = new RangeSet<TEntity>(idSelector);
            _newInstanceCounter = new NewInstanceCounter();
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
            _newInstanceCounter.Setup();
        }


        //configure
        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="storageSelectorfilter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistRangeGenerator<TEntity, TOrderByKey> SetFilter(
            Expression<Func<TEntity, bool>> storageSelectorfilter)
        {
            if (storageSelectorfilter == null)
            {
                throw new ArgumentNullException(nameof(storageSelectorfilter));
            }
            _storageSelectorFilter = storageSelectorfilter;
            return this;
        }

        /// <summary>
        /// Set optional OrderBy expression to select existing entity instances with expected order.
        /// By default will select unordered instances.
        /// </summary>
        /// <param name="storageSelectorOrderBy"></param>
        /// <param name="isAscOrder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistRangeGenerator<TEntity, TOrderByKey> SetOrderBy(
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
            RangeSet<TEntity> existingInstancesCache = GetExistingInstancesCache(context, nextInstances);
            return CombineInstances(context, existingInstancesCache, nextInstances);
        }

        protected virtual RangeSet<TEntity> GetExistingInstancesCache(GeneratorContext context, List<TEntity> nextInstances)
        {
            long[] nextIds = nextInstances.Select(_idSelector).ToArray();
            bool nextIdsInCacheRange = _rangeSet.IsWithinKnownRange(nextIds);

            if (!nextIdsInCacheRange)
            {
                _rangeSet.RemoveItemsBeforeNewIds(nextIds);

                int skipNumber = (int)context.CurrentCount; //when invoking Generate method first time, it is 0
                long nextCount = context.TargetCount - context.CurrentCount;
                int takeNumber = Math.Min(_storageSelectorBatchSize, (int)nextCount);

                IPersistentStorageSelector persistentStorageSelector = context.Defaults.GetPersistentStorageSelector(context.Description);
                List<TEntity> storageInstances = persistentStorageSelector.Select(
                    _storageSelectorFilter, _storageSelectorOrderBy, _isAscOrder, skipNumber, takeNumber);

                _rangeSet.AddRange(storageInstances);
            }
            
            return _rangeSet;
        }

        protected virtual IList CombineInstances(GeneratorContext context, RangeSet<TEntity> existingInstances, List<TEntity> nextInstances)
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
            if (_newInstancesGenerator is IDelegateParameterizedGenerator)
            {
                //not a perfect solution, better to check all generic arguments
                Type[] typeArguments = newGenType.GetGenericArguments();
                if (typeof(TEntity) != typeArguments[0])
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
            IPersistentStorageSelector persistentStorageSelector = defaults.GetPersistentStorageSelector(entity);
            long storageCount = persistentStorageSelector.Count(_storageSelectorFilter);
            int maxCacheSize = 100000;
            if (storageCount > maxCacheSize)
            {
                throw new NotSupportedException($"Number of selectable instances of type {typeof(TEntity)} in persistent storage {storageCount} is larger then max cap of {maxCacheSize} instances. " +
                    $"This is a measure to prevent selecting too large datasets into inmemory cache. " +
                    $"Optionally can override {nameof(ValidateBeforeSetup)} method to disable this check.");
            }

            //validate inner generator
            _newInstancesGenerator.ValidateBeforeSetup(entity, defaults);
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
            //validate inner generator
            _newInstancesGenerator.ValidateAfterSetup(entityContext, defaults);
        }


        //IStorageInsertGuard methods
        public virtual void PreventInsertion(EntityContext entityContext, IList nextItems)
        {
            RangeSet<TEntity> existingInstancesCache = _rangeSet ?? throw new NullReferenceException(
                $"Method {nameof(PreventInsertion)} called before {nameof(Generate)} so {nameof(_rangeSet)} was not initialized yet");

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
                if (previousRangesEnd.Length > 0)
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
            //2. Compare to _storageSelectorBatchSize capacity
            //Total instances count, including new and existing instances.
            //Even if it is not enough new instances to make full capacity insert, then still perform insert to get rid of existing instances in TempStorage.

            flushRange.UpdateCapacity(isNewInstanceCountExceeded ? requestCapacity : _storageSelectorBatchSize);
        }

    }
}
