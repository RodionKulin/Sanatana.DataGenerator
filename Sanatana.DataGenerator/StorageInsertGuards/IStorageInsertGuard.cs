using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.StorageInsertGuards
{
    public interface IStorageInsertGuard
    {
        void PreventInsertion(EntityContext entityContext, IList nextItems);
    }
}
