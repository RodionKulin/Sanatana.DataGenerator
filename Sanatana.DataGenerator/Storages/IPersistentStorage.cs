﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public interface IPersistentStorage : IDisposable
    {
        Task Insert<TEntity>(List<TEntity> instances)
            where TEntity : class;
    }
}
