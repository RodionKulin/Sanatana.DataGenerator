using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    /// <summary>
    /// Enum value with selection of Entity types that will can be effected by configuration methods in SubsetGeneratorSetup.
    /// </summary>
    public enum EntitiesSelection
    {
        /// <summary>
        /// Includes target and required entities subset.
        /// </summary>
        All,
        /// <summary>
        /// Only target entities. Target entities that were provided as parameter in ToSubsetSetup method.
        /// </summary>
        Target,
        /// <summary>
        /// Only required entities. Required are entities that will be passed as parameters during generation of target entities.
        /// </summary>
        Required
    }
}
