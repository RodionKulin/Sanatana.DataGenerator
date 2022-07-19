using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.Generators
{
    [TestClass]
    public class DelegateParameterizedGeneratorSpecs
    {
        [TestMethod]
        public void Generate_WhenParametersOutOfOrder_ThenReturnsEntity()
        {
            //Arrange
            var t = DelegateParameterizedGenerator<Comment>.Factory.Create(
                (GeneratorContext ctx, Category cat, Post pst) =>
                {
                    return new Comment
                    {
                        PostId = pst.Id,
                        CommentText = "text"
                    };
                });
            var generatorContext = new GeneratorContext()
            {
                RequiredEntities = new Dictionary<Type, object>
                {
                    { typeof(Post), new Post() },
                    { typeof(Category), new Category() },
                }
            };

            //Act
            List<Comment> comment = (List<Comment>)t.Generate(generatorContext);
            List<Comment> comment2 = (List<Comment>)t.Generate(generatorContext);

            //Assert 
            Assert.IsNotNull(comment);
            Assert.AreEqual(1, comment.Count);
            Assert.IsNotNull(comment2);
            Assert.AreEqual(1, comment2.Count);
        }
    }
}
