using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;
using System;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Remove entity instances already flushed to persistent storage from temporary storage as well.
    /// Should be fired after GenerateStorageIdsCommand.
    /// </summary>
    public class ReleaseCommand : ICommand
    {
        //field
        protected EntityContext _entityContext;
        protected FlushRange _releaseRange;
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected string _invokedBy;


        //init
        public ReleaseCommand(EntityContext entityContext, FlushRange releaseRange, GeneratorSetup setup,
            IFlushCandidatesRegistry flushCandidatesRegistry, string invokedBy)
        {
            _entityContext = entityContext;
            _releaseRange = releaseRange ?? throw new ArgumentNullException(nameof(releaseRange));
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
            _invokedBy = invokedBy;
        }


        //methods
        public virtual void Execute()
        {
            _setup.TemporaryStorage.ReleaseFromTemporary(_entityContext, _releaseRange);

            _releaseRange.SetFlushStatus(FlushStatus.FlushedAndReleased);
            _entityContext.EntityProgress.RemoveRange(_releaseRange);
        }

        public virtual string GetLogEntry()
        {
            return $"Release from temp storage {_entityContext.Type.Name} ReleasedCount={_releaseRange.PreviousRangeFlushedCount} NextReleaseCount={_releaseRange.ThisRangeFlushCount} invokedBy={_invokedBy}";
        }
    }
}
