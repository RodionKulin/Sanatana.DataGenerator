using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// PassThroughModifier modifier makes not changes to generated entity instances.
    /// Add PassThroughModifier to list of entity modifiers to make it not use list of default modifiers.
    /// </summary>
    public class PassThroughModifier : IModifier
    {
        public virtual IList Modify(GeneratorContext context, IList instances)
        {
            return instances;
        }

        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
        }
    }
}
