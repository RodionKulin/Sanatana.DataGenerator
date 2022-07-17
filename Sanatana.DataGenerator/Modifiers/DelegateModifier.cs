using System;
using System.Collections.Generic;
using System.Collections;

namespace Sanatana.DataGenerator.Modifiers
{
    public class DelegateModifier<TEntity> : IModifier
        where TEntity : class
    {
        //internal types
        protected enum Output { Void, Single, Multi }

        //fields
        protected object _modifyFunc;
        protected bool _isMultiInput;
        protected Output _output;


        //init
        protected DelegateModifier(object modifyFunc, bool isMultiInput, Output output)
        {
            _modifyFunc = modifyFunc;
            _isMultiInput = isMultiInput;
            _output = output;
        }

        public static class Factory
        {
            public static DelegateModifier<TEntity> Create(
                Action<GeneratorContext, TEntity> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateModifier<TEntity> Create(
                Action<GeneratorContext, List<TEntity>> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateModifier<TEntity> Create(
                Func<GeneratorContext, TEntity, TEntity> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateModifier<TEntity> Create(
                Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateModifier<TEntity> Create(
                Func<GeneratorContext, TEntity, List<TEntity>> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateModifier<TEntity> Create(
                Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
            {
                return new DelegateModifier<TEntity>(modifyFunc, true, Output.Multi);
            }
        }


        //methods
        public virtual IList Modify(GeneratorContext context, IList entities)
        {
            if(_output == Output.Void)
            {
                return _isMultiInput
                   ? InvokeMultiInVoidOut(context, entities)
                   : InvokeSingleInVoidOut(context, entities);
            }
            if (_output == Output.Single)
            {
                return _isMultiInput
                   ? InvokeMultiInSingleOut(context, entities)
                   : InvokeSingleInSingleOut(context, entities);
            }
            if (_output == Output.Multi)
            {
                return _isMultiInput
                   ? InvokeMultiInMultiOut(context, entities)
                   : InvokeSingleInMultiOut(context, entities);
            }

            throw new NotImplementedException($"Unexpected value {_output} received for type {nameof(Output)}");
        }

        protected virtual List<TEntity> InvokeSingleInVoidOut(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Action<GeneratorContext, TEntity>;

            var inputList = (List<TEntity>)entities;

            for (int i = 0; i < inputList.Count; i++)
            {
                TEntity entity = inputList[i];
                GeneratorContext contextClone = context.Clone();
                contextClone.CurrentCount += i;

                func.Invoke(contextClone, entity);
            }

            return inputList;
        }

        protected virtual List<TEntity> InvokeSingleInSingleOut(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Func<GeneratorContext, TEntity, TEntity>;

            var inputList = (List<TEntity>)entities;
            var outputList = new List<TEntity>();

            for (int i = 0; i < inputList.Count; i++)
            {
                TEntity entity = inputList[i];
                GeneratorContext contextClone = context.Clone();
                contextClone.CurrentCount += i;

                TEntity response = func.Invoke(contextClone, entity);
                if (response != null)
                {
                    outputList.Add(response);
                }
            }

            return outputList;
        }

        protected virtual List<TEntity> InvokeSingleInMultiOut(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Func<GeneratorContext, TEntity, List<TEntity>>;

            var inputList = (List<TEntity>)entities;
            var outputList = new List<TEntity>();

            for (int i = 0; i < inputList.Count; i++)
            {
                TEntity entity = inputList[i];
                GeneratorContext contextClone = context.Clone();
                contextClone.CurrentCount += i;

                List<TEntity> response = func.Invoke(contextClone, entity);
                if (response != null)
                {
                    outputList.AddRange(response);
                }
            }

            return outputList;
        }

        protected virtual List<TEntity> InvokeMultiInVoidOut(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Action<GeneratorContext, List<TEntity>>;
            var entitiesList = (List<TEntity>)entities;
            func.Invoke(context, entitiesList);

            return entitiesList;
        }

        protected virtual List<TEntity> InvokeMultiInSingleOut(GeneratorContext context, IList entities)
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

        protected virtual List<TEntity> InvokeMultiInMultiOut(GeneratorContext context, IList entities)
        {
            var func = _modifyFunc as Func<GeneratorContext, List<TEntity>, List<TEntity>>;
            var entitiesList = (List<TEntity>)entities;
            List<TEntity> res = func.Invoke(context, entitiesList);

            if (res == null)
            {
                return new List<TEntity>();
            }
            return res;
        }

    }
}
