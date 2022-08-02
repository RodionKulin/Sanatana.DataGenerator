using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.CombineStrategies.ProportionalCombine
{
    public class ChunkRange
    {
        public long StartIndexInclusive { get; set; }
        public long EndIndexExclusive { get; set; }
        public int ChunkIndex { get; set; }
        public Chunk Chunk { get; set; }
    }
}
