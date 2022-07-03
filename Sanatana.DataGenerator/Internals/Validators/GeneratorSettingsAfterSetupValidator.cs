﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Internals.Validators.Contracts;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Validators
{
    /// <summary>
    /// Validate that IEntityDescription is supported in Generator and all settings for Generator are present.
    /// Will provide EntityContext as argument, that contains TargetCount and list of parent and child entities, based on Required settings.
    /// </summary>
    public class GeneratorSettingsAfterSetupValidator : IAfterSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            EntityContext[] entitiesWithGenerators = generatorServices.EntityContexts.Values
                .Where(x => x.Description.Generator != null)
                .ToArray();
            foreach (EntityContext entity in entitiesWithGenerators)
            {
                entity.Description.Generator.ValidateAfterSetup(entity, generatorServices.Defaults);
            }

            if (generatorServices.Defaults.Generator != null)
            {
                EntityContext[] entitiesWithoutGenerators = generatorServices.EntityContexts.Values
                    .Where(x => x.Description.Generator == null)
                    .ToArray();
                foreach (EntityContext entity in entitiesWithoutGenerators)
                {
                    generatorServices.Defaults.Generator.ValidateAfterSetup(entity, generatorServices.Defaults);
                }
            }
        }

    }
}
