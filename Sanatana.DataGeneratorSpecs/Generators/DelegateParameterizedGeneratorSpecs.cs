using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGeneratorSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.Generators
{
    [TestClass]
    public class DelegateParameterizedGeneratorSpecs
    {
        [TestMethod]
        public void Generate_WhenParametersOutOfOrder_ReturnsEntity()
        {
            //Prepare
            var t = new DelegateParameterizedGenerator();
            t.RegisterDelegate((GeneratorContext ctx, Category cat, Post pst) =>
            {
                return new Comment
                {
                    PostId = pst.Id,
                    CommentText = "text"
                };
            });

            //Invoke
            var generatorContext = new GeneratorContext()
            {
                RequiredEntities = new Dictionary<Type, object>
                {
                    { typeof(Post), new Post() },
                    { typeof(Category), new Category() },
                }
            };
            List<Comment> comment = t.Generate<Comment>(generatorContext);
            List<Comment> comment2 = t.Generate<Comment>(generatorContext);

            //Assert 
            Assert.IsNotNull(comment);
            Assert.AreEqual(1, comment.Count);
            Assert.IsNotNull(comment2);
            Assert.AreEqual(1, comment2.Count);
        }
    }
}
