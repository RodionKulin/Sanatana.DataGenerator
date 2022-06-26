using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public enum EntitiesSelection
    {
        /// <summary>
        /// Includes target and required entities subset.
        /// </summary>
        All,
        /// <summary>
        /// Only only target entities. Target entities that were provided as parameter in ToSubsetSetup method.
        /// </summary>
        Target,
        /// <summary>
        /// Only required entities. Required are entities that will be passed as parameters during generation of target entities.
        /// </summary>
        Required
    }
}
