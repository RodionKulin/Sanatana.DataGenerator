using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.RequestCapacityProviders;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Internals.Commands
{
    /// <summary>
    /// Call entity's generator and increment counters
    /// </summary>
    public class GenerateCommand : ICommand
    {
        //fields
        protected GeneratorServices _generatorServices;
        protected Dictionary<Type, EntityContext> _entityContexts;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public GenerateCommand(EntityContext entityContext, GeneratorServices generatorServices)
        {
            EntityContext = entityContext;
            _generatorServices = generatorServices;
            _entityContexts = generatorServices.EntityContexts;
        }


        //methods
        public virtual void Execute()
        {
            IEntityDescription description = EntityContext.Description;
            IGenerator generator = _generatorServices.Defaults.GetGenerator(description);
            List<IModifier> modifiers = _generatorServices.Defaults.GetModifiers(description);
            IRequestCapacityProvider requestCapacityProvider = _generatorServices.Defaults.GetRequestCapacityProvider(description);

            var context = new GeneratorContext
            {
                Description = description,
                EntityContexts = _entityContexts,
                TargetCount = EntityContext.EntityProgress.TargetCount,
                CurrentCount = EntityContext.EntityProgress.CurrentCount,
                RequiredEntities = GetRequiredEntities(),
                Defaults = _generatorServices.Defaults,
            };


            IList instances = generator.Generate(context);
            _generatorServices.Validators.ValidateGenerated(instances, description.Type, generator);

            foreach (IModifier modifier in modifiers)
            {
                instances = modifier.Modify(context, instances);
                _generatorServices.Validators.ValidateModified(instances, description.Type, modifier);
            }

            requestCapacityProvider.TrackEntityGeneration(EntityContext, instances);
            _generatorServices.TemporaryStorage.InsertToTemporary(EntityContext, instances);
            _generatorServices.Supervisor.HandleGenerateCompleted(EntityContext, instances);
        }

        protected virtual Dictionary<Type, object> GetRequiredEntities()
        {
            var result = new Dictionary<Type, object>();

            foreach (RequiredEntity requiredEntity in EntityContext.Description.Required)
            {
                Type parent = requiredEntity.Type;
                EntityContext parentEntityContext = _entityContexts[parent];

                ISpreadStrategy spreadStrategy = _generatorServices.Defaults.GetSpreadStrategy(EntityContext.Description, requiredEntity);
                long parentEntityIndex = spreadStrategy.GetParentIndex(parentEntityContext, EntityContext);

                object parentEntity = _generatorServices.TemporaryStorage.Select(parentEntityContext, parentEntityIndex);
                result.Add(parent, parentEntity);
            }

            return result;
        }


        //Logging methods
        public virtual string GetLogEntry()
        {
            return $"Generate {EntityContext.Type.FullName} CurrentCount={EntityContext.EntityProgress.CurrentCount}";
        }
    }
}
