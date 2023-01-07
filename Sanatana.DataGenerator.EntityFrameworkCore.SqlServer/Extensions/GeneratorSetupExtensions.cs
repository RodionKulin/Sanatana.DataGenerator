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
        public static GeneratorSetup SetupWithEntityFrameworkCoreSqlServer(this GeneratorSetup generatorSetup, 
            Func<DbContext> dbContextFactory, Func<EntityFrameworkSetupSqlServer, EntityFrameworkSetupSqlServer> efSetup)
        {
            var entityFrameworkSetup = new EntityFrameworkSetupSqlServer(generatorSetup, dbContextFactory);
            entityFrameworkSetup = efSetup(entityFrameworkSetup);
            return entityFrameworkSetup.GeneratorSetup;
        }

    }

}
