using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.FlushTriggers
{
    public class LimitedCapacityFlushTrigger : FlushTriggerBase
    {
        public LimitedCapacityFlushTrigger(long capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than 0");
            }
            
            _capacity = capacity;
        }

    }
}
