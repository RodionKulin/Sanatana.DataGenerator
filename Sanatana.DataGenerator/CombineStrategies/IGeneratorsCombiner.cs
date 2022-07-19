using Sanatana.DataGenerator.Generators;
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
        IGenerator GetNext(List<IGenerator> generators, GeneratorContext generatorContext);
    }
}
