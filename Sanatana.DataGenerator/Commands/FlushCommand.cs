using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Insert entity instances to Persistent storage and remove from Temporary storage
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
        public virtual bool Execute()
        {
            //set FlushStatus.FlushInProgress because it is an async operation and do not want to start it again while previous flush not ended
            _flushRange.SetFlushStatus(FlushStatus.FlushInProgress);

            List<IPersistentStorage> persistentStorages = _setup.Defaults.GetPersistentStorages(EntityContext.Description);
            _setup.TemporaryStorage.FlushToPersistent(EntityContext, _flushRange, persistentStorages)
                .ContinueWith(prev =>
                {
                    _setup.Supervisor.EnqueueCommand(new CheckFlushRequiredCommand(EntityContext, _setup, _flushCandidatesRegistry));
                });

            return true;
        }

        public virtual string GetDescription()
        {
            return $"Flush to persistent storage {EntityContext.Type.Name} PreviousRangeFlushedCount={_flushRange.PreviousRangeFlushedCount} ThisRangeFlushCount={_flushRange.ThisRangeFlushCount}";
        }
    }
}
