using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Entities;
using System.Linq;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Generators
{
    public class DelegateGenerator<TEntity> : IGenerator
        where TEntity : class
    {
        //fields
        protected bool _isMultiDelegate;
        protected object _generateFunc;


        //init
        protected DelegateGenerator(object generateFunc, bool isMultiDelegate)
        {
            if(generateFunc == null)
            {
                throw new ArgumentNullException(nameof(generateFunc));
            }

            _generateFunc = generateFunc;
            _isMultiDelegate = isMultiDelegate;
        }

        public static class Factory
        {
            public static DelegateGenerator<TEntity> Create(
                Func<GeneratorContext, TEntity> generateFunc)
            {
                return new DelegateGenerator<TEntity>(generateFunc, false);
            }

            public static DelegateGenerator<TEntity> CreateMulti(
                Func<GeneratorContext, List<TEntity>> generateFunc)
            {
                return new DelegateGenerator<TEntity>(generateFunc, true);
            }
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            if(_generateFunc is Func<GeneratorContext, List<TEntity>>)
            {
                return InvokeMultiResult(context);
            }
            else if (_generateFunc is Func<GeneratorContext, TEntity>)
            {
                return InvokeSingleResult(context);
            }
            else
            {
                throw new NotImplementedException(
                    $"Unexpected {nameof(_generateFunc)} of type {_generateFunc.GetType()}");
            }
        }

        protected virtual List<TEntity> InvokeMultiResult(GeneratorContext context)
        {
            var generateFunc = _generateFunc as Func<GeneratorContext, List<TEntity>>;
            List<TEntity> entities = generateFunc.Invoke(context);

            if (entities == null)
            {
                return new List<TEntity>();
            }

            return entities;
        }

        protected virtual List<TEntity> InvokeSingleResult(GeneratorContext context)
        {
            var generateFunc = _generateFunc as Func<GeneratorContext, TEntity>;
            TEntity entity = generateFunc.Invoke(context);

            if (entity == null)
            {
                return new List<TEntity>();
            }

            return new List<TEntity>() { entity };
        }


        //validation
        public virtual void ValidateEntitySettings(IEntityDescription entity, DefaultSettings defaults)
        {
        }
    }
}
