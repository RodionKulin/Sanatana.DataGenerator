using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.QuantityProviders
{
    public class StrictQuantityProvider : IQuantityProvider
    {
        //properties
        public long TotalQuantity { get; protected set; }
        
        //init
        public StrictQuantityProvider(long totalQuantity)
        {
            if(totalQuantity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(totalQuantity), "Total quantity to generate should be greater than 0");
            }

            TotalQuantity = totalQuantity;
        }


        //methods
        public static implicit operator StrictQuantityProvider(long count)
        {
            return new StrictQuantityProvider(count);
        }

        public static implicit operator StrictQuantityProvider(int count)
        {
            return new StrictQuantityProvider(count);
        }


        public virtual long GetTargetTotalQuantity()
        {
            return TotalQuantity;
        }
    }
}
