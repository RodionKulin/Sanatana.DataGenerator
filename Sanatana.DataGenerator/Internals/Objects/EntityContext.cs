using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.GenerationOrder;
using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Internals
{
    public class EntityContext
    {
        public Type Type { get; set; }
        public IEntityDescription Description { get; set; }
        public EntityProgress EntityProgress { get; set; }
        public List<IEntityDescription> ChildEntities { get; set; }
        public List<IEntityDescription> ParentEntities { get; set; }
    }
}
