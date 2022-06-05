using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGenerator.Commands
{
    public class CheckFlushRequiredCommand : ICommand
    { 
        //field
        protected EntityContext _entityContext;
        protected GeneratorSetup _setup;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;


        //init
        public CheckFlushRequiredCommand(EntityContext entityContext, GeneratorSetup setup,
             IFlushCandidatesRegistry flushCandidatesRegistry)
        {
            _entityContext = entityContext;
            _setup = setup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
        }


        //methods
        public virtual bool Execute()
        {
            EntityContext entityContext = _entityContext;

            bool flushRequired = _flushCandidatesRegistry.UpdateFlushRequired(entityContext);
            if (flushRequired)
            {
                List<ICommand> flushCommands = _flushCandidatesRegistry.GetNextFlushCommands(entityContext);
                flushCommands.ForEach(command => _setup.Supervisor.EnqueueCommand(command));
            }

            return true;
        }

        public virtual string GetDescription()
        {
            EntityProgress progress = _entityContext.EntityProgress;
            long flushedCount = _entityContext.EntityProgress.GetFlushedCount();
            return $"Check flush required for {_entityContext.Type.Name} CurrentCount={progress.CurrentCount} FlushedCount={flushedCount}";
        }
    }
}
