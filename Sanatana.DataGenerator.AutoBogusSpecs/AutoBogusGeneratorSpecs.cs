using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.AutoBogus;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using System.Collections.Generic;
using AutoBogus;
using Sanatana.DataGenerator.AutoBogusSpecs.Samples;
using FluentAssertions;

namespace Sanatana.DataGenerator.AutoBogusSpecs
{
    [TestClass]
    public class AutoBogusGeneratorSpecs
    {
        [TestMethod]
        public void Generate_GeneratesEntityWithRandomValuesByDefault()
        {
            //setup
            var target = new AutoBogusGenerator();

            //invoke
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //assert
            posts.Should().NotBeNull();
            posts.Count.Should().Be(1);

            Post post = posts[0];
            post.Id.Should().NotBe(0);
            post.PostText.Should().NotBeNullOrWhiteSpace();
            post.CategoryId.Should().NotBe(0);
        }

        [TestMethod]
        public void Generate_OverrideRulesAreAppliedGlobally()
        {
            //setup
            AutoFaker.Configure(builder =>
            {
                builder.WithOverride(new PostOverride());
            });
            var target = new AutoBogusGenerator();

            //invoke
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_OverrideRulesAreAppliedPerType()
        {
            //setup
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

            //invoke
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_OverrideRulesAreAppliedWithRules()
        {
            //setup
            var target = new AutoBogusGenerator<Post>(new AutoFaker<Post>()
                .RuleFor(x => x.CategoryId, f => 5)
                .RuleFor(x => x.PostText, f => f.Lorem.Text()));

            //invoke
            List<Post> posts = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>()
            });

            //assert
            AssertPostNotEmpty(posts);
        }

        [TestMethod]
        public void Generate_DetermenisticContentGeneratedOnSameSeed()
        {
            //setup
            var target = new AutoBogusGenerator<Post>(new AutoFaker<Post>()
                .RuleFor(x => x.CategoryId, f => 5)
                .RuleFor(x => x.PostText, f => f.Lorem.Text()));

            //invoke
            List<Post> posts1 = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>(),
                CurrentCount = (long)int.MaxValue + 1
            });
            List<Post> posts2 = (List<Post>)target.Generate(new GeneratorContext
            {
                Description = new EntityDescription<Post>(),
                CurrentCount = (long)int.MaxValue + 1
            });

            //assert
            AssertPostNotEmpty(posts1);
            AssertPostNotEmpty(posts2);

            posts1[0].Should().BeEquivalentTo(posts2[0]);
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
