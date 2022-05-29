using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.RequestCapacityProviders
{
    /// <summary>
    /// Provider of number of entity instances that can be inserted with next request to persistent storage.
    /// </summary>
    public interface IRequestCapacityProvider
    {
        /// <summary>
        /// Track entity instance generated to measure capacity for next request to persistent storage.
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="instances"></param>
        void TrackEntityGeneration(EntityContext entityContext, IList instances);

        /// <summary>
        /// Return number of entity instances that can be inserted with next request to persistent storage.
        /// </summary>
        /// <param name="entityContext"></param>
        /// <returns></returns>
        long GetCapacity(EntityContext entityContext);
    }
}
