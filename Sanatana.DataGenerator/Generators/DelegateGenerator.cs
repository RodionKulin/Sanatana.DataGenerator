using System;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Generators
{
    public class DelegateGenerator : IGenerator
    {
        //properties
        public object GenerateFunc { get; set; }


        //init
        public DelegateGenerator(object generateFunc)
        {
            if(generateFunc == null)
            {
                throw new ArgumentNullException(nameof(generateFunc));
            }

            GenerateFunc = generateFunc;
        }

        public static DelegateGenerator CreateMulti<TEntity>(
            Func<GeneratorContext, List<TEntity>> generateFunc)
            where TEntity : class
        {
            return new DelegateGenerator(generateFunc);
        }

        public static DelegateGenerator Create<TEntity>(
            Func<GeneratorContext, TEntity> generateFunc)
            where TEntity : class
        {
            return new DelegateGenerator(generateFunc);
        }


        //methods
        public virtual List<TEntity> Generate<TEntity>(GeneratorContext context)
            where TEntity : class
        {
            if(GenerateFunc is Func<GeneratorContext, List<TEntity>>)
            {
                return InvokeMultiResult<TEntity>(context);
            }
            else if (GenerateFunc is Func<GeneratorContext, TEntity>)
            {
                return InvokeSingleResult<TEntity>(context);
            }
            else
            {
                throw new NotImplementedException(
                    $"Unexpected {nameof(GenerateFunc)} of type {GenerateFunc.GetType()}");
            }
        }

        protected virtual List<TEntity> InvokeMultiResult<TEntity>(GeneratorContext context)
            where TEntity : class
        {
            var generateFunc = GenerateFunc as Func<GeneratorContext, List<TEntity>>;
            List<TEntity> res = generateFunc.Invoke(context);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return res;
        }

        protected virtual List<TEntity> InvokeSingleResult<TEntity>(GeneratorContext context)
            where TEntity : class
        {
            var generateFunc = GenerateFunc as Func<GeneratorContext, TEntity>;
            TEntity res = generateFunc.Invoke(context);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return new List<TEntity>() { res };
        }
    }
}
