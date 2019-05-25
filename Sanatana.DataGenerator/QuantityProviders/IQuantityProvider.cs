using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.QuantityProviders
{
    /// <summary>
    /// Provider of total number of entities to generate
    /// </summary>
    public interface IQuantityProvider
    {
        /// <summary>
        /// Return total number of entities that will be generated.
        /// </summary>
        /// <returns></returns>
        long GetTargetQuantity();
    }
}
