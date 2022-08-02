using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using SpecsFor.Core;
using SpecsFor.Core.Configuration;
using SpecsFor.StructureMap;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Providers
{
    public class SampleDbContextProvider : Behavior<INeedSampleDatabase>
    {
        //methods
        public override void SpecInit(INeedSampleDatabase instance)
        {
            instance.SampleDatabase = new SampleDbContext();
        }

        public override void AfterSpec(INeedSampleDatabase instance)
        {
            instance.SampleDatabase.Dispose();
        }
    }
}
