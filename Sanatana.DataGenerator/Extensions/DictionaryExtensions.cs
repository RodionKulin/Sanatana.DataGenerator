using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Extensions
{
    /// <summary>
    /// Extentions for entity instance collections returned from SubsetGeneratorSetup.
    /// </summary>
    public static class DictionaryExtensions
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

            List<TEntity> instancesGenerated = instances[entityType].Cast<TEntity>().ToList();
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

        /// <summary>
        /// Select array of TEntity from dictionary and return full list;
        /// </summary>
        /// <typeparam name="TEntity">Entity type to return from instances dictionary.</typeparam>
        /// <param name="instances">Instances dictionary returned from SubsetGeneratorSetup.</param>
        /// <returns></returns>
        public static TEntity[] GetArray<TEntity>(this Dictionary<Type, IList> instances)
        {
            Type entityType = typeof(TEntity);
            if (!instances.ContainsKey(entityType))
            {
                throw new ArgumentException($"No entity of type {entityType.FullName} found. It is not part of target entity or required entities to generate.");
            }

            return instances[typeof(TEntity)].Cast<TEntity>().ToArray();
        }

    }
}
