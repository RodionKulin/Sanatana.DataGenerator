using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Insert entity instances to persistent storage and remove from temporary storage.
    /// </summary>
    public class FlushCommand : ICommand
    { 
        //fields
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected FlushRange _flushRange;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public FlushCommand(EntityContext entityContext, FlushRange flushRange, GeneratorSetup setup,
             IFlushCandidatesRegistry flushCandidatesRegistry)
        {
            EntityContext = entityContext;
            _flushRange = flushRange ?? throw new ArgumentNullException(nameof(flushRange));
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
        }


        //methods
        public virtual void Execute()
        {
            List<IPersistentStorage> persistentStorages = _setup.Defaults.GetPersistentStorages(EntityContext.Description);
            _setup.TemporaryStorage.FlushToPersistent(EntityContext, _flushRange, persistentStorages);

            _flushRange.SetFlushStatus(FlushStatus.FlushedAndReleased);
            EntityContext.EntityProgress.RemoveRange(_flushRange);
        }

        public virtual string GetLogEntry()
        {
            return $"Flush to persistent storage {EntityContext.Type.Name} PreviousRangeFlushedCount={_flushRange.PreviousRangeFlushedCount} ThisRangeFlushCount={_flushRange.ThisRangeFlushCount}";
        }
    }
}
