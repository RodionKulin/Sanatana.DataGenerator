using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Modifiers;
using Sanatana.DataGenerator.Internals.Validators.AfterSetup;

namespace Sanatana.DataGenerator.EntityFrameworkCore.Validators
{
    /// <summary>
    /// Validate that all foreign key parent entities are present in Required entities list.
    /// </summary>
    public class EfCoreSetForeignKeysModifierValidator : IAfterSetupValidator
    {
        protected EfCoreModelService _modelService;
        protected HashSet<Type> _excludeValidationTypes;


        //init
        public EfCoreSetForeignKeysModifierValidator(Func<DbContext> dbContextFactory)
        {
            dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _modelService = new EfCoreModelService(dbContextFactory);
            _excludeValidationTypes = new HashSet<Type>();
        }



        //configure methods
        public virtual EfCoreSetForeignKeysModifierValidator ExcludeEntity<TEntity>()
        {
            _excludeValidationTypes.Add(typeof(TEntity));
            return this;
        }

        public virtual EfCoreSetForeignKeysModifierValidator ExcludeEntity(params Type[] typesToExclude)
        {
            typesToExclude = typesToExclude ?? throw new ArgumentNullException(nameof(typesToExclude));
            foreach (Type typeToExclude in typesToExclude)
            {
                _excludeValidationTypes.Add(typeToExclude);
            }
            return this;
        }

        public virtual EfCoreSetForeignKeysModifierValidator RemoveEntityExclusions()
        {
            _excludeValidationTypes.Clear();
            return this;
        }


        //validate methods
        public virtual void ValidateSetup(GeneratorServices generatorServices)
        {
            Dictionary<Type, IEntityDescription> entityDescriptions = generatorServices.EntityDescriptions;
            IEnumerable<EntityContext> allEntities = generatorServices.EntityContexts.Select(x => x.Value);

            IEntityDescription[] entitiesWithModifier = entityDescriptions.Values
                .Where(x => !_excludeValidationTypes.Contains(x.Type))
                .Where(x => x.Modifiers != null 
                    && x.Modifiers.Any(m => m.GetType().IsSameTypeOrSubclass(typeof(EfCoreSetForeignKeysModifier))))
                .ToArray();
            foreach (IEntityDescription description in entitiesWithModifier)
            {
                ValidateEntity(description, allEntities);
            }

            bool hasDefaultForeignKeyModifier = generatorServices.Defaults.Modifiers != null
                && generatorServices.Defaults.Modifiers.Any(m => m.GetType().IsSameTypeOrSubclass(typeof(EfCoreSetForeignKeysModifier)));
            if (hasDefaultForeignKeyModifier)
            {
                IEntityDescription[] entitiesWithoutModifier = entityDescriptions.Values
                    .Where(x => !_excludeValidationTypes.Contains(x.Type))
                    .Where(x => x.Modifiers == null || x.Modifiers.Count == 0)
                    .ToArray();
                foreach (IEntityDescription description in entitiesWithoutModifier)
                {
                    ValidateEntity(description, allEntities);
                }
            }
        }

        protected virtual void ValidateEntity(IEntityDescription description, IEnumerable<EntityContext> allEntityContexts)
        {
            Type[] foreignKeyParentEntities = _modelService.GetParentEntities(description.Type);
            
            Type[] requiredEntities = description.Required.Select(x => x.Type).ToArray();
            Type[] allConfiguredEntities = allEntityContexts.Select(x => x.Type).ToArray();

            Type[] entitiesRequiredMissing = foreignKeyParentEntities.Except(requiredEntities).ToArray();
            Type[] entitiesConfiguredMissing = entitiesRequiredMissing.Except(allConfiguredEntities).ToArray();

            if (entitiesRequiredMissing.Length > 0)
            {
                string fullName = description.Type.FullName;
                string missingRequiredNames = string.Join(",", entitiesRequiredMissing.Select(x => x.FullName));
                string msgMissingRequired = $"{entitiesRequiredMissing.Length} foreign entities set in DbContext, that are not provided as Required: {missingRequiredNames}.";
                string missingConfiguredNames = string.Join(",", entitiesConfiguredMissing.Select(x => x.FullName));
                string msgMissingConfigured = entitiesConfiguredMissing.Length == 0
                    ? ""
                    : $" Among them {entitiesConfiguredMissing.Length} foreign entities set in DbContext, that are not configured in {nameof(GeneratorSetup)}: {missingConfiguredNames}.";

                throw new NotSupportedException($"Entity {fullName} has {msgMissingRequired}{msgMissingConfigured}");
            }
        }

    }
}
