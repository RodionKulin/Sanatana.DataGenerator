using AutoBogus;
using Bogus;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.AutoBogus
{
    /// <summary>
    /// Create new entity instance and populate properties. 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class AutoBogusGenerator<TEntity> : IGenerator
        where TEntity : class
    {
        //fields
        protected Faker<TEntity> _faker;


        //init
        /// <summary>
        /// Create new entity instance and populate properties. Need to provide settings how to populate instance properties.
        /// </summary>
        /// <param name="faker"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AutoBogusGenerator(Faker<TEntity> faker)
        {
            if (faker == null)
            {
                throw new ArgumentNullException(nameof(faker));
            }

            _faker = faker;
        }

        /// <summary>
        /// Create new entity instance and populate properties. By default will auto populate value properties.
        /// </summary>
        /// <param name="faker"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AutoBogusGenerator(AutoFaker<TEntity> faker)
        {
            if (faker == null)
            {
                throw new ArgumentNullException(nameof(faker));
            }

            _faker = faker;
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
        }


        //generation methods
        public virtual IList Generate(GeneratorContext context)
        {
            int seed = GetNextSeed(context);
            return _faker.UseSeed(seed).Generate(1);
        }

        protected virtual int GetNextSeed(GeneratorContext context)
        {
            long seed = context.CurrentCount % int.MaxValue;
            return (int)seed;
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
