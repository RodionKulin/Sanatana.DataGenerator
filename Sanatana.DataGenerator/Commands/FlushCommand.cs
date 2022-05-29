using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Insert entities to Persistent storage and remove from Temporary storage
    /// </summary>
    public class FlushCommand : ICommand
    { 
        //fields
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public FlushCommand(EntityContext entityContext, GeneratorSetup setup,
             IFlushCandidatesRegistry flushCandidatesRegistry)
        {
            EntityContext = entityContext;
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
        }


        //methods
        public virtual bool Execute()
        {
            //set IsFlushingInProgress=true because it is async operation and do not want to start it again while previous flush not ended
            EntityContext.EntityProgress.IsFlushInProgress = true;

            List<IPersistentStorage> persistentStorages = _setup.Defaults.GetPersistentStorages(EntityContext.Description);
            _setup.TemporaryStorage.FlushToPersistent(EntityContext, persistentStorages)
                .ContinueWith(prev =>
                {
                    EntityContext.EntityProgress.IsFlushInProgress = false;
                    _setup.Supervisor.EnqueueCommand(new CheckFlushRequiredCommand(EntityContext, _setup, _flushCandidatesRegistry));
                });

            return true;
        }

        public virtual string GetDescription()
        {
            EntityProgress progress = EntityContext.EntityProgress;
            return $"Flush to persistent storage {EntityContext.Type.Name} FlushedCount={progress.FlushedCount} NextFlushCount={progress.NextFlushCount}";
        }
    }
}
