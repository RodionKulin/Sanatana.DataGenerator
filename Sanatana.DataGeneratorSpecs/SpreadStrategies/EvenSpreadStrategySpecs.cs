﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using System.Diagnostics;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGeneratorSpecs.SpreadStrategiesSpecs
{
    //CurrentCount is incremented after entity generation and 
    //this GetParentIndex query is happening before generation
    //So CurrentCount will be 0 when first query happens
    //and CurrentCount = TargetCount - 1 when last query happens

    [TestClass]
    public class EvenSpreadStrategySpecs
    {
        [TestMethod]
        [DataRow(0, 0)]     //no children generated
        [DataRow(1, 0)]
        [DataRow(2, 0)]
        [DataRow(3, 1)]
        [DataRow(4, 1)]
        [DataRow(6, 2)]
        [DataRow(7, 2)]
        [DataRow(8, 2)]
        [DataRow(9, 2)]
        [DataRow(100, 2)]   //child count beyond child's TargetCount
        public void GetParentIndex_WhenParentCountLower_ThenReturnsExpected(
            long currentChildCount, long expectedParentIndex)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                CurrentCount = currentChildCount,
                TargetCount = 9
            });
            
            //Act
            long actualParentIndex = target.GetParentIndex(parentProgress, childProgress);
            
            //Assert
            Assert.AreEqual(expectedParentIndex, actualParentIndex);
        }


        [TestMethod]
        [DataRow(0, 2)]       //no children planned on next iteration
        [DataRow(1, 5)]
        [DataRow(2, 8)]
        [DataRow(3, 8)]       //child count beyond child's TargetCount. 
        [DataRow(100, 8)]     //child count beyond child's TargetCount
        public void GetParentIndex_WhenParentCountHigher_ThenReturnsExpected(
            long currentChildCount, long expectedParentIndex)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 9
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                CurrentCount = currentChildCount,
                TargetCount = 3
            });

            //Act
            long actualParentIndex = target.GetParentIndex(parentProgress, childProgress);

            //Assert
            actualParentIndex.Should().Be(expectedParentIndex);
        }


        [TestMethod]
        [DataRow(0, 0)]     //no children planned on next iteration
        [DataRow(1, 1)]
        [DataRow(2, 1)]
        [DataRow(3, 2)]
        [DataRow(4, 2)]
        [DataRow(5, 2)]
        [DataRow(8, 3)]
        [DataRow(9, 3)]     //child count beyond child's TargetCount
        [DataRow(100, 3)]   //child count beyond child's TargetCount
        public void GetNextIterationParentCount_WhenParentCountLower_ThenReturnsExpected(
            long nextIterationChildCount, long expectedParentCount)
        {
            //Arrange
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

            //Act
            long actualParentCount = target.GetNextIterationParentCount(parentProgress, childProgress);

            //Assert
            Assert.AreEqual(expectedParentCount, actualParentCount);
        }


        [TestMethod]
        [DataRow(0, 0)]       //no children planned on next iteration
        [DataRow(1, 3)]
        [DataRow(2, 6)]
        [DataRow(3, 8)]
        [DataRow(4, 8)]       //child count beyond child's TargetCount
        [DataRow(100, 8)]     //child count beyond child's TargetCount
        public void GetNextIterationParentCount_WhenParentCountLarger_ThenReturnsExpected(
            long nextIterationChildCount, long expectedParentCount)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 8
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3,
                NextIterationCount = nextIterationChildCount
            });

            //Act
            long actualParentCount = target.GetNextIterationParentCount(parentProgress, childProgress);

            //Assert
            Assert.AreEqual(expectedParentCount, actualParentCount);
        }

        [TestMethod]
        [DataRow(0, 0, false)]   //no parents/children were generated yet
        [DataRow(0, 1, false)]   //child was generated and parent not (impossible scenario with EvenSpread)
        [DataRow(1, 0, true)]    //not enough children generated to flush parent
        [DataRow(1, 2, true)]    //not enough children generated to flush parent
        [DataRow(2, 4, true)]    //not enough children generated to flush parent
        [DataRow(1, 3, false)]   //just enough children generated to flush parent
        [DataRow(1, 4, false)]   //more children were generated from previous parent
        [DataRow(2, 100, false)] //more children were generated from previous parent
        [DataRow(100, 4, true)]  //more parents were generated than required and child does not have TargetCount
        [DataRow(100, 9, false)] //more parents were generated than required and child already has TargetCount
        public void CanGenerateMoreFromParentsNextFlushCount_WhenParentCountLower_ThenReturnsExpected(
            int parentNextReleaseCount, int childCurrentCount, bool expectedCanGenegate)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 9,
                CurrentCount = childCurrentCount
            });
            FlushRange parentRange = new FlushRange(0, parentNextReleaseCount);

            //Act
            bool actualCanBeGenerated = target.CanGenerateFromParentRange(
                parentProgress, parentRange, childProgress);

            //Assert
            Assert.AreEqual(expectedCanGenegate, actualCanBeGenerated);
        }

        [TestMethod]
        [DataRow(0, 0, false)]      //no parents/children were generated yet
        [DataRow(0, 1, false)]      //child was generated and parent not (impossible scenario with EvenSpread)
        [DataRow(1, 0, true)]       //not enough children generated to flush parent
        [DataRow(4, 1, true)]       //not enough children generated to flush parent
        [DataRow(3, 1, false)]      //just enough children generated to flush parent
        [DataRow(6, 2, false)]      //just enough children generated to flush parent
        [DataRow(10, 3, false)]     //just enough children generated to flush parent
        [DataRow(100, 2, true)]     //more parents were generated than required and child does not have TargetCount
        [DataRow(100, 3, false)]    //more parents were generated than required and child already has TargetCount
        public void CanGenerateMoreFromParentsNextFlushCount_WhenParentCountLarger_ThenReturnsExpected(
           int parentNextReleaseCount, int childCurrentCount, bool expectedCanGenegate)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 10
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3,
                CurrentCount = childCurrentCount
            });
            FlushRange parentRange = new FlushRange(0, parentNextReleaseCount);

            //Act
            bool actualCanBeGenerated = target.CanGenerateFromParentRange(
                parentProgress, parentRange, childProgress);

            //Assert
            Assert.AreEqual(expectedCanGenegate, actualCanBeGenerated);
        }
        
        [TestMethod]
        [DataRow(1)]      
        [DataRow(2)]      
        [DataRow(3)]     //child finished generation 
        [DataRow(100)]   //more children were generated than required
        public void GetParentIndexAndGetParentCount_WhenCalledWithVariousInputs_ThenReturnConsistentResults(
            long childCurrentCount)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 10
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 3,
                CurrentCount = childCurrentCount,
                NextIterationCount = childCurrentCount + 1
            });

            //Act
            long actualParentIndex = target.GetParentIndex(
                parentProgress, childProgress);
            long actualParentsCount = target.GetNextIterationParentCount(
                parentProgress, childProgress);

            //Assert
            actualParentsCount.Should().Be(actualParentIndex + 1);
        }

        [TestMethod]
        [DataRow(10, 27, true, 9)]
        [DataRow(10, 28, true, 9)]
        [DataRow(10, 29, true, 9)]
        [DataRow(10, 30, false, 10)]
        [DataRow(10, 31, false, 10)]
        public void GetParentCountAndCanGenerate_WhenCalledWithVariousInputs_ThenReturnConsistentResults(
            int parentNextReleaseCount, int childCurrentCount, 
            bool expectedCanGenerate, int expectedParentIndex)
        {
            //Arrange
            var target = new EvenSpreadStrategy();
            EntityContext parentProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 30
            });
            EntityContext childProgress = ToEntityContext(new EntityProgress
            {
                TargetCount = 90,
                CurrentCount = childCurrentCount,
                NextIterationCount = childCurrentCount + 1
            });
            FlushRange parentRange = new FlushRange(0, parentNextReleaseCount);

            //Act
            long actualParentIndex = target.GetParentIndex(
                parentProgress, childProgress);
            long actualParentCount = target.GetNextIterationParentCount(
                parentProgress, childProgress);
            bool actualCanGenerate = target.CanGenerateFromParentRange(
                parentProgress, parentRange, childProgress);

            //Assert
            actualParentIndex.Should().Be(expectedParentIndex);
            actualParentIndex.Should().Be(actualParentCount -1);
            actualCanGenerate.Should().Be(expectedCanGenerate);

            if (actualCanGenerate)
            {
                actualParentCount.Should().BeLessOrEqualTo(parentNextReleaseCount);
            }
            else
            {
                actualParentCount.Should().BeGreaterThan(parentNextReleaseCount);
            }            
        }

        [TestMethod]
        public void GetParentCountAndCanGenerate_WhenAllRangeIterated_ThenReturnConsistentResults()
        {
            int targetChildCount = 90;
            int parentNextReleaseCount = 0;
            for (int childCurrentCount = 0; childCurrentCount < targetChildCount; childCurrentCount++)
            {
                if (childCurrentCount % 30 == 0)
                {
                    parentNextReleaseCount += 10;
                }

                //Arrange
                var target = new EvenSpreadStrategy();
                EntityContext parentProgress = ToEntityContext(new EntityProgress
                {
                    TargetCount = 30
                });
                EntityContext childProgress = ToEntityContext(new EntityProgress
                {
                    TargetCount = 90,
                    CurrentCount = childCurrentCount,
                    NextIterationCount = childCurrentCount + 1
                });
                FlushRange parentRange = new FlushRange(0, parentNextReleaseCount);

                //Act
                long actualParentIndex = target.GetParentIndex(
                    parentProgress, childProgress);
                long actualParentCount = target.GetNextIterationParentCount(
                    parentProgress, childProgress);
                bool actualCanGenerate = target.CanGenerateFromParentRange(
                    parentProgress, parentRange, childProgress);

                //Assert
                bool expectedCanGenerate = actualParentCount <= parentNextReleaseCount;
                actualParentIndex.Should().Be(actualParentCount - 1);
                actualCanGenerate.Should().Be(expectedCanGenerate);

                Debug.WriteLine($"Child: {childCurrentCount} Parent {actualParentCount} NextRelease {parentNextReleaseCount} CanGenerate {actualCanGenerate}");
            }
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
