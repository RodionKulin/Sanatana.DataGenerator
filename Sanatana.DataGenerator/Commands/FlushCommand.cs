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
            List<IPersistentStorage> persistentStorages = _setup.GetPersistentStorages(EntityContext.Description);
            _setup.TemporaryStorage.FlushToPersistent(EntityContext, persistentStorages)
                .ContinueWith(prev =>
                {
                    _setup.Supervisor.EnqueueCommand(
                        new CheckFlushRequiredCommand(EntityContext, _setup, _flushCandidatesRegistry));
                });

            return true;
        }
    }
}
