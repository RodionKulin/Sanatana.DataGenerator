using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Sanatana.DataGenerator.TotalCountProviders;

namespace Sanatana.DataGenerator.Internals.EntitySettings
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


        //Factory
        public static class Factory
        {
            public static EntityContext Create(Dictionary<Type, IEntityDescription> allDescriptions,
                IEntityDescription description, DefaultSettings defaultSettings)
            {
                List<IEntityDescription> children = allDescriptions.Values
                    .Where(x => x.Required != null
                        && x.Required.Select(req => req.Type).Contains(description.Type))
                    .ToList();

                var parents = new List<IEntityDescription>();
                if (description.Required != null)
                {
                    IEnumerable<Type> parentTypes = description.Required.Select(x => x.Type);
                    parents = allDescriptions.Values
                       .Where(x => parentTypes.Contains(x.Type))
                       .ToList();
                }

                ITotalCountProvider totalCountProvider = defaultSettings.GetTotalCountProvider(description);
                long targetTotalCount = totalCountProvider.GetTargetCount();

                return new EntityContext
                {
                    Type = description.Type,
                    Description = description,
                    ChildEntities = children,
                    ParentEntities = parents,
                    EntityProgress = new EntityProgress
                    {
                        TargetCount = targetTotalCount
                    }
                };
            }
        }



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
