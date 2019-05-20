using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder;
using Sanatana.DataGeneratorSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Sanatana.DataGenerator.GenerationOrder.Contracts;
using Sanatana.DataGenerator.GenerationOrder.Complete;

namespace Sanatana.DataGenerator.GenerationOrderSpecs
{
    [TestClass]
    public class CompleteOrderProviderSpecs
    {
        [TestMethod]
        public void GetNext_WhenPyramidHierarcy_ReturnsExpectedOrder()
        {
            //Prepare
            var category = new EntityDescription<Category>()
                .SetTargetCount(1);
            var post = new EntityDescription<Post>()
                .SetTargetCount(2)
                .SetRequired(typeof(Category));
            var comment = new EntityDescription<Comment>()
                .SetTargetCount(3)
                .SetRequired(typeof(Post));
            var descriptions = new List<IEntityDescription>
            {
                category,
                post,
                comment
            };
            CompleteOrderProvider target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<EntityAction> actualActions = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualActions);
            AssertPlanCount(descriptions, actualActions);
        }

        [TestMethod]
        public void GetNext_WhenMiddleBulgeHierarcy_ReturnsExpectedOrder()
        {
            //Prepare
            var category = new EntityDescription<Category>()
                .SetTargetCount(1);
            var post = new EntityDescription<Post>()
                .SetTargetCount(4)
                .SetRequired(typeof(Category));
            var comment = new EntityDescription<Comment>()
                .SetTargetCount(2)
                .SetRequired(typeof(Post));
            var descriptions = new List<IEntityDescription>
            {
                category,
                post,
                comment
            };
            CompleteOrderProvider target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<EntityAction> actualActions = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualActions);
            AssertPlanCount(descriptions, actualActions);
        }

        [TestMethod]
        public void GetNext_WhenBranchingHierarcy_ReturnsExpectedOrder()
        {
            //Prepare
            var category = new EntityDescription<Category>()
                .SetTargetCount(1);
            var post = new EntityDescription<Post>()
                .SetTargetCount(3)
                .SetRequired(typeof(Category));
            var comment = new EntityDescription<Comment>()
                .SetTargetCount(2)
                .SetRequired(typeof(Post));
            var attachment = new EntityDescription<Attachment>()
                .SetTargetCount(2)
                .SetRequired(typeof(Post));
            var descriptions = new List<IEntityDescription>
            {
                category,
                post,
                comment,
                attachment
            };
            CompleteOrderProvider target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<EntityAction> actualActions = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualActions);
            AssertPlanCount(descriptions, actualActions);
        }



        //Setup helpers
        private CompleteOrderProvider SetupCompleteOrderProvider(
            List<IEntityDescription> descriptions)
        {
            var generatorSetup = new GeneratorSetup();
            Dictionary<Type, IEntityDescription> dictDescriptions = 
                descriptions.ToDictionary(x => x.Type, x => x);
            Dictionary<Type, EntityContext> contexts =
                generatorSetup.SetupEntityContexts(dictDescriptions);

            var target = new CompleteOrderProvider();
            target.Setup(generatorSetup, contexts);
            return target;
        }

        private List<EntityAction> GetNextList(IOrderProvider plan, 
            Func<List<object>> generator = null)
        {
            if(generator == null)
            {
                generator = () => new List<object> { "new object" };
            }

            var list = new List<EntityAction>();
            while (true)
            {
                EntityAction next = plan.GetNextAction();
                if (next.ActionType == ActionType.Finish)
                {
                    break;
                }

                if(next.ActionType == ActionType.Generate)
                {
                    list.Add(next);

                    IList generatedList = generator();
                    plan.HandleGenerateCompleted(next.EntityContext, generatedList);
                }

                int limit = 1000;
                if (list.Count > limit)
                {
                    throw new InternalTestFailureException($"{typeof(IOrderProvider)} did not complete after {limit} next items");
                }
            }

            return list;
        }


        //Assert helpers
        private void AssertPlanCount(List<IEntityDescription> descriptions,
            List<EntityAction> actualActions)
        {
            Dictionary<Type, long> expectedCalls = descriptions
                .Select(x => new
                {
                    Type = x.Type,
                    Total = x.QuantityProvider.GetTargetTotalQuantity()
                })
                .OrderBy(x => x.Type.FullName)
                .ToDictionary(x => x.Type, x => x.Total);

            Dictionary<Type, long> actualCalls = actualActions
                .Where(x => x.ActionType == ActionType.Generate)
                .Select(x => x.EntityContext.Type)
                .GroupBy(x => x)
                .Select(x => new
                {
                    Type = x.Key,
                    Total = x.LongCount()
                })
                .OrderBy(x => x.Type.FullName)
                .ToDictionary(x => x.Type, x => x.Total);

            bool actualListEqualsExpected = expectedCalls.SequenceEqual(actualCalls);
            string expectedString = string.Join(", ", expectedCalls.Select(x => $"{x.Key.Name}:{x.Value}"));
            string actualString = string.Join(", ", actualCalls.Select(x => $"{x.Key.Name}:{x.Value}"));
            Assert.IsTrue(actualListEqualsExpected
                , $"Expected plan {expectedString} does not equal actual plan {actualString}");
        }

        private void AssertPlanOrder(List<IEntityDescription> expectedList,
            List<EntityAction> actualActions)
        {
            List<IEntityDescription> actualEntities = actualActions
                .Where(x => x.ActionType == ActionType.Generate)
                .Select(x => x.EntityContext.Description)
                .ToList();

            bool actualListEqualsExpected = expectedList.SequenceEqual(actualEntities);
            string expectedString = string.Join(", ", expectedList.Select(x => x.Type.Name));
            string actualString = string.Join(", ", actualEntities.Select(x => x.Type.Name));
            Assert.IsTrue(actualListEqualsExpected
                , $"Expected plan {expectedString} does not equal actual plan {actualString}");
        }
    }
}
