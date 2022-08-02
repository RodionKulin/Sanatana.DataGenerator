using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Iterator of sequential long Id per Type.
    /// </summary>
    public class IdIterator
    {
        //fields
        private static Dictionary<Type, long> _typeIds = new Dictionary<Type, long>();


        //methods
        public static long GetNextId<T>()
        {
            return GetNextId(typeof(T));
        }

        public static long GetNextId(Type type)
        {
            if (!_typeIds.ContainsKey(type))
            {
                _typeIds.Add(type, -1);
            }

            _typeIds[type]++;
            return _typeIds[type];
        }

        public static long GetCurrentId<T>()
        {
            return GetCurrentId(typeof(T));
        }

        public static long GetCurrentId(Type type)
        {
            if (!_typeIds.ContainsKey(type))
            {
                throw new IndexOutOfRangeException($"Have to call {nameof(GetNextId)} first time before {nameof(GetCurrentId)} will be available.");

            }

            return _typeIds[type];
        }

        public static void Reset()
        {
            _typeIds.Clear();
        }

        public static void Reset<T>()
        {
            Reset(typeof(T));
        }

        public static void Reset(Type type)
        {
            if (_typeIds.ContainsKey(type))
            {
                _typeIds.Remove(type);
            }
        }
    }
}
