using Moq;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.SpecsForAddons
{
    public class NoMockAutoMocker : IAutoMocker
    {
        public void ConfigureContainer()
        {

        }

        public TSut CreateSUT<TSut>() where TSut : class
        {
            return null;
        }

        public Mock<T> GetMockFor<T>() where T : class
        {
            return null;
        }
    }
}
