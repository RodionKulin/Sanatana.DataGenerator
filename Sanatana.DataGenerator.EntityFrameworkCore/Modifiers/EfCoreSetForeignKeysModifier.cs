using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.EntityFrameworkCore.Modifiers
{
    /// <summary>
    /// Modifier that will take foreign keys from Required entity instances and set to entity instance generated.
    /// </summary>
    public class EfCoreSetForeignKeysModifier : IModifier
    {
        protected EfCoreModelService _modelService;


        //init
        public EfCoreSetForeignKeysModifier(Func<DbContext> dbContextFactory)
        {
            dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _modelService = new EfCoreModelService(dbContextFactory);
        }


        //methods
        public IList Modify(GeneratorContext context, IList instances)
        {
            Type[] foreignKeyParentEntities = _modelService.GetParentEntities(context.Description.Type);
        
            for (int i = 0; i < foreignKeyParentEntities.Length; i++)
            {
                object parentInstance = context.RequiredEntities[foreignKeyParentEntities[i]];
                foreach (object childInstance in instances)
                {
                    _modelService.SetForeignKeysOnChild(childInstance, parentInstance);
                }
            }

            return instances;
        }

    }
}
