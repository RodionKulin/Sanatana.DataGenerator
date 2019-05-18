using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.DataGenerator.Modifiers
{
    public class DelegateModifier<TEntity> : IModifier
        where TEntity : class
    {
        //fields
        protected bool _isMultiDelegate;
        protected object _modifyFunc;


        //init
        protected DelegateModifier(object modifyFunc, bool isMultiDelegate)
        {
            _modifyFunc = modifyFunc;
            _isMultiDelegate = isMultiDelegate;
        }

        public static class Factory
        {
            public static DelegateModifier<TEntity> Create(
                Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, false);
            }

            public static DelegateModifier<TEntity> CreateMulti(
                 Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, true);
            }
        }


        //methods
        public virtual IList Modify(GeneratorContext context, IList entities)
        {
            if (_isMultiDelegate)
            {
                return InvokeMultiResult(context, entities);
            }
            else
            {
                return InvokeSingleResult(context, entities);
            }
        }

        protected virtual IList InvokeMultiResult(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Func<GeneratorContext, List<TEntity>, List<TEntity>>;
            var entitiesList = (List<TEntity>)entities;
            List <TEntity> res = func.Invoke(context, entitiesList);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return res;
        }

        protected virtual List<TEntity> InvokeSingleResult(
            GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Func<GeneratorContext, List<TEntity>, TEntity>;
            var entitiesList = (List<TEntity>)entities;
            TEntity res = func.Invoke(context, entitiesList);

            if (res == null)
            {
                return new List<TEntity>();
            }

            return new List<TEntity>() { res };
        }

    }
}
