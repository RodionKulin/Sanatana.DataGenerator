using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.QuantityProviders
{
    public interface IQuantityProvider
    {
        long GetTargetTotalQuantity();
    }
}
