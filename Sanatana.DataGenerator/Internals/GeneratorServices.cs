using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals
{
    public class GeneratorServices
    {
        /// <summary>
        /// InMemory storage for generated entities to accumulate batches before inserting to persistent storage.
        /// </summary>
        public TemporaryStorage TemporaryStorage { get; set; }

        /// <summary>
        /// Configuration validator that will throw errors on missing or inconsistent setup
        /// </summary>
        public Validator Validator { get; set; }

        /// <summary>
        /// All entity types configured that will be used by OrderProvider to pick generation order.
        /// </summary>
        public Dictionary<Type, IEntityDescription> EntityDescriptions { get; set; }

        /// <summary>
        /// Default settings used for entity generation if not specified entity specific settings.
        /// </summary>
        public DefaultSettings Defaults { get; set; }



        //methods
        public virtual Dictionary<Type, EntityContext> SetupEntityContexts(
            Dictionary<Type, IEntityDescription> entityDescriptions)
        {
            Dictionary<Type, EntityContext> entityContexts = entityDescriptions.Values
                .Select(description => EntityContext.Factory.Create(entityDescriptions, description, Defaults))
                .ToDictionary(entityContext => entityContext.Type, entityContext => entityContext);
            return entityContexts;
        }
    }
}
