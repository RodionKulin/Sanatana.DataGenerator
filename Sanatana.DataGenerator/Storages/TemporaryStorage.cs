using Sanatana.DataGenerator.GenerationOrder;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public class TemporaryStorage
    {
        //fields
        protected int _maxTasksRunning = Environment.ProcessorCount;
        protected Dictionary<Type, IList> _entitiesAwaitingFlush;
        protected List<Task> _runningTasks;
        protected ReflectionInvoker _reflectionInvoker;
        protected ListOperations _listOperations;
        protected ConcurrentQueue<EntityAction> _completedActions;

        //properties
        /// <summary>
        /// Maximum running parallel tasks to insert entities into permanent storage.
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
            _completedActions = new ConcurrentQueue<EntityAction>();
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

            IList list = _entitiesAwaitingFlush[entityContext.Type];

            object itemAtIndex = null;
            entityContext.RunWithReadLock(() =>
            {
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
                entitiesAwaitingFlush = _reflectionInvoker.CreateEntityList(entityContext.Type);
                _entitiesAwaitingFlush.Add(entityContext.Type, entitiesAwaitingFlush);
            }

            entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];
            entityContext.RunWithWriteLock(() =>
            {
                _listOperations.AddRange(entitiesAwaitingFlush, generatedEntities);
            });
        }


        //Flush to permanent
        /// <summary>
        /// Insert entities to persistent storage and remove from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="storage"></param>
        public virtual void FlushToPersistent(EntityAction action, IPersistentStorage storage)
        {
            if (action.EntityContext.Description.InsertToPersistentStorageBeforeUse)
            {
                ReleaseFromTempStorage(action, storage);
                _completedActions.Enqueue(action);
                return;
            }

            WaitRunningTask();
            Task nextFlushTask = FlushToPermanentTask(action, storage)
                 .ContinueWith((prev) =>
                 {
                     _completedActions.Enqueue(action);
                 });
            _runningTasks.Add(nextFlushTask);
        }

        protected virtual Task FlushToPermanentTask(EntityAction action, IPersistentStorage storage)
        {
            EntityContext entityContext = action.EntityContext;
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return Task.FromResult(0);
            }

            IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];
            if (entitiesAwaitingFlush.Count == 0)
            {
                return Task.FromResult(0);
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithWriteLock(() =>
            {
                long numberToFlush = progress.NextFlushCount - progress.FlushedCount;

                //take number of items to flush
                nextItems = _listOperations
                    .Take(entityContext.Type, entitiesAwaitingFlush, (int)numberToFlush);

                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToFlush);

                //update progess
                progress.AddFlushedCount(nextItems.Count);
                progress.AddRemovedFromTempStorageCount(nextItems.Count);
            });

            if (nextItems.Count == 0)
            {
                return Task.FromResult(0);
            }

            return _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems);
        }


        //InsertToPersistentStorageBeforeUse
        /// <summary>
        /// Insert entities to persistent storage but keep in temporary storage
        /// </summary>
        public virtual void GenerateStorageIds(EntityAction action, IPersistentStorage storage)
        {
            //Must be a sync operation, so inserted Ids are available immediatly
            EntityContext entityContext = action.EntityContext;
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return;
            }

            IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];
            if (entitiesAwaitingFlush.Count == 0)
            {
                return;
            }

            //lock operations on list for same Type
            IList nextItems = null;
            entityContext.RunWithReadLock(() =>
            {
                long numberToSkip = progress.FlushedCount - progress.ReleasedCount;
                long numberToFlush = progress.NextFlushCount - progress.FlushedCount;

                //skip entities that already were flushed
                nextItems = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToSkip);

                //take number of items to flush
                nextItems = _listOperations
                    .Take(entityContext.Type, nextItems, (int)numberToFlush);

                //update progress
                progress.AddFlushedCount(nextItems.Count);
            });

            if (nextItems.Count == 0)
            {
                return;
            }

            _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems);
        }

        public virtual void ReleaseFromTempStorage(EntityAction action, IPersistentStorage storage)
        {
            EntityContext entityContext = action.EntityContext;
            EntityProgress progress = entityContext.EntityProgress;

            if (_entitiesAwaitingFlush.ContainsKey(entityContext.Type) == false)
            {
                return;
            }

            IList entitiesAwaitingFlush = _entitiesAwaitingFlush[entityContext.Type];
            if (entitiesAwaitingFlush.Count == 0)
            {
                return;
            }

            //lock operations on list for same Type
            entityContext.RunWithWriteLock(() =>
            {
                long numberToRemove = progress.NextReleaseCount - progress.ReleasedCount;

                //remove items from temporary storage
                _entitiesAwaitingFlush[entityContext.Type] = _listOperations
                    .Skip(entityContext.Type, entitiesAwaitingFlush, (int)numberToRemove);

                //update progress
                progress.AddRemovedFromTempStorageCount(numberToRemove);
            });
        }

        
        //Get completed action
        public virtual List<EntityAction> GetCompletedActions()
        {
            var completedActions = new List<EntityAction>();

            EntityAction completedAction = null;
            while (_completedActions.TryDequeue(out completedAction))
            {
                completedActions.Add(completedAction);
            }

            return completedActions;
        }
        

        //Tasks handling
        protected virtual void WaitRunningTask()
        {
            _runningTasks.RemoveAll(p => p.Status == TaskStatus.RanToCompletion
                || p.Status == TaskStatus.Canceled
                || p.Status == TaskStatus.Faulted);

            bool isMaxTasksRunning = _runningTasks.Count >= MaxTasksRunning;
            if (isMaxTasksRunning)
            {
                int finishedIndex = Task.WaitAny(_runningTasks.ToArray());
                _runningTasks.RemoveAt(finishedIndex);
            }
        }

        public virtual void WaitAllTasks()
        {
            _runningTasks.RemoveAll(p => p.Status == TaskStatus.RanToCompletion
                || p.Status == TaskStatus.Canceled
                || p.Status == TaskStatus.Faulted);

            bool isMaxTasksRunning = _runningTasks.Count >= MaxTasksRunning;
            if (isMaxTasksRunning)
            {
                int finishedIndex = Task.WaitAny(_runningTasks.ToArray());
                _runningTasks.RemoveAt(finishedIndex);
            }
        }
    }
}
