using System;
using System.Collections.Generic;
using System.Text;
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
        public IList Take(EntityContext entityContext, IList list, int number)
        {
            IList newList = _reflectionInvoker.CreateEntityList(entityContext.Type);

            //take larger than list length with take all
            int takeCount = Math.Min(number, list.Count);

            //take negative number of 0 will return empty list
            for (int i = 0; i < takeCount; i++)
            {
                newList.Add(list[i]);
            }

            return newList;
        }

        public IList Skip(EntityContext entityContext, IList list, int number)
        {
            IList newList = _reflectionInvoker.CreateEntityList(entityContext.Type);

            //skip negative number will behave as skip 0 and return full list
            int skipIndex = number - 1;
            skipIndex = Math.Max(skipIndex, 0);

            //take larger than list length with return empty list
            for (int i = skipIndex; i < list.Count; i++)
            {
                newList.Add(list[i]);
            }

            return newList;
        }

    }
}
