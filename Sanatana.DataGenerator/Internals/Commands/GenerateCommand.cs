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
            };

            IList instances = generator.Generate(context);
            CheckGeneratedCount(instances, description.Type, generator);

            foreach (IModifier modifier in modifiers)
            {
                instances = modifier.Modify(context, instances);
                CheckModifiedCount(instances, description.Type, modifier);
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

        /// <summary>
        /// Validate that number of generated entities is greater than 0 and returned type is List<>
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="entityType"></param>
        /// <param name="generator"></param>
        protected virtual void CheckGeneratedCount(IList entities, Type entityType, IGenerator generator)
        {
            string generatorType = generator.GetType().FullName;
            string resultCountMsg = "Number of entities returned must be greater than 0. " +
                $"{nameof(IGenerator)} {generatorType} for entity {entityType} returned 0 entities.";

            if (entities == null)
            {
                throw new NotSupportedException(resultCountMsg);
            }

            Type entitiesListType = entities.GetType();
            if (entitiesListType.IsAssignableFrom(typeof(List<>)))
            {
                string resultTypeMessage = $"List returned from {nameof(IGenerator)} must be a generic List<>. {nameof(IGenerator)} {generatorType} returned list of type {entitiesListType}.";

                throw new NotSupportedException(resultTypeMessage);
            }

            if (entities.Count == 0)
            {
                throw new NotSupportedException(resultCountMsg);
            }
        }

        /// <summary>
        /// Validate that number of modified entities is greater than 0 and returned type is List&lt;&gt;
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="entityType"></param>
        /// <param name="modifier"></param>
        protected virtual void CheckModifiedCount(IList entities, Type entityType, IModifier modifier)
        {
            string modifierType = modifier.GetType().FullName;
            string resultCountMsg = "Number of entities returned must be greater than 0. " +
                $"Modifier {modifierType} for entity {entityType} returned 0 entities.";

            if (entities == null)
            {
                throw new NotSupportedException(resultCountMsg);
            }

            Type entitiesListType = entities.GetType();
            if (entitiesListType.IsAssignableFrom(typeof(List<>)))
            {
                string resultTypeMessage = $"List returned from {nameof(IModifier)} must be a generic List<>. Modifier {modifierType} returned list of type {entitiesListType}.";
                throw new NotSupportedException(resultTypeMessage);
            }

            if (entities == null || entities.Count == 0)
            {
                throw new NotSupportedException(resultCountMsg);
            }
        }



        //Logging methods
        public virtual string GetLogEntry()
        {
            return $"Generate {EntityContext.Type.FullName} CurrentCount={EntityContext.EntityProgress.CurrentCount}";
        }
    }
}
