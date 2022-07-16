using Moq;
using SpecsFor.Core;
using SpecsFor.StructureMap;
using StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.SpecsForAddons
{
    public class NoMockSpecsFor : SpecsFor<GeneratorSetup>
    {
        protected override IAutoMocker CreateAutoMocker()
        {
            return new NoMockAutoMocker();
        }
    }

    
}
