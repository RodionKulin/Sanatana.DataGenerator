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
            IPersistentStorage persistentStorage =
                _setup.GetPersistentStorage(_entityContext.Description);
            _setup.TemporaryStorage.GenerateStorageIds(_entityContext, persistentStorage);

            return true;
        }

    }
}
