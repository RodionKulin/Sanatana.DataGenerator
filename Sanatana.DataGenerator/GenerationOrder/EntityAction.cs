using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.GenerationOrder
{
    public class EntityAction : IEquatable<EntityAction>
    {
        //properties
        public ActionType ActionType { get; set; }
        public EntityContext EntityContext { get; set; }


        //methods
        public virtual bool Equals(EntityAction other)
        {
            Type type = EntityContext?.Description?.Type;
            Type otherType = other?.EntityContext?.Description?.Type;

            if (ActionType == other.ActionType 
                && type == otherType)
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashAction = ActionType.GetHashCode();

            Type type = EntityContext?.Description?.Type;
            int hashType = type == null ? 0 : type.GetHashCode();

            return hashAction ^ hashType;
        }
    }
}
