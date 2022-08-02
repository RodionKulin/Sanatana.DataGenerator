using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using FluentAssertions;
using Sanatana.DataGenerator.Internals.Reflection;
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator;
using System;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;

namespace Sanatana.DataGeneratorSpecs.Internals.Extensions
{
    [TestClass]
    public class TypeExtensionsSpecs
    {
        [TestMethod]
        public void IsGenericTypeOf_WhenCalledOnDerivedType_ThenReturnsTrue()
        {
            //Arrange
            var generator = new DerivedGenerator<Comment>();
            Type generatorType = generator.GetType();

            //Act
            bool isDerived = typeof(DelegateParameterizedGenerator<>).IsGenericTypeOf(generatorType);

            //Assert
            isDerived.Should().BeTrue();
        }

        [TestMethod]
        public void IsGenericTypeOf_WhenCalledOnSamneType_ThenReturnsTrue()
        {
            //Arrange
            DelegateParameterizedGenerator<Comment> generator = DelegateParameterizedGenerator<Comment>.Factory.Create(
                (Func<GeneratorContext, Post, Comment>)((ctx, post) => default));
            Type generatorType = generator.GetType();

            //Act
            bool isDerived = typeof(DelegateParameterizedGenerator<>).IsGenericTypeOf(generatorType);

            //Assert
            isDerived.Should().BeTrue();
        }

        [TestMethod]
        public void IsGenericTypeOf_WhenCalledOnNonDerivedType_ThenReturnsTrue()
        {
            //Arrange
            Type otherType = typeof(Comment);

            //Act
            bool isDerived = typeof(DelegateParameterizedGenerator<>).IsGenericTypeOf(otherType);

            //Assert
            isDerived.Should().BeFalse();
        }
    }


    //test helpers
    class DerivedGenerator<TEntity> : DelegateParameterizedGenerator<TEntity>
        where TEntity : class
    {
        public DerivedGenerator()
            : base((Func<GeneratorContext, TEntity>)(ctx => default))
        {

        }
    }
}
