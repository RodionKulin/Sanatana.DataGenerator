using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Strategies
{
    public class LimitedCapacityFlushStrategy : FlushStrategyBase
    {
        public LimitedCapacityFlushStrategy(long capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than 0");
            }
            
            _capacity = capacity;
        }

    }
}
