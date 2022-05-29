using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.TotalCountProviders;
using System.Linq.Expressions;

namespace Sanatana.DataGenerator.Generators
{
    public class ReuseExistingGenerator<TEntity, TOrderByKey> : IGenerator
        where TEntity : class
    {
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected IList _nextEntitiesBatch;
        protected int _storageSelectorBatchSize = 1000;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;


        //init
        public ReuseExistingGenerator(IPersistentStorageSelector persistentStorageSelector)
        {
            _persistentStorageSelector = persistentStorageSelector;
        }


        //setup methods
        /// <summary>
        /// Set batch size of selected entity instances from persistent storage.
        /// By default will select 1000 instances.
        /// </summary>
        /// <param name="storageSelectorBatchSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetBatchSize(int storageSelectorBatchSize)
        {
            if(storageSelectorBatchSize < 1)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorBatchSize)} can not be less then 1");
            }
            _storageSelectorBatchSize = storageSelectorBatchSize;
            return this;
        }

        /// <summary>
        /// Set batch size int.MaxValue to select all instances with single request.
        /// </summary>
        /// <returns></returns>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetBatchSizeMax()
        {
            _storageSelectorBatchSize = int.MaxValue;
            return this;
        }

        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="storageSelectorFilter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetFilter(
            Expression<Func<TEntity, bool>> storageSelectorFilter)
        {
            if (storageSelectorFilter == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorFilter)} can not be null");
            }
            _storageSelectorFilter = storageSelectorFilter;
            return this;
        }

        /// <summary>
        /// Set optional OrderBy expression to select existing entity instances with expected order.
        /// By default will select unordered instances.
        /// </summary>
        /// <param name="storageSelectorOrderBy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetOrderBy(
            Expression<Func<TEntity, TOrderByKey>> storageSelectorOrderBy)
        {
            if (storageSelectorOrderBy == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorOrderBy)} can not be null");
            }
            _storageSelectorOrderBy = storageSelectorOrderBy;
            return this;
        }

        /// <summary>
        /// Set persistent storage that will provide existing entity instances.
        /// </summary>
        /// <param name="persistentStorageSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetPersistentStorage(IPersistentStorageSelector persistentStorageSelector)
        {
            if (persistentStorageSelector == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(persistentStorageSelector)} can not be null");
            }
            _persistentStorageSelector = persistentStorageSelector;
            return this;
        }


        //validation
        public virtual void ValidateSetup()
        {
            if (_persistentStorageSelector == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(_persistentStorageSelector)} can not be null. To set it use method {nameof(SetPersistentStorage)}.");
            }
        }
        
        public virtual void ValidateEntitySettings(IEntityDescription description)
        {
            string generatorName = $"Generator of type {nameof(ReuseExistingGenerator<TEntity, TOrderByKey>)} for entity {description.Type.FullName}";

            if (description.Required != null && description.Required.Count > 0)
            {
                string requiredTypes = description.Required
                    .Select(x => x.Type.FullName)
                    .Aggregate((x, y) => $"{x},{y}");
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.Required)} list, but provided {requiredTypes}");
            }

            long targetCount = description.TotalCountProvider.GetTargetCount();
            long storageCount = _persistentStorageSelector.Count(_storageSelectorFilter);
            if(targetCount > storageCount)
            {
                throw new NotSupportedException($"{generatorName} returned not supported value from {nameof(description.TotalCountProvider.GetTargetCount)} {targetCount} that is higher, then number of selectable instances in persistent storage {storageCount}. " +
                    $"Possible solutions: 1. Make sure that same {nameof(_storageSelectorFilter)} is provided. " +
                    $"2. Make sure that persistent storage is not changing during generation. " +
                    $"3. Use {nameof(CountExistingTotalCountProvider<TEntity>)} to provide total count.");
            }

            if(targetCount > int.MaxValue)
            {
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.TotalCountProvider.GetTargetCount)} value of {targetCount} that is higher then int.MaxValue to support Skip and Take System.Linq parameters that expect int value.");
            }
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            if (_nextEntitiesBatch == null)
            {
                int skipNumber = (int)context.CurrentCount; //when invoking Generate method first time, it is 0
                long nextCount = context.TargetCount - context.CurrentCount;
                int takeNumber = Math.Min(_storageSelectorBatchSize, (int)nextCount);
                _nextEntitiesBatch = _persistentStorageSelector.Select(_storageSelectorFilter, _storageSelectorOrderBy, skipNumber, takeNumber);
            }

            return _nextEntitiesBatch;
        }
    }
}
