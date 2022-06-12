using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Modifiers;
using System.Linq.Expressions;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Entities
{
    /// <summary>
    /// Entity configuration for generation process
    /// </summary>
    public class EntityDescription<TEntity> : IEntityDescription
            where TEntity : class
    {
        #region Properties
        /// <summary>
        /// Type of entity to generate
        /// </summary>
        public Type Type
        {
            get
            {
                return typeof(TEntity);
            }
        }
        /// <summary>
        /// Entities that are needed for generator as foreign keys or other type of dependency.
        /// </summary>
        public List<RequiredEntity> Required { get; set; }
        /// <summary>
        /// Entity instances generator. Can return instances one by one or in small batches. 
        /// Usually better to return single instance not to store extra instances in memory.
        /// Be default will use DefaultGenerator from GeneratorSetup.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// List of methods to make adjustments to entity instance after generation.
        /// Be default will use DefaultModifiers from GeneratorSetup.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Database storages for generated entities.
        /// Be default will use DefaultPersistentStorages from GeneratorSetup.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Provider of total number of entity instances that need to be generated.
        /// Be default will use DefaultTotalCountProvider from GeneratorSetup.
        /// </summary>
        public ITotalCountProvider TotalCountProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entities to database.
        /// Be default will use DefaultFlushStrategy from GeneratorSetup.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Provider of number of entity instances that can be inserted with next request to persistent storage.
        /// Be default will use DefaultRequestCapacityProvider from GeneratorSetup.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Checker of entity instances to be inserted into database. 
        /// Excludes unwanted instances, like the ones that already exist in database for EnsureExistGenerator.
        /// By default is not used.
        /// </summary>
        public IStorageInsertGuard StorageInsertGuard { get; set; }
        /// <summary>
        /// Get database generated columns after inserting entities first (for example Id).
        /// Only after receiving such columns pass entity instances as required for generation.
        /// Also makes insert requests to persistent storage sync for this entity.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }
        #endregion


        //init
        public EntityDescription()
        {
            Required = new List<RequiredEntity>();
            PersistentStorages = new List<IPersistentStorage>();
            Modifiers = new List<IModifier>();
        }


        #region Configure methods
        /// <summary>
        /// Add required entity type that will be generated first and then pasted as parameter to generator.
        /// </summary>
        /// <param name="requiredEntity"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetRequired(RequiredEntity requiredEntity)
        {
            if (requiredEntity == null)
            {
                throw new ArgumentNullException(nameof(requiredEntity));
            }

            bool hasType = Required.Any(x => x.Type == requiredEntity.Type);
            if (hasType)
            {
                string message = $"Type {requiredEntity.Type.FullName} already registered among required types for {typeof(TEntity).FullName}.";
                throw new ArgumentException(message, nameof(requiredEntity));
            }

            Required.Add(requiredEntity);
            return this;
        }

        /// <summary>
        /// Add required entity type that will be generated first and then pasted as parameter to generator.
        /// </summary>
        /// <param name="requiredType">Type of foreign key entity</param>
        /// <param name="spreadStrategy">Distribution handler that defines how many times same foreign key entity can be reused</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetRequired(Type requiredType, ISpreadStrategy spreadStrategy = null)
        {
            var requiredEntity = new RequiredEntity
            {
                Type = requiredType,
                SpreadStrategy = spreadStrategy
            };

            return SetRequired(requiredEntity);
        }

        protected virtual void SetRequiredFromGenerator(Type delegateType)
        {
            Type[] genericArgs = delegateType.GetGenericArguments();
            List<Type> argumentTypes = genericArgs
                .Skip(1)                        //the first one is GeneratorContext
                .Take(genericArgs.Length - 2)   //and the last argument is result
                .ToList();                      //the rest are required entities

            foreach (Type requiredType in argumentTypes)
            {
                var requiredEntity = new RequiredEntity
                {
                    Type = requiredType
                };

                SetRequired(requiredEntity);
            }
        }

        /// <summary>
        /// Set custom SpreadStrategy for Required Entity. 
        /// Required Entity should already be registered when calling this method.        /// 
        /// </summary>
        /// <param name="requiredType"></param>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategy(
            Type requiredType, ISpreadStrategy spreadStrategy)
        {
            if (spreadStrategy == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(spreadStrategy)}] of {nameof(SetSpreadStrategy)} can not be null.");
            }

            RequiredEntity requiredEntity = Required.FirstOrDefault(x => x.Type == requiredType);
            if (requiredEntity == null)
            {
                string message = $"Type {requiredType.FullName} not found among required entities for {typeof(TEntity).FullName}." + 
                    $" Consider registering it with {nameof(SetRequired)} method or using as {typeof(DelegateParameterizedGenerator<>)} argument to auto register." +
                    $" Generator should be registered before {nameof(SetSpreadStrategy)} method call in later case.";
                throw new ArgumentException(message, nameof(requiredType));
            }

            requiredEntity.SpreadStrategy = spreadStrategy;

            return this;
        }

        /// <summary>
        /// Set custom SpreadStrategy for all Required Entities. 
        /// Required Entities should already be registered when calling this method.
        /// All Required Entities registered after this method call will not be effected and will have default SpreadStrategy value.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategy(ISpreadStrategy spreadStrategy)
        {
            if (spreadStrategy == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(spreadStrategy)}] of {nameof(SetSpreadStrategy)} can not be null.");
            }

            if (Required.Count == 0)
            {
                throw new NotSupportedException($"Was not able to set {nameof(ISpreadStrategy)}. No {nameof(Required)} entities were configured on type {Type.FullName}.");
            }

            foreach (RequiredEntity requiredEntity in Required)
            {
                requiredEntity.SpreadStrategy = spreadStrategy;
            }

            return this;
        }

        /// <summary>
        /// Call SetSpreadStrategy on Required entities and set TargetCount as total number of distinct combinations that can be generated.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <param name="generatorSetup"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategyAndTargetCount(
            CombinatoricsSpreadStrategy spreadStrategy, GeneratorSetup generatorSetup)
        {
            SetSpreadStrategy(spreadStrategy);

            //Get subset of all entities that are required
            var requiredTypes = Required.Select(x => x.Type);
            Dictionary<Type, IEntityDescription> requiredDescriptions = generatorSetup.EntityDescriptions
                .Where(x => requiredTypes.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);

            //validate
            var validator = new Validator(generatorSetup);
            validator.CheckGeneratorSetupComplete(requiredDescriptions);

            //Setup spread strategy
            var entityContext = new EntityContext
            {
                Description = this,
                EntityProgress = new EntityProgress(),
                Type = Type
            };
            Dictionary<Type, EntityContext> requiredEntities = generatorSetup.SetupEntityContexts(requiredDescriptions);
            spreadStrategy.Setup(entityContext, requiredEntities);

            //Get total count of combinations
            long totalCount = spreadStrategy.GetTotalCount();
            SetTargetCount(totalCount);

            return this;
        }

        /// <summary>
        /// Set total number of entities that need to be generated.
        /// </summary>
        /// <param name="totalCountProvider"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetTargetCount(ITotalCountProvider totalCountProvider)
        {
            if (totalCountProvider == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(totalCountProvider)}] of {nameof(SetTargetCount)} can not be null.");
            }

            TotalCountProvider = totalCountProvider;
            return this;
        }

        /// <summary>
        /// Set total number of entities that need to be generated.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetTargetCount(long count)
        {
            TotalCountProvider = new StrictTotalCountProvider(count);
            return this;
        }

        /// <summary>
        /// Add database storage provider that will receive generated entity instances.
        /// Multiple storages can be used. Will insert instances to each of them.
        /// </summary>
        /// <param name="persistentStorage"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddPersistentStorage(IPersistentStorage persistentStorage)
        {
            if (persistentStorage == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(persistentStorage)}] of {nameof(AddPersistentStorage)} can not be null.");
            }
            
            PersistentStorages.Add(persistentStorage);
            return this;
        }

        /// <summary>
        /// Add database storage provider that will receive generated entity instances.
        /// Multiple storages can be used. Will insert instances to each of them.
        /// </summary>
        /// <param name="insertFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddPersistentStorage(Func<List<TEntity>, Task> insertFunc)
        {
            if (insertFunc == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(insertFunc)}] of {nameof(AddPersistentStorage)} can not be null.");
            }

            PersistentStorages.Add(new DelegatePersistentStorage(insertFunc));
            return this;
        }

        /// <summary>
        /// Set entity persistent storage write trigger, that signals a required flush.
        /// </summary>
        /// <param name="flushStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetFlushStrategy(IFlushStrategy flushStrategy)
        {
            if (flushStrategy == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(flushStrategy)}] of {nameof(SetFlushStrategy)} can not be null.");
            }

            FlushStrategy = flushStrategy;
            return this;
        }

        /// <summary>
        /// Set RequestCapacityProvider that returns number of entity instances that can be inserted with next request to persistent storage.
        /// </summary>
        /// <param name="requestCapacityProvider"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetRequestCapacityProvider(IRequestCapacityProvider requestCapacityProvider)
        {
            if (requestCapacityProvider == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(requestCapacityProvider)}] of {nameof(SetRequestCapacityProvider)} can not be null.");
            }

            RequestCapacityProvider = requestCapacityProvider;
            return this;
        }

        /// <summary>
        /// Set StrictRequestCapacityProvider that returns static capacity number of entity instances that can be inserted with next request to persistent storage.
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetRequestCapacityProvider(int capacity)
        {
            RequestCapacityProvider = new StrictRequestCapacityProvider(capacity);
            return this;
        }

        /// <summary>
        /// Set checker of entity instances to be inserted into database. Excludes unwanted instances, like the ones that already exist in database.
        /// </summary>
        /// <param name="storageInsertGuard"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetStorageInsertGuard(IStorageInsertGuard storageInsertGuard)
        {
            StorageInsertGuard = storageInsertGuard;
            return this;
        }

        /// <summary>
        /// Set InsertToPersistentStorageBeforeUse.
        /// Get database generated columns like Id after inserting entities first. 
        /// Than only pass generated entities as required.
        /// Default is false.
        /// </summary>
        /// <param name="insertToPersistentStorageBeforeUse"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetInsertToPersistentStorageBeforeUse(
            bool insertToPersistentStorageBeforeUse)
        {
            InsertToPersistentStorageBeforeUse = insertToPersistentStorageBeforeUse;
            return this;
        }
        #endregion


        #region Generator that reuses instances from persistent storage
        /// <summary>
        /// Generator that provides existing entity instances from persistent storage instead of creating new.
        /// Such existing entities can be used to populate foreign key of other entities.
        /// Will use VoidStorage as PersistentStorage to prevent inserting already existing instances to persistent storage.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetReuseExistingGenerator<TOrderByKey>(ReuseExistingGenerator<TEntity, TOrderByKey> generator)
        {
            generator = generator ?? throw new ArgumentNullException(nameof(generator));
            generator.ValidateSetup();
            Generator = generator;

            PersistentStorages.Add(new VoidStorage());

            return this;
        }

        /// <summary>
        /// Generator that provides existing entity instances from persistent storage instead of creating new.
        /// Will use VoidStorage as PersistentStorage to prevent inserting already existing instances to persistent storage.
        /// Will set CountExistingTotalCountProvider as TotalCountProvider to select all instances.
        /// </summary>
        /// <param name="storageSelector">Persistent storage that will provide existing entity instances.</param>
        /// <param name="filter">Optional filter expression to select existing entity instances from persistent storage. By default will include all instances.</param>
        /// <param name="orderBy">Optional OrderBy expression to select existing entity instances with expected order. By default will select unordered instances.</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual EntityDescription<TEntity> SetReuseExistingGeneratorForAll<TOrderByKey>(
            IPersistentStorageSelector storageSelector,
            Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, TOrderByKey>> orderBy = null)
        {
            if (filter == null)
            {
                filter = (entity) => true;
            }

            var generator = new ReuseExistingGenerator<TEntity, TOrderByKey>(storageSelector)
                .SetBatchSizeMax()
                .SetFilter(filter);
            if(orderBy != null)
            {
                generator.SetOrderBy(orderBy);
            }

            generator.ValidateSetup();
            Generator = generator;

            PersistentStorages.Add(new VoidStorage());
            TotalCountProvider = new CountExistingTotalCountProvider<TEntity>(storageSelector, filter);

            return this;
        }

        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey>(
            EnsureExistGenerator<TEntity, TOrderByKey> generator)
        {
            generator = generator ?? throw new ArgumentNullException(nameof(generator));

            Generator = generator;
            FlushStrategy = generator;
            StorageInsertGuard = generator;

            return this;
        }

        #endregion


        #region Generator that creates new entity instances
        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(IGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(generator)}] of {nameof(SetGenerator)} can not be null.");
            }

            Generator = generator;
            return this;
        }

        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(Func<GeneratorContext, TEntity> generateFunc)
        {
            if (generateFunc == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(generateFunc)}] of {nameof(SetGenerator)} can not be null.");
            }

            Generator = DelegateGenerator<TEntity>.Factory.Create(generateFunc);
            return this;
        }

        

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1>(
            Func<GeneratorContext, T1, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3>(
            Func<GeneratorContext, T1, T2, T3, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4>(
            Func<GeneratorContext, T1, T2, T3, T4, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <typeparam name="T15"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }



        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator(
            Func<GeneratorContext, List<TEntity>> generateFunc)
        {
            Generator = DelegateGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1>(
            Func<GeneratorContext, T1, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3>(
            Func<GeneratorContext, T1, T2, T3, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4>(
            Func<GeneratorContext, T1, T2, T3, T4, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as arguments and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <typeparam name="T15"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, List<TEntity>> generateFunc)
        {
            Generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }

        #endregion


        #region Modifier
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier(
            Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier(
            Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.CreateMulti(modifyFunc));
            return this;
        }


        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc));

            return this;
        }

        #endregion
    }
}
