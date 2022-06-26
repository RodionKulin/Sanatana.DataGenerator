using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.Modifiers
{
    [TestClass]
    public class DelegateParameterizedModifierSpecs
    {
        [TestMethod]
        public void Modify_WhenParametersOutOfOrder_ThenReturnsEntity()
        {
            //Arrange
            var t = DelegateParameterizedModifier<Comment>.Factory.Create(
                (GeneratorContext ctx, List<Comment> comments, Category cat, Post pst) =>
                {
                    return new Comment
                    {
                        PostId = pst.Id,
                        CommentText = "text"
                    };
                });

            //Act
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
            List<Comment> comment = (List<Comment>)t.Modify(generatorContext, inputComments);
            List<Comment> comment2 = (List<Comment>)t.Modify(generatorContext, inputComments);

            //Assert 
            Assert.IsNotNull(comment);
            Assert.AreEqual(1, comment.Count);
            Assert.IsNotNull(comment2);
            Assert.AreEqual(1, comment2.Count);
        }
    }

}
