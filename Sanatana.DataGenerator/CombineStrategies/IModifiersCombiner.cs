using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Pick next set of modifiers from CombineModifier's list of inner modifiers.
    /// </summary>
    public interface IModifiersCombiner
    {
        /// <summary>
        /// Pick next IModifier from CombineModifier's list of inner modifiers.
        /// </summary>
        /// <param name="modifierSets"></param>
        /// <param name="generatorContext"></param>
        /// <returns></returns>
        List<IModifier> GetNext(List<List<IModifier>> modifierSets, GeneratorContext generatorContext);

        /// <summary>
        /// Reset variables when starting new generation.
        /// </summary>
        void Setup(List<List<IModifier>> modifierSets, GeneratorServices generatorServices);


        /// <summary>
        /// Validate IModifiersCombiner before setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="modifiers"></param>
        /// <param name="entity"></param>
        /// <param name="defaults"></param>
        void ValidateBeforeSetup(List<List<IModifier>> modifiers, IEntityDescription entity, DefaultSettings defaults);

        /// <summary>
        /// Validate IModifiersCombiner after setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="defaults"></param>
        void ValidateAfterSetup(List<List<IModifier>> modifiers, EntityContext entityContext, DefaultSettings defaults);

    }
}
