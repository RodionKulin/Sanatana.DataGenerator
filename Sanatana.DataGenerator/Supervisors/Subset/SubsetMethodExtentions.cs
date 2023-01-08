using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Sanatana.DataGenerator.Storages;

namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Extentions for entity instance collections returned from SubsetGeneratorSetup.
    /// </summary>
    public static class SubsetMethodExtentions
    {
        /// <summary>
        /// Select list of TEntity from dictionary and return first item from the list.
        /// </summary>
        /// <typeparam name="TEntity">Entity type to return from instances dictionary.</typeparam>
        /// <param name="instances">Instances dictionary returned from SubsetGeneratorSetup.</param>
        /// <returns></returns>
        public static TEntity GetFirst<TEntity>(this Dictionary<Type, IList> instances)
        {
            Type entityType = typeof(TEntity);
            if (!instances.ContainsKey(entityType))
            {
                throw new ArgumentException($"No entity of type {entityType.FullName} found. It is not part of target entity or required entities to generate.");
            }

            var instancesGenerated = instances[entityType].Cast<TEntity>().ToList();
            if (instancesGenerated.Count == 0)
            {
                throw new ArgumentException($"No instances of type {entityType.FullName} found in {nameof(InMemoryStorage)}.");
            }

            return instancesGenerated.First();
        }

        /// <summary>
        /// Select list of TEntity from dictionary and return full list;
        /// </summary>
        /// <typeparam name="TEntity">Entity type to return from instances dictionary.</typeparam>
        /// <param name="instances">Instances dictionary returned from SubsetGeneratorSetup.</param>
        /// <returns></returns>
        public static List<TEntity> GetList<TEntity>(this Dictionary<Type, IList> instances)
        {
            Type entityType = typeof(TEntity);
            if (!instances.ContainsKey(entityType))
            {
                throw new ArgumentException($"No entity of type {entityType.FullName} found. It is not part of target entity or required entities to generate.");
            }

            return instances[entityType].Cast<TEntity>().ToList();
        }
    }
}
