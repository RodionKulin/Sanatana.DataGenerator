using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Objects;
using Sanatana.DataGenerator.Internals.Reflection;
using Sanatana.DataGenerator.StorageInsertGuards;
using Sanatana.DataGenerator.Storages;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Internals
{
    /// <summary>
    /// In memory storage for generated entities to accumulate some descent batches before inserting to persistent storage.
    /// </summary>
    public class TemporaryStorage
    {
        //fields
        protected int _maxTasksRunning = Environment.ProcessorCount;
        protected Dictionary<Type, IList> _entitiesAwaitingFlush;
        protected List<Task> _runningTasks;
        protected ReflectionInvoker _reflectionInvoker;
        protected ListOperations _listOperations;

        //props
        internal GeneratorSetup GeneratorSetup { get; set; }


        //properties
        /// <summary>
        /// Maximum running parallel tasks to insert entities into persistent storage.
        /// Default value equals to number of processors count.
        /// </summary>
        public int MaxTasksRunning
        {
            get { return _maxTasksRunning; }
            set
            {
                if (_maxTasksRunning < 1)
                {
                    throw new ArgumentOutOfRangeException($"{nameof(MaxTasksRunning)} can not be lower than 1.");
                }
                _maxTasksRunning = value;
            }
        }


        //init
        public TemporaryStorage()
        {
            _reflectionInvoker = new ReflectionInvoker();
            _listOperations = new ListOperations();
            _runningTasks = new List<Task>();
            _entitiesAwaitingFlush = new Dictionary<Type, IList>();
        }


        //Temporary storage command and query
        /// <summary>
        /// Select from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual object Select(EntityContext entityContext, long index)
        {
            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                throw new NullReferenceException($"No entities of type [{entityContext.Type}] are stored in {nameof(TemporaryStorage)}");
            }

            object itemAtIndex = null;
            entityContext.RunWithReadLock(() =>
            {
                IList list = _entitiesAwaitingFlush[entityContext.Type];

                int currentIndex = (int)(index - entityContext.EntityProgress.ReleasedCount);
                if (currentIndex < 0 || currentIndex >= list.Count)
                {
                    throw new IndexOutOfRangeException($"Index [{currentIndex}] was out of range for type [{entityContext.Type}] in {nameof(TemporaryStorage)}." +
                        $"Overall entity's index: {index}, " +
                        $"Flushed entities count: {entityContext.EntityProgress.FlushedCount}, " +
                        $"Removed from temp storage entities count: {entityContext.EntityProgress.ReleasedCount}, " +
                        $"Current index {currentIndex}, " +
                        $"Current {nameof(TemporaryStorage)} entities count: {list.Count}");
                }

                itemAtIndex = list[currentIndex];
            });

            return itemAtIndex;
        }

        /// <summary>
        /// Insert to temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="generatedEntities"></param>
        public virtual void InsertToTemporary(EntityContext entityContext, IList generatedEntities)
        {
            IList entitiesAwaitingFlush;
            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                entitiesAwaitingFlush = _listOperations.CreateEntityList(entityContext.Type);
                _entitiesAwaitingFlush.Add(entityContext.Type, entitiesAwaitingFlush);
            }

            entityContext.RunWithWriteLock(() =>
            {
                entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];
                _listOperations.AddRange(entitiesAwaitingFlush, generatedEntities);
            });
        }


        //Flush to persistent
        /// <summary>
        /// Insert entities to persistent storage and remove from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="storages"></param>
        public virtual Task FlushToPersistent(EntityContext entityContext, List<IPersistentStorage> storages)
        {
            WaitRunningTask();
            Task[] nextFlushTasks = FlushToPersistentTask(entityContext, storages);
            _runningTasks.AddRange(nextFlushTasks);

            return Task.WhenAll(nextFlushTasks);
        }

        protected virtual Task[] FlushToPersistentTask(EntityContext entityContext, List<IPersistentStorage> storages)
        {
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return new Task[0];
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithWriteLock(() =>
            {
                IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];

                FlushRange nextRange = progress.GetNextFlushRange();
                long numberToFlush = nextRange.FlushRequestCapacity;

                //take number of items to flush
                nextItems = _listOperations
                    .Take(entityContext.Type, entitiesAwaitingFlush, (int)numberToFlush);
                
                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToFlush);

                //update progess
                nextRange.SetFlushedAndReleased();
            });

            IStorageInsertGuard guard = entityContext.Description.StorageInsertGuard;
            if (guard != null)
            {
                guard.PreventInsertion(entityContext, nextItems);
            }
            if (nextItems.Count == 0)
            {
                return new Task[0];
            }

            return storages
                .Select(storage => _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems))
                .ToArray();
        }


        //InsertToPersistentStorageBeforeUse
        /// <summary>
        /// Insert entities to persistent storage but keep in temporary storage
        /// </summary>
        public virtual void GenerateStorageIds(EntityContext entityContext, List<IPersistentStorage> storages)
        {
            //Must be a sync operation, so inserted Ids are available immediatly
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return;
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithReadLock(() =>
            {
                IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];

                FlushRange nextRange = progress.GetNextFlushRange();

                long numberToSkip = progress.FlushedCount - progress.ReleasedCount;
                long numberToFlush = nextRange.FlushRequestCapacity;

                //skip entities that already were flushed
                nextItems = _listOperations.Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToSkip);

                //take number of items to flush
                nextItems = _listOperations.Take(entityContext.Type, nextItems, (int)numberToFlush);

                //update progress
                nextRange.SetFlushedAndReleased();
                progress.AddFlushedCount(nextItems.Count);
                
            });

            if (nextItems.Count == 0)
            {
                return;
            }

            storages.ForEach(storage =>
            {
                _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems).Wait();
            });
        }

        public virtual void ReleaseFromTemporary(EntityContext entityContext)
        {
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return;
            }

            //lock operations on list for same Type
            entityContext.RunWithWriteLock(() =>
            {
                IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];

                FlushRange nextRange = progress.GetNextReleaseRange();
                long numberToRemove = nextRange.FlushRequestCapacity;

                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToRemove);

                //update progress
                nextRange.SetReleased();
            });
        }

                

        //Tasks handling
        protected virtual void WaitRunningTask()
        {
            _runningTasks.RemoveAll(p => p.IsCompleted);

            bool isMaxTasksRunning = _runningTasks.Count >= MaxTasksRunning;
            if (isMaxTasksRunning)
            {
                int finishedIndex = Task.WaitAny(_runningTasks.ToArray());
                _runningTasks.RemoveAll(p => p.IsCompleted);
            }
        }

        public virtual void WaitAllTasks()
        {
            _runningTasks.RemoveAll(p => p.IsCompleted);
            Task.WaitAll(_runningTasks.ToArray());
            _runningTasks.RemoveAll(p => p.IsCompleted);
        }
    }
}
