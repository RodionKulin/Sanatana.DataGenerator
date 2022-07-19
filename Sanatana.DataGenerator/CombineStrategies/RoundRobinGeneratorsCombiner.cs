using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Each generator is called in equal portions and in circular order.
    /// </summary>
    public class RoundRobinGeneratorsCombiner : IGeneratorsCombiner
    {
        public IGenerator GetNext(List<IGenerator> generators, GeneratorContext generatorContext)
        {
            if(generators.Count == 0)
            {
                throw new NotSupportedException($"Provided empty list of generators for entity {generatorContext.Description.Type.FullName} in {nameof(CombineGenerator)} {nameof(RoundRobinGeneratorsCombiner)}.");
            }

            int nextGeneratorInd = (int)(generatorContext.CurrentCount % generators.Count);
            return generators[nextGeneratorInd];
        }
    }
}
