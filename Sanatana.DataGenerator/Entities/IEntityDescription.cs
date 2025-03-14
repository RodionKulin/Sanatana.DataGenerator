﻿using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Strategies;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGenerator.TargetCountProviders;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.RequestCapacityProviders;

namespace Sanatana.DataGenerator.Entities
{
    public interface IEntityDescription
    {
        Type Type { get; }
        List<RequiredEntity> Required { get; set; }
        IGenerator Generator { get; set; }
        List<IModifier> Modifiers { get; set; }
        public bool? KeepDefaultModifiers { get; set; }
        ITargetCountProvider TargetCountProvider { get; set; }
        List<IPersistentStorage> PersistentStorages { get; set; }
        IFlushStrategy FlushStrategy { get; set; }
        IRequestCapacityProvider RequestCapacityProvider { get; set; }
        IStorageInsertGuard StorageInsertGuard { get; set; }
        bool InsertToPersistentStorageBeforeUse { get; set; }
        IPersistentStorageSelector PersistentStorageSelector { get; set; }


        IEntityDescription Clone();
    }
}