using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Collections;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.TotalCountProviders;
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

        public Dictionary<Type, EntityContext> EntityContexts { get; set; }

        public ISupervisor Supervisor { get; set; }


        //methods
        public virtual void SetupEntityContexts(Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            EntityContexts = entityDescriptions.Values
                .Select(description => EntityContext.Factory.Create(entityDescriptions, description))
                .ToDictionary(ctx => ctx.Type, ctx => ctx);
        }

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
                ITotalCountProvider totalCountProvider = Defaults.GetTotalCountProvider(entityCtx.Description);
                entityCtx.EntityProgress = new EntityProgress
                {
                    TargetCount = totalCountProvider.GetTargetCount()
                };
            }
        }
    }
}
