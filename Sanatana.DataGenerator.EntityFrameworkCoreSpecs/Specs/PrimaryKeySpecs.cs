using NUnit.Framework;
using Sanatana.DataGenerator.Entities;
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
using Sanatana.DataGenerator.Internals.Validators;
using Sanatana.DataGenerator.EntityFrameworkCore;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    [TestFixture]
    public class PrimaryKeySpecs
    {
        public class when_inserting_entity_with_autoincremented_key : SpecsFor<EfCorePersistentStorage>
            , INeedSampleDatabase, INeedDatabaseCleared
        {
            public SampleDbContext SampleDatabase { get; set; }

            [Test]
            public void then_inserted_instance_has_key_not_default()
            {
                var postCreated = new Post();
                SampleDatabase.Posts.Add(postCreated);
                SampleDatabase.SaveChanges();

                Post inserted = SampleDatabase.Posts.FirstOrDefault();
                inserted.Should().NotBeNull();
                inserted.Id.Should().NotBe(0);
            }
        }

    }
}
