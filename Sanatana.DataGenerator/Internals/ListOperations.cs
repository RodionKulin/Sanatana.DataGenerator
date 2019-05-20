using System;
using System.Collections;

namespace Sanatana.DataGenerator.Internals
{
    public class ListOperations
    {
        //fields
        protected ReflectionInvoker _reflectionInvoker;


        //init
        public ListOperations()
        {
            _reflectionInvoker = new ReflectionInvoker();
        }


        //methods
        public IList Take(Type newListType, IList list, int number)
        {
            IList newList = _reflectionInvoker.CreateEntityList(newListType);

            //take larger than list length with take all
            int takeCount = Math.Min(number, list.Count);

            //taking negative number of 0 will return empty list
            for (int i = 0; i < takeCount; i++)
            {
                newList.Add(list[i]);
            }

            return newList;
        }

        public IList Skip(Type newListType, IList list, int number)
        {
            IList newList = _reflectionInvoker.CreateEntityList(newListType);

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

        public void AddRange(IList list, IList anotherList)
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
    }
}
