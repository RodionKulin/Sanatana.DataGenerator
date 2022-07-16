using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Entities
{
    public class RequiredEntity
    {
        /// <summary>
        /// Required entity type. Instances of required entity type will be passed to generator as arguments.
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Strategy to reuse same parent entity instances among multiple child entity instances.
        /// </summary>
        public ISpreadStrategy SpreadStrategy { get; set; }



        public RequiredEntity() {}

        public RequiredEntity(Type type)
        {
            Type = type;
        }
    }
}
