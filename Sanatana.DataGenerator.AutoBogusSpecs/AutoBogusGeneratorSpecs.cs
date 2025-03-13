using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.AutoBogus;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using System.Collections.Generic;
using AutoBogus;
using Sanatana.DataGenerator.AutoBogusSpecs.Samples;
using FluentAssertions;
using System.Diagnostics;
using Sanatana.DataGenerator.AutoBogus.Binders;
using Bogus;
using Newtonsoft.Json;
using Sanatana.DataGenerator.AutoBogus.AutoGeneratorOverrides;

namespace Sanatana.DataGenerator.AutoBogusSpecs
{
    [TestClass]
    public class AutoBogusGeneratorSpecs
    {
        [TestMethod]
        public void Generate_WhenCalledWithDefaults_ThenGeneratesEntityWithRandomValues()
        {
            //Arrange
            var target = new AutoBogusGenerator()
            {
                GenerationBatchSize = 1
            };

            //Act
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //Assert
            posts.Should().NotBeNull();
            posts.Count.Should().Be(1);

            Post post = posts[0];
            post.Id.Should().NotBe(0);
            post.PostText.Should().NotBeNullOrWhiteSpace();
            post.CategoryId.Should().NotBe(0);
        }

        [TestMethod]
        public void Generate_WhenOverrideRulesAreAppliedGlobally_ThenGeneratesInstanceWithExpectedProps()
        {
            //Arrange
            AutoFaker.Configure(builder =>
            {
                builder.WithOverride(new PostOverride());
            });
            var target = new AutoBogusGenerator();

            //Act
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //Assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_WhenOverrideRulesAreAppliedForType_ThenGeneratesInstanceWithExpectedProps()
        {
            //Arrange
            var target = new AutoBogusGenerator(AutoFaker.Create(builder =>
            {
                builder.WithOverride<Post>((ov) =>
                {
                    var instance = (Post)ov.Instance;
                    instance.CategoryId = 5;
                    instance.PostText = "1234";
                    return instance;
                });
            }));

            //Act
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //Assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_WhenOverrideRulesAreAppliedWithRules_ThenGeneratesInstanceWithExpectedProps()
        {
            //Arrange
            var target = new AutoBogusGenerator<Post>(new AutoFaker<Post>()
                .RuleFor(x => x.CategoryId, f => 5)
                .RuleFor(x => x.PostText, f => f.Lorem.Text())
                .Ignore(x => x.ValueToIgnore)
            );

            //Act
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //Assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_WhenUsingSameSeed_ThenDetermenisticContentGeneratedOnSameSeed()
        {
            //Arrange
            var target = new AutoBogusGenerator<Post>(new AutoFaker<Post>()
                .RuleFor(x => x.CategoryId, f => 5)
                .RuleFor(x => x.PostText, f => f.Lorem.Text()));

            //Act
            List<Post> posts1 = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>(),
                CurrentCount = (long)int.MaxValue + 1
            });
            //Assert
            AssertPostNotEmpty(posts1);
            // Serialize posts1 to JSON
            posts1[0].PostText.Should().Be("aut");
            posts1[0].ValueToIgnore.Should().Be("withdrawal");
            posts1[0].CommentId.Should().Be(1386074234);
            // New DateTime is generated with context.Faker.Date.Recent() in UtcDateTimeGeneratorOverride

        }

        [TestMethod]
        public void Generate_WhenUsingHighGenerationBatchSize_ThenHasBetterPerformance()
        {
            //Arrange
            var target = new AutoBogusGenerator()
            {
                GenerationBatchSize = 1
            };
            var context = new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            };

            //Act
            Stopwatch timer = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                List<Post> posts1 = (List<Post>)target.Generate(context);
            }
            TimeSpan iteratorGenerationTime = timer.Elapsed;

            target.GenerationBatchSize = 100;
            timer.Restart();
            List<Post> posts2 = (List<Post>)target.Generate(context);
            TimeSpan batchGenerationTime = timer.Elapsed;

            //Assert
            iteratorGenerationTime.Should().BeGreaterThan(batchGenerationTime);
        }

        [TestMethod]
        public void Generate_WhenUsingUtcDateTimeGeneratorOverride_ThenGeneratesDateTimeWithUtcKind()
        {
            //Arrange
            AutoFaker.Configure(builder =>
            {
                builder.WithOverride(new UtcDateTimeGeneratorOverride());
            });
            var target = new AutoBogusGenerator();

            //Act
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //Assert
            posts.Should().NotBeNull();
            posts.Count.Should().Be(1);
            Post post = posts[0];
            post.CreatedDate.Kind.Should().Be(DateTimeKind.Utc);
        }

        //Assert help methods
        private void AssertPostNotEmpty(List<Post> posts)
        {
            posts.Should().NotBeNull();
            posts.Count.Should().Be(1);

            Post post = posts[0];
            post.Id.Should().NotBe(0);
            post.PostText.Should().NotBeNullOrWhiteSpace();
            post.CategoryId.Should().Be(5);
        }

    }

}
