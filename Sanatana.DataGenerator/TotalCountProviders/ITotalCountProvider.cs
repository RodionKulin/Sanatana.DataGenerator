using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.TotalCountProviders
{
    /// <summary>
    /// Provider of total number of instances to generate.
    /// </summary>
    public interface ITotalCountProvider
    {
        /// <summary>
        /// Returns total number of instances that will be generated.
        /// </summary>
        /// <returns></returns>
        long GetTargetCount(IEntityDescription description, DefaultSettings defaults);
    }
}
