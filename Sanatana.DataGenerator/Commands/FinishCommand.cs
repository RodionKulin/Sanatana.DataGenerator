using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.RequestCapacityProviders;

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
            var nextFlushEntities = new List<EntityContext>();

            do
            {
                foreach (EntityContext entityContext in nextFlushEntities)
                {
                    IRequestCapacityProvider requestCapacityProvider = _setup.Defaults.GetRequestCapacityProvider(entityContext.Description);
                    List<IPersistentStorage> storage = _setup.Defaults.GetPersistentStorages(entityContext.Description);
                    IFlushStrategy flushTrigger = _setup.Defaults.GetFlushStrategy(entityContext.Description);

                    long capacity = requestCapacityProvider.GetCapacity(entityContext);
                    flushTrigger.UpdateFlushRangeCapacity(entityContext, capacity);

                    //this is an async method, but waiting is done with TemporaryStorage.WaitAllTasks
                    _setup.TemporaryStorage.FlushToPersistent(entityContext, storage);
                    //or GenerateNewIds if it is entity with GenerateIds
                }

                _setup.TemporaryStorage.WaitAllTasks();

                nextFlushEntities = _entityContexts.Values
                    .Where(x => x.EntityProgress.HasNotReleasedRange())
                    .ToList();
            } while (nextFlushEntities.Count > 0);

            _setup.TemporaryStorage.WaitAllTasks();
        }

        protected virtual void CallDispose()
        {
            foreach (EntityContext entityContext in _entityContexts.Values)
            {
                List<IPersistentStorage> storages = _setup.Defaults.GetPersistentStorages(entityContext.Description);
                storages.ForEach(storage => storage.Dispose());

                entityContext.Dispose();
            }
        }

        public virtual string GetDescription()
        {
            string[] entityFlushStates = _entityContexts
                .Select(x => $"{x.Key.Name}={x.Value.EntityProgress.CurrentCount}")
                .ToArray();
            string entityFlushState = string.Join("; ", entityFlushStates);
            return $"Finish and flush remaining instances. CurrentCount {entityFlushState}";
        }
    }
}
