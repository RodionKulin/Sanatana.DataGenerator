using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.DataGenerator.Internals.Validators.AfterGenerate
{
    /// <summary>
    /// Validate that number of generated entities is greater than 0 and returned type is List&lt;TEntity&gt;
    /// </summary>
    public class InstancesCountGeneratedValidator : IGenerateValidator
    {
        public virtual void ValidateGenerated(IList entities, Type entityType, IGenerator generator)
        {
            string generatorType = generator.GetType().FullName;
            string resultCountMsg = "Number of entities returned should be greater than 0. " +
                $"{nameof(IGenerator)} {generatorType} for entity {entityType} returned 0 entities. " +
                $"If you want to allow such behavior, then remove {nameof(InstancesCountGeneratedValidator)} from Validators.";

            if (entities == null)
            {
                throw new NotSupportedException(resultCountMsg);
            }

            Type entitiesListType = entities.GetType();
            if (entitiesListType.IsAssignableFrom(typeof(List<>)))
            {
                string resultTypeMessage = $"List returned from {nameof(IGenerator)} should be a generic List<>. {nameof(IGenerator)} {generatorType} returned list of type {entitiesListType}.";

                throw new NotSupportedException(resultTypeMessage);
            }

            if (entities.Count == 0)
            {
                throw new NotSupportedException(resultCountMsg);
            }
        }

    }
}
