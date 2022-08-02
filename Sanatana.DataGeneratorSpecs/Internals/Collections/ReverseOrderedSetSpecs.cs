using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Internals.Collections;
using System;
using System.Collections.Generic;
using FluentAssertions;

namespace Sanatana.DataGeneratorSpecs.Internals.Collections
{
    [TestClass]
    public class ReverseOrderedSetSpecs
    {
        [TestMethod]
        public void ReverseOrderedSet_WhenIterated_ThenReturnsReverseOrder()
        {
            //Arrange
            var set = new ReverseOrderedSet<int>();
            set.Add(1);
            set.Add(2);
            set.Add(3);

            //Act
            List<int> iterationOrder = new List<int>();
            foreach (int item in set)
            {
                iterationOrder.Add(item);
            }

            //Arrange
            iterationOrder.Should().BeEquivalentTo(new int [] { 3, 2, 1});
        }


    }
}
