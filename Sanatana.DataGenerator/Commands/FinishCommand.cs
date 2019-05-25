using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Strategies;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Finish generation and dispose resources
    /// </summary>
    public class FinishCommand : ICommand
    {
        //field
        protected GeneratorSetup _setup;
        protected Dictionary<Type, EntityContext> _entityContexts;


        //init
        public FinishCommand(GeneratorSetup setup,
            Dictionary<Type, EntityContext> entityContexts)
        {
            _setup = setup;
            _entityContexts = entityContexts;
        }


        //methods
        public virtual bool Execute()
        {
            FlushTempStorage();
            CallDispose();

            return false;
        }

        protected virtual void FlushTempStorage()
        {
            List<EntityContext> nextFlushEntities = _entityContexts.Values
                .Where(x => x.EntityProgress.CurrentCount < x.EntityProgress.TargetCount)
                .ToList();

            while (nextFlushEntities.Count > 0)
            {
                nextFlushEntities = nextFlushEntities
                    .Where(x => x.EntityProgress.IsFlushInProgress() == false)
                    .ToList();

                foreach (EntityContext entityContext in nextFlushEntities)
                {
                    IPersistentStorage storage = _setup.GetPersistentStorage(entityContext.Description);
                    IFlushStrategy flushTrigger = _setup.GetFlushTrigger(entityContext.Description);

                    flushTrigger.SetNextFlushCount(entityContext);
                    flushTrigger.SetNextReleaseCount(entityContext);

                    if (entityContext.Description.InsertToPersistentStorageBeforeUse)
                    {
                        _setup.TemporaryStorage.GenerateStorageIds(entityContext, storage);
                    }

                    _setup.TemporaryStorage.FlushToPersistent(entityContext, storage);
                }

                _setup.TemporaryStorage.WaitAllTasks();
                nextFlushEntities = _entityContexts.Values
                    .Where(x => x.EntityProgress.CurrentCount < x.EntityProgress.TargetCount)
                    .ToList();
            }
        }

        protected virtual void CallDispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                IPersistentStorage storage = _setup.GetPersistentStorage(entityContext.Description);
                storage.Dispose();

                entityContext.Dispose();
            }
        }
    }
}
