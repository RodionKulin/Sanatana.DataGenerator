using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.TargetCountProviders;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Comparers;

namespace Sanatana.DataGenerator.Internals.EntitySettings
{
    /// <summary>
    /// Default settings used for entity generation if not specified entity specific settings.
    /// </summary>
    public class DefaultSettings
    {
        /// <summary>
        /// Default entities generator. 
        /// Will be used for entity types that does not have a Generator specified.
        /// By default is not set.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// Default modifiers to make adjustments to entity instance after generation.
        /// Will be used for entity types that does not have Modifers specified.
        /// By default is not set.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// If true, will execute default Modifiers before executing Entity specific modifiers.
        /// If false and if Entity specific modifiers provided, than will execute only Entity specific modifiers.
        /// Entity setting has higher priority, than default: KeepDefaultModifiers = Entity.KeepDefaultModifiers ?? Default.KeepDefaultModifiers.
        /// By default Default.KeepDefaultModifiers=false and Entity.KeepDefaultModifiers=null.
        /// </summary>
        public bool KeepDefaultModifiers { get; set; }
        /// <summary>
        /// Default provider of total number of entity instances that need to be generated.
        /// Will be used for entity types that does not have a ITargetCountProvider specified.
        /// By default is not set.
        /// </summary>
        public ITargetCountProvider TargetCountProvider { get; set; }
        /// <summary>
        /// Default persistent storage(s) to store generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// By default is not set.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }
        /// <summary>
        /// Default strategy to trigger entity persistent storage writes.
        /// Will be used for entity types that does not have a FlushStrategy specified.
        /// By default using DefaultFlushStrategy.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Default IRequestCapacityProvider that returns number of entity instances that can be inserted per single request to persistent storage.
        /// Will be used for entity types that does not have a IRequestCapacityProvider specified.
        /// By default using StrictRequestCapacityProvider with a limit of 100.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Default spread strategy to reuse same required entity instances among multiple child entity instances.
        /// Will be used for Required entity types that does not have a SpreadStrategy specified.
        /// By default using EvenSpreadStrategy.
        /// </summary>
        public ISpreadStrategy SpreadStrategy { get; set; }
        /// <summary>
        /// Default selector from persistent storage, that will provide existing instances for EnsureExistGenerator or ReuseExistingGenerator.
        /// Will be used for entities with generator, that does not have a PersistentStorageSelector specified.
        /// By default is not set.
        /// </summary>
        public IPersistentStorageSelector PersistentStorageSelector { get; set; }
        /// <summary>
        /// Default factory that provides IEqualityComparer for entities with EnsureExistGenerator.
        /// Only required if EnsureExistGenerator is used.
        /// Will be used for entities with generator, that does not have a IEqualityComparer specified.
        /// By default is not set.
        /// </summary>
        public IEqualityComparerFactory EqualityComparerFactory { get; set; }


        //init
        public DefaultSettings()
        {
            PersistentStorages = new List<IPersistentStorage>();
            Modifiers = new List<IModifier>();
            FlushStrategy = new DefaultFlushStrategy();
            RequestCapacityProvider = new StrictRequestCapacityProvider(100);
            SpreadStrategy = new EvenSpreadStrategy();
        }

        public virtual DefaultSettings Clone()
        {
            return new DefaultSettings()
            {
                Generator = Generator,
                Modifiers = Modifiers == null ? null : new List<IModifier>(Modifiers),
                KeepDefaultModifiers = KeepDefaultModifiers,
                TargetCountProvider = TargetCountProvider,
                PersistentStorages = PersistentStorages == null ? null : new List<IPersistentStorage>(PersistentStorages),
                FlushStrategy = FlushStrategy,
                RequestCapacityProvider = RequestCapacityProvider,
                SpreadStrategy = SpreadStrategy,
                PersistentStorageSelector = PersistentStorageSelector,
                EqualityComparerFactory = EqualityComparerFactory
            };
        }


        #region Set entity specific or default service

        /// <summary>
        /// Set default entities generator. 
        /// Will be used for entity types that does not have a Generator specified.
        /// By default is not set.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetGenerator(IGenerator generator)
        {
            Generator = generator;
            return this;
        }

        /// <summary>
        /// Add default modifier to make adjustments to entity instance after generation.
        /// Will be used for entity types that does not have Modifers specified.
        /// By default is not set.
        /// </summary>
        /// <param name="modifier"></param>
        /// <returns></returns>
        public virtual DefaultSettings AddModifier(IModifier modifier)
        {
            Modifiers.Add(modifier);
            return this;
        }

