using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.SpreadStrategies
{
    public interface ISpreadStrategy
    {
        /// <summary>
        /// Setup before serving any results
        /// </summary>
        /// <param name="parentEntities"></param>
        void Setup(EntityContext childEntity, Dictionary<Type, EntityContext> allEntities);
        /// <summary>
        /// Index of parent(Required) entity that will be used for child entity. 
        /// First item in the Index should be starting with 0.
        /// </summary>
        /// <param name="parentProgress"></param>
        /// <param name="childProgress"></param>
        /// <returns></returns>
        long GetParentIndex(EntityContext parentEntity, EntityContext childEntity);
        /// <summary>
        /// Get number of parents required for child entity during next iteration. 
        /// First item in Count should be starting with 1.
        /// </summary>
        /// <param name="parentProgress"></param>
        /// <param name="childProgress"></param>
        /// <returns></returns>
        long GetNextIterationParentCount(EntityContext parentEntity, EntityContext childEntity);
        /// <summary>
        /// Check if all children were generated from provided parent. 
        /// If so then parents stored in temporary storage will be released from temp storage.
        /// </summary>
        /// <param name="parentProgress"></param>
        /// <param name="childProgress"></param>
        /// <returns></returns>
        bool CanGenerateFromParentNextReleaseCount(EntityContext parentEntity, EntityContext childEntity);
    }
}
