using Sanatana.DataGenerator.SpreadStrategies;
using System;

namespace Sanatana.DataGenerator.Entities
{
    public class RequiredEntity : IEquatable<RequiredEntity>
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


        //IEquatable<RequiredEntity> methods
        public virtual bool Equals(RequiredEntity? other)
        {
            if(other == null)
            {
                return false;
            }

            return Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }

    }
}
