using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using System.Collections;

namespace Sanatana.DataGenerator.GenerationOrder.Complete
{
    public class CompleteNextNodeFinder : INextNodeFinder
    {
        //fields
        protected GeneratorSetup _generatorSetup;
        protected IProgressState _progressState;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;

        //init
        public CompleteNextNodeFinder(GeneratorSetup generatorSetup, 
            IFlushCandidatesRegistry flushCandidatesRegistry,
            IProgressState progressState)
        {
            _generatorSetup = generatorSetup;
            _flushCandidatesRegistry = flushCandidatesRegistry;
            _progressState = progressState;
        }

        
        //next entities finding
        /// <summary>
        /// Find next entity to start generation with. 
        /// Will first return flush candidate children and then check remaining leaf nodes.
        /// If all entities are completed will return null.
        /// </summary>
        /// <returns></returns>
        public virtual EntityContext FindNextNode()
        {
            bool isFinished = _progressState.GetIsFinished();
            if (isFinished)
            {
                return null;
            }

            EntityContext childCanGenerate = _flushCandidatesRegistry.FindChildOfFlushCandidates();
            if(childCanGenerate != null)
            {
                return childCanGenerate;
            }

            EntityContext nextLeafNode = FindNextLeaf();
            if(nextLeafNode != null)
            {
                return nextLeafNode;
            }

            _generatorSetup.Validator.ThrowNoNextGeneratorFound(_progressState);
            return null;
        }
        
        /// <summary>
        /// Find next leaf node. Leaf nodes have no children depending on them, so can be flushed to storage as soon as generated. That makes em a good to start with.
        /// </summary>
        /// <returns></returns>
        protected virtual EntityContext FindNextLeaf()
        {
            EntityContext leafNode = _progressState.NotCompletedEntities
                .Where(x => x.ChildEntities
                    .Select(req => req.Type)
                    .Except(_progressState.CompletedEntityTypes)
                    .Count() == 0)
                .FirstOrDefault();

            return leafNode;
        }
        
    }
}
