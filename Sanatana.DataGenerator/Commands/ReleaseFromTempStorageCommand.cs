using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Commands
{
    public class ReleaseFromTempStorageCommand : ICommand
    {
        //field
        protected EntityContext _entityContext;
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;


        //init
        public ReleaseFromTempStorageCommand(EntityContext entityContext, GeneratorSetup setup,
            IFlushCandidatesRegistry flushCandidatesRegistry)
        {
            _entityContext = entityContext;
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
        }


        //methods
        public virtual bool Execute()
        {
            _setup.TemporaryStorage.ReleaseFromTempStorage(_entityContext);

            _setup.Supervisor.EnqueueCommand(
                new CheckFlushRequiredCommand(_entityContext, _setup, _flushCandidatesRegistry));

            return true;
        }
    }
}
