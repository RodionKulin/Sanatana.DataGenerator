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
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Providers;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    public class EnsureExistGeneratorSpecs
    {
        [TestFixture]
        public class when_inserting_entities_to_storage : SpecsFor<EntityFrameworkCorePersistentStorage>
               , INeedSampleDatabase, INeedDatabaseCleared
        {
            private GeneratorSetup _generatorSetup;
            private List<Comment> _insertedComments;
            private List<Post> _insertedPosts;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                using(var db = new SampleDbContext())
                {
                    db.Comments.AddRange(new Comment[]
                    {
                        new Comment() { CommentText = "existing comment"},
                        new Comment() { CommentText = "existing comment"},
                        new Comment() { CommentText = "existing comment"},
                    });
                    db.SaveChanges();
                }

                _generatorSetup = new GeneratorSetup()
                    .SetDefaultSettings(defaults => defaults
                        .SetRequestCapacityProvider(new StrictRequestCapacityProvider(10))
                        .AddPersistentStorage(new EntityFrameworkCorePersistentStorage(() => new SampleDbContext()))
                        .SetDefaultEqualityComparer(new SimpleEqualityComparerFactory())
                        .SetPersistentStorageSelector(new EntityFrameworkCorePersistentStorage(() => new SampleDbContext()))
                    )
                    .RegisterEntity<Post>(entity => entity
                        .SetTargetCount(100)
                        .SetGenerator(ctx => new Post() { Id = (int)ctx.CurrentCount })
                    )
                    .RegisterEntity<Comment>(entity => entity
                        .SetTargetCount(50)
                        .SetEnsureExistGenerator<int, Post>(
                            (ctx, post) => new Comment() { 
                                Id = (int)ctx.CurrentCount,
                                PostId = post.Id,
                                CommentText = "new comment"
                            }, 
                            gen => gen.SetOrderBy(x => x.Id))
                        );

            }

            protected override void When()
            {
                _generatorSetup.Generate();

                _insertedPosts = SampleDatabase.Posts.ToList();
                _insertedComments = SampleDatabase.Comments.ToList();
            }

            [Test]
            public void then_generated_expected_count_of_new_comments()
            {
                _insertedComments.Where(x => x.CommentText == "new comment")
                    .Should()
                    .HaveCount(50 - 3); //TargetCount - ExistingCount
            }

            [Test]
            public void then_existing_comments_count_should_remain_as_before_generation()
            {
                _insertedComments.Where(x => x.CommentText == "existing comment")
                    .Should()
                    .HaveCount(3); //ExistingCount
            }

            [Test]
            public void then_generated_comments_should_have_new_ids()
            {
                int[] existingIds = _insertedComments.Where(x => x.CommentText == "existing comment")
                    .Select(x => x.Id)
                    .ToArray();
                int[] newIds = _insertedComments.Where(x => x.CommentText == "new comment")
                    .Select(x => x.Id)
                    .ToArray();

                newIds.Should().NotIntersectWith(existingIds);
            }

            [Test]
            public void then_inserted_posts_should_have_expected_count()
            {
                _insertedPosts.Should().HaveCount(100);
            }
        }

    }
}
