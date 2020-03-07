using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sanatana.DataGenerator.SpreadStrategies;

namespace Sanatana.DataGenerator.Commands
{
    /// <summary>
    /// Call entity's generator and increment counters
    /// </summary>
    public class GenerateEntitiesCommand : ICommand
    {
        //fields
        protected GeneratorSetup _setup;
        protected Dictionary<Type, EntityContext> _entityContexts;


        //properties
        internal EntityContext EntityContext { get; set; }


        //init
        public GenerateEntitiesCommand(EntityContext entityContext, GeneratorSetup setup,
            Dictionary<Type, EntityContext> entityContexts)
        {
            EntityContext = entityContext;
            _setup = setup;
            _entityContexts = entityContexts;
        }


        //methods
        public virtual bool Execute()
        {
            IEntityDescription entityDescription = EntityContext.Description;
            IGenerator generator = _setup.GetGenerator(entityDescription);
            List<IModifier> modifiers = _setup.GetModifiers(entityDescription);

            var context = new GeneratorContext
            {
                Description = entityDescription,
                EntityContexts = _entityContexts,
                TargetCount = EntityContext.EntityProgress.TargetCount,
                CurrentCount = EntityContext.EntityProgress.CurrentCount,
                RequiredEntities = GetRequiredEntities(),
            };

            IList entities = generator.Generate(context);
            _setup.Validator.CheckGeneratedCount(entities, entityDescription.Type, generator);

            foreach (IModifier modifier in modifiers)
            {
                entities = modifier.Modify(context, entities);
                _setup.Validator.CheckModifiedCount(entities, entityDescription.Type, modifier);
            }

            _setup.TemporaryStorage.InsertToTemporary(EntityContext, entities);
            _setup.Supervisor.HandleGenerateCompleted(EntityContext, entities);

            return true;
        }

        protected virtual Dictionary<Type, object> GetRequiredEntities()
        {
            var result = new Dictionary<Type, object>();

            foreach (RequiredEntity requiredEntity in EntityContext.Description.Required)
            {
                Type parent = requiredEntity.Type;
                EntityContext parentEntityContext = _entityContexts[parent];

                ISpreadStrategy spreadStrategy = _setup.GetSpreadStrategy(EntityContext.Description, requiredEntity);
                long parentEntityIndex = spreadStrategy.GetParentIndex(parentEntityContext, EntityContext);

                object parentEntity = _setup.TemporaryStorage.Select(parentEntityContext, parentEntityIndex);
                result.Add(parent, parentEntity);
            }

            return result;
        }

    }
}
