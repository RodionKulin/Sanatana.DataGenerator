using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Collections;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.Validators;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.TargetCountProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.EntitySettings
{
    public class GeneratorServices
    {
        /// <summary>
        /// InMemory storage for generated entities to accumulate batches before inserting to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }

        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }

        /// <summary>
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public DefaultSettings Defaults { get; set; }

        /// <summary>
        /// All entity types configured that will be used to generate new instances.
        /// EntityContext includes information number of instances generated, parent and child relations with other entities.
        /// </summary>
        public Dictionary<Type, EntityContext> EntityContexts { get; set; }

        /// <summary>
        /// Producer of generation and flush commands. It determines the order in which entity instances will be generated and inserted to persistent storages.
        /// </summary>
        public ISupervisor Supervisor { get; set; }

        /// <summary>
        /// List of validators to run during setup and generation.
        /// </summary>
        public ValidatorsSetup Validators { get; set; }



        //methods
        /// <summary>
        /// Internal method to prepare EntityContexts.
        /// </summary>
        public virtual void SetupEntityContexts()
        {
            EntityContexts = EntityDescriptions.Values
                .Select(description => EntityContext.Factory.Create(EntityDescriptions, description))
                .ToDictionary(ctx => ctx.Type, ctx => ctx);
        }

        /// <summary>
        /// Internal method to prepare SpreadStrategies.
        /// </summary>
        public virtual void SetupSpreadStrategies()
        {
            IEnumerable<EntityContext> entitiesWithRequired = EntityContexts.Values
                .Where(x => x.Description.Required != null);

            foreach (EntityContext entityCtx in entitiesWithRequired)
            {
                foreach (RequiredEntity required in entityCtx.Description.Required)
                {
                    ISpreadStrategy spreadStrategy = Defaults.GetSpreadStrategy(entityCtx.Description, required);
                    spreadStrategy.Setup(entityCtx, EntityContexts);
                }
            }
        }

        /// <summary>
        /// Internal method to prepare TargetCount.
        /// </summary>
        public virtual void SetupTargetCount()
        {
            //TargetCount for some entities depend from TargetCount of their Required entities.
            //So ordering them to set TargetCount for Required entities first.
            var requiredOrderedList = new EntitiesOrderedList();
            foreach (EntityContext entityCtx in EntityContexts.Values)
            {
                requiredOrderedList.Add(entityCtx);
            }

            //Set TargetCount
            foreach (Type type in requiredOrderedList)
            {
                EntityContext entityCtx = EntityContexts[type];
                ITargetCountProvider targetCountProvider = Defaults.GetTargetCountProvider(entityCtx.Description);
                entityCtx.EntityProgress = new EntityProgress
                {
                    TargetCount = targetCountProvider.GetTargetCount(entityCtx.Description, Defaults)
                };
            }
        }

        /// <summary>
        /// Internal method to prepare PersistentStorages.
        /// </summary>
        public virtual void SetupPersistentStorages()
        {
            foreach (EntityContext entityCtx in EntityContexts.Values)
            {
                entityCtx.Description.PersistentStorages.ForEach(x => x.Setup());
            }
        }


        //validation methods
        /// <summary>
        /// Internal method to validate entity type is registered.
        /// </summary>
        /// <param name="entityType"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void ValidateEntitiesConfigured(Type entityType)
        {
            if (!EntityDescriptions.ContainsKey(entityType))
            {
                throw new ArgumentException($"Entity of type {entityType.FullName} not found in {nameof(EntityDescriptions)}. Need to invoke {nameof(GeneratorSetup)}.{nameof(GeneratorSetup.RegisterEntity)} method first.");
            }
        }

        /// <summary>
        /// Internal method to validate that multiple entity types are registered.
        /// </summary>
        /// <param name="entityTypes"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void ValidateEntitiesConfigured(IEnumerable<Type> entityTypes)
        {
            string[] missingTypes = entityTypes
                .Where(entityType => !EntityDescriptions.ContainsKey(entityType))
                .Select(entityType => $"Entity of type {entityType.FullName} not found in {nameof(EntityDescriptions)}. Need to invoke {nameof(GeneratorSetup)}.{nameof(GeneratorSetup.RegisterEntity)} method first.")
                .ToArray();
            if (missingTypes.Length > 0)
            {
                throw new ArgumentException(string.Join(", ", missingTypes));
            }
        }

        /// <summary>
        /// Internal method to validate that provided entity types not have duplicates.
        /// </summary>
        /// <param name="entityTypes"></param>
        /// <exception cref="ArgumentException"></exception>
        public virtual void ValidateNoEntityDuplicates(IEnumerable<Type> entityTypes)
        {
            string[] duplicateTypes = entityTypes.GroupBy(type => type)
                 .Where(group => group.Count() > 1)
                 .Select(group => $"Entity of type {group.Key.FullName} included multiple times in {nameof(entityTypes)} parameter. Duplicates are not allowed.")
                 .ToArray();
            if (duplicateTypes.Length > 0)
            {
                throw new ArgumentException(string.Join(", ", duplicateTypes));
            }
        }
    }
}
