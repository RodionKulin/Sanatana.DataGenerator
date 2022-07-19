using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCore.Internals;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.EntityFrameworkCore.Modifiers
{
    /// <summary>
    /// Modifier that will increment primary key value if 
    /// 1. Primary key is not configured to auto increment by database;
    /// 2. If primary key is of type long of int.
    /// </summary>
    public class EfCoreSetPrimaryKeysModifier : IModifier
    {
        //fields
        protected EfCoreModelService _modelService;
        protected Dictionary<Type, long> _primaryKeyStartFrom;


        //properties
        public static readonly ImmutableArray<Type> SupportedPropTypes = ImmutableArray.Create(new[]
        {
            typeof(int),
            typeof(long),
        });


        //init
        public EfCoreSetPrimaryKeysModifier(Func<DbContext> dbContextFactory, Dictionary<Type, long> primaryKeyStartFrom = null)
        {
            dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
            _modelService = new EfCoreModelService(dbContextFactory);
            _primaryKeyStartFrom = primaryKeyStartFrom ?? new Dictionary<Type, long>();
        }


        //configure methods
        public virtual EfCoreSetPrimaryKeysModifier SetPrimaryKeyStartFrom<TEntity>(long startFromId)
        {
            _primaryKeyStartFrom[typeof(TEntity)] = startFromId;
            return this;
        }


        //modify methods
        public IList Modify(GeneratorContext context, IList instances)
        {
            PropertyInfo[] primaryKeys = _modelService.GetPrimaryKeysManuallyGenerated(context.Description.Type);
            primaryKeys = primaryKeys.Where(x => SupportedPropTypes.Contains(x.PropertyType)).ToArray();

            for (int i = 0; i < primaryKeys.Length; i++)
            {
                foreach (object instance in instances)
                {
                    long instanceNumber = context.CurrentCount + i;
                    if (_primaryKeyStartFrom.ContainsKey(context.Description.Type))
                    {
                        instanceNumber += _primaryKeyStartFrom[context.Description.Type];
                    }

                    object propVaue = instanceNumber;
                    Type propType = primaryKeys[i].PropertyType;
                    if (propType == typeof(int))
                    {
                        propVaue = (int)instanceNumber;
                    }

                    primaryKeys[i].SetValue(instance, propVaue);
                }
            }

            return instances;
        }

        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
        }
    }
}
