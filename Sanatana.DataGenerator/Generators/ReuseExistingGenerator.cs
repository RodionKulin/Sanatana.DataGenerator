using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.QuantityProviders;

namespace Sanatana.DataGenerator.Generators
{
    public class ReuseExistingGenerator<TEntity> : IGenerator
        where TEntity : class
    {
        protected IPersistentStorageSelector _persistentStorageSelector;
        protected IList _nextEntitiesBatch;
        protected int _batchSize = 1000;
        protected Func<TEntity, bool> _filter = (entity) => true;


        //setup methods
        /// <summary>
        /// Set batch size of selected entity instances from persistent storage.
        /// By default will select 1000 instances.
        /// </summary>
        /// <param name="batchSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ReuseExistingGenerator<TEntity> SetBatchSize(int batchSize)
        {
            if(batchSize < 1)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(batchSize)} can not be less then 1");
            }
            _batchSize = batchSize;
            return this;
        }

        /// <summary>
        /// Set batch size int.MaxValue to select all instances with single request.
        /// </summary>
        /// <returns></returns>
        public ReuseExistingGenerator<TEntity> SetBatchSizeMax()
        {
            _batchSize = int.MaxValue;
            return this;
        }

        /// <summary>
        /// Set optional filter expression to select existing entity instances from persistent storage.
        /// By default will include all instances.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ReuseExistingGenerator<TEntity> SetFilter(Func<TEntity, bool> filter)
        {
            if (filter == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(filter)} can not be null");
            }
            _filter = filter;
            return this;
        }

        /// <summary>
        /// Set persistent storage that will provide existing entity instances.
        /// </summary>
        /// <param name="persistentStorageSelector"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public ReuseExistingGenerator<TEntity> SetPersistentStorage(IPersistentStorageSelector persistentStorageSelector)
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
            string generatorName = $"Generator of type {nameof(ReuseExistingGenerator<TEntity>)} for entity {description.Type.FullName}";

            if (description.Required != null && description.Required.Count > 0)
            {
                string requiredTypes = description.Required
                    .Select(x => x.Type.FullName)
                    .Aggregate((x, y) => $"{x},{y}");
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.Required)} list, but provided {requiredTypes}");
            }

            long targetQuantity = description.QuantityProvider.GetTargetQuantity();
            long storageQuantity = _persistentStorageSelector.Count(_filter);
            if(targetQuantity > storageQuantity)
            {
                throw new NotSupportedException($"{generatorName} returned not supported value from {nameof(description.QuantityProvider.GetTargetQuantity)} {targetQuantity} that is higher, then number of selectable instances in persistent storage {storageQuantity}. " +
                    $"Possible solutions: 1. Make sure that same {nameof(_filter)} is provided. " +
                    $"2. Make sure that persistent storage is not changing during generation. " +
                    $"3. Use {nameof(CountExistingQuantityProvider<TEntity>)} to provide total count.");
            }

            if(targetQuantity > int.MaxValue)
            {
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.QuantityProvider.GetTargetQuantity)} value of {targetQuantity} that is higher then int.MaxValue to support Skip and Take System.Linq parameters that expect int value.");
            }
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            if (_nextEntitiesBatch == null)
            {
                int skipNumber = (int)context.CurrentCount; //when invoking Generate method first time, it is 0
                long nextCount = context.TargetCount - context.CurrentCount;
                int takeNumber = Math.Min(_batchSize, (int)nextCount);
                _nextEntitiesBatch = _persistentStorageSelector.Select(_filter, skipNumber, takeNumber);
            }

            return _nextEntitiesBatch;
        }
    }
}
