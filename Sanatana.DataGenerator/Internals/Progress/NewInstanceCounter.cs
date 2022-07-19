using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public class NewInstanceCounter
    {
        protected List<long> _newInstancesCountPointsInTime;


        //init
        public NewInstanceCounter()
        {
            _newInstancesCountPointsInTime = new List<long>();
        }


        //methods
        /// <summary>
        /// Count number of new instances generated, that dont exist in persistent storage yet.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="exists"></param>
        public virtual void TrackNewInstances(GeneratorContext context, List<bool> exists)
        {
            for (int i = 0; i < exists.Count; i++)
            {
                if (!exists[i])
                {
                    //Entity context.CurrentCount is incremented after generation, so on first entity instance it will be 0.
                    //"exists" var i value is 0-starting.
                    //Convert this 0-starting count to 1-starting count.
                    _newInstancesCountPointsInTime.Add(context.CurrentCount + i + 1);
                }
            }
        }

        /// <summary>
        /// Return number of new instances generated and stored in TempStorage.
        /// </summary>
        /// <param name="latestFlushedCount">number of instances flush to persistent storage (1-starting count)</param>
        /// <returns></returns>
        public virtual long GetNewInstanceCount(long latestFlushedCount)
        {
            return _newInstancesCountPointsInTime
                .Where(x => x > latestFlushedCount)
                .Count();
        }

        /// <summary>
        /// Remove previous history rows with number of new instances for range of instances already flushed to persistent storage.
        /// </summary>
        /// <param name="latestFlushedCount">number of instances flush to persistent storage (1-starting count)</param>
        public virtual void RemoveHistoryRecords(long latestFlushedCount)
        {
            _newInstancesCountPointsInTime = _newInstancesCountPointsInTime
                .Where(x => x <= latestFlushedCount)
                .ToList();
        }
    
        /// <summary>
        /// Reset new instance count.
        /// </summary>
        public virtual void Setup()
        {
            _newInstancesCountPointsInTime.Clear();
        }
    }
}
