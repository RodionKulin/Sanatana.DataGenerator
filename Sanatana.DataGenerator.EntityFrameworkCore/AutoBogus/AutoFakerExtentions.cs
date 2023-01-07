using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AutoBogus;

namespace AutoBogus
{
    public static class AutoFakerExtentions
    {
        /// <summary>
        /// Skip generation of Entity Framework navigation properties.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dbContextFactory"></param>
        public static void WithNavigationPropertiesSkip(this IAutoFakerDefaultConfigBuilder builder, Func<DbContext> dbContextFactory)
        {
            var modelService = new EfCoreModelService(dbContextFactory);

            Type[] entities = modelService.GetConfiguredEntities();
            foreach (Type entityType in entities)
            {
                PropertyInfo[] navigationProps = modelService.GetNavigationProperties(entityType);

                foreach (PropertyInfo navigationProp in navigationProps)
                {
                    builder.WithSkip(entityType, navigationProp.Name);
                }
            }
        }

    }
}
