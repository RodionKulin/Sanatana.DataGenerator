using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFramework;
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

            IAutoMocker mocker = instance.Mocker;
            var smm = (StructureMapAutoMocker<EntityFrameworkCorePersistentStorage>)mocker;
            smm.MoqAutoMocker.Container.Configure(cfg =>
            {
                cfg.For<Func<DbContext>>().Use((Func<DbContext>)(() => new SampleDbContext()));
            });
        }

        public override void AfterSpec(INeedSampleDatabase instance)
        {
            instance.SampleDatabase.Dispose();
        }
    }
}
