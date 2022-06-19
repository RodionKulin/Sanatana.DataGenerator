using Sanatana.DataGenerator.SpreadStrategies;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Entities
{
    public class RequiredEntity
    {
        /// <summary>
        /// Parent required entity type
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// Strategy to reuse same parent entities among multiple child entities.
        /// </summary>
        public ISpreadStrategy SpreadStrategy { get; set; }
    }
}
