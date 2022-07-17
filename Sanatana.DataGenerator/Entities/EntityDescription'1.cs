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
    /// Entity configuration for generation process.
    /// </summary>
    public class EntityDescription<TEntity> : IEntityDescription
            where TEntity : class
    {
        #region Properties
        /// <summary>
        /// Type of entity to generate.
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
        /// Required entity intstances are generated first, then will be passed as parameters to generator.
        /// </summary>
        public List<RequiredEntity> Required { get; set; }
        /// <summary>
        /// Entity instances generator. Can return instances one by one or in small batches. 
        /// Usually better to return single instance not to store extra instances in memory.
        /// If not specified will use Generator from DefaultSettings.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// List of methods to make adjustments to entity instance after generation.
        /// If not specified will use Modifiers from DefaultSettings.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Database storages for generated entity instances.
        /// If not specified will use PersistentStorages from DefaultSettings.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Provider of total number of entity instances that need to be generated.
        /// If not specified will use TotalCountProvider from DefaultSettings.
        /// </summary>
        public ITotalCountProvider TotalCountProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entity instances to persistent storage.
        /// If not specified will use FlushStrategy from DefaultSettings.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Provider of number of entity instances that can be inserted with next request to persistent storage.
        /// If not specified will use RequestCapacityProvider from DefaultSettings.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Checker of entity instances to be inserted into database. 
        /// Excludes unwanted instances, like the ones that already exist in database for EnsureExistGenerator.
        /// If not specified is not used.
        /// </summary>
        public IStorageInsertGuard StorageInsertGuard { get; set; }
        /// <summary>
        /// Get database generated columns after inserting entities instances to persistent storage. For example Id.
        /// Only after receiving such columns pass entity instances as required to generator.
        /// Also makes insert requests to persistent storage sync for this entity. Multiple parallel inserts wont be possible.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }
        /// <summary>
        /// Selector from persistent storage, that will provide existing instances.
        /// Only required if EnsureExistGenerator or EnsureExistGenerator is used.
        /// If not specified will use PersistentStorageSelector from DefaultSettings.
        /// </summary>
        public IPersistentStorageSelector PersistentStorageSelector { get; set; }
        #endregion


        //init
        public EntityDescription()
        {
            Required = new List<RequiredEntity>();
            Modifiers = new List<IModifier>();
            PersistentStorages = new List<IPersistentStorage>();
        }

        public virtual IEntityDescription Clone()
        {
            return new EntityDescription<TEntity>
            {
                Required = new List<RequiredEntity>(Required),
                Generator = Generator,
                Modifiers = new List<IModifier>(Modifiers),
                PersistentStorages = new List<IPersistentStorage>(PersistentStorages),
                TotalCountProvider = TotalCountProvider,
                FlushStrategy = FlushStrategy,
                RequestCapacityProvider = RequestCapacityProvider,
                StorageInsertGuard = StorageInsertGuard,
                InsertToPersistentStorageBeforeUse = InsertToPersistentStorageBeforeUse,
                PersistentStorageSelector = PersistentStorageSelector
            };
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
        /// Add Required entity type that will be generated first and then passed as parameter to Generator.
        /// Required entities should also be registered for generation.
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

        protected virtual void SetRequiredFromGenerator(IDelegateParameterizedGenerator parameterizedGenerator)
        {
            Required.Clear();
            List<Type> argumentTypes = parameterizedGenerator.GetRequiredEntitiesFuncArguments();
            foreach (Type argumentType in argumentTypes)
            {
                SetRequired(new RequiredEntity(argumentType));
            }
        }

        protected virtual void SetRequiredFromModifier(IDelegateParameterizedModifier parameterizedModifier)
        {
            Required.Clear();
            List<Type> argumentTypes = parameterizedModifier.GetRequiredEntitiesFuncArguments();
            foreach (Type argumentType in argumentTypes)
            {
                SetRequired(new RequiredEntity(argumentType));
            }
        }

        /// <summary>
        /// Remove all Required entity types.
        /// </summary>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> RemoveRequired()
        {
            Required.Clear();
            return this;
        }

        /// <summary>
        /// Set custom SpreadStrategy for Required Entity. 
        /// Required Entity should already be registered when calling this method.
        /// </summary>
        /// <param name="requiredType"></param>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategy(
            Type requiredType, ISpreadStrategy spreadStrategy)
        {
            if (spreadStrategy == null)
            {
                throw new ArgumentNullException(nameof(spreadStrategy));
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
        /// Set custom SpreadStrategy for all Required entities. 
        /// Required entities should already be registered when calling this method.
        /// All Required entities registered after this method call will not be effected and will have default SpreadStrategy value.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategy(ISpreadStrategy spreadStrategy)
        {
            if (spreadStrategy == null)
            {
                throw new ArgumentNullException(nameof(spreadStrategy));
            }

            if (Required.Count == 0)
            {
                throw new NotSupportedException($"Not able to set {nameof(ISpreadStrategy)}. No {nameof(Required)} entities were configured on type {Type.FullName}.");
            }

            foreach (RequiredEntity requiredEntity in Required)
            {
                requiredEntity.SpreadStrategy = spreadStrategy;
            }

            return this;
        }

        /// <summary>
        /// Set SpreadStrategy for Required entities and set TargetCount as total number of combinations that can be generated.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategyAndTargetCount(CombinatoricsSpreadStrategy spreadStrategy)
        {
            SetSpreadStrategy(spreadStrategy);
            SetTargetCount(spreadStrategy);

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
                throw new ArgumentNullException(nameof(totalCountProvider));
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
                throw new ArgumentNullException(nameof(persistentStorage));
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
                throw new ArgumentNullException(nameof(insertFunc));
            }

            PersistentStorages.Add(new DelegatePersistentStorage(insertFunc));
            return this;
        }

        /// <summary>
        /// Remove all database storage providers that will receive generated entity instances.
        /// </summary>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> RemovePersistentStorages()
        {
            PersistentStorages.Clear();
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
                throw new ArgumentNullException(nameof(flushStrategy));
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
                throw new ArgumentNullException(nameof(requestCapacityProvider));
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

        /// <summary>
        /// Set selector from persistent storage, that will provide existing instances.
        /// Only required if EnsureExistGenerator or EnsureExistGenerator is used.
        /// If not specified will use PersistentStorageSelector from DefaultSettings.
        /// </summary>
        /// <param name="persistentStorageSelector"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetPersistentStorageSelector(
            IPersistentStorageSelector persistentStorageSelector)
        {
            PersistentStorageSelector = persistentStorageSelector;
            return this;
        }
        #endregion


        #region Generator set and remove
        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(IGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            Generator = generator;
            return this;
        }

        /// <summary>
        /// Set CombineGenerator, that uses multiple inner generators in turn to produce entity instances.
        /// </summary>
        /// <param name="combineGeneratorSetup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual EntityDescription<TEntity> SetCombineGenerator(Func<CombineGenerator, CombineGenerator> combineGeneratorSetup)
        {
            if(combineGeneratorSetup == null)
            {
                throw new ArgumentNullException(nameof(combineGeneratorSetup));
            }

            var combineGenerator = new CombineGenerator();
            combineGenerator = combineGeneratorSetup(combineGenerator);
            Generator = combineGenerator ?? throw new ArgumentNullException(nameof(combineGenerator));
            return this;
        }

        /// <summary>
        /// Remove Generator.
        /// </summary>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> RemoveGenerator()
        {
            Generator = null;
            return this;
        }
        #endregion


        #region Generator with single output
        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(
            Func<GeneratorContext, TEntity> generateFunc)
        {
            if (generateFunc == null)
            {
                throw new ArgumentNullException(nameof(generateFunc));
            }
            Generator = DelegateGenerator<TEntity>.Factory.Create(generateFunc);
            return this;
        }
               
        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1>(
            Func<GeneratorContext, T1, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }
        #endregion


        #region Generator with multi outputs
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
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1>(
            Func<GeneratorContext, T1, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetMultiGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;


            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        /// <summary>
        /// Set generator Func that will receive Required type instances as parameters and create new TEntity instances.
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
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            SetRequiredFromGenerator(generator);
            Generator = generator;

            return this;
        }

        #endregion


        #region Generator that reuses instances from persistent storage
        /// <summary>
        /// Set generator that provides existing entity instances from persistent storage instead of creating new.
        /// Such existing entities can be used to populate foreign key of other entities.
        /// Will use VoidStorage as PersistentStorage to prevent inserting already existing instances to persistent storage.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetReuseExistingGenerator<TOrderByKey>(ReuseExistingGenerator<TEntity, TOrderByKey> generator)
        {
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));

            PersistentStorages.Add(new VoidStorage());

            return this;
        }

        /// <summary>
        /// Set generator that provides existing entity instances from persistent storage instead of creating new.
        /// Such existing entities can be used to populate foreign key of other entities.
        /// Will set SetBatchSizeMax to select all instances from persistent storage matchign filter.
        /// Will use VoidStorage as PersistentStorage to prevent inserting already existing instances to persistent storage.
        /// Will set CountExistingTotalCountProvider as TotalCountProvider to select all instances.
        /// </summary>
        /// <param name="filter">Optional filter expression to select existing entity instances from persistent storage. By default will include all instances.</param>
        /// <param name="orderBy">Optional OrderBy expression to select existing entity instances with expected order. By default will select unordered instances.</param>
        /// <param name="isAscOrder"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public virtual EntityDescription<TEntity> SetReuseExistingGeneratorForAll<TOrderByKey>(
            Expression<Func<TEntity, bool>> filter = null,
            Expression<Func<TEntity, TOrderByKey>> orderBy = null, bool isAscOrder = false)
        {
            if (filter == null)
            {
                filter = (entity) => true;
            }

            var generator = new ReuseExistingGenerator<TEntity, TOrderByKey>()
                .SetBatchSizeMax()
                .SetFilter(filter);
            if (orderBy != null)
            {
                generator.SetOrderBy(orderBy, isAscOrder);
            }

            Generator = generator;

            PersistentStorages.Add(new VoidStorage());
            TotalCountProvider = new CountExistingTotalCountProvider<TEntity>(filter);

            return this;
        }
        #endregion


        #region Generator that inserts instances only if they dont exist in persistent storage

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey"></typeparam>
        /// <param name="generator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey>(
            EnsureExistGenerator<TEntity, TOrderByKey> generator)
        {
            generator = generator ?? throw new ArgumentNullException(nameof(generator));

            Generator = generator;
            FlushStrategy = generator;
            StorageInsertGuard = generator;

            return this;
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will create new TEntity instances.</param>

        /// <param name="generatorSetupFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey>(
            Func<GeneratorContext, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateGenerator<TEntity> newInstanceGenerator =
                DelegateGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1>(
            Func<GeneratorContext, T1, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2>(
            Func<GeneratorContext, T1, T2, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3>(
            Func<GeneratorContext, T1, T2, T3, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4>(
            Func<GeneratorContext, T1, T2, T3, T4, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
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
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistGenerator<TOrderByKey, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity> newInstanceGenerateFunc,
            Func<EnsureExistGenerator<TEntity, TOrderByKey>, EnsureExistGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistGenerator<TEntity, TOrderByKey>(newInstanceGenerator);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistGenerator(ensureExistGenerator);
        }

        #endregion


        #region Generator that inserts instances only if they dont exist in persistent storage in ranges

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey"></typeparam>
        /// <param name="generator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual EntityDescription<TEntity> SetEnsureExistRangeGenerator<TOrderByKey>(
            EnsureExistRangeGenerator<TEntity, TOrderByKey> generator)
        {
            generator = generator ?? throw new ArgumentNullException(nameof(generator));

            Generator = generator;
            FlushStrategy = generator;
            StorageInsertGuard = generator;

            return this;
        }

        /// <summary>
        /// Set generator that provides new instance only if it does not exist in PersistentStorage yet.
        /// Will set FlushStrategy that counts only new instances returned from Generator, excluding instances existing in persistent storage. It is required to plan number of rows inserted to persistent storage.
        /// Will set StorageInsertGuard that excludes all existing instances when preparing insert request to persistent storage.
        /// </summary>
        /// <typeparam name="TOrderByKey">property type that will be used to order instances in persistent storage. Required to predictably repeat same order of instances during generation.</typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="newInstanceGenerateFunc">Generator Func that will receive Required type instances as parameters and create new TEntity instance. Will also register generic arguments as Required types.</param>
        /// <param name="idSelector"></param>
        /// <param name="generatorSetupFunc">Configure EnsureExistGenerator expression</param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetEnsureExistRangeGenerator<TOrderByKey, T1>(
            Func<GeneratorContext, T1, TEntity> newInstanceGenerateFunc, Func<TEntity, long> idSelector,
            Func<EnsureExistRangeGenerator<TEntity, TOrderByKey>, EnsureExistRangeGenerator<TEntity, TOrderByKey>> generatorSetupFunc)
        {
            DelegateParameterizedGenerator<TEntity> newInstanceGenerator =
                DelegateParameterizedGenerator<TEntity>.Factory.Create(newInstanceGenerateFunc);
            SetRequiredFromGenerator(newInstanceGenerator);

            var ensureExistGenerator = new EnsureExistRangeGenerator<TEntity, TOrderByKey>(newInstanceGenerator, idSelector);
            ensureExistGenerator = generatorSetupFunc(ensureExistGenerator);
            return SetEnsureExistRangeGenerator(ensureExistGenerator);
        }

        #endregion


        #region Modifier add and remove
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        /// <summary>
        /// Set CombineModifier, that uses multiple IModifier sets in turn to modify entity instances.
        /// </summary>
        /// <param name="combineModifierSetup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual EntityDescription<TEntity> SetCombineModifier(Func<CombineModifier, CombineModifier> combineModifierSetup)
        {
            if (combineModifierSetup == null)
            {
                throw new ArgumentNullException(nameof(combineModifierSetup));
            }

            var combineModifier = new CombineModifier();
            combineModifier = combineModifierSetup(combineModifier);
            combineModifier = combineModifier ?? throw new ArgumentNullException(nameof(combineModifier));
            Modifiers.Add(combineModifier);
            return this;
        }

        /// <summary>
        /// Remove all Modifiers.
        /// </summary>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> RemoveModifiers()
        {
            Modifiers.Clear();
            return this;
        }
        #endregion


        #region Modifier with single input, void output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier(
            Action<GeneratorContext, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1>(
            Action<GeneratorContext, TEntity, T1> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with single input, single output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier(
            Func<GeneratorContext, TEntity, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier<T1>(
            Func<GeneratorContext, TEntity, T1, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2>(
            Func<GeneratorContext, TEntity, T1, T2, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3>(
            Func<GeneratorContext, TEntity, T1, T2, T3, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with single input, multiple outputs
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier(
            Func<GeneratorContext, TEntity, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier<T1>(
            Func<GeneratorContext, TEntity, T1, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, void output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier(
            Action<GeneratorContext, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddModifier<T1>(
            Action<GeneratorContext, List<TEntity>, T1> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, single output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier(
            Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
        public virtual EntityDescription<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, multi outputs
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> AddMultiModifier(
            Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

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
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            Modifiers.Add(modifier);
            SetRequiredFromModifier(modifier);

            return this;
        }

        #endregion

    }
}
