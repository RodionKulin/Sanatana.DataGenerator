using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder;
using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.SpreadStrategiesSpecs
{
    [TestClass]
    public class EvenSpreadStrategySpecs
    {
        [TestMethod]
        [DataRow(0, 0)]
        [DataRow(2, 0)]
        [DataRow(3, 1)]
        [DataRow(4, 1)]
        [DataRow(6, 2)]
        [DataRow(7, 2)]
        [DataRow(8, 2)]
        public void GetParentIndexSpec(long currentChildCount, long expectedParentCount)
        {
            //Prepare
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                CurrentCount = currentChildCount,
                TargetCount = 8
            });
            
            //Invoke
            long actualParentIndex = target.GetParentIndex(parentProgress, childProgress);
            
            //Assert
            Assert.AreEqual(expectedParentCount, actualParentIndex);
        }


        [TestMethod]
        [DataRow(0, 0)]
        [DataRow(1, 1)]
        [DataRow(2, 1)]
        [DataRow(3, 1)]
        [DataRow(4, 2)]
        [DataRow(9, 3)]
        public void GetNextIterationParentCountSpec(long nextIterationChildCount, long expectedParentCount)
        {
            //Prepare
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 8,
                NextIterationCount = nextIterationChildCount
            });

            //Invoke
            long actualParentCount = target.GetNextIterationParentsCount(parentProgress, childProgress);

            //Assert
            Assert.AreEqual(expectedParentCount, actualParentCount);
        }


        [TestMethod]
        [DataRow(0, 0, false)]
        [DataRow(0, 1, false)]
        [DataRow(1, 2, true)]
        [DataRow(1, 3, false)]
        [DataRow(1, 4, false)]
        [DataRow(2, 4, true)]
        [DataRow(2, 100, false)]
        [DataRow(100, 4, true)]
        public void CheckIfMoreChildrenCanBeGeneratedFromParentsNextFlushCountSpec(
            long parentNextFlushCount, long childCurrentCount, bool expectedParentCount)
        {
            //Prepare
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3,
                NextFlushCount = parentNextFlushCount
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 9,
                CurrentCount = childCurrentCount
            });

            //Invoke
            bool actualCanBeGenerated = target.CheckIfMoreChildrenCanBeGeneratedFromParentsNextFlushCount(
                parentProgress, childProgress);

            //Assert
            Assert.AreEqual(expectedParentCount, actualCanBeGenerated);
        }


        //Helper
        private EntityContext ToEntityContext(EntityProgress entityProgress)
        {
            return new EntityContext
            {
                EntityProgress = entityProgress
            };
        }
    }
}
