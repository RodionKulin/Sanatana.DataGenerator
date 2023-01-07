using NUnit.Framework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using SpecsFor.StructureMap;
using System.Collections.Generic;
using System.Linq;
using SpecsFor.Core.ShouldExtensions;
using FluentAssertions;
using Sanatana.DataGenerator.EntityFrameworkCore;
using Sanatana.EntityFrameworkCore.Batch.PostgreSql.Repositories;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    public class EfCorePersistentStorageSpecs
    {
        [TestFixture]
        public class when_inserting_entities_to_storage : SpecsFor<EfCorePersistentStorage>
            , INeedSampleDatabase, INeedDatabaseCleared
        {
            private string _markerString;
            private List<Category> _insertedCategories;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                _markerString = GetType().FullName;
                _insertedCategories = new List<Category>
                {
                    new Category()
                    {
                        MarkerText = _markerString
                    },
                    new Category()
                    {
                        MarkerText = _markerString
                    }
                };

                Func<SampleDbContext> dbContextFactory = () => new SampleDbContext();
                var repositoryFactory = new SqlRepositoryFactory(dbContextFactory);

                var sut = new EfCorePersistentStorage(repositoryFactory);
                sut.Insert(_insertedCategories).Wait();
            }

            [Test]
            public void then_inserted_instances_are_found_in_database()
            {
                List<Category> actual = SampleDatabase.Categories
                    .Where(x => x.MarkerText == _markerString)
                    .ToList();

                actual.Should().BeEquivalentTo(_insertedCategories);
            }

            [Test]
            public void then_id_returned_from_database_and_set_on_instances()
            {
                _insertedCategories.Should().OnlyContain(x => x.Id != 0);
            }
        }

    }
}
