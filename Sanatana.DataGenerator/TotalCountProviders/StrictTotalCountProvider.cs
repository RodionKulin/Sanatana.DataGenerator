using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.TotalCountProviders
{
    public class StrictTotalCountProvider : ITotalCountProvider
    {
        //properties
        public long TotalCount { get; protected set; }
        

        //init
        public StrictTotalCountProvider(long totalCount)
        {
            if(totalCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalCount), "Total count to generate should be greater than 0");
            }

            TotalCount = totalCount;
        }


        //methods
        public static implicit operator StrictTotalCountProvider(long count)
        {
            return new StrictTotalCountProvider(count);
        }

        public static implicit operator StrictTotalCountProvider(int count)
        {
            return new StrictTotalCountProvider(count);
        }


        public virtual long GetTargetCount(IEntityDescription description, DefaultSettings defaults)
        {
            return TotalCount;
        }
    }
}
