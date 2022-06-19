using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.TotalCountProviders;

namespace Sanatana.DataGenerator.SpreadStrategies
{
    public abstract class CombinatoricsSpreadStrategy : ISpreadStrategy, ITotalCountProvider
    {
        //fields
        protected Dictionary<Type, EntityContext> _parentEntities;
        protected Dictionary<Type, int> _parentsCombinationPlacements;
        protected IEnumerator<long[]> _combinationsEnumerator;
        protected long _currentCombinationIndex = -1;
        protected Dictionary<long, long[]> _combinationsBuffer;
        protected bool _isSetupCompleted = false;


        //init
        public CombinatoricsSpreadStrategy()
        {
            _combinationsBuffer = new Dictionary<long, long[]>();
        }

        public virtual void Setup(EntityContext childEntity, Dictionary<Type, EntityContext> allEntities)
        {
            //1. This method prepares combination placements for all parents of child entity.
            //2. Setup can be called multiple times for each parent entity.
            //3. Should be called before GetTargetCount.

            _parentEntities = childEntity.Description.Required
                .Select(x => allEntities[x.Type])
                .ToDictionary(x => x.Type, x => x);

            _parentsCombinationPlacements = _parentEntities.Keys
                .Select((x, i) => new { Type = x, Placement = i })
                .ToDictionary(keyValue => keyValue.Type, keyValue => keyValue.Placement);

            _isSetupCompleted = true;
        }


        //ISpreadStrategy methods
        public virtual long GetParentIndex(
            EntityContext parentEntity, EntityContext childEntity)
        {
            //CurrentCount is 0 by default and incremented after entity generation.
            //GetParentIndex is called before generation to collect Required entities.
            //So childEntityIndex will be 0-based index
            long childEntityIndex = childEntity.EntityProgress.CurrentCount;

            //Get parent index
            Type parentType = parentEntity.Type;
            long parentEntityIndex = GetParentIndexFromCombination(parentType, childEntityIndex);

            //Clear buffered combinations
            //CurrentCount is the lowest bound that will request combination
            ClearBufferedCombinations(childEntityIndex);

            return parentEntityIndex;
        }

        public virtual long GetNextIterationParentCount(
            EntityContext parentEntity, EntityContext childEntity)
        {
            //Convert child number 1-based count to zero-based index
            long childEntityIndex = childEntity.EntityProgress.NextIterationCount - 1;

            //Get parent index
            Type parentType = parentEntity.Type;
            long parentEntityIndex = GetParentLargestIndexFromCombination(parentType, childEntityIndex);

            //Convert parent 0-based index to 1-based count
            long parentEntityCount = parentEntityIndex + 1;
            return parentEntityCount;
        }

        public virtual bool CanGenerateFromParentRange(EntityContext parentEntity, 
            FlushRange parentRange, EntityContext childEntity)
        {
            //Always accumulate parent entities in memory and never release to persistent storage
            //until child item generates all instances.
            //1.Parents are used multiple times starting from 1 to end.
            //2.List of combinations can reset and start from beginnning if TargetCount is larger then total number of possible combinations.
            return childEntity.EntityProgress.CurrentCount < childEntity.EntityProgress.TargetCount;
        }


        //Combinatorics methods
        protected virtual long GetParentIndexFromCombination(Type parentType, long childEntityIndex)
        {
            int parentCombinationPlacement = _parentsCombinationPlacements[parentType];
            long[] combination = GetCombination(childEntityIndex);
            long parentEntityIndex = combination[parentCombinationPlacement];
            return parentEntityIndex;
        }

        protected virtual long GetParentLargestIndexFromCombination(Type parentType, long childEntityIndex)
        {
            int parentCombinationPlacement = _parentsCombinationPlacements[parentType];

            //get next combinations and buffer for childEntityIndex
            GetCombination(childEntityIndex);

            //Will need to return largest parent's count, that is not necessarily is the latest combination.
            //That's why will take all combinations and pick largest parent's index.
            long parentLargestEntityIndex = _combinationsBuffer
                .TakeWhile(x => x.Key <= childEntityIndex)
                .Select(x => x.Value[parentCombinationPlacement])
                .Max();

            return parentLargestEntityIndex;
        }

        protected virtual long[] GetCombination(long childEntityIndex)
        {
            if (_combinationsEnumerator == null)
            {
                _combinationsEnumerator = GetCombinationsEnumerator();
            }

            //For planning NextIterationCount can be much higher than previous _currentCombinationIndex,
            //so need to skip forward to next combinations

            //At same time combinations will need to be stored to get used GetParentIndex incrementally.
            while (childEntityIndex > _currentCombinationIndex)
            {
                _currentCombinationIndex++;

                //reset to beginning of sequence when reached the end
                bool hasNext = _combinationsEnumerator.MoveNext();
                if (!hasNext)
                {
                    _combinationsEnumerator = GetCombinationsEnumerator();
                    _combinationsEnumerator.MoveNext();
                }

                _combinationsBuffer.Add(
                    _currentCombinationIndex, _combinationsEnumerator.Current);
            }

            return _combinationsBuffer[childEntityIndex];
        }

        protected virtual IEnumerator<long[]> GetCombinationsEnumerator()
        {
            List<long> sequencesLengths = _parentEntities.Values
                .Select(x => x.EntityProgress.TargetCount)
                .ToList();
            if (sequencesLengths.Count == 0)
            {
                yield break;
            }

            IEnumerator<long[]> enumerator = GetCombinationsEnumerator(sequencesLengths);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
        }

        protected virtual void ClearBufferedCombinations(long childEntityIndex)
        {
            long bufferFirstIndex = _currentCombinationIndex - _combinationsBuffer.Count;
            long numberOfItemsToClear = childEntityIndex - bufferFirstIndex;
            if (numberOfItemsToClear < 100)
            {
                return;
            }

            _combinationsBuffer = _combinationsBuffer
                .SkipWhile(x => x.Key < childEntityIndex)
                .ToDictionary(x => x.Key, x => x.Value);
        }


        //Abstract methods to implement combinatorics
        protected abstract IEnumerator<long[]> GetCombinationsEnumerator(List<long> sequencesLengths);

        public abstract long GetTargetCount();
    }
}
