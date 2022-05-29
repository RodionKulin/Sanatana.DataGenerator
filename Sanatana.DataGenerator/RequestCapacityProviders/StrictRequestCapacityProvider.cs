using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.RequestCapacityProviders
{
    /// <summary>
    /// Returns static capacity number of entity instances that can be inserted with next request to persistent storage.
    /// </summary>
    public class StrictRequestCapacityProvider : IRequestCapacityProvider
    {
        //properties
        protected long _capacity;



        //init
        public StrictRequestCapacityProvider(long capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than 0");
            }
            
            _capacity = capacity;
        }



        //methods
        public long GetCapacity(EntityContext entityContext)
        {
            return _capacity;
        }

        public void TrackEntityGeneration(EntityContext entityContext, IList instances)
        {
        }

        public static implicit operator StrictRequestCapacityProvider(long capacity)
        {
            return new StrictRequestCapacityProvider(capacity);
        }

        public static implicit operator StrictRequestCapacityProvider(int capacity)
        {
            return new StrictRequestCapacityProvider(capacity);
        }
    }
}
