using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Insert entities to Persistent storage to get generated ids and keep in Temporary storage
    /// </summary>
    public class GenerateStorageIdsCommand : ICommand
    {
        //field
        protected EntityContext _entityContext;
        protected GeneratorSetup _setup;


        //init
        public GenerateStorageIdsCommand(EntityContext entityContext, GeneratorSetup setup)
        {
            _entityContext = entityContext;
            _setup = setup;
        }


        //methods
        public virtual bool Execute()
        {
            List<IPersistentStorage> persistentStorages = _setup.Defaults.GetPersistentStorages(_entityContext.Description);
            _setup.TemporaryStorage.GenerateStorageIds(_entityContext, persistentStorages);

            return true;
        }

        public virtual string GetDescription()
        {
            EntityProgress progress = _entityContext.EntityProgress;
            return $"Get storage Ids for {_entityContext.Type.Name} FlushedCount={progress.FlushedCount} NextFlushCount={progress.NextFlushCount}";
        }
    }
}
