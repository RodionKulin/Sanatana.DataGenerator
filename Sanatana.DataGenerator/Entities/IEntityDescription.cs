using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.QuantityProviders;
using Sanatana.DataGenerator.Storages;

namespace Sanatana.DataGenerator.Entities
{
    public interface IEntityDescription
    {
        Type Type { get; }
        List<RequiredEntity> Required { get; set; }
        IGenerator Generator { get; set; }
        List<IModifier> Modifiers { get; set; }
        IQuantityProvider QuantityProvider { get; set; }
        List<IPersistentStorage> PersistentStorages { get; set; }
        IFlushStrategy FlushTrigger { get; set; }
        bool InsertToPersistentStorageBeforeUse { get; set; }
    }
}