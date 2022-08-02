using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that IEntityDescription is supported in IModifier and all settings for IModifier are present.
    /// </summary>
    public class ModifiersBeforeSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;
            IEntityDescription[] entitiesWithModifiers = entityDescriptions.Values
                .Where(x => x.Modifiers != null && x.Modifiers.Count > 0)
                .ToArray();
            foreach (IEntityDescription entity in entitiesWithModifiers)
                foreach (IModifier modifier in entity.Modifiers)
                {
                    modifier.ValidateBeforeSetup(entity, generatorServices.Defaults);
                }

            if (generatorServices.Defaults.Modifiers != null)
            {
                IEntityDescription[] entitiesWithoutModifiers = entityDescriptions.Values
                    .Where(x => x.Modifiers == null || x.Modifiers.Count == 0)
                    .ToArray();
                foreach (IEntityDescription entity in entitiesWithoutModifiers)
                    foreach (IModifier modifier in generatorServices.Defaults.Modifiers)
                    {
                        modifier.ValidateBeforeSetup(entity, generatorServices.Defaults);
                    }
            }
        }

    }
}
