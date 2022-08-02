using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Commands
{
    /// <summary>
    /// Insert entity instances to persistent storage and remove from temporary storage.
    /// </summary>
    public class FlushCommand : ICommand
    { 
        //fields
        protected GeneratorServices _generatorServices;
        protected FlushRange _flushRange;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public FlushCommand(EntityContext entityContext, FlushRange flushRange, GeneratorServices generatorServices)
        {
            EntityContext = entityContext;
            _flushRange = flushRange ?? throw new ArgumentNullException(nameof(flushRange));
            _generatorServices = generatorServices;
        }


        //methods
        public virtual void Execute()
        {
            List<IPersistentStorage> persistentStorages = _generatorServices.Defaults.GetPersistentStorages(EntityContext.Description);
            _generatorServices.TemporaryStorage.FlushToPersistent(EntityContext, _flushRange, persistentStorages);

            _flushRange.SetFlushStatus(FlushStatus.FlushedAndReleased);
            EntityContext.EntityProgress.RemoveRange(_flushRange);
        }

        public virtual string GetLogEntry()
        {
            return $"Flush to persistent storage {EntityContext.Type.FullName} PreviousRangeFlushedCount={_flushRange.PreviousRangeFlushedCount} ThisRangeFlushCount={_flushRange.ThisRangeFlushCount}";
        }
    }
}
