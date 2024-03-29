﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that IEntityDescription is supported in IGenerator and all settings for IGenerator are present.
    /// </summary>
    public class GeneratorsBeforeSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;
            IEntityDescription[] entitiesWithGenerators = entityDescriptions.Values.Where(x => x.Generator != null).ToArray();
            foreach (IEntityDescription entity in entitiesWithGenerators)
            {
                entity.Generator.ValidateBeforeSetup(entity, generatorServices.Defaults);
            }

            if (generatorServices.Defaults.Generator != null)
            {
                IEntityDescription[] entitiesWithoutGenerators = entityDescriptions.Values.Where(x => x.Generator == null).ToArray();
                foreach (IEntityDescription entity in entitiesWithoutGenerators)
                {
                    generatorServices.Defaults.Generator.ValidateBeforeSetup(entity, generatorServices.Defaults);
                }
            }
        }

    }
}
