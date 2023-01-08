using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using FluentAssertions;
using Sanatana.DataGeneratorSpecs.TestTools;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Validators;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Supervisors.Subset;
using FluentAssertions.Equivalency;
using System;

namespace Sanatana.DataGeneratorSpecs.Entities
{
    [TestClass]
    public class CloneSpecs
    {
        //TemporaryStorage also has Clone, but throws exception of Fake
        //GeneratorSetup also has Clone, but throws exception of Fake

        [TestMethod]
        public void EntityDescriptionGenericClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            EntityDescription<Comment> original = CloneVerification.Fake<EntityDescription<Comment>>();

            //Act
            IEntityDescription actualClone = original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original);
        }

        [TestMethod]
        public void EntityDescriptionClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            EntityDescription original = CloneVerification.Fake<EntityDescription>();

            //Act
            IEntityDescription actualClone = original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original);
        }

        [TestMethod]
        public void GeneratorContextClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            GeneratorContext original = CloneVerification.Fake<GeneratorContext>();

            //Act
            GeneratorContext actualClone = original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original);
        }

        [TestMethod]
        public void DefaultSettingsClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            DefaultSettings original = CloneVerification.Fake<DefaultSettings>();

            //Act
            DefaultSettings actualClone = original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original);
        }

        [TestMethod]
        public void ValidatorsSetupClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            ValidatorsSetup original = CloneVerification.Fake<ValidatorsSetup>();

            //Act
            ValidatorsSetup actualClone = original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "No members found for comparison. Please specify some members to include in the comparison or choose a more meaningful assertion.")]
        public void CompleteSupervisorClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            CompleteSupervisor original = CloneVerification.Fake<CompleteSupervisor>();

            //Act
            CompleteSupervisor actualClone = (CompleteSupervisor)original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original, opt => opt
                .Excluding((IMemberInfo memberinfo) => memberinfo.Name == "ProgressState"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "No members found for comparison. Please specify some members to include in the comparison or choose a more meaningful assertion.")]
        public void SubsetSupervisorClone_WhenCalledOnFullClone_CopiesAllProperties()
        {
            //Arrange
            SubsetSupervisor original = CloneVerification.Fake<SubsetSupervisor>();

            //Act
            SubsetSupervisor actualClone = (SubsetSupervisor)original.Clone();

            //Assert
            actualClone.Should().BeEquivalentTo(original, opt => opt
                .Excluding(x => x.Name == "ProgressState"));
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void Clone_WhenCalledOnUnfinishedClone_HasDifferentProperties()
        {
            //Arrange
            EntityDescription<Comment> original = CloneVerification.Fake<EntityDescription<Comment>>();

            //Act
            IEntityDescription unfinishedClone = original.Clone();
            unfinishedClone.Generator = null;

            //Assert
            unfinishedClone.Should().BeEquivalentTo(original);
        }

    }
}
