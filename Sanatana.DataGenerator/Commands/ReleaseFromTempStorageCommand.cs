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
        protected string _invokedBy;


        //init
        public ReleaseFromTempStorageCommand(EntityContext entityContext, GeneratorSetup setup,
            IFlushCandidatesRegistry flushCandidatesRegistry, string invokedBy)
        {
            _entityContext = entityContext;
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
            _invokedBy = invokedBy;
        }


        //methods
        public virtual bool Execute()
        {
            _setup.TemporaryStorage.ReleaseFromTemporary(_entityContext);

            _setup.Supervisor.EnqueueCommand(
                new CheckFlushRequiredCommand(_entityContext, _setup, _flushCandidatesRegistry));

            return true;
        }

        public virtual string GetDescription()
        {
            EntityProgress progress = _entityContext.EntityProgress;
            return $"Release from temp storage {_entityContext.Type.Name} ReleasedCount={progress.ReleasedCount} NextReleaseCount={progress.NextReleaseCount} invokedBy={_invokedBy}";
        }
    }
}
