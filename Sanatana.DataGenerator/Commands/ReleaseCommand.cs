using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using Sanatana.DataGenerator.Internals.EntitySettings;

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
        protected GeneratorServices _generatorServices;
        protected string _invokedBy;


        //init
        public ReleaseCommand(EntityContext entityContext, FlushRange releaseRange, GeneratorServices generatorServices,
            string invokedBy)
        {
            _entityContext = entityContext;
            _releaseRange = releaseRange ?? throw new ArgumentNullException(nameof(releaseRange));
            _generatorServices = generatorServices;
            _invokedBy = invokedBy;
        }


        //methods
        public virtual void Execute()
        {
            _generatorServices.TemporaryStorage.ReleaseFromTemporary(_entityContext, _releaseRange);

            _releaseRange.SetFlushStatus(FlushStatus.FlushedAndReleased);
            _entityContext.EntityProgress.RemoveRange(_releaseRange);
        }

        public virtual string GetLogEntry()
        {
            return $"Release from temp storage {_entityContext.Type.FullName} ReleasedCount={_releaseRange.PreviousRangeFlushedCount} NextReleaseCount={_releaseRange.ThisRangeFlushCount} invokedBy={_invokedBy}";
        }
    }
}
