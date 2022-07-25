using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Each set of modifiers is called in equal portions and in circular order.
    /// </summary>
    public class RoundRobinModifiersCombiner : IModifiersCombiner
    {
        public virtual List<IModifier> GetNext(List<List<IModifier>> modifierSets, GeneratorContext generatorContext)
        {
            if (modifierSets.Count == 0)
            {
                throw new NotSupportedException($"Provided empty list of {nameof(modifierSets)} for entity {generatorContext.Description.Type.FullName} in {nameof(CombineModifier)} {nameof(RoundRobinModifiersCombiner)}.");
            }

            int nextIndex = (int)(generatorContext.CurrentCount % modifierSets.Count);
            return modifierSets[nextIndex];
        }

        public virtual void Setup(List<List<IModifier>> modifierSets, GeneratorServices generatorServices)
        {
        }

        public void ValidateAfterSetup(List<List<IModifier>> modifiers, EntityContext entityContext, DefaultSettings defaults)
        {
        }

        public void ValidateBeforeSetup(List<List<IModifier>> modifiers, IEntityDescription entity, DefaultSettings defaults)
        {
        }
    }
}
