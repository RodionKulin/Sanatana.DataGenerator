using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanatana.DataGenerator.Generators
{
    public class DelegateParameterizedGenerator : IGenerator
    {
        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected Dictionary<Type, int[]> _delegateMapping;

        //properties
        public object GenerateFunc { get; set; }

        //init
        public DelegateParameterizedGenerator()
        {
            _delegateMapping = new Dictionary<Type, int[]>();
        }
        

        //Invoke generate method
        public virtual List<TEntity> Generate<TEntity>(GeneratorContext context)
            where TEntity : class
        {
            //order required types according to delegate parameters order
            int[] argsOrder = GetRequiredTypesOrder(context);
            object[] requiredValues = context.RequiredEntities.Values.ToArray();
            Array.Sort(argsOrder, requiredValues);

            object[] arguments = new object[argsOrder.Length + 1];
            arguments[0] = context;
            Array.Copy(requiredValues, 0, arguments, 1, requiredValues.Length);
            
            //invoke delegate
            object res = _delegateInvokeMethod.Invoke(GenerateFunc, arguments);

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
            Type delegateType = GenerateFunc.GetType();

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

        internal virtual List<Type> GetRequiredEntitiesFuncParameters()
        {
            Type delegateType = GenerateFunc.GetType();

            Type[] genericArgs = delegateType.GetGenericArguments();
            List<Type> requiredParametersTypes = genericArgs
                .Skip(1)                     //first parameter is GeneratorContext
                .Take(genericArgs.Length - 2)//last is returned type
                .ToList();                   //the rest are required entity types

            return requiredParametersTypes;
        }

    


        //Register
        public virtual void RegisterDelegate<T1, TEntity>(
            Func<GeneratorContext, T1, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, TEntity>(
            Func<GeneratorContext, T1, T2, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, TEntity>(
            Func<GeneratorContext, T1, T2, T3, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, TEntity>(
            Func<GeneratorContext, T1, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, TEntity>(
            Func<GeneratorContext, T1, T2, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, TEntity>(
            Func<GeneratorContext, T1, T2, T3, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }
        
        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, List<TEntity>> generateFunc)
        {
            GenerateFunc = generateFunc;
        }

    }
}
