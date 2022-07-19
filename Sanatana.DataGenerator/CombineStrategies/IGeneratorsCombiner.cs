using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Pick next generator from CombineGenerator's list of inner generators.
    /// </summary>
    public interface IGeneratorsCombiner
    {
        /// <summary>
        /// Pick next IGenerator from CombineGenerator's list of inner generators.
        /// </summary>
        /// <param name="generators"></param>
        /// <param name="generatorContext"></param>
        /// <returns></returns>
        IGenerator GetNext(List<IGenerator> generators, GeneratorContext generatorContext);

        /// <summary>
        /// Reset variables when starting new generation.
        /// </summary>
        void Setup(GeneratorServices generatorServices);
    }
}
