using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Internals.Validators.Contracts;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Extensions;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Modifiers;
using System.Reflection;
using System.Collections.Immutable;

namespace Sanatana.DataGenerator.EntityFrameworkCore.Validators
{
    /// <summary>
    /// Validate that all primary keys manually incremented has supported type of long or int.
    /// </summary>
    public class EfCoreSetPrimaryKeysModifierValidator : IAfterSetupValidator
    {
        protected EfCoreModelService _modelService;
        protected HashSet<Type> _excludeValidationTypes;


        //init
        public EfCoreSetPrimaryKeysModifierValidator(Func<DbContext> dbContextFactory)
        {
            dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _modelService = new EfCoreModelService(dbContextFactory);
            _excludeValidationTypes = new HashSet<Type>();
        }



        //configure methods
        public virtual EfCoreSetPrimaryKeysModifierValidator ExcludeEntity<TEntity>()
        {
            _excludeValidationTypes.Add(typeof(TEntity));
            return this;
        }

        public virtual EfCoreSetPrimaryKeysModifierValidator ExcludeEntity(params Type[] typesToExclude)
        {
            typesToExclude = typesToExclude ?? throw new ArgumentNullException(nameof(typesToExclude));
            foreach (Type typeToExclude in typesToExclude)
            {
                _excludeValidationTypes.Add(typeToExclude);
            }
            return this;
        }

        public virtual EfCoreSetPrimaryKeysModifierValidator RemoveEntityExclusions()
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
                    && x.Modifiers.Any(m => m.GetType().IsSameTypeOrSubclass(typeof(EfCoreSetPrimaryKeysModifier))))
                .ToArray();
            foreach (IEntityDescription description in entitiesWithModifier)
            {
                ValidateEntity(description, allEntities);
            }

            bool hasDefaultForeignKeyModifier = generatorServices.Defaults.Modifiers != null
                && generatorServices.Defaults.Modifiers.Any(m => m.GetType().IsSameTypeOrSubclass(typeof(EfCoreSetPrimaryKeysModifier)));
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
            PropertyInfo[] primaryKeys = _modelService.GetPrimaryKeysManuallyGenerated(description.Type);
            ImmutableArray<Type> supportedPropTypes = EfCoreSetPrimaryKeysModifier.SupportedPropTypes;

            for (int i = 0; i < primaryKeys.Length; i++)
            {
                bool isSupportedType = supportedPropTypes.Contains(primaryKeys[i].PropertyType);
                if (!isSupportedType)
                {
                    string fullName = description.Type.FullName;
                    string supportedTypes = string.Join(", ", supportedPropTypes.Select(x => x.Name));
                    throw new NotSupportedException($"Entity {fullName} has Primary key [{primaryKeys[i].Name}] of type {primaryKeys[i].PropertyType}. " +
                        $"Only {supportedTypes} types are supported to auto increment in {nameof(EfCoreSetForeignKeysModifier)}.");
                }
            }

        }
    }
}
