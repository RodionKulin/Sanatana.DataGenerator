using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Extensions
{
    public static class ListExtentions
    {
        public static void Pop<T>(this List<T> list)
        {
            if(list.Count == 0)
            {
                throw new InvalidOperationException("Can not remove item from empty list");
            }

            list.RemoveAt(list.Count - 1);
        }

        public static T Peek<T>(this List<T> list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException("Can return item from empty list");
            }

            return list.Last();
        }
    }
}
