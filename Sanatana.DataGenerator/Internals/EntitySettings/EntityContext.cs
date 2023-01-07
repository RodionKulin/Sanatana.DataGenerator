using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.EntitySettings
{
    public class EntityContext
    {
        //properties
        public Type Type { get; set; }
        public IEntityDescription Description { get; set; }
        public EntityProgress EntityProgress { get; set; }
        /// <summary>
        /// Entities where this type is Required to generate child.
        /// </summary>
        public List<IEntityDescription> ChildEntities { get; set; }
        /// <summary>
        /// Parent entities are Required to generate this entity type.
        /// </summary>
        public List<IEntityDescription> ParentEntities { get; set; }


        //init
        public static class Factory
        {
            public static EntityContext Create(Dictionary<Type, IEntityDescription> allDescriptions, IEntityDescription description)
            {
                List<IEntityDescription> children = allDescriptions.Values
                    .Where(x => x.Required != null
                        && x.Required.Select(req => req.Type).Contains(description.Type))
                    .Where(x => x.Type != description.Type) //skip self reference by entity
                    .ToList();
                
                var parents = new List<IEntityDescription>();
                if (description.Required != null)
                {
                    IEnumerable<Type> parentTypes = description.Required.Select(x => x.Type);
                    parents = allDescriptions.Values
                        .Where(x => parentTypes.Contains(x.Type))
                        .Where(x => x.Type != description.Type) //skip self reference by entity
                        .ToList();
                }

                return new EntityContext
                {
                    Type = description.Type,
                    Description = description,
                    ChildEntities = children,
                    ParentEntities = parents,
                    EntityProgress = null
                };
            }

        }

    }
}
