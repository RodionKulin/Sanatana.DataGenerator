using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Extensions
{
    public static class DictionaryExtensions
    {
        public static TEntity GetFirst<TEntity>(this Dictionary<Type, IList> generatedItems)
        {
            return (TEntity)generatedItems[typeof(TEntity)][0]!;
        }

        public static TEntity[] GetArray<TEntity>(this Dictionary<Type, IList> generatedItems)
        {
            return generatedItems[typeof(TEntity)].Cast<TEntity>().ToArray();
        }

        public static List<TEntity> GetList<TEntity>(this Dictionary<Type, IList> generatedItems)
        {
            return generatedItems[typeof(TEntity)].Cast<TEntity>().ToList();
        }
    }
}
