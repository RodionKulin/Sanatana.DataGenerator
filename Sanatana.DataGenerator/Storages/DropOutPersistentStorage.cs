using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    /// <summary>
    /// Randomly removes some percent of entities and inserts only remaining
    /// </summary>
    public class DropOutPersistentStorage : IPersistentStorage
    {
        //fields
        protected IPersistentStorage _persistentStorage;
        protected double _dropOutChance;


        //init
        /// <summary>
        /// Initialize with another persistent storage that will actually insert entities and a dropOutChance between 0 and 1 to remove some portion on entities.
        /// </summary>
        /// <param name="persistentStorage"></param>
        /// <param name="dropOutChance"></param>
        public DropOutPersistentStorage(IPersistentStorage persistentStorage, double dropOutChance)
        {
            if (dropOutChance < 0 || 1 < dropOutChance)
            {
                throw new ArgumentOutOfRangeException(nameof(dropOutChance), "Range should be between 0 and 1");
            }

            _persistentStorage = persistentStorage;
            _dropOutChance = dropOutChance;
        }


        //methods
        public Task Insert<TEntity>(List<TEntity> entities)
            where TEntity : class
        {
            List<TEntity> newList = new List<TEntity>();
            foreach (TEntity item in entities)
            {
                bool drop = RandomPicker.NextBoolean(_dropOutChance);
                if (drop)
                {
                    continue;
                }

                newList.Add(item);
            }

            return _persistentStorage.Insert(newList);
        }

        public virtual void Dispose()
        {
            _persistentStorage.Dispose();
        }

    }
}