        /// <summary>
        /// If true, will execute default Modifiers before executing Entity specific modifiers.
        /// If false and if Entity specific modifiers provided, than will execute only Entity specific modifiers.
        /// Default is false.
        /// </summary>
        /// <param name="keepDefaultModifiers"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetKeepDefaultModifiers(bool keepDefaultModifiers)
        {
            KeepDefaultModifiers = keepDefaultModifiers;
            return this;
        }

        /// <summary>
        /// Set default provider of total number of entity instances that need to be generated.
        /// Will be used for entity types that does not have a ITargetCountProvider specified.
        /// By default is not set.
        /// </summary>
        /// <param name="targetCountProvider"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetTargetCount(ITargetCountProvider targetCountProvider)
        {
            TargetCountProvider = targetCountProvider;
            return this;
        }

        /// <summary>
        /// Set default total number of entity instances that need to be generated.
        /// Will use StrictTargetCountProvider that holds constant total number.
        /// Will be used for entity types that does not have a ITargetCountProvider specified.
        /// By default is not set.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetTargetCount(long count)
        {
            TargetCountProvider = new StrictTargetCountProvider(count);
            return this;
        }

        /// <summary>
        /// Add default persistent storage to store generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// Can use multple persistent storages.
        /// By default is not set.
        /// </summary>
        /// <param name="persistentStorage"></param>
        /// <returns></returns>
        public virtual DefaultSettings AddPersistentStorage(IPersistentStorage persistentStorage)
        {
            PersistentStorages.Add(persistentStorage);
            return this;
        }

        /// <summary>
        /// Remove default persistent storage to store generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// Can use multple persistent storages.
        /// By default is not set.
        /// </summary>
        /// <returns></returns>
        public virtual DefaultSettings RemovePersistentStorages()
        {
            PersistentStorages.Clear();
            return this;
        }

        /// <summary>
        /// Set default strategy to trigger entity persistent storage writes.
        /// Will be used for entity types that does not have a FlushStrategy specified.
        /// By default using DefaultFlushStrategy.
        /// </summary>
        /// <param name="flushStrategy"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetFlushStrategy(IFlushStrategy flushStrategy)
        {
            FlushStrategy = flushStrategy;
            return this;
        }

        /// <summary>
        /// Set default IRequestCapacityProvider that returns number of entity instances that can be inserted per single request to persistent storage.
        /// Will be used for entity types that does not have a IRequestCapacityProvider specified.
        /// By default using StrictRequestCapacityProvider with a limit of 100.
        /// </summary>
        /// <param name="requestCapacityProvider"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetRequestCapacityProvider(IRequestCapacityProvider requestCapacityProvider)
        {
            RequestCapacityProvider = requestCapacityProvider;
            return this;
        }

        /// <summary>
        /// Set default spread strategy to reuse same required entity instances among multiple child entity instances.
        /// Will be used for Required entity types that does not have a SpreadStrategy specified.
        /// By default using EvenSpreadStrategy.
        /// </summary>
        /// <param name="spreadStrategy"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetSpreadStrategy(ISpreadStrategy spreadStrategy)
        {
            SpreadStrategy = spreadStrategy;
            return this;
        }

        /// <summary>
        /// Set default selector from persistent storage, that will provide existing instances for EnsureExistGenerator or ReuseExistingGenerator.
        /// Will be used for entities with generator, that does not have a PersistentStorageSelector specified.
        /// By default is not set.
        /// </summary>
        /// <param name="persistentStorageSelector"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetPersistentStorageSelector(IPersistentStorageSelector persistentStorageSelector)
        {
            PersistentStorageSelector = persistentStorageSelector;
            return this;
        }

        /// <summary>
        /// Set default factory that provides IEqualityComparer for entities with EnsureExistGenerator.
        /// Only required if EnsureExistGenerator is used.
        /// Will be used for entities with generator, that does not have a IEqualityComparer specified.
        /// By default is not set.
        /// </summary>
        /// <param name="equalityComparerFactory"></param>
        /// <returns></returns>
        public virtual DefaultSettings SetDefaultEqualityComparer(IEqualityComparerFactory equalityComparerFactory)
        {
            EqualityComparerFactory = equalityComparerFactory;
            return this;
        }

        #endregion


