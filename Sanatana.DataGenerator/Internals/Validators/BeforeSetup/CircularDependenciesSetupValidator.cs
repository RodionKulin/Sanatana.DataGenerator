using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that there is no circle of required entities
    /// </summary>
    public class CircularDependenciesSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            //find first entity with circle of dependencies
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;
            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                List<Type> circleOfDependencies = FindCircularDependency(entity, null, entityDescriptions);
                if (circleOfDependencies != null)
                {
                    string listOfTypesJoined = string.Join(" > ", circleOfDependencies);
                    throw new NotSupportedException($"Circle of dependencies found starting with type {listOfTypesJoined}");
                }
            }
        }

        protected virtual List<Type> FindCircularDependency(IEntityDescription entity,
            List<Type> requiredEntities, Dictionary<Type, IEntityDescription> allEntityDescriptions)
        {
            requiredEntities = requiredEntities ?? new List<Type>();

            bool isCircle = requiredEntities.Contains(entity.Type);
            requiredEntities.Add(entity.Type);
            if (isCircle)
            {
                return requiredEntities;
            }

            //return null if no circle dependencies found
            if (entity.Required == null)
            {
                return null;
            }

            foreach (RequiredEntity required in entity.Required)
            {
                //skip self reference by entity
                if(required.Type == entity.Type)
                {
                    continue;
                }

                List<Type> dependenciesBranch = new List<Type>(requiredEntities);
                IEntityDescription requiredEntity = allEntityDescriptions[required.Type];
                List<Type> circleOfDependencies = FindCircularDependency
                    (requiredEntity, dependenciesBranch, allEntityDescriptions);
                if (circleOfDependencies != null)
                {
                    return circleOfDependencies;
                }
            }

            //return null if no circle dependencies found
            return null;
        }

    }
}
