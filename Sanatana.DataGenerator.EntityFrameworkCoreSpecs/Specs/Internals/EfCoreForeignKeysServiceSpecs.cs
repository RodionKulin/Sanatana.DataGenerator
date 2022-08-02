using NUnit.Framework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using System;
using System.Linq;
using FluentAssertions;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using System.Reflection;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Internals.Specs
{
    [TestFixture]
    public class EfCoreForeignKeysServiceSpecs
    {
        [Test]
        [TestCase(typeof(Post), new string[0])]
        [TestCase(typeof(Comment), new string[] { nameof(Comment.Id) })]
        public void GetPrimaryKeysManuallyGenerated_WhenGivenEntity_ReturnsExpectedPrimaryKeys(Type type, string[] expectedPrimaryKeysManuallyGenerated)
        {
            //Arrange
            Func<SampleDbContext> sampleDatabase = () => new SampleDbContext();
            var relationsService = new EfCoreModelService(sampleDatabase);

            //Act
            PropertyInfo[] primaryKeys = relationsService.GetPrimaryKeysManuallyGenerated(type);

            //Assert
            string[] actualPrimaryKeysNames = primaryKeys.Select(x => x.Name).ToArray();
            Enumerable.SequenceEqual(actualPrimaryKeysNames, expectedPrimaryKeysManuallyGenerated).Should().BeTrue();
        }

        [Test]
        public void FindParentEntities_WhenGivenChildEntity_ReturnsRelatedParentEntities()
        {
            //Arrange
            Func<SampleDbContext> sampleDatabase = () => new SampleDbContext();
            var relationsService = new EfCoreModelService(sampleDatabase);

            //Act
            Type[] parentTypes = relationsService.GetParentEntities(typeof(Comment));

            //Assert
            Enumerable.SequenceEqual(parentTypes, new[] { typeof(Post) }).Should().BeTrue();
        }

        [Test]
        public void SetForeignKeysOnChild_WhenGivenChildAndParentInstance_SetsForeignKeyOnChildInstance()
        {
            //Arrange
            Func<SampleDbContext> sampleDatabase = () => new SampleDbContext();
            var relationsService = new EfCoreModelService(sampleDatabase);
            var childInstance = new Comment();

            //Act
            relationsService.SetForeignKeysOnChild(childInstance , new Post() { Id = 1});

            //Assert
            childInstance.PostId.Should().Be(1);
        }
    }
}
