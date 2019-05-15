using System;
using System.Collections.Generic;
using System.Text;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.DataGenerator.Modifiers
{
    public class DelegateModifier : IModifier
    {
        //properties
        public object ModifyFunc { get; set; }


        //init
        public DelegateModifier(object modifyFunc)
        {
            ModifyFunc = modifyFunc;
        }

        public static DelegateModifier CreateMulti<TEntity>(
            Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
            where TEntity : class
        {
            return new DelegateModifier(modifyFunc);
        }

        public static DelegateModifier Create<TEntity>(
            Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
            where TEntity : class
        {
            return new DelegateModifier(modifyFunc);
        }


        //methods
        public virtual List<TEntity> Modify<TEntity>(
            GeneratorContext context, List<TEntity> entities)
            where TEntity : class
        {
            if (ModifyFunc is Func<GeneratorContext, List<TEntity>, List<TEntity>>)
            {
                return InvokeMultiResult(context, entities);
            }
            else if (ModifyFunc is Func<GeneratorContext, List<TEntity>, TEntity>)
            {
                return InvokeSingleResult(context, entities);
            }
            else
            {
                throw new NotImplementedException(
                    $"Unexpected {nameof(ModifyFunc)} of type {ModifyFunc.GetType()}");
            }
        }

        protected virtual List<TEntity> InvokeMultiResult<TEntity>(
            GeneratorContext context, List<TEntity> entities)
            where TEntity : class
        {
            var func = ModifyFunc as Func<GeneratorContext, List<TEntity>, List<TEntity>>;
            List<TEntity> res = func.Invoke(context, entities);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return res;
        }

        protected virtual List<TEntity> InvokeSingleResult<TEntity>(
            GeneratorContext context, List<TEntity> entities)
            where TEntity : class
        {
            var func = ModifyFunc as Func<GeneratorContext, List<TEntity>, TEntity>;
            TEntity res = func.Invoke(context, entities);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return new List<TEntity>() { res };
        }

    }
}
