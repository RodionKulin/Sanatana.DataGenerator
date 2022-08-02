using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;

namespace Sanatana.DataGenerator.Internals.Validators.BeforeSetup
{
    /// <summary>
    /// Validate that Required types list for each entity does not contain duplicates and is alligned with Generator's generic arguments list.
    /// Validate that Parameterized Modifiers have the same parameters as list of Required types.
    /// </summary>
    public class RequiredCountSetupValidator : IBeforeSetupValidator
    {
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            CheckGeneratorParams(generatorServices);
            CheckModifierParams(generatorServices);
        }

        protected virtual void CheckGeneratorParams(GeneratorServices generatorServices)
        {
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;

            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                //Check that no duplicates in Required types
                Type[] requiredEntitiesTypes = entity.Required
                    .Select(x => x.Type)
                    .ToArray();
                IGenerator generator = generatorServices.Defaults.GetGenerator(entity);
                Type generatorType = generator.GetType();
                CheckDuplicates(requiredEntitiesTypes, entity, generatorType);

                //Compare Required types from IEntityDescription.Required vs IDelegateParameterizedGenerator to make sure it was not changed manually before start.
                if (generator is IDelegateParameterizedGenerator)
                {
                    Type[] generatorParams = (generator as IDelegateParameterizedGenerator)
                       .GetRequiredEntitiesFuncArguments()
                       .ToArray();
                    CompareToRequiredParams(requiredEntitiesTypes, generatorParams, entity, generatorType);
                }
            }
        }

        protected virtual void CheckModifierParams(GeneratorServices generatorServices)
        {
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;

            foreach (IEntityDescription entity in entityDescriptions.Values)
            {
                Type[] requiredEntitiesTypes = entity.Required.Select(x => x.Type).ToArray();
                List<IModifier> modifiers = generatorServices.Defaults.GetModifiers(entity);

                modifiers = modifiers.Where(x => x is IDelegateParameterizedModifier).ToList();
                for (int i = 0; i < modifiers.Count; i++)
                {
                    IModifier modifier = modifiers[i];
                    Type modifierType = modifier.GetType();
                    Type[] modifierParams = (modifier as IDelegateParameterizedModifier)
                        .GetRequiredEntitiesFuncArguments()
                        .ToArray();

                    CheckDuplicates(modifierParams, entity, modifierType);
                    CompareToRequiredParams(requiredEntitiesTypes, modifierParams, entity, modifierType);
                }
            }
        }

        /// <summary>
        /// Check that Required types don't have duplicates
        /// </summary>
        /// <param name="paramsTypes"></param>
        /// <param name="entity"></param>
        /// <param name="paramsHoldingType"></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void CheckDuplicates(Type[] paramsTypes, IEntityDescription entity, Type paramsHoldingType)
        {
            List<string> actualParamsDuplicates = paramsTypes
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key.FullName)
                .ToList();

            if (actualParamsDuplicates.Count > 0)
            {
                string duplicates = string.Join(", ", actualParamsDuplicates);
                string message = $"Entity {entity.Type.FullName} {paramsHoldingType.FullName} contains duplicate parameter(s) with type: {duplicates}.";
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Compare Required types from IEntityDescription.Required vs IDelegateParameterizedGenerator to make sure it was not changed manually before start.
        /// </summary>
        /// <param name="requiredParams"></param>
        /// <param name="actualParams"></param>
        /// <param name="entity"></param>
        /// <param name="whereFound"></param>
        /// <exception cref="NotSupportedException"></exception>
        protected virtual void CompareToRequiredParams(Type[] requiredParams, Type[] actualParams,
            IEntityDescription entity, Type whereFound)
        {
            string[] extraParams = requiredParams.Except(actualParams)
                .Select(x => x.FullName)
                .ToArray();
            if (extraParams.Length > 0)
            {
                string paramsJoined = string.Join(", ", extraParams);
                string message = $"Entity {entity.Type.FullName} {whereFound.FullName} contains less parameters than configured in Entities's Required. Those are missing parameters: {paramsJoined}.";
                throw new NotSupportedException(message);
            }

            extraParams = actualParams.Except(requiredParams)
                .Select(x => x.FullName)
                .ToArray();
            if (extraParams.Length > 0)
            {
                string paramsJoined = string.Join(", ", extraParams);
                string message = $"Entity {entity.Type.FullName} {whereFound.FullName} contains more parameters than configured in Entities's Required. Those are extra parameters: {paramsJoined}.";
                throw new NotSupportedException(message);
            }
        }

    }
}
