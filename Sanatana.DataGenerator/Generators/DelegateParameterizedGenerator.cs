using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Generators
{
    public class DelegateParameterizedGenerator<TEntity> : IDelegateParameterizedGenerator
        where TEntity : class
    {
        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected int[] _requiredTypesOrder;
        protected object _generateFunc;


        //init
        protected DelegateParameterizedGenerator(object generateFunc)
        {
            if (generateFunc == null)
            {
                throw new ArgumentNullException($"Argument [{nameof(generateFunc)}] can not be null.");
            }

            _generateFunc = generateFunc;
            _delegateInvokeMethod = GetDelegateInvokeMethod();
        }
        
        public static class Factory
        {
            #region Single output
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
            #endregion

            #region Multi output
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
            #endregion
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
            _requiredTypesOrder = null;
        }


        //generation
        public virtual IList Generate(GeneratorContext context)
        {
            //order required types according to delegate parameters order
            int[] delegateArgumentsOrder = GetRequiredTypesOrder(context);
            object[] requiredValues = context.RequiredEntities.Values.ToArray();
            Array.Sort(delegateArgumentsOrder, requiredValues);

            object[] arguments = new object[delegateArgumentsOrder.Length + 1];
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

        protected virtual MethodInfo GetDelegateInvokeMethod()
        {
            Type delegateType = _generateFunc.GetType();
            return delegateType.GetMethod("Invoke");
        }

        protected virtual int[] GetRequiredTypesOrder(GeneratorContext context)
        {
            if (_requiredTypesOrder == null)
            {
                List<Type> requiredParametersTypes = GetRequiredEntitiesFuncArguments();

                List<Type> requiredTypes = context.RequiredEntities.Keys.ToList();
                int[] requiredTypesOrder = requiredTypes
                    .Select(x => requiredParametersTypes.IndexOf(x))
                    .ToArray();

                _requiredTypesOrder = requiredTypesOrder;
            }

            //need to clone it every time, because it gets changed
            return _requiredTypesOrder.ToArray();
        }

        public virtual List<Type> GetRequiredEntitiesFuncArguments()
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
        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults) { }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults) { }
    }
}
