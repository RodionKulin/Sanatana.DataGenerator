using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;

namespace Sanatana.DataGeneratorSpecs.Internals.SubsetGeneration
{
    [TestClass]
    public class SubsetGeneratorSetupSingleGenericSpecs
    {
        [TestMethod]
        public void GetTarget_WhenInMemoryStorageSet_ThenTargetEntityReturned()
        {
            //Arrange
            SubsetGeneratorSetupSingle<Comment> generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup()
                .ToSubsetSetup<Comment>()
                .SetStorageInMemory(EntitiesSelection.All, true)
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            object targetComment = generatorSetup.GetTarget();

            //Assert
            targetComment.Should().NotBeNull();
        }

        [TestMethod]
        public void GetTargetMany_WhenInMemoryStorageSet_ThenTargetEntityReturned()
        {
            //Arrange
            SubsetGeneratorSetupSingle<Comment> generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup()
                .ToSubsetSetup<Comment>()
                .SetStorageInMemory(EntitiesSelection.All, true)
                .SetTargetCountSingle(EntitiesSelection.All)
                .SetTargetCount<Comment>(2);

            //Act
            object[] targetComments = generatorSetup.GetTargetMany();

            //Assert
            targetComments.Should().NotBeNull()
                .And.HaveCount(2);
        }

        [TestMethod]
        public void GetAll_WhenInMemoryStorageSet_ThenTargetAndRequiredEntitiesReturned()
        {
            //Arrange
            SubsetGeneratorSetupSingle<Comment> generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup()
                .ToSubsetSetup<Comment>()
                .SetStorageInMemory(EntitiesSelection.All, true)
                .SetTargetCountSingle(EntitiesSelection.All)
                .SetTargetCount<Comment>(2);

            //Act
            Dictionary<Type, object[]> targetAndRequiredEntities = generatorSetup.GetAll();

            //Assert
            targetAndRequiredEntities.Should().HaveCount(3);
            targetAndRequiredEntities[typeof(Comment)].Should().HaveCount(2);
            targetAndRequiredEntities[typeof(Post)].Should().HaveCount(1);
            targetAndRequiredEntities[typeof(Category)].Should().HaveCount(1);
        }
    }
}
