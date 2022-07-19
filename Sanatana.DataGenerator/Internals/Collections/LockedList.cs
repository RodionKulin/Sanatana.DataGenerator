using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Internals.Collections
{
    public class LockedList<T>: ILockedList
    {
        private object _listLock = new object();
        private List<T> _list = new List<T>();


        //methods
        public void Add(T item)
        {
            lock (_listLock)
            {
                _list.Add(item);
            }
        }

        public void AddRange(List<T> items)
        {
            lock (_listLock)
            {
                _list.AddRange(items);
            }
        }

        public void Clear()
        {
            lock (_listLock)
            {
                _list.Clear();
            }
        }

        public List<T> SelectGeneric()
        {
            return new List<T>(_list);
        }

        public IList SelectNonGeneric()
        {
            return new List<T>(_list);
        }
    }

    public interface ILockedList
    {
        IList SelectNonGeneric();
    }
}
