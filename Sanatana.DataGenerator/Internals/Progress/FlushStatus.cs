using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Progress
{
    public enum FlushStatus : long
    {
        /// <summary>
        /// FlushRange has not generated enough instances in TempStorage to flush them to persistent storage. 
        /// </summary>
        FlushNotRequired,
        /// <summary>
        /// FlushRange has enough instances in TempStorage to flush them to persistent storage. 
        /// If child entity can be generated from parent entity in TempStorage, then IsFlushRequired won't be checked untill child used all parent entity instances.
        /// </summary>
        FlushRequired,
        /// <summary>
        /// Indicates if FlushCommand or GenerateStorageIdsCommand was enqueued for FlushRange.
        /// </summary>
        FlushInProgress,
        /// <summary>
        /// FlushRange is considered flushed, when instances were inserted to persistent storage and database generated columns were set on instances.
        /// This happens 1. After all child entities were generated from FlushRange or 2. If InsertToPersistentStorageBeforeUse=true and capacity for flush request is reached.
        /// </summary>
        Flushed,
        /// <summary>
        /// Release is removing instances from TempStorage.
        /// If instances are released, they no longer can be used by child entities to generate. 
        /// </summary>
        FlushedAndReleased
    }
}
