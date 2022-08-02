using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Comparers
{
    public class KeyEqualityComparer<T, TKey> : IEqualityComparer<T>
    {
        protected readonly Func<T, TKey> _keyExtractor;

        public KeyEqualityComparer(Func<T, TKey> keyExtractor)
        {
            _keyExtractor = keyExtractor;
        }

        public virtual bool Equals(T x, T y)
        {
            return _keyExtractor(x).Equals(_keyExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            return _keyExtractor(obj).GetHashCode();
        }
    }
}
