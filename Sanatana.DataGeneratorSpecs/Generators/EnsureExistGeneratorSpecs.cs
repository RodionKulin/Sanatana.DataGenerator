using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Comparers;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using System;

namespace Sanatana.DataGeneratorSpecs.Generators
{
    [TestClass]
    public class EnsureExistGeneratorSpecs
    {
        [TestMethod]
        public void Generate_WhenTargetCountIsLargerThenStorageSize_ThenReturnStorageAndNewInstances()
        {
            //Arrange
            var existingInstances = new List<Comment>() {
                new Comment(){ Id = 1 },
                new Comment(){ Id = 2 },
                new Comment(){ Id = 3 },
            };
            DelegateGenerator<Comment> newInstanceGenerator =
                DelegateGenerator<Comment>.Factory.Create((ctx) => new Comment() { Id = (int)ctx.CurrentCount});
            var target = new EnsureExistGenerator<Comment, int>(newInstanceGenerator)
                .SetComparer(x => x.Id);
            var generatorContext = new GeneratorContext()
            {
                TargetCount = 5,
                Description = new EntityDescription() { Type = typeof(Comment) },
                RequiredEntities = new Dictionary<Type, object>
                {
                    { typeof(Post), new Post() },
                    { typeof(Category), new Category() },
                },
                Defaults = new DefaultSettings()
                    .SetPersistentStorageSelector(new InMemoryPersistentStorageSelector(existingInstances))
            };

            //Act
            List<Comment> comments = new List<Comment>();
            for (int i = 0; i < 5; i++)
            {
                generatorContext.CurrentCount = i;
                List<Comment> comment = (List<Comment>)target.Generate(generatorContext);
                comments.AddRange(comment);
            }

            //Assert 
            comments.Should().HaveCount(5, "because that is a target count");
            comments.Select(x => x.Id).Distinct().Should().HaveCount(5, "because it should contain unique items");
        }
    }
}
