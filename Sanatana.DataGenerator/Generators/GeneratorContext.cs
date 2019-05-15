using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Generators
{
    public class GeneratorContext
    {
        public IEntityDescription Description { get; set; }
        public long TargetQuantity { get; set; }
        public Dictionary<Type, object> RequiredEntities { get; set; }
    }
}
