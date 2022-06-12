using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.SpreadStrategies
{
    /// <summary>
    /// Distribution strategy of parent entity instances among child entity instances.
    /// </summary>
    public interface ISpreadStrategy
    {
        /// <summary>
        /// Setup before serving any results
        /// </summary>
        /// <param name="childEntity"></param>
        /// <param name="allEntities"></param>
        void Setup(EntityContext childEntity, Dictionary<Type, EntityContext> allEntities);
        /// <summary>
        /// Index of parent(Required) entity that will be used for child entity. 
        /// First item in the Index should be starting with 0.
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="childEntity"></param>
        /// <returns></returns>
        long GetParentIndex(EntityContext parentEntity, EntityContext childEntity);
        /// <summary>
        /// Get number of parents required for child entity during next iteration. 
        /// First item in Count should be starting with 1.
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="childEntity"></param>
        /// <returns></returns>
        long GetNextIterationParentCount(EntityContext parentEntity, EntityContext childEntity);
        /// <summary>
        /// Check if all children were generated from provided parent range. 
        /// If so then parents stored in temporary storage will be released from it.
        /// </summary>
        /// <param name="parentEntity"></param>
        /// <param name="parentRange"></param>
        /// <param name="childEntity"></param>
        /// <returns></returns>
        bool CanGenerateFromParentRange(EntityContext parentEntity, FlushRange parentRange, EntityContext childEntity);
    }
}
