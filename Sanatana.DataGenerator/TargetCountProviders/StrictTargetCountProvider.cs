using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.TargetCountProviders
{
    public class StrictTargetCountProvider : ITargetCountProvider
    {
        //properties
        public long TargetCount { get; protected set; }
        

        //init
        public StrictTargetCountProvider(long targetCount)
        {
            if(targetCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(targetCount), "Total count to generate should be greater than 0");
            }

            TargetCount = targetCount;
        }


        //methods
        public static implicit operator StrictTargetCountProvider(long count)
        {
            return new StrictTargetCountProvider(count);
        }

        public static implicit operator StrictTargetCountProvider(int count)
        {
            return new StrictTargetCountProvider(count);
        }


        public virtual long GetTargetCount(IEntityDescription description, DefaultSettings defaults)
        {
            return TargetCount;
        }
    }
}
