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
        IGenerator Generator { get; }
        List<IModifier> Modifiers { get; set; }
        IQuantityProvider QuantityProvider { get; }
        IPersistentStorage PersistentStorage { get; }
        IFlushStrategy FlushTrigger { get; }
        bool InsertToPersistentStorageBeforeUse { get; set; }
    }
}