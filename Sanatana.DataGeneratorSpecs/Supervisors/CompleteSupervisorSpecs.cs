using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGeneratorSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator;

namespace Sanatana.DataGeneratorSpecs.Supervisors
{
    [TestClass]
    public class CompleteSupervisorSpecs
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
            CompleteSupervisor target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<ICommand> actualCommands = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualCommands);
            AssertPlanCount(descriptions, actualCommands);
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
            CompleteSupervisor target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<ICommand> actualCommands = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualCommands);
            AssertPlanCount(descriptions, actualCommands);
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
            CompleteSupervisor target = SetupCompleteOrderProvider(descriptions);

            //Invoke
            List<ICommand> actualCommands = GetNextList(target);

            //Assert
            Assert.IsNotNull(actualCommands);
            AssertPlanCount(descriptions, actualCommands);
        }



        //Setup helpers
        private CompleteSupervisor SetupCompleteOrderProvider(
            List<IEntityDescription> descriptions)
        {
            var generatorSetup = new GeneratorSetup();
            Dictionary<Type, IEntityDescription> dictDescriptions = 
                descriptions.ToDictionary(x => x.Type, x => x);
            Dictionary<Type, EntityContext> contexts =
                generatorSetup.SetupEntityContexts(dictDescriptions);

            var target = new CompleteSupervisor();
            target.Setup(generatorSetup, contexts);
            return target;
        }

        private List<ICommand> GetNextList(ISupervisor plan)
        {
            Func<List<object>> generator = null;
            if (generator == null)
            {
                generator = () => new List<object> { "new object" };
            }

            var list = new List<ICommand>();
            while (true)
            {
                ICommand next = plan.GetNextCommand();
                if (next.GetType() == typeof(FinishCommand))
                {
                    break;
                }

                if(next.GetType() == typeof(GenerateEntitiesCommand))
                {
                    list.Add(next);

                    IList generatedList = generator();
                    plan.HandleGenerateCompleted(((GenerateEntitiesCommand)next).EntityContext, generatedList);
                }

                int limit = 1000;
                if (list.Count > limit)
                {
                    throw new InternalTestFailureException($"{typeof(ISupervisor)} did not complete after {limit} next items");
                }
            }

            return list;
        }


        //Assert helpers
        private void AssertPlanCount(List<IEntityDescription> descriptions,
            List<ICommand> actualCommands)
        {
            Dictionary<Type, long> expectedCalls = descriptions
                .Select(x => new
                {
                    Type = x.Type,
                    Total = x.QuantityProvider.GetTargetQuantity()
                })
                .OrderBy(x => x.Type.FullName)
                .ToDictionary(x => x.Type, x => x.Total);

            Dictionary<Type, long> actualCalls = actualCommands
                .Where(x => x.GetType() == typeof(GenerateEntitiesCommand))
                .Select(x => ((GenerateEntitiesCommand)x).EntityContext.Type)
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
            List<ICommand> actualCommands)
        {
            List<IEntityDescription> actualEntities = actualCommands
                .Where(x => x.GetType() == typeof(GenerateEntitiesCommand))
                .Select(x => ((GenerateEntitiesCommand)x).EntityContext.Description)
                .ToList();

            bool actualListEqualsExpected = expectedList.SequenceEqual(actualEntities);
            string expectedString = string.Join(", ", expectedList.Select(x => x.Type.Name));
            string actualString = string.Join(", ", actualEntities.Select(x => x.Type.Name));
            Assert.IsTrue(actualListEqualsExpected
                , $"Expected plan {expectedString} does not equal actual plan {actualString}");
        }
    }
}
