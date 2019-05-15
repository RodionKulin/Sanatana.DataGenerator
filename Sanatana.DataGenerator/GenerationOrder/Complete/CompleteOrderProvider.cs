using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.GenerationOrder.Complete
{
    /// <summary>
    /// Provides ordered actions to generate complete list of all entities configured.
    /// </summary>
    public class CompleteOrderProvider : IOrderProvider
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
        protected Queue<EntityAction> _actionsQueue;

        protected IFlushCandidatesRegistry _flushCandidatesRegistry;
        protected IRequiredQueueBuilder _requiredQueueBuilder;
        protected INextNodeFinder _nextNodeFinder;


        //properties
        public IProgressState ProgressState { get; protected set; }


        //init
        public virtual void Setup(GeneratorSetup generatorSetup, 
            Dictionary<Type, EntityContext> entityContexts)
        {
            _actionsQueue = new Queue<EntityAction>();

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
        public virtual EntityAction GetNextAction()
        {
            if(_actionsQueue.Count > 0)
            {
                return _actionsQueue.Dequeue();
            }

            EntityAction nextAction = _requiredQueueBuilder.GetNextAction();
            return nextAction;
        }


        //Update current counters
        public virtual void UpdateCounters(Type type, IList generatedEntities)
        {
            //increment total number of generated entities
            EntityContext entityContext = _entityContexts[type];
            entityContext.EntityProgress.CurrentCount += generatedEntities.Count;

            //check if flush to permanent storage required
            //and enqueue flush actions
            bool flushRequired = _flushCandidatesRegistry.CheckIsFlushRequired(entityContext);
            if (flushRequired)
            {
                List<EntityAction> flushActions = _flushCandidatesRegistry.
                    GetFlushActions(entityContext, generatedEntities);
                flushActions.ForEach(action => _actionsQueue.Enqueue(action));
            }

            //update state variables
            ProgressState.UpdateCounters(entityContext, generatedEntities);
            _requiredQueueBuilder.UpdateCounters(type, generatedEntities, flushRequired);
        }


    }
}
