using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// Generator of specific entity. Can return entities one by one or in small batches. 
    /// Usually better to return single entity, so number of entities in memory before 
    /// flushing to persistent storage can be best optimized.
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Generate next entity instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        IList Generate(GeneratorContext context);
        /// <summary>
        /// Validate IGenerator Setup before start.
        /// </summary>
        /// <param name="entity"></param>
        void ValidateEntitySettings(IEntityDescription entity);
    }
}
