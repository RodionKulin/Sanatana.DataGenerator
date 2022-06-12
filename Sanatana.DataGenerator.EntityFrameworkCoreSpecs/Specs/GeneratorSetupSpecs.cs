using NUnit.Framework;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.EntityFramework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using Sanatana.DataGenerator.Strategies;
using SpecsFor.StructureMap;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using FluentAssertions;
using Sanatana.DataGenerator.RequestCapacityProviders;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    public class GeneratorSetupSpecs
    {
        [TestFixture]
        public class when_inserting_entities_to_storage : SpecsFor<EntityFrameworkCorePersistentStorage>
            , INeedSampleDatabase
        {
            private GeneratorSetup _generatorSetup;
            private string _markerString;
            private List<Comment> _insertedComments;
            private List<Post> _insertedPosts;
            private List<Category> _insertedCategories;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                _markerString = GetType().FullName;

                _generatorSetup = new GeneratorSetup();
                _generatorSetup.CommandsHistory.IsFileLoggingEnabled = true;
                _generatorSetup.Defaults.RequestCapacityProvider = new StrictRequestCapacityProvider(10);
                _generatorSetup.Defaults.PersistentStorages.Add(
                    new EntityFrameworkCorePersistentStorage(() => new SampleDbContext()));

                _generatorSetup.RegisterEntity<Category>()
                    .SetTargetCount(25)
                    .SetGenerator(ctx => new Category()
                    {
                        Name = $"Category #{ctx.CurrentCount}",
                        MarkerText = _markerString
                    });
                _generatorSetup.RegisterEntity<Post>()
                    .SetInsertToPersistentStorageBeforeUse(true)
                    .SetTargetCount(100)
                    .SetGenerator(ctx => new Post()
                    {
                        MarkerText = _markerString
                    });
                _generatorSetup.RegisterEntity<Comment>()
                    .SetTargetCount(50)
                    .SetGenerator<Category, Post>((ctx, category, post) => new Comment()
                    {
                        PostId = post.Id,
                        CommentText = $"Comment in category: {category.Name}",
                        MarkerText = _markerString
                    });
            }

            protected override void When()
            {
                _generatorSetup.Generate();

                _insertedPosts = SampleDatabase.Posts
                    .Where(x => x.MarkerText == _markerString)
                    .ToList();
                _insertedCategories = SampleDatabase.Categories
                    .Where(x => x.MarkerText == _markerString)
                    .ToList();
                _insertedComments = SampleDatabase.Comments
                    .Where(x => x.MarkerText == _markerString)
                    .ToList();
            }

            [Test]
            public void then_generated_categories_count_matches_target()
            {
                _insertedCategories.Count.Should().Be(25);
            }

            [Test]
            public void then_generated_posts_count_matches_target()
            {
                _insertedPosts.Count.Should().Be(100);
            }

            [Test]
            public void then_generated_comments_count_matches_target()
            {
                _insertedComments.Count.Should().Be(50);
            }

            [Test]
            public void then_generated_comments_have_post_id_not_zero()
            {
                _insertedComments.Should().OnlyContain(x => x.PostId != 0);
            }

        }

    }
}
