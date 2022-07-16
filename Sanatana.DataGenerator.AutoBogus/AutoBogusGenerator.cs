using AutoBogus;
using Bogus;
using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Linq;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.AutoBogus
{
    public class AutoBogusGenerator : IGenerator
    {
        //fields
        protected IAutoFaker _autoFaker;
        protected Dictionary<Type, MethodInfo> _generateMethods = new Dictionary<Type, MethodInfo>();


        //init
        public AutoBogusGenerator()
        {
            _autoFaker = AutoFaker.Create();
        }

        public AutoBogusGenerator(IAutoFaker autoFaker)
        {
            if (autoFaker == null)
            {
                throw new ArgumentNullException(nameof(autoFaker));
            }

            _autoFaker = autoFaker;
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            MethodInfo generateMethod = GetGenerateMethod(context.Description.Type);

            IList list = (IList)generateMethod.Invoke(_autoFaker, new object[] { 1, null });
            return list;
        }

        protected virtual MethodInfo GetGenerateMethod(Type entityType)
        {
            if (!_generateMethods.ContainsKey(entityType))
            {
                Type autoFakerType = typeof(IAutoFaker);
                MethodInfo generateMethod = autoFakerType.GetMethods().Where(
                    x => x.Name == nameof(IAutoFaker.Generate)
                    && x.GetGenericArguments().Length == 1
                    && x.GetParameters().First().ParameterType == typeof(int))
                    .First();

                generateMethod = generateMethod.MakeGenericMethod(entityType);
                _generateMethods.Add(entityType, generateMethod);
            }

            return _generateMethods[entityType];
        }


        //validation
        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
        }
    }
}
