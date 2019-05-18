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
        /// Insert entities to Persistent storage and remove from Temporary storage
        /// </summary>
        FlushToPersistentStorage,
        /// <summary>
        /// Insert entities to Persistent storage but do not remove from Temporary storage
        /// </summary>
        GenerateStorageIds,
        /// <summary>
        /// Finish generation
        /// </summary>
        Finish

    }
}
