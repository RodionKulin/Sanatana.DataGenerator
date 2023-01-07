using Microsoft.EntityFrameworkCore;
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
    public class DbContextCleanerProvider : Behavior<INeedDatabaseCleared>
    {
        //methods
        public override void SpecInit(INeedDatabaseCleared instance)
        {
            instance.SampleDatabase.Database.ExecuteSqlRaw($@"
DELETE FROM [Comments];
DELETE FROM [Posts];
DELETE FROM [Categories];
");
        }

        public override void AfterSpec(INeedDatabaseCleared instance)
        {
        }
    }
}
