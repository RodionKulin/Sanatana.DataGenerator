using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using Sanatana.DataGenerator.Commands;
using System.Collections.Concurrent;

namespace Sanatana.DataGenerator.Supervisors.Complete
{
    /// <summary>
    /// Provides commands to generate complete list of all entities configured.
    /// </summary>
    public class CompleteSupervisor : ISupervisor
    {
        //fields
        /// <summary>
        /// Default services for entities.
        /// </summary>
        protected GeneratorSetup _generatorSetup;
        /// <summary>
        /// All entities with their current generated count.
        /// </summary>
        protected Dictionary<Type, EntityContext> _entityContexts;
        /// <summary>
        /// Next actions to execute between generate entity actions
        /// </summary>
        protected ConcurrentQueue<ICommand> _commandsQueue;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected IRequiredQueueBuilder _requiredQueueBuilder;
        protected INextNodeFinder _nextNodeFinder;


        //properties
        public IProgressState ProgressState { get; protected set; }


        //init
        public virtual void Setup(GeneratorSetup generatorSetup, 
            Dictionary<Type, EntityContext> entityContexts)
        {
            _commandsQueue = new ConcurrentQueue<ICommand>();

            _entityContexts = entityContexts;
            _generatorSetup = generatorSetup;

            ProgressState = new CompleteProgressState(entityContexts);
            _flushCandidatesRegistry = new CompleteFlushCandidatesRegistry(
                generatorSetup, entityContexts, ProgressState);
            _nextNodeFinder = new CompleteNextNodeFinder(
                generatorSetup, _flushCandidatesRegistry, ProgressState);
            _requiredQueueBuilder = new CompleteRequiredQueueBuilder(
                generatorSetup, entityContexts, _nextNodeFinder);
        }


        //methods
        public virtual ICommand GetNextCommand()
        {
            ICommand nextCommand = null;
            _commandsQueue.TryDequeue(out nextCommand);
            if (nextCommand != null)
            {
                return nextCommand;
            }

            nextCommand = _requiredQueueBuilder.GetNextCommand();
            return nextCommand;
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

            //check if flush to permanent storage required
            //and enqueue flush actions
            bool isFlushRequired = _flushCandidatesRegistry.CheckIsFlushRequired(entityContext);
            if (isFlushRequired)
            {
                List<ICommand> flushCommands = _flushCandidatesRegistry.GetNextFlushCommands(entityContext);
                flushCommands.ForEach(command => _commandsQueue.Enqueue(command));
            }

            //update progress state variables
            ProgressState.UpdateCounters(entityContext, generatedEntities);
            _requiredQueueBuilder.UpdateCounters(entityContext, generatedEntities, isFlushRequired);
        }

      
    }
}
