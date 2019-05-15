using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.GenerationOrder
{
    public enum ActionType
    {
        /// <summary>
        /// Call generator for provided Type
        /// </summary>
        Generate,
        /// <summary>
        /// Insert entities to PersistentStorage and remove from Temporary storage
        /// </summary>
        FlushToPersistentStorare,
        /// <summary>
        /// Insert entities to PersistentStorage but do not remove from Temporary stora
        /// </summary>
        GenerateStorageIds,
        /// <summary>
        /// Finish generation
        /// </summary>
        Finish

    }
}
