using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.Generators
{
    [TestClass]
    public class ReuseExistingGeneratorSpecs
    {
        [TestMethod]
        public void Generate_WhenStorageIsSmall_ThenReturnsAllStorageInstances()
        {
            //Arrange
            var existingInstances = new List<Comment>() {
                new Comment(){ Id = 1 },
                new Comment(){ Id = 2 },
                new Comment(){ Id = 3 },
            };
            var target = new ReuseExistingGenerator<Comment, int>()
                .SetBatchSizeMax();
            var generatorContext = new GeneratorContext()
            {
                TargetCount = 3,
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
            List<Comment> comment = (List<Comment>)target.Generate(generatorContext);

            //Assert 
            Assert.IsNotNull(comment);
            Assert.AreEqual(3, comment.Count);
        }
    }
}
