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
    /// <summary>
    /// Create new entity instance and populate properties. 
    /// </summary>
    public class AutoBogusGenerator : IGenerator
    {
        //fields
        protected IAutoFaker _autoFaker;
        protected Dictionary<Type, MethodInfo> _generateMethods = new Dictionary<Type, MethodInfo>();
        private int _generationBatchSize = 1;

        //properties
        /// <summary>
        /// Number of items generated together in a single batch.
        /// </summary>
        public int GenerationBatchSize
        {
            get { return _generationBatchSize; }
            set
            {
                if (value < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(GenerationBatchSize));
                }
                _generationBatchSize = value;
            }
        }


        //init
        /// <summary>
        /// Create new entity instance and populate properties. By default will auto populate value properties.
        /// </summary>
        public AutoBogusGenerator()
        {
            _autoFaker = AutoFaker.Create();
        }

        /// <summary>
        /// Create new entity instance and populate properties. By default will auto populate value properties.
        /// </summary>
        /// <param name="autoFaker"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AutoBogusGenerator(IAutoFaker autoFaker)
        {
            if (autoFaker == null)
            {
                throw new ArgumentNullException(nameof(autoFaker));
            }

            _autoFaker = autoFaker;
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
            //_generateMethods var does not need to be reseted.
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            MethodInfo generateMethod = GetGenerateMethod(context.Description.Type);

            long itemsLeft = context.TargetCount - context.CurrentCount;
            long generateCount = Math.Min(GenerationBatchSize, itemsLeft);
            generateCount = generateCount < 1 ? 1 : generateCount;

            return (IList)generateMethod.Invoke(_autoFaker, new object[] { (int)generateCount, null });
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
