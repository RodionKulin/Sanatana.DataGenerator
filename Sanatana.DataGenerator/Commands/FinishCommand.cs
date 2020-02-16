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
            List<EntityContext> nextFlushEntities = new List<EntityContext>();

            do
            {
                foreach (EntityContext entityContext in nextFlushEntities)
                {
                    List<IPersistentStorage> storage = _setup.GetPersistentStorages(entityContext.Description);
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
                    .Where(x => x.EntityProgress.ReleasedCount < x.EntityProgress.CurrentCount)
                    .ToList();
            } while (nextFlushEntities.Count > 0);

            _setup.TemporaryStorage.WaitAllTasks();
        }

        protected virtual void CallDispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                List<IPersistentStorage> storages = _setup.GetPersistentStorages(entityContext.Description);
                storages.ForEach(storage => storage.Dispose());

                entityContext.Dispose();
            }
        }
    }
}
