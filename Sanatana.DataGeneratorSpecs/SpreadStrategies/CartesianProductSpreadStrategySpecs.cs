using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGeneratorSpecs.Samples;
using System.Diagnostics;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGeneratorSpecs.SpreadStrategiesSpecs
{
    [TestClass]
    public class CartesianProductSpreadStrategySpecs
    {
        [TestMethod]
        [DataRow(50, 10, 5)]
        [DataRow(1, 1, 1)]
        [DataRow(2, 1, 2)]
        [Ignore]
        public void GetTotalCount_ReturnsExpectedProductsLength(
            int expectedCombinationsCount, int categoriesCount, int postsCount)
        {
            //Prepare
            Dictionary<Type, EntityContext> entities = ToEntityDictionary(new[]
            {
                (typeof(Category), categoriesCount),
                (typeof(Post), postsCount),
            });
            var target = new CartesianProductSpreadStrategy();
            //target.Setup(entities);

            //Invoke
            //long actual = target.GetTotalCount();

            //Assert
            //Assert.AreEqual(expectedCombinationsCount, actual);
        }

        [TestMethod]
        [Ignore]
        public void GetParentIndex_WhenTargetCountIncludeAllCombinations_ReturnDistinct()
        {
            //Prepare
            Dictionary<Type, EntityContext> entities = ToEntityDictionary(new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5),
            });
            var target = new CartesianProductSpreadStrategy();
            //target.Setup(entities);
            //long expectedCombinationsCount = target.GetTotalCount();

            ////Invoke
            //List<long[]> resultingCombinations = InvokeGetParentIndex(target,
            //    entities.Keys.ToList(), expectedCombinationsCount);

            ////Assert
            //Assert.AreEqual(expectedCombinationsCount, resultingCombinations.Count);

            //long distinctCount = resultingCombinations
            //    .Select(x => string.Join(",", x))
            //    .Distinct()
            //    .Count();
            //Assert.AreEqual(expectedCombinationsCount, distinctCount);
        }

        [TestMethod]
        [Ignore]
        public void GetParentIndex_WhenTargetCountExceedsAllCombinations_ResetAndRepeat()
        {
            //Prepare
            Dictionary<Type, EntityContext> entities = ToEntityDictionary(new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5),
            });
            var target = new CartesianProductSpreadStrategy();
            //target.Setup(entities);
            //long expectedDistinctCount = target.GetTotalCount();
            //int numberOfRepeats = 2;
            //long expectedCombinationsCount = expectedDistinctCount * numberOfRepeats;

            ////Invoke
            //List<long[]> resultingCombinations = InvokeGetParentIndex(target,
            //    entities.Keys.ToList(), expectedCombinationsCount);

            ////Assert
            //Assert.AreEqual(expectedCombinationsCount, resultingCombinations.Count);

            //int distinctCount = resultingCombinations
            //    .Select(x => string.Join(",", x))
            //    .GroupBy(x => x)
            //    .Count();
            //Assert.AreEqual(expectedDistinctCount, distinctCount);

            //bool eachCombinationRepeated = resultingCombinations
            //    .Select(x => string.Join(",", x))
            //    .GroupBy(x => x)
            //    .Select(x => x.Count())
            //    .All(x => x == numberOfRepeats);
            //Assert.IsTrue(eachCombinationRepeated);
        }

        [TestMethod]
        [Ignore]
        public void GetNextIterationParentsCount_Increment_ResetAndRepeat()
        {
            //Prepare
            Dictionary<Type, EntityContext> entities = ToEntityDictionary(new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5),
            });
            var target = new CartesianProductSpreadStrategy();
            //target.Setup(entities);
            //long expectedCombinationsCount = target.GetTotalCount();
            //int nextIterationIncrement = 5;

            ////Invoke
            //List<long[]> actualParentsCount = InvokeGetNextIterationParentsCount(target,
            //    entities.Keys.ToList(), expectedCombinationsCount, nextIterationIncrement);
            //List<long[]> actualParentIndex = InvokeGetParentIndex(target,
            //    entities.Keys.ToList(), expectedCombinationsCount);

            ////Assert
            //Assert.AreEqual(expectedCombinationsCount, actualParentIndex.Count);

            //int distinctCount = actualParentIndex
            //    .Select(x => string.Join(",", x))
            //    .GroupBy(x => x)
            //    .Count();
            //Assert.AreEqual(expectedCombinationsCount, distinctCount);

            ////Assert same combinations on ParentIndex and ParentsCount
            //List<long[]> parentIndexesMatchingParentCountSteps = actualParentIndex
            //    .Where((x, i) => i % nextIterationIncrement == 0)
            //    .ToList();

            //for (int i = 0; i < parentIndexesMatchingParentCountSteps.Count; i++)
            //{
            //    long[] parentIndexStep = parentIndexesMatchingParentCountSteps[i];
            //    long[] parentsCountStep = actualParentsCount[i];

            //    for (int place = 0; place < parentIndexStep.Length; place++)
            //    {
            //        long parentIndex = parentIndexStep[place];
            //        long parentsCount = parentsCountStep[place];
            //        bool parentIndexInBoundsOfCount = parentIndex < parentsCount;
            //        Assert.IsTrue(parentIndexInBoundsOfCount);
            //    }
            //}
        }


        //Prepare Helpers
        private CartesianProductSpreadStrategy GetConfiguredTarget()
        {
            Dictionary<Type, EntityContext> entities = ToEntityDictionary(new[]
            {
                (typeof(Category), 10),
                (typeof(Post), 5),
            });
            var target = new CartesianProductSpreadStrategy();
            //target.Setup(entities);
            return target;
        }

        private Dictionary<Type, EntityContext> ToEntityDictionary(
            IEnumerable<(Type, int)> targetCounts)
        {
            Dictionary<Type, EntityContext> entities = targetCounts
                .Select(x => new EntityContext
                {
                    Type = x.Item1,
                    EntityProgress = new EntityProgress
                    {
                        TargetCount = x.Item2
                    }
                })
                .ToDictionary(x => x.Type, x => x);

            return entities;
        }


        //Invoke helpers
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
            Debug.WriteLine($"Starting {nameof(spreadStrategy.GetNextIterationParentsCount)} invocations");

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

                    long parentIndex = spreadStrategy.GetNextIterationParentsCount(parentEntity, childEntity);
                    nextCombo.Add(parentIndex);
                }

                combos.Add(nextCombo.ToArray());
                Debug.WriteLine(string.Join(",", nextCombo));
            }

            return combos;
        }


    }
}
