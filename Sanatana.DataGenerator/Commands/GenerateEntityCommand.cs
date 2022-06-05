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

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Call entity's generator and increment counters
    /// </summary>
    public class GenerateEntityCommand : ICommand
    {
        //fields
        protected GeneratorSetup _setup;
        protected Dictionary<Type, EntityContext> _entityContexts;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public GenerateEntityCommand(EntityContext entityContext, GeneratorSetup setup,
            Dictionary<Type, EntityContext> entityContexts)
        {
            EntityContext = entityContext;
            _setup = setup;
            _entityContexts = entityContexts;
        }


        //methods
        public virtual void Execute()
        {
            IEntityDescription description = EntityContext.Description;
            IGenerator generator = _setup.Defaults.GetGenerator(description);
            List<IModifier> modifiers = _setup.Defaults.GetModifiers(description);
            IRequestCapacityProvider requestCapacityProvider = _setup.Defaults.GetRequestCapacityProvider(description);

            var context = new GeneratorContext
            {
                Description = description,
                EntityContexts = _entityContexts,
                TargetCount = EntityContext.EntityProgress.TargetCount,
                CurrentCount = EntityContext.EntityProgress.CurrentCount,
                RequiredEntities = GetRequiredEntities(),
            };

            IList entities = generator.Generate(context);
            _setup.Validator.CheckGeneratedCount(entities, description.Type, generator);

            foreach (IModifier modifier in modifiers)
            {
                entities = modifier.Modify(context, entities);
                _setup.Validator.CheckModifiedCount(entities, description.Type, modifier);
            }

            requestCapacityProvider.TrackEntityGeneration(EntityContext, entities);
            _setup.TemporaryStorage.InsertToTemporary(EntityContext, entities);
            _setup.Supervisor.HandleGenerateCompleted(EntityContext, entities);
        }

        protected virtual Dictionary<Type, object> GetRequiredEntities()
        {
            var result = new Dictionary<Type, object>();

            foreach (RequiredEntity requiredEntity in EntityContext.Description.Required)
            {
                Type parent = requiredEntity.Type;
                EntityContext parentEntityContext = _entityContexts[parent];

                ISpreadStrategy spreadStrategy = _setup.Defaults.GetSpreadStrategy(EntityContext.Description, requiredEntity);
                long parentEntityIndex = spreadStrategy.GetParentIndex(parentEntityContext, EntityContext);

                object parentEntity = _setup.TemporaryStorage.Select(parentEntityContext, parentEntityIndex);
                result.Add(parent, parentEntity);
            }

            return result;
        }

        public virtual string GetDescription()
        {
            return $"Generate {EntityContext.Type.Name} CurrentCount={EntityContext.EntityProgress.CurrentCount}";
        }
    }
}
