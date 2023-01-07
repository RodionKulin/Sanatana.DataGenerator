using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore;
using System;
using Sanatana.DataGenerator.EntityFrameworkCore.Extensions;

namespace Sanatana.DataGenerator
{
    public static class GeneratorSetupExtensions
    {
        /// <summary>
        /// Configure storage and other settings using EntityFramework model.
        /// </summary>
        /// <param name="generatorSetup"></param>
        /// <param name="dbContextFactory"></param>
        /// <param name="efSetup"></param>
        /// <returns></returns>
        public static GeneratorSetup SetupWithEntityFrameworkCorePostgreSql(this GeneratorSetup generatorSetup, 
            Func<DbContext> dbContextFactory, Func<EntityFrameworkSetupPostgreSql, EntityFrameworkSetupPostgreSql> efSetup)
        {
            var entityFrameworkSetup = new EntityFrameworkSetupPostgreSql(generatorSetup, dbContextFactory);
            entityFrameworkSetup = efSetup(entityFrameworkSetup);
            return entityFrameworkSetup.GeneratorSetup;
        }

    }

}
