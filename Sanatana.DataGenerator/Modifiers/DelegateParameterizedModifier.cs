using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Generators;

namespace Sanatana.DataGenerator.Modifiers
{
    public class DelegateParameterizedModifier : IModifier
    {
        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected Dictionary<Type, int[]> _delegateMapping;


        //properties
        public object MofifyFunc { get; set; }


        //init
        public DelegateParameterizedModifier()
        {
            _delegateMapping = new Dictionary<Type, int[]>();
        }



        //Invoke generate method
        public virtual List<TEntity> Modify<TEntity>(
            GeneratorContext context, List<TEntity> entities)
            where TEntity : class
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
            object res = _delegateInvokeMethod.Invoke(MofifyFunc, arguments);

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
            Type delegateType = MofifyFunc.GetType();

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
            Type delegateType = MofifyFunc.GetType();
            Type[] genericArgs = delegateType.GetGenericArguments();

            List<Type> requiredParametersTypes = genericArgs
                //1 parameter is GeneratorContext
                //2 parameter is List<TEntity> list of entities
                .Skip(2)   
                .Take(genericArgs.Length - 3)   //last is returned type    
                .ToList();

            return requiredParametersTypes;
        }


        //Register
        public virtual void RegisterDelegate<T1, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegate<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

        public virtual void RegisterDelegateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            MofifyFunc = modifyFunc;
        }

    }
}
