using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
namespace Sanatana.DataGenerator.TargetCountProviders
{
    /// <summary>
    /// Provider of total number of instances to generate.
    /// </summary>
    public interface ITargetCountProvider
    {
        /// <summary>
        /// Returns total number of instances that will be generated.
        /// </summary>
        /// <returns></returns>
        long GetTargetCount(IEntityDescription description, DefaultSettings defaults);
    }
}
