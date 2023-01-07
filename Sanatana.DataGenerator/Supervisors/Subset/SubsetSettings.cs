using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Supervisors.Subset
{
    public class SubsetSettings
    {
        //properties
        public List<Type> TargetEntities { get; protected set; }
        public List<Type> RequiredEntities { get; protected set; }
        public List<Type> TargetAndRequiredEntities { get; protected set; }



        //init
        public SubsetSettings(IEnumerable<Type> targetEntitiesSubset)
        {
            targetEntitiesSubset = targetEntitiesSubset ?? throw new ArgumentNullException(nameof(targetEntitiesSubset));
            TargetEntities = targetEntitiesSubset.ToList();
            if(targetEntitiesSubset.Any(x => x == null))
            {
                throw new ArgumentException("Provided entityType can not be null", nameof(targetEntitiesSubset));
            }
            if (TargetEntities.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetEntitiesSubset), "Target entities subset can not be empty");
            }
        }


        //methods
        public virtual void Setup(GeneratorServices generatorServices)
        {
            generatorServices.SetupEntityContexts();

            TargetAndRequiredEntities = GetTargetAndRequiredEntities(generatorServices);
            RequiredEntities = TargetAndRequiredEntities.Except(TargetEntities).ToList();
        }

        protected virtual List<Type> GetTargetAndRequiredEntities(GeneratorServices services)
        {
            //validate
            services.ValidateEntitiesConfigured(TargetEntities);
            services.ValidateNoEntityDuplicates(TargetEntities);

            //build target + required entities list recursive
            var entitiesSubset = new HashSet<Type>();
            foreach (Type targetType in TargetEntities)
            {
                FindRequiredEntitiesRecursive(entitiesSubset, targetType, services);
            }

            return entitiesSubset.ToList();
        }

        protected virtual void FindRequiredEntitiesRecursive(HashSet<Type> fullEntitiesSubset, Type nextType,
            GeneratorServices services)
        {
            fullEntitiesSubset.Add(nextType);

            EntityContext nextEntityContext = services.EntityContexts[nextType];
            if (nextEntityContext.Description.Required == null)
            {
                return;
            }

            foreach (RequiredEntity required in nextEntityContext.Description.Required)
            {
                FindRequiredEntitiesRecursive(fullEntitiesSubset, required.Type, services);
            }
        }
    }
}
