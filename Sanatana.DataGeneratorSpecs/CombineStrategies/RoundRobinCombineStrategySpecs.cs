using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.CombineStrategies;
using FluentAssertions;
using System.Linq;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System.Collections.Generic;
using Sanatana.DataGenerator;

namespace Sanatana.DataGeneratorSpecs.CombineStrategies
{
    [TestClass]
    public class RoundRobinCombineStrategySpecs
    {
        [TestMethod]
        [DataRow(1, 0, 0)]
        [DataRow(1, 1, 0)]
        [DataRow(10, 0, 0)]
        [DataRow(10, 1, 1)]
        [DataRow(10, 10, 0)]
        [DataRow(10, 11, 1)]
        [DataRow(10, 12, 2)]
        [DataRow(10, 512, 2)]
        public void GetNextGenerator_WhenCallWithProvidedArguments_ThenReturnsExpectedIndex(int generatorsCount,
            int instancesCurrentCount, int expectedGeneratorIndex)
        {
            //Arrange
            var target = new RoundRobinGeneratorsCombiner();
            List<IGenerator> generators = Enumerable.Range(0, generatorsCount)
                .Select(x => (IGenerator)DelegateGenerator<Post>.Factory.Create(ctx => null))
                .ToList();
            var generatorCtx = new GeneratorContext()
            {
                CurrentCount = instancesCurrentCount
            };

            //Act
            IGenerator actualGenerator = target.GetNext(generators, generatorCtx);

            //Assert
            int actualGeneratorIndex = generators.IndexOf(actualGenerator);
            actualGeneratorIndex.Should().Be(expectedGeneratorIndex);
        }
    }
}
