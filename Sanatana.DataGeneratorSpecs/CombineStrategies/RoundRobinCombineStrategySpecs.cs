using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.CombineStrategies;
using FluentAssertions;

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
            var target = new RoundRobinCombineStrategy();

            //act
            int actualGeneratorIndex = target.GetNext(generatorsCount, instancesCurrentCount);

            //assert
            actualGeneratorIndex.Should().Be(expectedGeneratorIndex);
        }
    }
}
