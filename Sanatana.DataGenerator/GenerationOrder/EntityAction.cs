using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.GenerationOrder
{
    public class EntityAction
    {
        public ActionType ActionType { get; set; }
        public EntityContext EntityContext { get; set; }
    }
}
