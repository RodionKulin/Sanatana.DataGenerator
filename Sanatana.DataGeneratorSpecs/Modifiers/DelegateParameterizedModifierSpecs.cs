using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Modifiers;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Sanatana.DataGeneratorSpecs.Modifiers
{
    [TestClass]
    public class DelegateParameterizedModifierSpecs
    {
        [TestMethod]
        public void Modify_WhenParametersOutOfOrder_ThenReturnsEntity()
        {
            //Arrange
            var modifier = DelegateParameterizedModifier<Comment>.Factory.Create(
                (GeneratorContext ctx, List<Comment> comments, Category cat, Post pst) =>
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
            var inputComments = new List<Comment> {
                new Comment()
            };

            //Act
            List<Comment> comments1 = (List<Comment>)modifier.Modify(generatorContext, inputComments);
            List<Comment> comments2 = (List<Comment>)modifier.Modify(generatorContext, inputComments);

            //Assert 
            comments1.Should().NotBeNull()
                .And.HaveCount(1);
            comments2.Should().NotBeNull()
                .And.HaveCount(1);
        }

        [TestMethod]
        public void GetRequiredEntitiesFuncArguments_WhenCalledForReturnVoidModifyFunction_ThenReturnsExpectedTypesCount()
        {
            //Arrange
            var modifier = DelegateParameterizedModifier<Comment>.Factory.Create(
                (GeneratorContext ctx, List<Comment> comments, Category cat) => {});

            //Act
            List<Type> requiredTypes = modifier.GetRequiredEntitiesFuncArguments();

            //Assert 
            requiredTypes.Should().HaveCount(1)
                .And.Contain(typeof(Category));
        }


        [TestMethod]
        public void GetRequiredEntitiesFuncArguments_WhenCalledForReturnMultiModifyFunction_ThenReturnsExpectedTypesCount()
        {
            //Arrange
            var modifier = DelegateParameterizedModifier<Comment>.Factory.CreateMulti(
                (GeneratorContext ctx, List<Comment> comments, Category cat) => comments);

            //Act
            List<Type> requiredTypes = modifier.GetRequiredEntitiesFuncArguments();

            //Assert 
            requiredTypes.Should().HaveCount(1)
                .And.Contain(typeof(Category));
        }
    }

}
