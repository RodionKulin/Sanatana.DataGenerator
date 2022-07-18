using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.TargetCountProviders;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that all parameters were set for each EntityDescription or defaults.
    /// </summary>
    public class EntityDescriptionSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            ISupervisor supervisor = generatorServices.Supervisor;
            if (supervisor == null)
            {
                throw new ArgumentNullException(nameof(generatorServices.Supervisor));
            }

            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;
            foreach (IEntityDescription description in entityDescriptions.Values)
            {
                string msgFormat = $"Entity [{description.Type}] not have {{0}} configured and {nameof(generatorServices.Defaults)} {{1}} not provided";

                ITargetCountProvider targetCountProvider = description.TargetCountProvider
                    ?? generatorServices.Defaults.TargetCountProvider;
                if (targetCountProvider == null)
                {
                    string defName = nameof(generatorServices.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.TargetCountProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                IGenerator generator = description.Generator ?? generatorServices.Defaults.Generator;
                if (generator == null)
                {
                    string defName = nameof(generatorServices.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.TargetCountProvider), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                List<IPersistentStorage> persistentStorages = description.PersistentStorages == null || description.PersistentStorages.Count == 0
                    ? generatorServices.Defaults.PersistentStorages
                    : description.PersistentStorages;
                if (persistentStorages == null || persistentStorages.Count == 0)
                {
                    string defName = nameof(generatorServices.Defaults.PersistentStorages);
                    string msg = string.Format(msgFormat
                        , nameof(description.PersistentStorages), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                IFlushStrategy flushTrigger = description.FlushStrategy
                    ?? generatorServices.Defaults.FlushStrategy;
                if (flushTrigger == null)
                {
                    string defName = nameof(generatorServices.Defaults.FlushStrategy);
                    string msg = string.Format(msgFormat
                        , nameof(description.FlushStrategy), defName);
                    throw new ArgumentNullException(defName, msg);
                }

                if (description.Required != null)
                {
                    foreach (RequiredEntity required in description.Required)
                    {
                        ISpreadStrategy spreadStrategy = required.SpreadStrategy
                            ?? generatorServices.Defaults.SpreadStrategy;
                        if (spreadStrategy == null)
                        {
                            string defName = nameof(generatorServices.Defaults.SpreadStrategy);
                            string msg = $"Entity [{description.Type}] with required type [{required.Type}] not have {nameof(RequiredEntity.SpreadStrategy)} configured and {nameof(generatorServices.Defaults)} {defName} was not provided";
                            throw new ArgumentNullException(defName, msg);
                        }
                    }
                }
            }
        }

    }
}
