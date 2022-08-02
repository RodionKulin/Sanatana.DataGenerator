using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Supervisors.Subset;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using FluentAssertions;
using Sanatana.DataGenerator.Internals.SubsetGeneration;
using System.Collections.Generic;
using System;

namespace Sanatana.DataGeneratorSpecs.Modifiers
{
    [TestClass]
    public class GeneratorSetupGeneratorSpecs
    {
        [TestMethod]
        public void SetGenerator_WhenGeneratorIsSetWithSingularOutput_ThenGeneratedInstanceHasExpectedProperty()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetGenerator((GeneratorContext context) => new Comment() { CommentText = "Generated 1" })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Generated 1");
        }

        [TestMethod]
        public void SetGenerator_WhenGeneratorIsSetWithMultipleOutput_ThenGeneratedInstanceHasExpectedProperty()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetMultiGenerator((GeneratorContext context) =>
                    {
                        var comment = new Comment() { CommentText = "Generated 1" };
                        return new List<Comment> { comment };
                    })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Generated 1");
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No inner generators provided in CombineGenerator for type Sanatana.DataGeneratorSpecs.TestTools.Entities.Comment. Expected at least 1 inner generator. CombineGenerator should use multiple generators in turn to produce entity instances.")]
        public void AddCombineGenerator_WhenZeroGeneratorsAdded_ThenThrowsExpectedException()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineGenerator(combine => combine
                        //no Generator added
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();
        }

        [TestMethod]
        public void AddCombineGenerator_WhenSingleGeneratorAdded_ThenGeneratedInstanceHasExpectedProperty()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineGenerator(combine => combine
                        .AddGenerator(context => new Comment() { CommentText = "Generated 1" })
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();

            //Assert
            comment.Should().HaveCount(2);
            comment.Should().AllSatisfy(x => x.CommentText.Should().Be("Generated 1"));
        }

        [TestMethod]
        public void AddCombineGenerator_WhenMultipleGeneratorsAdded_ThenGeneratedInstanceHasExpectedProperty()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineGenerator(combine => combine
                        .AddGenerator(context => new Comment() { CommentText = "Generated 1" })
                        .AddGenerator(context => new Comment() { CommentText = "Generated 2" })
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();

            //Assert
            comment.Should().HaveCount(2);
            comment[0].CommentText.Should().Be("Generated 1");
            comment[1].CommentText.Should().Be("Generated 2");
        }

    }
}
