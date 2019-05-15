using Sanatana.DataGenerator.Internals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Storages
{
    public class TemporaryStorage
    {
        //fields
        protected Dictionary<Type, IList> _entities;
        protected ReflectionInvoker _reflectionInvoker;
        protected List<Task> _runningTasks;
        protected ListOperations _listOperations;

        //properties
        public int MaxTasksRunning { get; set; } = Environment.ProcessorCount;


        //init
        public TemporaryStorage()
        {
            _reflectionInvoker = new ReflectionInvoker();
            _runningTasks = new List<Task>();
            _entities = new Dictionary<Type, IList>();
            _listOperations = new ListOperations();
        }


        //public methods
        /// <summary>
        /// Select from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public virtual object Select(EntityContext entityContext, long index)
        {
            if (_entities.ContainsKey(entityContext.Type) == false)
            {
                throw new NullReferenceException($"No entities of type [{entityContext.Type}] are stored in {nameof(TemporaryStorage)}");
            }

            IList list = _entities[entityContext.Type];
            int currentIndex = (int)(index - entityContext.EntityProgress.FlushedCount);
            if (currentIndex < 0 || currentIndex >= list.Count)
            {
                throw new IndexOutOfRangeException($"Index [{currentIndex}] was out of range for type [{entityContext.Type}] in {nameof(TemporaryStorage)}." +
                    $"Overall entity's index: {index}, " +
                    $"Total flushed entities: {entityContext.EntityProgress.FlushedCount}, " +
                    $"Current index {currentIndex}, " +
                    $"Current {nameof(TemporaryStorage)} entities count: {list.Count}");
            }

            return list[currentIndex];
        }

        /// <summary>
        /// Insert to temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="entities"></param>
        public virtual void InsertToTemporary(EntityContext entityContext, IList entities)
        {
            IList list;
            if (_entities.ContainsKey(entityContext.Type) == false)
            {
                list = _reflectionInvoker.CreateEntityList(entityContext.Type);
                _entities.Add(entityContext.Type, list);
            }

            list = _entities[entityContext.Type];
            if(entities != null)
            {
                foreach (var item in entities)
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Insert entities to persistent storage and remove from temporary storage
        /// </summary>
        /// <param name="entityContext"></param>
        /// <param name="storage"></param>
        public virtual void FlushToPermanent(EntityContext entityContext, IPersistentStorage storage)
        {
            WaitRunningTask();
            Task nextFlushTask = FlushTask(entityContext, storage);
            _runningTasks.Add(nextFlushTask);
        }
        
        /// <summary>
        /// Insert entities to persistent storage but keep in temporary storage
        /// </summary>
        public virtual void InsertToPermanentAndKeep(EntityContext entityContext, IPersistentStorage storage)
        {
            WaitRunningTask();
            Task nextInsertTask = InsertToPermanentAndKeepTask(entityContext, storage);
            _runningTasks.Add(nextInsertTask);
        }


        //Tasks
        protected virtual Task FlushTask(EntityContext entityContext, IPersistentStorage storage)
        {
            if (_entities.ContainsKey(entityContext.Type) == false)
            {
                return Task.FromResult(0);
            }

            IList entitiesInTemporaryStorage = _entities[entityContext.Type];
            if (entitiesInTemporaryStorage.Count == 0)
            {
                return Task.FromResult(0);
            }

            EntityProgress progress = entityContext.EntityProgress;
            int numberOfItemsToFlush = (int)(progress.NextFlushCount - progress.FlushedCount);
            progress.FlushedCount = progress.NextFlushCount;

            //take number of items to flush
            IList nextItems = _listOperations
                .Take(entityContext, entitiesInTemporaryStorage, numberOfItemsToFlush);
            _entities[entityContext.Type] = _listOperations
                .Skip(entityContext, entitiesInTemporaryStorage, numberOfItemsToFlush);

            return _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems);
        } 

        protected virtual Task InsertToPermanentAndKeepTask(EntityContext entityContext, IPersistentStorage storage)
        {
            if (_entities.ContainsKey(entityContext.Type) == false)
            {
                return Task.FromResult(0);
            }

            IList list = _entities[entityContext.Type];
            if (list.Count == 0)
            {
                return Task.FromResult(0);
            }

            EntityProgress progress = entityContext.EntityProgress;
            int numberOfItemsToFlush = (int)(progress.NextFlushCount - progress.FlushedCount);

            IList nextItems = _listOperations
                .Take(entityContext, list, numberOfItemsToFlush);

            return _reflectionInvoker.InvokeInsert(storage, entityContext.Description, nextItems);
        }

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



    }
}
