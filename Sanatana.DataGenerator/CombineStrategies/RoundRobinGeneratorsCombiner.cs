using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Each generator is called in equal portions and in circular order.
    /// </summary>
    public class RoundRobinGeneratorsCombiner : IGeneratorsCombiner
    {
        public virtual IGenerator GetNext(List<IGenerator> generators, GeneratorContext generatorContext)
        {
            if(generators.Count == 0)
            {
                throw new NotSupportedException($"Provided empty list of {nameof(generators)} for entity {generatorContext.Description.Type.FullName} in {nameof(CombineGenerator)} {nameof(RoundRobinGeneratorsCombiner)}.");
            }

            int nextGeneratorInd = (int)(generatorContext.CurrentCount % generators.Count);
            return generators[nextGeneratorInd];
        }

        public virtual void Setup(List<IGenerator> generators, GeneratorServices generatorServices)
        {
        }

        public virtual void ValidateAfterSetup(List<IGenerator> generators, EntityContext entityContext, DefaultSettings defaults)
        {
        }

        public virtual void ValidateBeforeSetup(List<IGenerator> generators, IEntityDescription entity, DefaultSettings defaults)
        {
        }
    }
}
