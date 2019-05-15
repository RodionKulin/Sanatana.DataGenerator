using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public class DropOutPersistentStorage : IPersistentStorage
    {
        //fields
        protected IPersistentStorage _persistentStorage;
        protected double _dropOutChance;


        //init
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
                bool drop = RandomHelper.NextBoolean(_dropOutChance);
                if (drop)
                {
                    continue;
                }

                newList.Add(item);
            }

            return _persistentStorage.Insert(newList);
        }

        public void Dispose()
        {
            _persistentStorage.Dispose();
        }

    }
}
