using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sanatana.DataGenerator.Entities;

namespace Sanatana.DataGenerator.Generators
{
    public class DelegateParameterizedGenerator<TEntity> : IDelegateParameterizedGenerator
        where TEntity : class
    {
        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected Dictionary<Type, int[]> _delegateMapping;
        protected object _generateFunc;


        //init
        protected DelegateParameterizedGenerator(object generateFunc)
        {
            if (generateFunc == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(generateFunc)}] can not be null.");
            }

            _delegateMapping = new Dictionary<Type, int[]>();

            _generateFunc = generateFunc;
        }
        
        public static class Factory
        {
            public static DelegateParameterizedGenerator<TEntity> Create<T1>(
                Func<GeneratorContext, T1, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2>(
                Func<GeneratorContext, T1, T2, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3>(
                Func<GeneratorContext, T1, T2, T3, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4>(
                Func<GeneratorContext, T1, T2, T3, T4, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1>(
                Func<GeneratorContext, T1, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2>(
                Func<GeneratorContext, T1, T2, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3>(
                Func<GeneratorContext, T1, T2, T3, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4>(
                Func<GeneratorContext, T1, T2, T3, T4, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }

            public static DelegateParameterizedGenerator<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
                Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, List<TEntity>> generateFunc)
            {
                return new DelegateParameterizedGenerator<TEntity>(generateFunc);
            }
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            //order required types according to delegate parameters order
            int[] argsOrder = GetRequiredTypesOrder(context);
            object[] requiredValues = context.RequiredEntities.Values.ToArray();
            Array.Sort(argsOrder, requiredValues);

            object[] arguments = new object[argsOrder.Length + 1];
            arguments[0] = context;
            Array.Copy(requiredValues, 0, arguments, 1, requiredValues.Length);
            
            //invoke delegate
            object res = _delegateInvokeMethod.Invoke(_generateFunc, arguments);

            //convert results
            if (res == null)
            {
                return new List<TEntity>();
            }
            if(res is List<TEntity>)
            {
                return res as List<TEntity>;
            }
            if(res is TEntity)
            {
                return new List<TEntity> { res as TEntity };
            }

            throw new NotImplementedException($"Unexpected result type {res.GetType()} for {typeof(TEntity)} generateFunc");
        }

        protected virtual int[] GetRequiredTypesOrder(GeneratorContext context)
        {
            Type delegateType = _generateFunc.GetType();

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
            Type delegateType = _generateFunc.GetType();

            Type[] genericArgs = delegateType.GetGenericArguments();
            List<Type> requiredParametersTypes = genericArgs
                .Skip(1)                     //first parameter is GeneratorContext
                .Take(genericArgs.Length - 2)//last is returned type
                .ToList();                   //the rest are required entity types

            return requiredParametersTypes;
        }


        //validation
        public virtual void ValidateEntitySettings(IEntityDescription entity)
        {
        }
    }
}
