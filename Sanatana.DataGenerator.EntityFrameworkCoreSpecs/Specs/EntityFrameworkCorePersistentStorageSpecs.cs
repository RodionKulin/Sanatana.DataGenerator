using NUnit.Framework;
using Sanatana.DataGenerator.EntityFramework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using SpecsFor.StructureMap;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SpecsFor.Core.ShouldExtensions;
using StructureMap;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Specs
{
    public class EntityFrameworkCorePersistentStorageSpecs
    {
        [TestFixture]
        public class when_inserting_entities_to_storage : SpecsFor<EntityFrameworkCorePersistentStorage>
            , INeedSampleDatabase
        {
            private string _markerString;
            private List<Comment> _insertedComments;
            public SampleDbContext SampleDatabase { get; set; }

            protected override void When()
            {
                _markerString = GetType().FullName;

                _insertedComments = new List<Comment>
                {
                    new Comment()
                    {
                        Id = 1,
                        CommentText = _markerString
                    },
                    new Comment()
                    {
                        Id = 2,
                        CommentText = _markerString
                    }
                };
                SUT.Insert(_insertedComments).Wait();
            }

            [Test]
            public void then_inserted_entities_are_found_in_database()
            {
                List<Comment> actualComments = SampleDatabase.Comments
                    .Where(x => x.CommentText == _markerString)
                    .ToList();

                actualComments.Should().BeEquivalentTo(_insertedComments);
            }
        }

    }
}
