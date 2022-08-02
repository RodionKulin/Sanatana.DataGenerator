using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Internals.Validators.AfterSetup
{
    /// <summary>
    /// Validate that IEntityDescription is supported in IModifier and all settings for IModifier are present.
    /// Will provide EntityContext as argument, that contains TargetCount and list of parent and child entities, based on Required settings.
    /// </summary>
    public class ModifiersAfterSetupValidator : IAfterSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            EntityContext[] entitiesWithGenerators = generatorServices.EntityContexts.Values
                .Where(x => x.Description.Modifiers != null && x.Description.Modifiers.Count > 0)
                .ToArray();
            foreach (EntityContext entity in entitiesWithGenerators)
                foreach (IModifier modifier in entity.Description.Modifiers)
                {
                    modifier.ValidateAfterSetup(entity, generatorServices.Defaults);
                }

            if (generatorServices.Defaults.Modifiers != null)
            {
                EntityContext[] entitiesWithoutModifiers = generatorServices.EntityContexts.Values
                    .Where(x => x.Description.Modifiers == null || x.Description.Modifiers.Count == 0)
                    .ToArray();
                foreach (EntityContext entity in entitiesWithoutModifiers)
                    foreach (IModifier modifier in generatorServices.Defaults.Modifiers)
                    {
                        modifier.ValidateAfterSetup(entity, generatorServices.Defaults);
                    }
            }
        }
    }
}
