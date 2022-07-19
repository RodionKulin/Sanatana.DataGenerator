using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGenerator.Supervisors.Subset;
using System.Collections;

namespace Sanatana.DataGeneratorSpecs.Internals.SubsetGeneration
{
    [TestClass]
    public class SubsetGeneratorSetupManySpecs
    {
        [TestMethod]
        public void GetAll_WhenInMemoryStorageSet_ThenTargetAndRequiredEntitiesReturned()
        {
            //Arrange
            SubsetGeneratorSetupMany generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup()
                .ToSubsetSetup(typeof(Category), typeof(Comment))
                .AddInMemoryStorage(EntitiesSelection.All, true)
                .SetTargetCountSingle(EntitiesSelection.All)
                .SetTargetCount<Comment>(2);

            //Act
            Dictionary<Type, IList> targetAndRequiredEntities = generatorSetup.GetAll();

            //Assert
            targetAndRequiredEntities.Should().HaveCount(3);
            targetAndRequiredEntities[typeof(Comment)].Count.Should().Be(2);
            targetAndRequiredEntities[typeof(Post)].Count.Should().Be(1);
            targetAndRequiredEntities[typeof(Category)].Count.Should().Be(1);
        }
    }
}