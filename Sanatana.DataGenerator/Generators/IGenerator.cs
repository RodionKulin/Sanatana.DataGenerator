using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// Generator of specific entity. Can return entity instances one by one or in small batches. 
    /// Usually better to return single instance, so number of instances in memory before 
    /// flushing to persistent storage can be best optimized.
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Generate next entity instance batch.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IList Generate(GeneratorContext context);

        /// <summary>
        /// Validate IGenerator before setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="defaults"></param>
        /// <param name="entity"></param>
        void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults);

        /// <summary>
        /// Validate IGenerator after setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="defaults"></param>
        void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults);

        /// <summary>
        /// Reset variables when starting new generation.
        /// </summary>
        void Setup(GeneratorServices generatorServices);
    }
}
