using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using Sanatana.DataGenerator.Internals.Comparers;

namespace Sanatana.DataGenerator.Generators
{
    public class EnsureExistRangeGenerator<TEntity, TOrderByKey> : IGenerator
        where TEntity : class
    {
        protected IGenerator _newInstancesGenerator;
        protected IEqualityComparer<TEntity> _comparer;
        protected IPersistentStorageSelector _storageSelector;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected Dictionary<TEntity, TEntity> _existingInstancesCache;
        protected Func<TEntity, long> _idSelector = null;
        protected int _storageSelectorBatchSize = 1000;
        protected long? _cacheMaxId = null;


        //init
        public EnsureExistRangeGenerator(IPersistentStorageSelector persistentStorageSelector,
            IGenerator newInstancesGenerator, Func<TEntity, long> idSelector)
        {
            _storageSelector = persistentStorageSelector ?? throw new ArgumentNullException(nameof(persistentStorageSelector));
            _newInstancesGenerator = newInstancesGenerator ?? throw new ArgumentNullException(nameof(newInstancesGenerator));
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
            _comparer = new IdEqualityComparer<TEntity>(idSelector);
            _existingInstancesCache = new Dictionary<TEntity, TEntity>(_comparer);
        }


        //setup
        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="storageSelectorfilter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual EnsureExistRangeGenerator<TEntity, TOrderByKey> SetFilter(Expression<Func<TEntity, bool>> storageSelectorfilter)
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
            Dictionary<TEntity, TEntity> existingInstancesCache = GetExistingInstancesCache(context, nextInstances);
            return CombineInstances(existingInstancesCache, nextInstances);
        }

        protected virtual Dictionary<TEntity, TEntity> GetExistingInstancesCache(GeneratorContext context, List<TEntity> nextInstances)
        {
            long[] nextIds = nextInstances.Select(_idSelector).ToArray();
            bool nextIdsInCacheRange = _cacheMaxId != null && nextIds.All(x => x <= _cacheMaxId);

            if (_existingInstancesCache == null || !nextIdsInCacheRange)
            {
                //remove previous instances from cache
                long nextIdsMinId = nextIds.Min();
                _existingInstancesCache = _existingInstancesCache.Values
                    .Where(x => _idSelector(x) >= nextIdsMinId)
                    .ToDictionary(x => x, _comparer);

                //insert next instances to cache
                int skipNumber = (int)context.CurrentCount; //when invoking Generate method first time, it is 0
                long nextCount = context.TargetCount - context.CurrentCount;
                int takeNumber = Math.Min(_storageSelectorBatchSize, (int)nextCount);

                List<TEntity> storageInstances = _storageSelector.Select(
                    _storageSelectorFilter, _storageSelectorOrderBy, skipNumber, takeNumber);
                foreach (var storageInstance in storageInstances)
                {
                    if (!_existingInstancesCache.ContainsKey(storageInstance))
                        _existingInstancesCache.Add(storageInstance, storageInstance);
                }
                
                _cacheMaxId = storageInstances.Select(_idSelector).Max();
            }
            
            return _existingInstancesCache;
        }

        protected virtual IList CombineInstances(Dictionary<TEntity, TEntity> existingInstances, List<TEntity> nextInstances)
        {
            var combination = new List<TEntity>();

            foreach (TEntity nextInstance in nextInstances)
            {
                TEntity existingInstance = null;
                bool exist = existingInstances.TryGetValue(nextInstance, out existingInstance);
                combination.Add(exist ? existingInstance : nextInstance);
            }

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
            long storageCount = _storageSelector.Count(_storageSelectorFilter);
            int maxCacheSize = 100000;
            if (storageCount > maxCacheSize)
            {
                throw new NotSupportedException($"Number of selectable instances of type {typeof(TEntity)} in persistent storage {storageCount} is larger then max cap of {maxCacheSize} instances. " +
                    $"This is a measure to prevent selecting too large datasets into inmemory cache. " +
                    $"Optionally can override {nameof(ValidateEntitySettings)} method to disable this check.");
            }
        }
    }
}
