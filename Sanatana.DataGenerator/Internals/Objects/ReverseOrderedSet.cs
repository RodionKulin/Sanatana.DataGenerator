using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Sanatana.DataGenerator.Internals.Objects
{
    public class ReverseOrderedSet<T> : ICollection<T>
    {
        //fields
        private readonly IDictionary<T, LinkedListNode<T>> m_Dictionary;
        private readonly LinkedList<T> m_LinkedList;


        //properties
        public int Count => m_Dictionary.Count;
        public virtual bool IsReadOnly => m_Dictionary.IsReadOnly;


        //init
        public ReverseOrderedSet()
            : this(EqualityComparer<T>.Default)
        {
        }

        public ReverseOrderedSet(IEqualityComparer<T> comparer)
        {
            m_Dictionary = new Dictionary<T, LinkedListNode<T>>(comparer);
            m_LinkedList = new LinkedList<T>();

        }


        //methods
        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        public bool Add(T item)
        {
            if (m_Dictionary.ContainsKey(item)) 
                return false;
            LinkedListNode<T> node = m_LinkedList.AddLast(item);
            m_Dictionary.Add(item, node);
            return true;
        }

        public void Clear()
        {
            m_LinkedList.Clear();
            m_Dictionary.Clear();
        }

        public bool Remove(T item)
        {
            if (item == null) 
                return false;

            bool found = m_Dictionary.TryGetValue(item, out LinkedListNode<T> node);
            if (!found) 
                return false;

            m_Dictionary.Remove(item);
            m_LinkedList.Remove(node);
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            LinkedListNode<T> node = m_LinkedList.Last;
            while (true)
            {
                if (node == null)
                {
                    yield break;
                }

                yield return node.Value;
                node = node.Previous;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Contains(T item)
        {
            return item != null && m_Dictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_LinkedList.CopyTo(array, arrayIndex);
        }

    }
}
