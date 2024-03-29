﻿using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.Commands;
using System.Collections.Concurrent;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Complete
{
    /// <summary>
    /// Provides commands to generate complete list of all entities configured.
    /// </summary>
    public class CompleteSupervisor : ISupervisor
    {
        //fields
        /// <summary>
        /// Next commands to execute between generate entity commands
        /// </summary>
        protected ConcurrentQueue<ICommand> _commandsQueue;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected IRequiredQueueBuilder _requiredQueueBuilder;
        protected INextNodeFinder _nextNodeFinder;


        //properties
        public IProgressState ProgressState { get; protected set; }


        //init
        public virtual void Setup(GeneratorServices generatorServices)
        {
            _commandsQueue = new ConcurrentQueue<ICommand>();

            ProgressState = new CompleteProgressState(generatorServices.EntityContexts);
            _flushCandidatesRegistry = new CompleteFlushCandidatesRegistry(
                generatorServices, ProgressState);
            _nextNodeFinder = new CompleteNextNodeFinder(
                generatorServices, _flushCandidatesRegistry, ProgressState);
            _requiredQueueBuilder = new CompleteRequiredQueueBuilder(
                generatorServices, _nextNodeFinder);
        }


        //methods
        public virtual IEnumerable<ICommand> IterateCommands()
        {
            while (true)
            {
                _commandsQueue.TryDequeue(out ICommand nextCommand);
                if (nextCommand != null)
                {
                    yield return nextCommand;
                    continue;
                }

                nextCommand = _requiredQueueBuilder.GetNextCommand();
                if (nextCommand != null)
                {
                    yield return nextCommand;
                    continue;
                }

                yield break;
            }
        }

        public virtual void EnqueueCommand(ICommand command)
        {
            _commandsQueue.Enqueue(command);
        }


        //Update current counters
        public virtual void HandleGenerateCompleted(EntityContext entityContext, IList generatedEntities)
        {
            //increment total number of generated entities
            entityContext.EntityProgress.CurrentCount += generatedEntities.Count;

            //check if flush to persistent storage required
            //and enqueue flush actions
            _flushCandidatesRegistry.UpdateRequestCapacity(entityContext);
            bool isFlushRequired = _flushCandidatesRegistry.UpdateFlushRequired(entityContext);
            if (isFlushRequired)
            {
                List<ICommand> flushCommands = _flushCandidatesRegistry.GetNextFlushCommands(entityContext);
                flushCommands.ForEach(command => _commandsQueue.Enqueue(command));
            }

            //update progress state variables
            ProgressState.UpdateCounters(entityContext, generatedEntities);
            _requiredQueueBuilder.UpdateCounters(entityContext, generatedEntities, isFlushRequired);
        }


        //Clone
        public virtual ISupervisor Clone()
        {
            return new CompleteSupervisor();
        }
    }
}
