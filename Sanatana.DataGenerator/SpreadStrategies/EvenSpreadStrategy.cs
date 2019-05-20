using System;
using System.Collections.Generic;
using System.Text;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder;

namespace Sanatana.DataGenerator.SpreadStrategies
{
    public class EvenSpreadStrategy : ISpreadStrategy
    {
        //methods
        public virtual void Setup(EntityContext childEntity, Dictionary<Type, EntityContext> allEntities)
        {
        }

        public virtual long GetParentIndex(
            EntityContext parentEntity, EntityContext childEntity)
        {
            if (parentEntity.EntityProgress.TargetCount == 0
                || childEntity.EntityProgress.TargetCount == 0)
            {
                return 0;
            }

            //CurrentCount is 0 by default and incremented after entity generation.
            //GetParentIndex is called before generation to collect Required entities.
            long matchingParentCount = GetParentMatchingSpread(
                parentEntity, childEntity, childEntity.EntityProgress.CurrentCount);

            //convert count to index
            return matchingParentCount == 0
                ? 0
                : matchingParentCount - 1;
        }
        
        public virtual long GetNextIterationParentsCount(
            EntityContext parentEntity, EntityContext childEntity)
        {
            if (parentEntity.EntityProgress.TargetCount == 0
                || childEntity.EntityProgress.TargetCount == 0)
            {
                return 0;
            }

            long matchingParentCount = GetParentMatchingSpread(
                parentEntity, childEntity, childEntity.EntityProgress.NextIterationCount);

            return matchingParentCount;
        }

        public virtual bool CanGenerateFromParentNextReleaseCount(
            EntityContext parentEntity, EntityContext childEntity)
        {
            if(parentEntity.EntityProgress.TargetCount == 0)
            {
                return false;
            }

            decimal childrenPerParent = (decimal)childEntity.EntityProgress.TargetCount / parentEntity.EntityProgress.TargetCount;

            //don't generate more than child TargetCount
            decimal childrenPossibleToGenerate = parentEntity.EntityProgress.NextReleaseCount * childrenPerParent;
            long childrenPossibleToGenerateRounded = (long)Math.Ceiling(childrenPossibleToGenerate);
            childrenPossibleToGenerateRounded = Math.Min(childrenPossibleToGenerateRounded, childEntity.EntityProgress.TargetCount);

            long missingNumberOfChildren = childrenPossibleToGenerateRounded - childEntity.EntityProgress.CurrentCount;
            bool canGenerateMore = missingNumberOfChildren > 0;
            return canGenerateMore;
        }


        //shared methods
        protected virtual long GetParentMatchingSpread(
            EntityContext parentEntity, EntityContext childEntity, long childCountToMatch)
        {
            decimal childrenPerParent = (decimal)childEntity.EntityProgress.TargetCount / parentEntity.EntityProgress.TargetCount;
            decimal parentsCountRequired = childCountToMatch / childrenPerParent;
            long parentsCountRequiredRounded = (long)Math.Ceiling(parentsCountRequired);

            //don't require more than parent's TargetCount
            parentsCountRequiredRounded = Math.Min(parentsCountRequiredRounded, parentEntity.EntityProgress.TargetCount);
            return parentsCountRequiredRounded;
        }


    }
}
