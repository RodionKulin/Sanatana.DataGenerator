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
        void Setup(GeneratorServices generatorServices);
    }
}
