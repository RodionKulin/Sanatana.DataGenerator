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
        List<IModifier> GetNext(List<List<IModifier>> modifierSets, GeneratorContext generatorContext);
    }
}
