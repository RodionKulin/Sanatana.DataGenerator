﻿using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples;
using SpecsFor.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Interfaces
{
    public interface INeedSampleDatabase : ISpecs
    {
        SampleDbContext SampleDatabase { get; set; }
    }
}
