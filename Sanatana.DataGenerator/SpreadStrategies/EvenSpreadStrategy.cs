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
        public void Setup(Dictionary<Type, EntityContext> parentEntities)
        {
        }

        public virtual long GetParentIndex(
            EntityContext parentEntity, EntityContext childEntity)
        {
            decimal itemsPerParent = (decimal)childEntity.EntityProgress.TargetCount / parentEntity.EntityProgress.TargetCount;
            long childrenPerParentRounded = (long)Math.Ceiling(itemsPerParent);

            long currentParentIndex = childEntity.EntityProgress.CurrentCount / childrenPerParentRounded;
            return currentParentIndex;
        }

        public virtual long GetNextIterationParentsCount(
            EntityContext parentEntity, EntityContext childEntity)
        {
            decimal childrenPerParent = (decimal)childEntity.EntityProgress.TargetCount / parentEntity.EntityProgress.TargetCount;
            long childrenPerParentRounded = (long)Math.Ceiling(childrenPerParent);

            decimal totalParentsCountRequired = (decimal)childEntity.EntityProgress.NextIterationCount / childrenPerParentRounded;
            long totalParentsCountRequiredRounded = (long)Math.Ceiling(totalParentsCountRequired);
            return totalParentsCountRequiredRounded;
        }

        public virtual bool CanGenerateMoreFromParentsNextFlushCount(
            EntityContext parentEntity, EntityContext childEntity)
        {
            decimal childrenPerParent = (decimal)childEntity.EntityProgress.TargetCount / parentEntity.EntityProgress.TargetCount;
            long childrenPerParentRounded = (long)Math.Ceiling(childrenPerParent);

            long childrenPossibleToGenerate = parentEntity.EntityProgress.NextFlushCount * childrenPerParentRounded;
            long missingNumberOfChildren = childrenPossibleToGenerate - childEntity.EntityProgress.CurrentCount;
            bool canGenerateMore = missingNumberOfChildren > 0;
            return canGenerateMore;
        }

    }
}
