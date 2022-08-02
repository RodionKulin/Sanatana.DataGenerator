using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.TargetCountProviders;
using System.Linq.Expressions;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// Generator that provides existing entity instances from persistent storage instead of creating new.
    /// Such existing entities can be used to populate foreign key of other entities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TOrderByKey"></typeparam>
    public class ReuseExistingGenerator<TEntity, TOrderByKey> : IGenerator
        where TEntity : class
    {
        protected int _storageSelectorBatchSize = 1000;
        protected Expression<Func<TEntity, bool>> _storageSelectorFilter = (entity) => true;
        protected Expression<Func<TEntity, TOrderByKey>> _storageSelectorOrderBy = null;
        protected bool _isAscOrder;


        //setup method
        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
        }


        //configuration methods
        /// <summary>
        /// Set batch size of entity instances to select from persistent storage.
        /// By default will select 1000 instances.
        /// </summary>
        /// <param name="storageSelectorBatchSize"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetBatchSize(int storageSelectorBatchSize = 1000)
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
        /// <param name="isAscOrder"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual ReuseExistingGenerator<TEntity, TOrderByKey> SetOrderBy(
            Expression<Func<TEntity, TOrderByKey>> storageSelectorOrderBy, bool isAscOrder)
        {
            if (storageSelectorOrderBy == null)
            {
                throw new ArgumentOutOfRangeException($"Parameter {nameof(storageSelectorOrderBy)} can not be null");
            }
            _storageSelectorOrderBy = storageSelectorOrderBy;
            _isAscOrder = isAscOrder;
            return this;
        }


        //validation        
        public virtual void ValidateBeforeSetup(IEntityDescription description, DefaultSettings defaults)
        {
            string generatorName = $"Generator of type {nameof(ReuseExistingGenerator<TEntity, TOrderByKey>)} for entity {description.Type.FullName}";

            if (description.Required != null && description.Required.Count > 0)
            {
                string requiredTypes = description.Required
                    .Select(x => x.Type.FullName)
                    .Aggregate((x, y) => $"{x},{y}");
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.Required)} list, but provided {requiredTypes}");
            }

        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
            //should invoke this validation after GeneratorServices.SetupSpreadStrategy() to support CombinatoricsSreadStrategy

            IEntityDescription description = entityContext.Description;
            string generatorName = $"Generator of type {nameof(ReuseExistingGenerator<TEntity, TOrderByKey>)} for entity {description.Type.FullName}";

            IPersistentStorageSelector persistentStorageSelector = defaults.GetPersistentStorageSelector(description);
            long targetCount = entityContext.EntityProgress.TargetCount;
            long storageCount = persistentStorageSelector.Count(_storageSelectorFilter);
            if (targetCount > storageCount)
            {
                throw new NotSupportedException($"{generatorName} returned not supported value from {nameof(description.TargetCountProvider.GetTargetCount)} {targetCount} that is higher, then number of selectable instances in persistent storage {storageCount}. " +
                    $"Possible solutions: " +
                    $"1. Make sure that persistent storage rows count is not changed during setup. " +
                    $"2. Use {nameof(CountExistingTargetCountProvider<TEntity>)} to provide TargetCount matching rows count in persistent storage." +
                    $"3. Make sure that same {nameof(_storageSelectorFilter)} is provided to {nameof(description.TargetCountProvider)} and {typeof(ReuseExistingGenerator<,>).Name}. "
                );
            }

            if (targetCount > int.MaxValue)
            {
                throw new NotSupportedException($"{generatorName} does not support {nameof(description.TargetCountProvider.GetTargetCount)} value of {targetCount} that is higher then int.MaxValue to support Skip and Take System.Linq parameters that expect int value.");
            }
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            int skipNumber = (int)context.CurrentCount; //when invoking Generate method first time, it is 0
            long nextCount = context.TargetCount - context.CurrentCount;
            int takeNumber = Math.Min(_storageSelectorBatchSize, (int)nextCount);

            IPersistentStorageSelector persistentStorageSelector = context.Defaults.GetPersistentStorageSelector(context.Description);
            return persistentStorageSelector.Select(_storageSelectorFilter, _storageSelectorOrderBy, _isAscOrder, skipNumber, takeNumber);
        }
    }
}
