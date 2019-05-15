using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGeneratorSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.Modifiers
{
    [TestClass]
    public class DelegateParameterizedPostProcessorSpecs
    {
        [TestMethod]
        public void Process_WhenParametersOutOfOrder_ReturnsEntity()
        {
            //Prepare
            var t = new DelegateParameterizedModifier();
            t.RegisterDelegate((GeneratorContext ctx, List<Comment> comments, Category cat, Post pst) =>
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
            List<Comment> inputComments = new List<Comment> {
                new Comment()
            };
            List<Comment> comment = t.Modify<Comment>(generatorContext, inputComments);
            List<Comment> comment2 = t.Modify<Comment>(generatorContext, inputComments);

            //Assert 
            Assert.IsNotNull(comment);
            Assert.AreEqual(1, comment.Count);
            Assert.IsNotNull(comment2);
            Assert.AreEqual(1, comment2.Count);
        }
    }

}
