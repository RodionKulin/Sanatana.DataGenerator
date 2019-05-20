using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sanatana.DataGenerator.Internals
{
    public class EntityContext : IDisposable
    {
        //fields
        /// <summary>
        /// Lock operations on Temp Storage list for same Entity type
        /// </summary>
        protected ReaderWriterLockSlim _tempStorageLock = new ReaderWriterLockSlim();


        //properties
        public Type Type { get; set; }
        public IEntityDescription Description { get; set; }
        public EntityProgress EntityProgress { get; set; }
        public List<IEntityDescription> ChildEntities { get; set; }
        public List<IEntityDescription> ParentEntities { get; set; }



        //methods
        public virtual void RunWithReadLock(Action action)
        {
            try
            {
                _tempStorageLock.EnterReadLock();
                action();
            }
            finally
            {
                _tempStorageLock.ExitReadLock();
            }
        }

        public virtual void RunWithWriteLock(Action action)
        {
            try
            {
                _tempStorageLock.EnterWriteLock();
                action();
            }
            finally
            {
                _tempStorageLock.ExitWriteLock();
            }
        }

        public virtual void Dispose()
        {
            _tempStorageLock.Dispose();
        }
    }
}
