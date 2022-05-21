using AutoBogus;
using Bogus;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.AutoBogus
{
    public class AutoBogusGenerator<TEntity> : IGenerator
        where TEntity : class
    {
        //fields
        protected Faker<TEntity> _faker;


        //init
        public AutoBogusGenerator(Faker<TEntity> faker)
        {
            if (faker == null)
            {
                throw new ArgumentNullException(nameof(faker));
            }

            _faker = faker;
        }

        public AutoBogusGenerator(AutoFaker<TEntity> faker)
        {
            if (faker == null)
            {
                throw new ArgumentNullException(nameof(faker));
            }

            _faker = faker;
        }


        //methods
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
        public virtual void ValidateEntitySettings(IEntityDescription entity)
        {
        }
    }
}
