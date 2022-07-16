using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Subset;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.SubsetGeneration
{
    public static class IEntityDescriptionExtensions
    {
        public static IEnumerable<IEntityDescription> SelectEntities(this IEnumerable<IEntityDescription> allEntityDescriptions,
            EntitiesSelection selection, SubsetSettings subsetSettings)
        {
            IEnumerable<IEntityDescription> entitiesSelected = allEntityDescriptions;
            if (selection == EntitiesSelection.Target)
            {
                entitiesSelected = entitiesSelected.Where(x => subsetSettings.TargetEntities.Contains(x.Type));
            }
            else if (selection == EntitiesSelection.Required)
            {
                entitiesSelected = entitiesSelected.Where(x => subsetSettings.RequiredEntities.Contains(x.Type));
            }

            return entitiesSelected;
        }
    }
}
