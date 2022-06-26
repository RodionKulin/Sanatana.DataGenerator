using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.Progress;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Sanatana.DataGenerator.TotalCountProviders;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGenerator.Internals.EntitySettings
{
    public class EntityContext
    {
        //properties
        public Type Type { get; set; }
        public IEntityDescription Description { get; set; }
        public EntityProgress EntityProgress { get; set; }
        public List<IEntityDescription> ChildEntities { get; set; }
        public List<IEntityDescription> ParentEntities { get; set; }


        //init
        public static class Factory
        {
            public static EntityContext Create(Dictionary<Type, IEntityDescription> allDescriptions, IEntityDescription description)
            {
                List<IEntityDescription> children = allDescriptions.Values
                    .Where(x => x.Required != null
                        && x.Required.Select(req => req.Type).Contains(description.Type))
                    .ToList();

                var parents = new List<IEntityDescription>();
                if (description.Required != null)
                {
                    IEnumerable<Type> parentTypes = description.Required.Select(x => x.Type);
                    parents = allDescriptions.Values
                       .Where(x => parentTypes.Contains(x.Type))
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
