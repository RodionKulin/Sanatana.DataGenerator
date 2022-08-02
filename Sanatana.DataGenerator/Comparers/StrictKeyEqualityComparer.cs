using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Comparers
{
    public class StrictKeyEqualityComparer<T, TKey> : KeyEqualityComparer<T, TKey>
        where TKey : IEquatable<TKey>
    {
        public StrictKeyEqualityComparer(Func<T, TKey> keyExtractor)
            : base(keyExtractor)
        {
        }

        public override bool Equals(T x, T y)
        {
            // This will use the overload that accepts a TKey parameter
            // instead of an object parameter.
            return _keyExtractor(x).Equals(_keyExtractor(y));
        }
    }
}
