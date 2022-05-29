using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.StorageInsertGuards;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    /// <summary>
    /// Randomly removes some percent of entity instances and inserts only remaining.
    /// </summary>
    public class DropOutStorageInsertGuard : IStorageInsertGuard
    {
        //fields
        protected double _dropOutChance;

        //init
        /// <summary>
        /// </summary>
        /// <param name="dropOutChance">dropOutChance between 0 and 1 to remove some portion on entity instances before inserting to persistent storage</param>
        public DropOutStorageInsertGuard(double dropOutChance)
        {
            if (dropOutChance < 0 || 1 < dropOutChance)
            {
                throw new ArgumentOutOfRangeException(nameof(dropOutChance), "Range should be between 0 and 1");
            }

            _dropOutChance = dropOutChance;
        }


        //methods
        public void PreventInsertion(EntityContext entityContext, IList nextItems)
        {
            for (int i = nextItems.Count - 1; i >= 0; i--)
            {
                bool drop = RandomPicker.NextBoolean(_dropOutChance);
                if (drop)
                {
                    nextItems.RemoveAt(i);
                }
            }
        }
    }
}
