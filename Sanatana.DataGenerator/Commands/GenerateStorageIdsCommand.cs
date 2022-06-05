using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Objects;
using Sanatana.DataGenerator.Internals.Progress;
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
        protected FlushRange _generateIdsRange;


        //init
        public GenerateStorageIdsCommand(EntityContext entityContext, FlushRange generateIdsRange, GeneratorSetup setup)
        {
            _entityContext = entityContext;
            _generateIdsRange = generateIdsRange ?? throw new ArgumentNullException(nameof(generateIdsRange));
            _setup = setup;
        }


        //methods
        public virtual bool Execute()
        {
            List<IPersistentStorage> persistentStorages = _setup.Defaults.GetPersistentStorages(_entityContext.Description);
            _setup.TemporaryStorage.GenerateStorageIds(_entityContext, _generateIdsRange, persistentStorages);

            return true;
        }

        public virtual string GetDescription()
        {
            return $"Get storage Ids for {_entityContext.Type.Name} PreviousRangeFlushedCount={_generateIdsRange.PreviousRangeFlushedCount} ThisRangeFlushCount={_generateIdsRange.ThisRangeFlushCount}";
        }
    }
}
