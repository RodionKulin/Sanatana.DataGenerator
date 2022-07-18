using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System.Diagnostics;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGeneratorSpecs.SpreadStrategiesSpecs
{
    [TestClass]
    public class CartesianProductSpreadStrategySpecs
    {
        [TestMethod]
        [DataRow(50, 10, 5)]
        [DataRow(1, 1, 1)]
        [DataRow(2, 1, 2)]
        public void GetTargetCount_WhenCalledWithVariousInputs_ThenReturnsExpectedProductsLength(
            int expectedCombinationsCount, int categoriesCount, int postsCount)
        {
            //Arrange
            CartesianProductSpreadStrategy target = SetupTarget<Comment>(new[]
            {
                (typeof(Category), categoriesCount),
                (typeof(Post), postsCount)
            });

            //Act
            long actual = target.GetTargetCount(null, null);

            //Assert
            Assert.AreEqual(expectedCombinationsCount, actual);
        }

        [TestMethod]
        public void GetTargetCount_WhenNoRequiredEntities_ThenReturnsExpectedProductsLength()
        {
            //Arrange
            var parentCounts = new (Type, int)[0];
            Dictionary<Type, EntityContext> parentEntities = GetEntities(parentCounts);
            CartesianProductSpreadStrategy target = SetupTarget<Comment>(parentCounts);

            //Act
            long actual = target.GetTargetCount(null, null);

            //Assert
            Assert.AreEqual(0, actual);
        }

        [TestMethod]
        public void GetParentIndex_WhenTargetCountIncludeAllCombinations_ThenReturnDistinct()
        {
            //Arrange
            (Type, int)[] parentCounts = new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5)
            };
            Dictionary<Type, EntityContext> parentEntities = GetEntities(parentCounts);
            CartesianProductSpreadStrategy target = SetupTarget<Comment>(parentCounts);

            //Act
            long expectedCombinationsCount = target.GetTargetCount(null, null);
            List<long[]> resultingCombinations = InvokeGetParentIndex(target,
                parentEntities.Keys.ToList(), expectedCombinationsCount);

            //Assert
            Assert.AreEqual(expectedCombinationsCount, resultingCombinations.Count);

            long distinctCount = resultingCombinations
                .Select(x => string.Join(",", x))
                .Distinct()
                .Count();
            Assert.AreEqual(expectedCombinationsCount, distinctCount);
        }

        [TestMethod]
        public void GetParentIndex_WhenTargetCountExceedsAllCombinations_ThenResetAndRepeat()
        {
            //Arrange
            (Type, int)[] parentCounts = new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5)
            };
            Dictionary<Type, EntityContext> parentEntities = GetEntities(parentCounts);
            CartesianProductSpreadStrategy target = SetupTarget<Comment>(parentCounts);

            long expectedDistinctCount = target.GetTargetCount(null, null);
            int numberOfRepeats = 2;
            long expectedCombinationsCount = expectedDistinctCount * numberOfRepeats;

            //Act
            List<long[]> resultingCombinations = InvokeGetParentIndex(target,
                parentEntities.Keys.ToList(), expectedCombinationsCount);

            //Assert
            Assert.AreEqual(expectedCombinationsCount, resultingCombinations.Count);

            int distinctCount = resultingCombinations
                .Select(x => string.Join(",", x))
                .GroupBy(x => x)
                .Count();
            Assert.AreEqual(expectedDistinctCount, distinctCount);

            bool eachCombinationRepeated = resultingCombinations
                .Select(x => string.Join(",", x))
                .GroupBy(x => x)
                .Select(x => x.Count())
                .All(x => x == numberOfRepeats);
            Assert.IsTrue(eachCombinationRepeated);
        }

        [TestMethod]
        public void GetNextIterationParentsCount_WhenReachesEndOfSequence_ThenResetAndRepeats()
        {
            //Arrange
            (Type, int)[] parentCounts = new[]
           {
                (typeof(Category), 10),
                (typeof(Post), 5)
            };
            Dictionary<Type, EntityContext> parentEntities = GetEntities(parentCounts);
            CartesianProductSpreadStrategy target = SetupTarget<Comment>(parentCounts);
            int nextIterationIncrement = 5;

            //Act
            long expectedCombinationsCount = target.GetTargetCount(null, null);
            List<long[]> actualParentsCount = InvokeGetNextIterationParentsCount(target,
                parentEntities.Keys.ToList(), expectedCombinationsCount, nextIterationIncrement);
            List<long[]> actualParentIndex = InvokeGetParentIndex(target,
                parentEntities.Keys.ToList(), expectedCombinationsCount);

            //Assert
            Assert.AreEqual(expectedCombinationsCount, actualParentIndex.Count);

            int distinctCount = actualParentIndex
                .Select(x => string.Join(",", x))
                .GroupBy(x => x)
                .Count();
            Assert.AreEqual(expectedCombinationsCount, distinctCount);

            //Assert same combinations on ParentIndex and ParentsCount
            List<long[]> parentIndexesMatchingParentCountSteps = actualParentIndex
                .Where((x, i) => i % nextIterationIncrement == 0)
                .ToList();

            for (int i = 0; i < parentIndexesMatchingParentCountSteps.Count; i++)
            {
                long[] parentIndexStep = parentIndexesMatchingParentCountSteps[i];
                long[] parentsCountStep = actualParentsCount[i];

                for (int place = 0; place < parentIndexStep.Length; place++)
                {
                    long parentIndex = parentIndexStep[place];
                    long parentsCount = parentsCountStep[place];
                    bool parentIndexInBoundsOfCount = parentIndex < parentsCount;
                    Assert.IsTrue(parentIndexInBoundsOfCount);
                }
            }
        }



        //Arrange Helpers
        private Dictionary<Type, EntityContext> GetEntities(
            IEnumerable<(Type, int)> targetCounts)
        {
            return targetCounts
                .Select(x => new EntityContext
                {
                    Type = x.Item1,
                    EntityProgress = new EntityProgress
                    {
                        TargetCount = x.Item2
                    }
                })
                .ToDictionary(x => x.Type, x => x);
        }

        private CartesianProductSpreadStrategy SetupTarget<TChild>(
            IEnumerable<(Type, int)> targetCounts)
            where TChild : class
        {
            //child entity
            var desc = new EntityDescription<TChild>();
            foreach ((Type, int) parentType in targetCounts)
            {
                desc.SetRequired(parentType.Item1);
            }

            var childEntity = new EntityContext
            {
                Type = typeof(TChild),
                Description = desc
            };

            //all entities
            Dictionary<Type, EntityContext> allEntities = GetEntities(targetCounts);

            //spread strategy
            var spread = new CartesianProductSpreadStrategy();
            spread.Setup(childEntity, allEntities);

            return spread;
        }



        //Act helpers
        private List<long[]> InvokeGetParentIndex(CartesianProductSpreadStrategy spreadStrategy, 
            List<Type> parents, long combosCount)
        {
            var combos = new List<long[]>();
            Debug.WriteLine($"Starting {nameof(spreadStrategy.GetParentIndex)} invocations");

            for (int currentChildCount = 0; currentChildCount < combosCount; currentChildCount++)
            {
                var nextCombo = new List<long>();

                foreach (Type parentType in parents)
                {
                    EntityContext parentEntity = new EntityContext
                    {
                        Type = parentType
                    };

                    EntityContext childEntity = new EntityContext
                    {
                        EntityProgress = new EntityProgress
                        {
                            CurrentCount = currentChildCount
                        }
                    };

                    long parentIndex = spreadStrategy.GetParentIndex(parentEntity, childEntity);
                    nextCombo.Add(parentIndex);
                }
                
                combos.Add(nextCombo.ToArray());
                Debug.WriteLine(string.Join(",", nextCombo));
            }

            return combos;
        }

        private List<long[]> InvokeGetNextIterationParentsCount(CartesianProductSpreadStrategy spreadStrategy,
            List<Type> parents, long combosCount, int nextIterationIncrement)
        {
            var combos = new List<long[]>();
            Debug.WriteLine($"Starting {nameof(spreadStrategy.GetNextIterationParentCount)} invocations");

            long nextIterationInvokes = combosCount / nextIterationIncrement;

            for (int i = 1; i < combosCount; i += nextIterationIncrement)
            {
                var nextCombo = new List<long>();

                foreach (Type parentType in parents)
                {
                    EntityContext parentEntity = new EntityContext
                    {
                        Type = parentType
                    };

                    EntityContext childEntity = new EntityContext
                    {
                        EntityProgress = new EntityProgress
                        {
                            NextIterationCount = i
                        }
                    };

                    long parentIndex = spreadStrategy.GetNextIterationParentCount(parentEntity, childEntity);
                    nextCombo.Add(parentIndex);
                }

                combos.Add(nextCombo.ToArray());
                Debug.WriteLine(string.Join(",", nextCombo));
            }

            return combos;
        }

    }
}
