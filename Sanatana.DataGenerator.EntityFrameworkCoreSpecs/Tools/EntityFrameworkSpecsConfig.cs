using NUnit.Framework;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Providers;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;


[SetUpFixture]
public class EntityFrameworkSpecsConfig : SpecsForConfiguration
{
    public EntityFrameworkSpecsConfig()
    {
        WhenTesting<INeedSampleDatabase>().EnrichWith<SampleDbCreator>();
        WhenTesting<INeedSampleDatabase>().EnrichWith<SampleDbContextProvider>();
        WhenTesting<INeedDatabaseCleared>().EnrichWith<DbContextCleanerProvider>();
    }
}
