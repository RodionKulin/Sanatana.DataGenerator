using Sanatana.DataGenerator.Entities;
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
        void Setup(List<IGenerator> generators, GeneratorServices generatorServices);


        /// <summary>
        /// Validate IGeneratorsCombiner before setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="generators"></param>
        /// <param name="entity"></param>
        /// <param name="defaults"></param>
        void ValidateBeforeSetup(List<IGenerator> generators, IEntityDescription entity, DefaultSettings defaults);

        /// <summary>
        /// Validate IGeneratorsCombiner after setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="generators"></param>
        /// <param name="entityContext"></param>
        /// <param name="defaults"></param>
        void ValidateAfterSetup(List<IGenerator> generators, EntityContext entityContext, DefaultSettings defaults);


    }
}
