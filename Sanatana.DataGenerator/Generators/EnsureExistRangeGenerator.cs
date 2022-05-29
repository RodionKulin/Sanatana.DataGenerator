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
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected Expression<Func<TEntity, bool>> _storageSelectorfilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected Dictionary<TEntity, TEntity> _existingInstancesCache;
        protected Func<TEntity, long> _idSelector = null;
        protected int _storageSelectorBatchSize = 1000;
        protected long? _cacheMaxId = null;


        //init
        public EnsureExistRangeGenerator(IPersistentStorageSelector persistentStorageSelector,
            IGenerator newInstancesGenerator, Func<TEntity, long> idSelector)
        {
            _persistentStorageSelector = persistentStorageSelector;
            _newInstancesGenerator = newInstancesGenerator;
            _comparer = new IdEqualityComparer<TEntity>(idSelector);
            _idSelector = idSelector;
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
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorfilter)} can not be null");
            }
            _storageSelectorfilter = storageSelectorfilter;
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

                List<TEntity> storageInstances = _persistentStorageSelector.Select(
                    _storageSelectorfilter, _storageSelectorOrderBy, skipNumber, takeNumber);
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
        }
    }
}
