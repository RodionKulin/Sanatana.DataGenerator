using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sanatana.DataGenerator
{
    /// <summary>
    /// Random pick methods
    /// </summary>
    public class RandomPicker
    {
        //fields
        private static Lazy<Random> _globalSeed =
            new Lazy<Random>(() => new Random(Environment.TickCount));
        private static ThreadLocal<Random> _randomWrapper = new ThreadLocal<Random>(() =>
            new Random(GenerateSeed())
        );

        //properties
        /// <summary>
        /// Thread local random instance
        /// </summary>
        public static Random Random
        {
            get
            {
                return _randomWrapper.Value;
            }
        }


        //init
        private static int GenerateSeed()
        {
            lock (_globalSeed)
            {
                int seed = _globalSeed.Value.Next();
                return seed;
            }
        }


        //methods
        public static bool NextBoolean(double trueChance)
        {
            double next = Random.NextDouble();
            return next <= trueChance;
        }

        public static int PickOneFromRange(Tuple<int, int> range)
        {
            return PickOneFromRange(range.Item1, range.Item2);
        }

        public static int PickOneFromRange(int[] range)
        {
            return PickOneFromRange(range[0], range[1]);
        }

        public static int PickOneFromRange(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        public static List<int> PickManyFromRange(int minValue, int maxValue,
            int count, bool uniqueValues)
        {
            int rangeLength = maxValue - minValue;
           
            return PickMany<int>(count, uniqueValues,
                () => PickOneFromRange(minValue, maxValue)
                , minValue, maxValue, rangeLength);
        }

        public static DateTime PickOneFromRange(DateTime minValue, DateTime maxValue)
        {
            TimeSpan range = maxValue - minValue;
            double randTicks = Random.NextDouble() * range.Ticks;
            var randTimeSpan = new TimeSpan((long)randTicks);
            return minValue + randTimeSpan;
        }

        public static List<DateTime> PickManyFromRange(DateTime minValue, DateTime maxValue,
            int count, bool uniqueValues)
        {
            long rangeLength = maxValue.Ticks - minValue.Ticks;
            
            return PickMany<DateTime>(count, uniqueValues,
                () => PickOneFromRange(minValue, maxValue)
                , minValue, maxValue, rangeLength);
        }

        public static T PickOneFormList<T>(List<T> items)
        {
            int index = Random.Next(items.Count);
            return items[index];
        }

        public static List<T> PickManyFromList<T>(List<T> items, int count, bool uniqueValues)
        {
            if (uniqueValues && items.Count < count)
            {
                throw new Exception($"Items count [{items.Count}]  is not enough to generate {count} unique values");
            }

            var usedList = new List<T>(items);
            var result = new List<T>();

            while (result.Count < count)
            {
                int index = Random.Next(usedList.Count);
                T value = usedList[index];
                result.Add(value);

                if (uniqueValues)
                {
                    usedList.RemoveAt(index);
                }
            }

            return result;
        }

        private static List<T> PickMany<T>(int count, bool uniqueValues, Func<T> nextGenerator,
            T minValue, T maxValue, long rangeLength)
            where T : IComparable<T>
        {
            int compareResult = Comparer<T>.Default.Compare(minValue, maxValue);

            if (compareResult > 0)
            {
                throw new Exception($"Min value of [{minValue}] is greater than Max value [{maxValue}]");
            }

            if (uniqueValues && rangeLength < count)
            {
                throw new Exception($"Values range between [{minValue}] and [{maxValue}] is not enough to generate {count} unique values");
            }

            var result = new List<T>();

            while (result.Count < count)
            {
                T value = nextGenerator();

                if (uniqueValues && result.Contains(value))
                {
                    continue;
                }

                result.Add(value);
            }

            return result;
        }
    }
}
