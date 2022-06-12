using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.RequestCapacityProviders;

namespace Sanatana.DataGenerator.Internals.EntitySettings
{
    /// <summary>
    /// Default settings used for entity generation if not specified entity specific settings.
    /// </summary>
    public class DefaultSettings
    {
        /// <summary>
        /// Default strategy to trigger entity persistent storage writes.
        /// Will be used for entity types that does not have a FlushStrategy specified.
        /// </summary>
        public IFlushStrategy FlushStrategy { get; set; }
        /// <summary>
        /// Default IRequestCapacityProvider that returns number of entity instances that can be inserted per single request to persistent storage.
        /// Will be used for entity types that does not have a IRequestCapacityProvider specified.
        /// StrictRequestCapacityProvider with a limit of 100 is set by default.
        /// </summary>
        public IRequestCapacityProvider RequestCapacityProvider { get; set; }
        /// <summary>
        /// Default entities generator. 
        /// Will be used for entity types that does not have a Generator specified.
        /// By default is not set.
        /// </summary>
        public IGenerator Generator { get; set; }
        /// <summary>
        /// Default modifiers to make adjustments to entity after generation.
        /// Will be used for entity types that does not have Modifers specified.
        /// </summary>
        public List<IModifier> Modifiers { get; set; }
        /// <summary>
        /// Default provider of total number of entity instances that need to be generated.
        /// Will be used for entity types that does not have a TotalCountProvider specified.
        /// By default is not set.
        /// </summary>
        public ITotalCountProvider TotalCountProvider { get; set; }
        /// <summary>
        /// Default strategy to reuse same parent entity instances among multiple child entity instances.
        /// Will be used for Required entity types that does not have a SpreadStrategy specified.
        /// EvenSpreadStrategy is set by default.
        /// </summary>
        public ISpreadStrategy SpreadStrategy { get; set; }
        /// <summary>
        /// Default persistent storage(s) to store generated entities.
        /// Will be used for entity types that does not have a PersistentStorage specified.
        /// By default is not set.
        /// </summary>
        public List<IPersistentStorage> PersistentStorages { get; set; }


        //init
        public DefaultSettings()
        {
            PersistentStorages = new List<IPersistentStorage>();
            FlushStrategy = new DefaultFlushStrategy();
            RequestCapacityProvider = new StrictRequestCapacityProvider(100);
            SpreadStrategy = new EvenSpreadStrategy();
        }


        //Get entity specific or default service
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

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IGenerator)} configured and {nameof(Generator)} also was not provided.");
        }

        public virtual List<IModifier> GetModifiers(IEntityDescription entityDescription)
        {
            if (entityDescription.Modifiers != null
                && entityDescription.Modifiers.Count > 0)
            {
                return entityDescription.Modifiers;
            }

            if (Modifiers != null)
            {
                return Modifiers;
            }

            return new List<IModifier>();
        }

        public virtual ITotalCountProvider GetTotalCountProvider(IEntityDescription entityDescription)
        {
            if (entityDescription.TotalCountProvider != null)
            {
                return entityDescription.TotalCountProvider;
            }

            if (TotalCountProvider != null)
            {
                return TotalCountProvider;
            }

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(ITotalCountProvider)} configured and {nameof(TotalCountProvider)} also was not provided.");
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

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IPersistentStorage)} configured and {nameof(PersistentStorages)} also was not provided.");
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

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IFlushStrategy)} configured and {nameof(FlushStrategy)} also was not provided.");
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

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} did not have {nameof(IRequestCapacityProvider)} configured and {nameof(RequestCapacityProvider)} also was not provided.");
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

            throw new NullReferenceException($"Type {entityDescription.Type.FullName} for required entity {requiredEntity.Type} did not have an {nameof(ISpreadStrategy)} configured and {nameof(SpreadStrategy)} also was not provided.");
        }

    }
}
