using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.DataGenerator.Modifiers
{
    public class DelegateParameterizedModifier<TEntity> : IDelegateParameterizedModifier
        where TEntity : class
    {
        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected Dictionary<Type, int[]> _delegateMapping;
        protected object _modifyFunc;


        //init
        protected DelegateParameterizedModifier(object modifyFunc)
        {
            _delegateMapping = new Dictionary<Type, int[]>();
            _modifyFunc = modifyFunc;
        }

        public static class Factory
        {
            public static DelegateParameterizedModifier<TEntity> Create<T1>(
                Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2>(
                Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1>(
                Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2>(
                Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc);
            }
        }


        //Invoke modify method
        public virtual IList Modify(GeneratorContext context, IList entities)
        {
            //order required types according to delegate parameters order
            int[] argsOrder = GetRequiredTypesOrder(context);
            object[] requiredValues = context.RequiredEntities.Values.ToArray();
            Array.Sort(argsOrder, requiredValues);

            object[] arguments = new object[argsOrder.Length + 2];
            arguments[0] = context;
            arguments[1] = entities;
            Array.Copy(requiredValues, 0, arguments, 2, requiredValues.Length);

            //invoke delegate
            object res = _delegateInvokeMethod.Invoke(_modifyFunc, arguments);

            //convert results
            if (res == null)
            {
                return new List<TEntity>();
            }
            if (res is List<TEntity>)
            {
                return res as List<TEntity>;
            }
            if (res is TEntity)
            {
                return new List<TEntity> { res as TEntity };
            }

            throw new NotImplementedException($"Unexpected result type {res.GetType()} for {typeof(TEntity)} generator");
        }

        protected virtual int[] GetRequiredTypesOrder(GeneratorContext context)
        {
            Type delegateType = _modifyFunc.GetType();

            if (_delegateInvokeMethod == null)
            {
                _delegateInvokeMethod = delegateType.GetMethod("Invoke");
            }

            if (!_delegateMapping.ContainsKey(delegateType))
            {
                List<Type> requiredParametersTypes = GetRequiredEntitiesFuncParameters();

                List<Type> requiredTypes = context.RequiredEntities.Keys.ToList();
                int[] requiredTypesOrder = requiredTypes
                    .Select(x => requiredParametersTypes.IndexOf(x))
                    .ToArray();

                _delegateMapping.Add(delegateType, requiredTypesOrder);
            }

            return _delegateMapping[delegateType].ToArray();
        }

        public virtual List<Type> GetRequiredEntitiesFuncParameters()
        {
            Type delegateType = _modifyFunc.GetType();
            Type[] genericArgs = delegateType.GetGenericArguments();

            List<Type> requiredParametersTypes = genericArgs
                //1 parameter is GeneratorContext
                //2 parameter is List<TEntity> list of entities
                .Skip(2)   
                .Take(genericArgs.Length - 3)   //last is returned type    
                .ToList();

            return requiredParametersTypes;
        }
    }
}
