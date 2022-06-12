using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
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
        protected int _capacity;



        //init
        public StrictRequestCapacityProvider(int capacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be greater than 0");
            }
            
            _capacity = capacity;
        }



        //methods
        public int GetCapacity(EntityContext entityContext, FlushRange flushRange)
        {
            return _capacity;
        }

        public void TrackEntityGeneration(EntityContext entityContext, IList instances)
        {
        }

        public static implicit operator StrictRequestCapacityProvider(int capacity)
        {
            return new StrictRequestCapacityProvider(capacity);
        }
    }
}
