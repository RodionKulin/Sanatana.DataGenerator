using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Supervisors.Contracts;
using System.Collections;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Supervisors.Complete
{
    public class CompleteNextNodeFinder : INextNodeFinder
    {
        //fields
        protected GeneratorServices _generatorServices;
        protected IProgressState _progressState;
        protected IFlushCandidatesRegistry _flushCandidatesRegistry;


        //init
        public CompleteNextNodeFinder(GeneratorServices generatorServices, 
            IFlushCandidatesRegistry flushCandidatesRegistry, IProgressState progressState)
        {
            _generatorServices = generatorServices;
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

            EntityContext childCanGenerate = _flushCandidatesRegistry.FindChildOfFlushCandidate();
            if(childCanGenerate != null)
            {
                return childCanGenerate;
            }

            EntityContext nextLeafNode = FindNextLeaf();
            if(nextLeafNode != null)
            {
                return nextLeafNode;
            }

            ThrowNoNextGeneratorFound(_progressState);
            return null;
        }

        /// <summary>
        /// Throw exception on Next node finding misfunction, when next node is not found.
        /// </summary>
        /// <param name="progressState"></param>
        protected virtual void ThrowNoNextGeneratorFound(IProgressState progressState)
        {
            string[] completedNames = progressState.CompletedEntityTypes
                .Select(x => $"[{x.FullName}]")
                .ToArray();
            string[] notCompletedNames = progressState.NotCompletedEntities
                .Select(x => $"[{x.Type.FullName}:{x.EntityProgress.TargetCount - x.EntityProgress.CurrentCount}]")
                .ToArray();

            string completedList = string.Join(", ", completedNames);
            string notCompletedList = string.Join(", ", notCompletedNames);

            throw new NullReferenceException("Could not find next entity to generate. "
                + $"Following list of entities generated successfully: {completedList}. "
                + $"Following list of entities still was not fully generated [TypeName:remainingCount]: {notCompletedList}");
        }

        /// <summary>
        /// Find next leaf node. Leaf nodes have no children depending on them, so can be flushed to storage as soon as generated. That makes em a good to start with.
        /// </summary>
        /// <returns></returns>
        protected virtual EntityContext FindNextLeaf()
        {
            //There should always be at least one entity with no Required entities,
            //otherwise it would have a cycle in dependencies.
            //There is a validation that checks, that no cycle exist before starting generation.

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
