using NUnit.Framework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using SpecsFor.StructureMap;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.Validators;
using Sanatana.DataGenerator.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.SampleDataSetup;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.SpecsForAddons;
using Sanatana.DataGenerator.AutoBogus;
using System;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    public class GeneratorSetupSpecs
    {
        [TestFixture]
        public class when_inserting_entities_to_storage_with_generator : SpecsFor<EfCorePersistentStorage>
            , INeedSampleDatabase, INeedDatabaseCleared
        {
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
            }

            protected override void When()
            {
                var generatorSetup = new GeneratorSetup()
                    .SetValidators(val => val.RemoveValidator<InstancesCountGeneratedValidator>())
                    .SetDefaultSettings(defaults => defaults
                        .SetRequestCapacityProvider(new StrictRequestCapacityProvider(10))
                        .AddPersistentStorage(new EfCorePersistentStorage(() => new SampleDbContext()))
                    )
                    .RegisterEntity<Category>(entity => entity
                        .SetTargetCount(25)
                        .SetGenerator(ctx => new Category()
                        {
                            Name = $"Category #{ctx.CurrentCount}"
                        })
                    )
                    .RegisterEntity<Post>(entity => entity
                        .SetInsertToPersistentStorageBeforeUse(true)
                        .SetTargetCount(100)
                        .SetGenerator(ctx => new Post())
                    )
                    .RegisterEntity<Comment>(entity => entity
                        .SetTargetCount(50)
                        .SetGenerator<Category, Post>((ctx, category, post) => new Comment()
                        {
                            Id = (int)ctx.CurrentCount,
                            PostId = post.Id,
                            CommentText = $"Comment in category: {category.Name}"
                        })
                    );
                generatorSetup.Generate();
            }

            [Test]
            public void then_generated_categories_count_matches_target()
            {
                List<Category> insertedCategories = SampleDatabase.Categories
                    .ToList();

                insertedCategories.Count.Should().Be(25);
            }

            [Test]
            public void then_generated_posts_count_matches_target()
            {
                List<Post> insertedPosts = SampleDatabase.Posts
                    .ToList();

                insertedPosts.Count.Should().Be(100);
            }

            [Test]
            public void then_generated_comments_count_matches_target()
            {
                List<Comment> insertedComments = SampleDatabase.Comments
                    .ToList();

                insertedComments.Count.Should().Be(50);
            }

            [Test]
            public void then_generated_comments_have_post_id_not_zero()
            {
                List<Comment> insertedComments = SampleDatabase.Comments
                    .ToList();

                insertedComments.Should().OnlyContain(x => x.PostId != 0);
            }
        }

        [TestFixture]
        public class when_inserting_entities_to_storage_with_ensure_exist_generator : NoMockSpecsFor
            , INeedSampleDatabase, INeedDatabaseCleared
        {
            public SampleDbContext SampleDatabase { get; set; }

            protected override void Given()
            {
                using (var db = new SampleDbContext())
                {
                    var existingPost = new Post();
                    db.Posts.Add(existingPost);
                    db.SaveChanges();

                    db.Comments.AddRange(new Comment[]
                    {
                        new Comment() { Id = 1, CommentText = "existing comment", PostId = existingPost.Id},
                        new Comment() { Id = 2, CommentText = "existing comment", PostId = existingPost.Id},
                        new Comment() { Id = 3, CommentText = "existing comment", PostId = existingPost.Id},
                    });
                    db.SaveChanges();
                }
            }

            protected override void When()
            {
                var generatorSetup = new GeneratorSetup()
                    .SetDefaultSettings(defaults => defaults
                        .SetRequestCapacityProvider(new StrictRequestCapacityProvider(10))
                        .AddPersistentStorage(new EfCorePersistentStorage(() => new SampleDbContext()))
                        .SetDefaultEqualityComparer(new SimpleEqualityComparerFactory())
                        .SetPersistentStorageSelector(new EfCorePersistentStorage(() => new SampleDbContext()))
                    )
                    .RegisterEntity<Post>(entity => entity
                        .SetTargetCount(100)
                        .SetGenerator(ctx => new Post())
                        .SetInsertToPersistentStorageBeforeUse(true)
                    )
                    .RegisterEntity<Comment>(entity => entity
                        .SetTargetCount(50)
                        .SetEnsureExistGenerator<int, Post>(
                            (ctx, post) => new Comment()
                            {
                                Id = (int)ctx.CurrentCount,
                                PostId = post.Id,
                                CommentText = "new comment"
                            },
                            gen => gen.SetOrderBy(x => x.Id))
                        );
                generatorSetup.Generate();

            }

            [Test]
            public void then_generated_expected_count_of_new_comments()
            {
                List<Comment> insertedComments = SampleDatabase.Comments.ToList();

                insertedComments.Where(x => x.CommentText == "new comment")
                    .Should()
                    .HaveCount(50 - 3); //TargetCount - ExistingCount
            }

            [Test]
            public void then_existing_comments_count_should_remain_as_before_generation()
            {
                List<Comment> insertedComments = SampleDatabase.Comments.ToList();

                insertedComments.Where(x => x.CommentText == "existing comment")
                    .Should()
                    .HaveCount(3); //ExistingCount
            }

            [Test]
            public void then_generated_comments_should_have_new_ids()
            {
                List<Comment> insertedComments = SampleDatabase.Comments.ToList();

                int[] existingIds = insertedComments.Where(x => x.CommentText == "existing comment")
                    .Select(x => x.Id)
                    .ToArray();
                int[] newIds = insertedComments.Where(x => x.CommentText == "new comment")
                    .Select(x => x.Id)
                    .ToArray();

                newIds.Should().NotIntersectWith(existingIds);
            }

            [Test]
            public void then_inserted_posts_should_have_expected_count()
            {
                List<Post> insertedPosts = SampleDatabase.Posts.ToList();

                int postsInsertedDuringGivenStage = 1;
                insertedPosts.Should().HaveCount(100 + postsInsertedDuringGivenStage);
            }
        }

        [TestFixture]
        public class when_inserting_entities_to_storage_with_full_ef_settings_bundle : NoMockSpecsFor
            , INeedSampleDatabase, INeedDatabaseCleared
        {
            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                Func<SampleDbContext> dbContextFactory = () => new SampleDbContext();

                var setup = new GeneratorSetup()
                    .SetDefaultSettings(def => def
                        .SetGenerator(new AutoBogusGenerator()) //will populate random values for entity instances
                        .SetTargetCount(100)    //set TargetCount for Category, other entities will override it
                    )
                    .SetupWithEntityFrameworkCore(dbContextFactory, efSetup => efSetup
                        .SetupFullEfSettingsBundle()
                    )
                    .ModifyEntity<Post>(entity => entity
                        .SetTargetCount(10)
                    )
                    .ModifyEntity<Comment>(entity => entity
                        .SetTargetCount(10)
                    );

                //Act
                setup.Generate();
            }

            [Test]
            public void then_generated_categories_count_matches_target()
            {
                List<Category> insertedCategories = SampleDatabase.Categories.ToList();

                insertedCategories.Count.Should().Be(100);
            }

            [Test]
            public void then_generated_posts_count_matches_target()
            {
                List<Post> insertedPosts = SampleDatabase.Posts.ToList();

                insertedPosts.Count.Should().Be(10);
            }

            [Test]
            public void then_generated_comments_count_matches_target()
            {
                List<Comment> insertedComments = SampleDatabase.Comments.ToList();

                insertedComments.Count.Should().Be(10);
            }

            [Test]
            public void then_generated_comments_have_post_id_not_zero()
            {
                List<Comment> insertedComments = SampleDatabase.Comments.ToList();

                insertedComments.Should().OnlyContain(x => x.PostId != 0);
            }

        }

    }
}
