using System;
using System.Collections;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// Modifier of specific entity. Can change entity instances one by one or in small batches.
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Modify next entity instance batch.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instances"></param>
        /// <returns></returns>
        IList Modify(GeneratorContext context, IList instances);

        /// <summary>
        /// Validate IModifier before setup.
        /// During setup TargetCount is calculated and Required entities ordered by their generation hierarchy.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="defaults"></param>
        void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults);

        /// <summary>
        /// Validate IModifier after setup.
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
