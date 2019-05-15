using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.FlushTriggers;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Entities
{
    public class EntityDescription<TEntity> : IEntityDescription
            where TEntity : class
    {
        //properties
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
        /// Entities generator. Can return entities one by one or in small batches. 
        /// Usually better to return single entity not to store extra entities in memory.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// A method to make adjustments to entity after generated.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Database storage for generated entities.
        /// </summary>
        public IPersistentStorage PersistentStorage { get; set; }
        /// <summary>
        /// Provider of total number of entities that needs to be generated.
        /// </summary>
        public IQuantityProvider QuantityProvider { get; set; }
        /// <summary>
        /// Checker of temporary storage if it is time to flush entities to database.
        /// </summary>
        public IFlushTrigger FlushTrigger { get; set; }
        /// <summary>
        /// Get database generated columns like Id after inserting entities first. 
        /// Than only pass entities as required.
        /// Default is false.
        /// </summary>
        public bool InsertToPersistentStorageBeforeUse { get; set; }


        //init
        public EntityDescription()
        {
            Required = new List<RequiredEntity>();
            Modifiers = new List<IModifier>();
        }


        //methods
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
            RequiredEntity requiredEntity = Required.FirstOrDefault(x => x.Type == requiredType);

            if (requiredEntity == null)
            {
                string message = $"Type {requiredType.FullName} not found among required entities for {typeof(TEntity).FullName}." + 
                    $" Consider registering it with {nameof(SetRequired)} method or using as {typeof(DelegateParameterizedGenerator)} argument to autoregister." +
                    $" Generator should be registered before {nameof(SetSpreadStrategy)} method call in later case.";
                throw new ArgumentException(message, nameof(requiredType));
            }
            else
            {
                requiredEntity.SpreadStrategy = spreadStrategy;
            }

            return this;
        }

        /// <summary>
        /// Set custom SpreadStrategy to all Required Entities. 
        /// Required Entities should already be registered when calling this method.
        /// All Required Entities registered after this method call will not be effected and will have default SpreadStrategy value.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetSpreadStrategy(ISpreadStrategy spreadStrategy)
        {
            if(Required.Count == 0)
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
            Dictionary<Type, EntityContext> requiredEntities = generatorSetup.SetupEntityContexts(requiredDescriptions);
            spreadStrategy.Setup(requiredEntities);

            //Get total count of combinations
            long totalCount = spreadStrategy.GetTotalCount();
            SetTargetCount(totalCount);

            return this;
        }

        /// <summary>
        /// Set total number of entities that need to be generated.
        /// </summary>
        /// <param name="quantityProvider"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetTargetCount(IQuantityProvider quantityProvider)
        {
            QuantityProvider = quantityProvider;
            return this;
        }

        /// <summary>
        /// Set total number of entities that need to be generated.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetTargetCount(long count)
        {
            QuantityProvider = new StrictQuantityProvider(count);
            return this;
        }

        /// <summary>
        /// Set database inserting storage.
        /// </summary>
        /// <param name="persistentStorage"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetPersistentStorage(IPersistentStorage persistentStorage)
        {
            PersistentStorage = persistentStorage;
            return this;
        }

        /// <summary>
        /// Set database inserting storage.
        /// </summary>
        /// <param name="insertFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetPersistentStorage(Func<List<TEntity>, Task> insertFunc)
        {
            PersistentStorage = new DelegatePersistentStorage(insertFunc);
            return this;
        }

        /// <summary>
        /// Set entity persistent storage write trigger, that signals a required flush.
        /// </summary>
        /// <param name="flushTrigger"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetFlushTrigger(IFlushTrigger flushTrigger)
        {
            FlushTrigger = flushTrigger;
            return this;
        }

        /// <summary>
        /// Set entity persistent storage write trigger, that signals a required flush, when capacity of generated items is filled.
        /// </summary>
        /// <param name="capacity"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetLimitedCapacityFlushTrigger(long capacity)
        {
            FlushTrigger = new LimitedCapacityFlushTrigger(capacity);
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


        //Generator
        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(IGenerator generator)
        {
            Generator = generator;
            return this;
        }

        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGenerator(
            Func<GeneratorContext, TEntity> generateFunc)
        {
            Generator = DelegateGenerator.Create(generateFunc);
            return this;
        }

        /// <summary>
        /// Set generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetGeneratorMulti(
            Func<GeneratorContext, List<TEntity>> generateFunc)
        {
            Generator = DelegateGenerator.CreateMulti(generateFunc);
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
            Generator = DelegateGenerator.Create(generateFunc);
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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

            SetRequiredFromGenerator(generateFunc.GetType());

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

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
            var generator = new DelegateParameterizedGenerator();
            generator.RegisterDelegate(generateFunc);
            Generator = generator;

            SetRequiredFromGenerator(generateFunc.GetType());

            return this;
        }
        


        //PostProcessor
        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier(
            Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
        {
            Modifiers.Add(DelegateModifier.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti(
            Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
        {
            Modifiers.Add(DelegateModifier.CreateMulti(modifyFunc));
            return this;
        }


        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegate(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1>(
            Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

        /// <summary>
        /// Add post processsing handler that is triggered after generation. Can be used to apply additional customization to existing entity instance.
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
        public virtual EntityDescription<TEntity> SetModifierMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            var modifier = new DelegateParameterizedModifier();
            modifier.RegisterDelegateMulti(modifyFunc);
            Modifiers.Add(modifier);

            return this;
        }

    }
}
