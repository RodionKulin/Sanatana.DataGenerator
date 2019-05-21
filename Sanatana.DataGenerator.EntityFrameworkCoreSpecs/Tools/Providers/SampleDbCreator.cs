using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using SpecsFor.Core.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Providers
{
    public class SampleDbCreator : Behavior<INeedSampleDatabase>
    {
        //fields
        private static bool _isInitialized;


        //methods
        public override void SpecInit(INeedSampleDatabase instance)
        {
            if (_isInitialized)
            {
                return;
            }


            using (var context = new SampleDbContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }

            _isInitialized = true;
        }
    }

    
}
