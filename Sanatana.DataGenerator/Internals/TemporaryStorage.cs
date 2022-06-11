using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.Progress;
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
    /// Inmemory storage for generated entity instances to accumulate batches before inserting to persistent storage.
    /// </summary>
    public class TemporaryStorage
    {
        //fields
        protected int _maxTasksRunning = Environment.ProcessorCount;
        protected ConcurrentDictionary<Type, IList> _entitiesAwaitingFlush;
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

                if(_entitiesAwaitingFlush.Count == 0)
                {
                    //Dont recreated dictionary during generation if it already contains values.
                    //Should usually change this MaxTasksRunning property before generation started .
                    _entitiesAwaitingFlush = new ConcurrentDictionary<Type, IList>(_maxTasksRunning, 100);
                }
            }
        }


        //init
        public TemporaryStorage()
        {
            _reflectionInvoker = new ReflectionInvoker();
            _listOperations = new ListOperations();
            _runningTasks = new List<Task>();
            _entitiesAwaitingFlush = new ConcurrentDictionary<Type, IList>(_maxTasksRunning, 100);
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

                long entityReleasedCount = entityContext.EntityProgress.GetReleasedCount();
                int currentIndex = (int)(index - entityReleasedCount);
                if (currentIndex < 0 || currentIndex >= list.Count)
                {
                    string rangesDump = entityContext.EntityProgress.GetRangesDump();
                    throw new IndexOutOfRangeException($"Index [{currentIndex}] was out of range for type [{entityContext.Type}] in {nameof(TemporaryStorage)}. " +
                        $"Overall entity's index: {index}, " +
                        $"Current index {currentIndex}, " +
                        $"Current {nameof(TemporaryStorage)} entities count: {list.Count}, " +
                        $"Ranges dump: {rangesDump}");
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
            IList entitiesAwaitingFlush = _entitiesAwaitingFlush.GetOrAdd(entityContext.Type,
                (type) => _listOperations.CreateEntityList(type));

            entityContext.RunWithWriteLock(() =>
            {
                _listOperations.AddRange(entitiesAwaitingFlush, generatedEntities);
            });
        }


        //Flush to persistent
        /// <summary>
        /// Insert entities to persistent storage and remove from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="flushRange"></param>
        /// <param name="storages"></param>
        public virtual void FlushToPersistent(EntityContext entityContext, FlushRange flushRange, List<IPersistentStorage> storages)
        {
            EntityProgress progress = entityContext.EntityProgress;
            bool entityExist = _entitiesAwaitingFlush.TryGetValue(entityContext.Type, out IList entitiesAwaitingFlush);
            if (!entityExist)
            {
                throw new KeyNotFoundException($"Entity {entityContext.Type.FullName} does not have any instances in {nameof(TemporaryStorage)}, but {nameof(FlushToPersistent)} method was called.");
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithWriteLock(() =>
            {
                //FlushCommand for same entity ranges should come in ASC order, so removing instances from start of entitiesAwaitingFlush list
                int numberToFlush = flushRange.FlushRequestCapacity;

                //take number of items to flush
                nextItems = _listOperations
                    .Take(entityContext.Type, entitiesAwaitingFlush, numberToFlush);

                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, numberToFlush);
            });

            IStorageInsertGuard guard = entityContext.Description.StorageInsertGuard;
            if (guard != null)
            {
                guard.PreventInsertion(entityContext, nextItems);
            }
            if (nextItems.Count == 0)
            {
                return;
            }

            WaitRunningTask();

            Task[] nextFlushTasks = storages
                .Select(storage => _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems))
                .ToArray();
            _runningTasks.AddRange(nextFlushTasks);

            Task.WhenAll(nextFlushTasks);
        }


        //InsertToPersistentStorageBeforeUse
        /// <summary>
        /// Insert entities to persistent storage but keep in temporary storage
        /// </summary>
        public virtual void GenerateStorageIds(EntityContext entityContext, FlushRange generateIdsRange, List<IPersistentStorage> storages)
        {
            //Must be a sync operation, so inserted Ids are available immediatly
            EntityProgress progress = entityContext.EntityProgress;
            bool entityExist = _entitiesAwaitingFlush.TryGetValue(entityContext.Type, out IList entitiesAwaitingFlush);
            if (!entityExist)
            {
                throw new KeyNotFoundException($"Entity {entityContext.Type.FullName} does not have any instances in {nameof(TemporaryStorage)}, but {nameof(GenerateStorageIds)} method was called.");
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithReadLock(() =>
            {
                long entityReleasedCount = entityContext.EntityProgress.GetReleasedCount();
                long numberToSkip = generateIdsRange.PreviousRangeFlushedCount - entityReleasedCount;
                int numberToRemove = generateIdsRange.FlushRequestCapacity;

                //skip instances that already received new db Id
                nextItems = _listOperations.Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToSkip);

                //take number of items to receive new db Id
                nextItems = _listOperations.Take(entityContext.Type, nextItems, numberToRemove);
            });

            IStorageInsertGuard guard = entityContext.Description.StorageInsertGuard;
            if (guard != null)
            {
                guard.PreventInsertion(entityContext, nextItems);
            }
            if (nextItems.Count == 0)
            {
                return;
            }

            storages.ForEach(storage =>
            {
                _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems).Wait();
            });
        }

        public virtual void ReleaseFromTemporary(EntityContext entityContext, FlushRange releaseRange)
        {
            EntityProgress progress = entityContext.EntityProgress;
            bool entityExist = _entitiesAwaitingFlush.TryGetValue(entityContext.Type, out IList entitiesAwaitingFlush);
            if (!entityExist)
            {
                throw new KeyNotFoundException($"Entity {entityContext.Type.FullName} does not have any instances in {nameof(TemporaryStorage)}, but {nameof(ReleaseFromTemporary)} method was called.");
            }

            //lock operations on list for same Type
            entityContext.RunWithWriteLock(() =>
            {
                //ReleaseCommand for same entity ranges should come in ASC order, so safely can remove instances from start of entitiesAwaitingFlush list
                int numberToRemove = releaseRange.FlushRequestCapacity;

                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, numberToRemove);
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
