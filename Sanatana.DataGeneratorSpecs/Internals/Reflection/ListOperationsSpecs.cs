using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using FluentAssertions;
using Sanatana.DataGenerator.Internals.Reflection;

namespace Sanatana.DataGeneratorSpecs.Internals.Reflection
{
    [TestClass]
    public class ListOperationsSpecs
    {
        [TestMethod]
        [DataRow(10, 6, 6)]
        [DataRow(10, 0, 0)]
        [DataRow(10, -1, 0)]
        [DataRow(10, 10, 10)]
        [DataRow(10, 20, 10)]
        [DataRow(0, 20, 0)]
        public void Take_WhenCalledWithVariousInputs_ThenReturnsExpectedLengthList(int startingListCount, int takeAmount, int expectedCount)
        {
            //Arrange
            var target = new ListOperations();
            List<int> startingList = Enumerable.Range(10, startingListCount).ToList();

            //Act
            IList newList = target.Take(typeof(int), startingList, takeAmount);

            //Assert
            newList.Count.Should().Be(expectedCount);

            List<int> expectedList = Enumerable.Range(10, expectedCount).ToList();
            newList.Should().BeEquivalentTo(expectedList);
        }

        [TestMethod]
        [DataRow(10, 6, 4, 16)]
        [DataRow(10, 0, 10, 10)]
        [DataRow(10, -1, 10, 10)]
        [DataRow(10, 10, 0, 0)]
        [DataRow(10, 20, 0, 0)]
        [DataRow(0, 20, 0, 0)]
        public void Skip_WhenCalledWithVariousInputs_ThenReturnsExpectedLengthList(int startingListCount, int skipAmount, 
            int expectedCount, int expectedStartsFrom)
        {
            //Arrange
            var target = new ListOperations();
            List<int> startingList = Enumerable.Range(10, startingListCount).ToList();

            //Act
            IList newList = target.Skip(typeof(int), startingList, skipAmount);

            //Assert
            newList.Count.Should().Be(expectedCount);

            List<int> expectedList = Enumerable.Range(expectedStartsFrom, expectedCount).ToList();
            newList.Should().BeEquivalentTo(expectedList);
        }
    }
}
