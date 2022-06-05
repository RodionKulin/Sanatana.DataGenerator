using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;
using System;

namespace Sanatana.DataGenerator.Commands
{
    public class ReleaseFromTempStorageCommand : ICommand
    {
        //field
        protected EntityContext _entityContext;
        protected FlushRange _releaseRange;
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected string _invokedBy;


        //init
        public ReleaseFromTempStorageCommand(EntityContext entityContext, FlushRange releaseRange, GeneratorSetup setup,
            IFlushCandidatesRegistry flushCandidatesRegistry, string invokedBy)
        {
            _entityContext = entityContext;
            _releaseRange = releaseRange ?? throw new ArgumentNullException(nameof(releaseRange));
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
            _invokedBy = invokedBy;
        }


        //methods
        public virtual bool Execute()
        {
            _setup.TemporaryStorage.ReleaseFromTemporary(_entityContext, _releaseRange);

            _setup.Supervisor.EnqueueCommand(
                new CheckFlushRequiredCommand(_entityContext, _setup, _flushCandidatesRegistry));

            return true;
        }

        public virtual string GetDescription()
        {
            return $"Release from temp storage {_entityContext.Type.Name} ReleasedCount={_releaseRange.PreviousRangeFlushedCount} NextReleaseCount={_releaseRange.ThisRangeFlushCount} invokedBy={_invokedBy}";
        }
    }
}