        #region Get entity specific or default service
        public virtual IGenerator GetGenerator(IEntityDescription entityDescription)
        {
            if (entityDescription.Generator != null)
            {
                return entityDescription.Generator;
            }

            if (Generator != null)
            {
                return Generator;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have {nameof(IModifier)} configured and {nameof(Generator)} also not provided.");
        }

        public virtual List<IModifier> GetModifiers(IEntityDescription entityDescription)
        {
            if (entityDescription.Modifiers != null && entityDescription.Modifiers.Count > 0)
            {
                bool keepDefaultModifiers = entityDescription.KeepDefaultModifiers ?? KeepDefaultModifiers;
                if (Modifiers != null && Modifiers.Count > 0 && keepDefaultModifiers)
                {
                    var combined = new List<IModifier>(Modifiers.Count + entityDescription.Modifiers.Count);
                    combined.AddRange(Modifiers);
                    combined.AddRange(entityDescription.Modifiers);
                    return combined;
                }
                else
                {
                    return entityDescription.Modifiers;
                }
            }

            if (Modifiers != null)
            {
                return Modifiers;
            }

            return Enumerable.Empty<IModifier>().ToList();
        }

        public virtual ITargetCountProvider GetTargetCountProvider(IEntityDescription entityDescription)
        {
            if (entityDescription.TargetCountProvider != null)
            {
                return entityDescription.TargetCountProvider;
            }

            if (TargetCountProvider != null)
            {
                return TargetCountProvider;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have {nameof(ITargetCountProvider)} configured and {nameof(TargetCountProvider)} also not provided.");
        }

        public virtual List<IPersistentStorage> GetPersistentStorages(IEntityDescription entityDescription)
        {
            if (entityDescription.PersistentStorages != null && entityDescription.PersistentStorages.Count > 0)
            {
                return entityDescription.PersistentStorages;
            }

            if (PersistentStorages != null && PersistentStorages.Count > 0)
            {
                return PersistentStorages;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have {nameof(IPersistentStorage)} configured and {nameof(PersistentStorages)} also not provided.");
        }

        public virtual IFlushStrategy GetFlushStrategy(IEntityDescription entityDescription)
        {
            if (entityDescription.FlushStrategy != null)
            {
                return entityDescription.FlushStrategy;
            }

            if (FlushStrategy != null)
            {
                return FlushStrategy;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have {nameof(IFlushStrategy)} configured and {nameof(FlushStrategy)} also not provided.");
        }

        public virtual IRequestCapacityProvider GetRequestCapacityProvider(IEntityDescription entityDescription)
        {
            if (entityDescription.RequestCapacityProvider != null)
            {
                return entityDescription.RequestCapacityProvider;
            }

            if (RequestCapacityProvider != null)
            {
                return RequestCapacityProvider;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have {nameof(IRequestCapacityProvider)} configured and {nameof(RequestCapacityProvider)} also not provided.");
        }

        public virtual ISpreadStrategy GetSpreadStrategy(IEntityDescription entityDescription, RequiredEntity requiredEntity)
        {
            if (requiredEntity.SpreadStrategy != null)
            {
                return requiredEntity.SpreadStrategy;
            }

            if (SpreadStrategy != null)
            {
                return SpreadStrategy;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} for required entity {requiredEntity.Type} does not have an {nameof(ISpreadStrategy)} configured and {nameof(SpreadStrategy)} also not provided.");
        }

        public virtual IPersistentStorageSelector GetPersistentStorageSelector(IEntityDescription entityDescription)
        {
            if (entityDescription.PersistentStorageSelector != null)
            {
                return entityDescription.PersistentStorageSelector;
            }

            if (PersistentStorageSelector != null)
            {
                return PersistentStorageSelector;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have a {nameof(IPersistentStorageSelector)} configured and default {nameof(PersistentStorageSelector)} also not provided.");
        }

        public virtual IEqualityComparer<TEntity> GetDefaultEqualityComparer<TEntity>(IEntityDescription entityDescription)
        {
            if (EqualityComparerFactory == null)
            {
                throw new NullReferenceException($"Type {entityDescription.Type.FullName} does not have a {nameof(IEqualityComparer<TEntity>)} configured and default {nameof(EqualityComparerFactory)} also not provided.");
            }

            IEqualityComparer<TEntity> comparer = EqualityComparerFactory.GetEqualityComparer<TEntity>(entityDescription);
            if(comparer == null)
            {
                throw new NullReferenceException($"Default {nameof(EqualityComparerFactory)} returned null instead of {nameof(IEqualityComparer<TEntity>)} for type {entityDescription.Type.FullName}.");
            }

            return comparer;
        }

        #endregion

    }
}
