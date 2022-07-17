using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Each generator is called in equal portions and in circular order.
    /// </summary>
    public class RoundRobinCombineStrategy : ICombineStrategy
    {
        public int GetNext(int generatorsCount, long instancesCurrentCount)
        {
            return (int)(instancesCurrentCount % generatorsCount);
        }
    }
}
