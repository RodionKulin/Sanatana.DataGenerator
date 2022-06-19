using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
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
        protected GeneratorServices _generatorServices;
        protected FlushRange _generateIdsRange;


        //init
        public GenerateStorageIdsCommand(EntityContext entityContext, FlushRange generateIdsRange, GeneratorServices generatorServices)
        {
            _entityContext = entityContext;
            _generateIdsRange = generateIdsRange ?? throw new ArgumentNullException(nameof(generateIdsRange));
            _generatorServices = generatorServices;
        }


        //methods
        public virtual void Execute()
        {
            List<IPersistentStorage> persistentStorages = _generatorServices.Defaults.GetPersistentStorages(_entityContext.Description);
            _generatorServices.TemporaryStorage.GenerateStorageIds(_entityContext, _generateIdsRange, persistentStorages);

            _generateIdsRange.SetFlushStatus(FlushStatus.Flushed);
        }

        public virtual string GetLogEntry()
        {
            return $"Get storage Ids for {_entityContext.Type.FullName} PreviousRangeFlushedCount={_generateIdsRange.PreviousRangeFlushedCount} ThisRangeFlushCount={_generateIdsRange.ThisRangeFlushCount}";
        }
    }
}
