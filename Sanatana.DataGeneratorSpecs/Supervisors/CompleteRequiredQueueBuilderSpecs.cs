using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Contracts;
using Sanatana.DataGenerator.Internals.Commands;

namespace Sanatana.DataGeneratorSpecs.Supervisors
{
    [TestClass]
    public class CompleteRequiredQueueBuilderSpecs
    {

        [TestMethod]
        public void GetNextCommand_WhenCalledWithMixedOrderRequired_ThenReturnesExpectedCommands()
        {
            //Arrange
            Dictionary<Type, IEntityDescription> mixedOrderEntities = EntityDescriptionProvider.GetMixedRequiredOrderEntities(1);
            var provider = new CompleteSupervisorProvider(mixedOrderEntities);
            IRequiredQueueBuilder target = provider.RequiredQueueBuilder;

            //Act
            List<Type> generationOrder = GetOrderedGenerationTypes(target, provider);

            //Assert
            var expectedOrder = new List<Type> { typeof(Category), typeof(Post), typeof(Comment) };
            generationOrder.Should().HaveCount(3)
                .And.Equal(expectedOrder);
        }


        //test helpers
        private List<Type> GetOrderedGenerationTypes(IRequiredQueueBuilder requiredQueueBuilder, CompleteSupervisorProvider provider)
        {
            var list = new List<Type>();

            GenerateCommand nextCommand = (GenerateCommand)requiredQueueBuilder.GetNextCommand();
            while (nextCommand != null)
            {
                list.Add(nextCommand.EntityContext.Type);

                var itemsGenerated = new List<string>() { "item" };
                provider.Supervisor.HandleGenerateCompleted(nextCommand.EntityContext, itemsGenerated);
              
                int limit = 1000;
                if (list.Count > limit)
                {
                    throw new InternalTestFailureException($"{typeof(ISupervisor)} did not complete after {limit} next items");
                }

                nextCommand = (GenerateCommand)requiredQueueBuilder.GetNextCommand();
            }

            return list;
        }
    }
}
