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
    public class GeneratorSetupModifierSpecs
    {
        [TestMethod]
        public void AddModifier_WhenModifierIsSetWithSingularInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddModifier((GeneratorContext context, Comment comment) => comment.CommentText = "Modified")
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        public void AddSingleModifier_WhenModifierIsSetWithSingularInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddSingleModifier((GeneratorContext context, Comment comment) =>
                    {
                        comment.CommentText = "Modified";
                        return comment;
                    })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        public void AddMultiModifier_WhenModifierIsSetWithSingularInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddMultiModifier((GeneratorContext context, Comment comment) =>
                    {
                        comment.CommentText = "Modified";
                        return new List<Comment> { comment };
                    })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        public void AddModifier_WhenModifierIsSetWithMultiInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddModifier((GeneratorContext context, List<Comment> comments) => comments[0].CommentText = "Modified")
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        public void AddSingleModifier_WhenModifierIsSetWithMultiInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddSingleModifier((GeneratorContext context, List<Comment> comments) =>
                    {
                        comments[0].CommentText = "Modified";
                        return comments[0];
                    })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        public void AddMultiModifier_WhenModifierIsSetWithMultiInput_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .AddMultiModifier((GeneratorContext context, List<Comment> comments) =>
                    {
                        comments[0].CommentText = "Modified";
                        return comments;
                    })
                )
                .SetTargetCountSingle(EntitiesSelection.All);

            //Act
            Comment comment = subsetSetup.GetSingleTarget();

            //Assert
            comment.CommentText.Should().Be("Modified");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No inner modifiers provided in CombineModifier for type Sanatana.DataGeneratorSpecs.TestTools.Entities.Comment. Expected at least 1 inner modifier. CombineModifier should use multiple modifiers in turn to produce entity instances.")]
        public void AddCombineModifier_WhenZeroModifierSetsUsed_ThenThrowsExpectedException()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineModifier(combine => combine
                        //no ModifierSet added
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();

        }

        [TestMethod]
        public void AddCombineModifier_WhenSingleModifierSetsUsed_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineModifier(combine => combine
                        .AddModifiersSet(set => set
                            .AddModifier((GeneratorContext context, Comment comment) => comment.CommentText = "Modified 1")
                        )
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();

            //Assert
            comment.Should().HaveCount(2);
            comment.Should().AllSatisfy(x => x.CommentText.Should().Be("Modified 1"));
        }

        [TestMethod]
        public void AddCombineModifier_WhenMultipleModifierSetsUsed_ThenGeneratedInstanceHasExpectedPropertyModified()
        {
            //Arrange
            GeneratorSetup generatorSetup = CompleteSupervisorProvider.GetMixedRequiredOrderGeneratorSetup();
            SubsetGeneratorSetupSingle<Comment> subsetSetup = generatorSetup.ToSubsetSetup<Comment>()
                .EditEntity<Comment>(entity => entity
                    .SetCombineModifier(combine => combine
                        .AddModifiersSet(set => set
                            .AddModifier((GeneratorContext context, Comment comment) => comment.CommentText = "Modified 1")
                        )
                        .AddModifiersSet(set => set
                            .AddModifier((GeneratorContext context, Comment comment) => comment.CommentText = "Modified 2")
                        )
                    )
                )
                .SetTargetCount(EntitiesSelection.All, 2);

            //Act
            Comment[] comment = subsetSetup.GetMultipleTargets();

            //Assert
            comment.Should().HaveCount(2);
            comment[0].CommentText.Should().Be("Modified 1");
            comment[1].CommentText.Should().Be("Modified 2");
        }
    }
}
