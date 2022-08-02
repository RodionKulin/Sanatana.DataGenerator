using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.CombineStrategies.ProportionalCombine
{
    public class Chunk
    {
        //properties
        public long? Proportion { get; set; }
        public long Absolute { get; set; }
        public virtual bool IsAbsolute
        {
            get { return Proportion == null; }
        }


        //init
        /// <summary>
        /// Absolute number of instances that need to be processed.
        /// It will be used following way:
        /// 1. Chunks with absolute number of instances get summed. Sum of absolute counts should not be higher, then TargetCount for entity;
        /// 2. Remaining number of instances is counted. RemainingCount = TargetCount - AbsoluteCountSum;
        /// 3. RemainingCount get splitted among proportional Chunks.
        /// </summary>
        /// <param name="absoluteCount"></param>
        /// <returns></returns>
        public static Chunk FromAbsolute(long absoluteCount)
        {
            const long minValue = 1;
            if (absoluteCount < minValue)
            {
                throw new ArgumentException($"Chunk {nameof(absoluteCount)} value can not be less then {minValue}.");
            }

            return new Chunk()
            {
                Absolute = absoluteCount
            };
        }

        /// <summary>
        /// Proportional number of instances that need to be processed.
        /// It will be used following way:
        /// 1. Chunks with absolute number of instances get summed. Sum of absolute counts should not be higher, then TargetCount for entity;
        /// 2. Remaining number of instances is counted. RemainingCount = TargetCount - AbsoluteCountSum;
        /// 3. RemainingCount get splitted among proportional Chunks.
        /// </summary>
        /// <param name="proportion"></param>
        /// <returns></returns>
        public static Chunk FromProportion(long proportion)
        {
            const long minValue = 0;
            if (proportion <= minValue)
            {
                throw new ArgumentException($"Chunk {nameof(proportion)} value can not be less then or equal to {minValue}.");
            }

            return new Chunk()
            {
                Proportion = proportion
            };
        }

        /// <summary>
        /// Convert string to Chunk. Allowed formats are:
        /// "\d"  for Absolute
        /// "\d*" for Proportion
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Chunk FromString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(nameof(value));
            }

            bool isProportion = false;
            if (value.EndsWith("*"))
            {
                value = value.Substring(0, value.Length - 1);
                isProportion = true;
            }

            long valueNumber;
            if (!long.TryParse(value, out valueNumber))
            {
                string chunkType = isProportion ? "Proportion" : "Absolute number";
                throw new ArgumentException($"{chunkType} value {value} is not a valid number.", nameof(value));
            }

            return isProportion
                ? FromProportion(valueNumber)
                : FromAbsolute(valueNumber);
        }


        //Clone
        public virtual Chunk Clone()
        {
            return new Chunk()
            {
                Proportion = Proportion,
                Absolute = Absolute
            };
        }
    }
}
