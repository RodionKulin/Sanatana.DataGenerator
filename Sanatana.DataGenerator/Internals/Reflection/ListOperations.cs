using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Internals.Reflection
{
    public class ListOperations
    {

        //methods
        public virtual IList Take(Type newListType, IList list, int number)
        {
            IList newList = CreateEntityList(newListType);

            //take larger than list length with take all
            int takeCount = Math.Min(number, list.Count);

            //taking negative number of 0 will return empty list
            for (int i = 0; i < takeCount; i++)
            {
                newList.Add(list[i]);
            }

            return newList;
        }

        public virtual IList Skip(Type newListType, IList list, int number)
        {
            IList newList = CreateEntityList(newListType);

            //skip negative number will behave as skip 0 and return full list
            int skipIndex = number;
            skipIndex = Math.Max(skipIndex, 0);

            //skipping larger than list length will return empty list
            for (int i = skipIndex; i < list.Count; i++)
            {
                newList.Add(list[i]);
            }

            return newList;
        }

        public virtual void AddRange(IList list, IList anotherList)
        {
            if (anotherList == null)
            {
                return;
            }

            foreach (var entity in anotherList)
            {
                list.Add(entity);
            }
        }

        public virtual IList CreateEntityList(Type entityType)
        {
            Type listType = typeof(List<>);
            Type constructedListType = listType.MakeGenericType(entityType);
            return (IList)Activator.CreateInstance(constructedListType);
        }
    }
}
