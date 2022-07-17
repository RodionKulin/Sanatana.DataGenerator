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
        //internal types
        protected enum Output { Void, Single, Multi }

        //fields
        protected MethodInfo _delegateInvokeMethod;
        protected Dictionary<Type, int[]> _delegateMapping;
        protected bool _isMultiInput;
        protected Output _output;
        protected object _modifyFunc;


        //init
        protected DelegateParameterizedModifier(object modifyFunc, bool isMultiInput, Output output)
        {
            _delegateMapping = new Dictionary<Type, int[]>();

            _modifyFunc = modifyFunc;
            _isMultiInput = isMultiInput;
            _output = output;

            _delegateInvokeMethod = GetDelegateInvokeMethod();
        }

        public static class Factory
        {
            #region Single input, void output
            public static DelegateParameterizedModifier<TEntity> Create<T1>(
                Action<GeneratorContext, TEntity, T1> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2>(
                Action<GeneratorContext, TEntity, T1, T2> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3>(
                Action<GeneratorContext, TEntity, T1, T2, T3> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Void);
            }
            #endregion

            #region Single input, single output
            public static DelegateParameterizedModifier<TEntity> Create<T1>(
                Func<GeneratorContext, TEntity, T1, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2>(
                Func<GeneratorContext, TEntity, T1, T2, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3>(
                Func<GeneratorContext, TEntity, T1, T2, T3, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Single);
            }
            #endregion

            #region Single input, multiple outputs
            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1>(
                Func<GeneratorContext, TEntity, T1, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2>(
                Func<GeneratorContext, TEntity, T1, T2, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3>(
                Func<GeneratorContext, TEntity, T1, T2, T3, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, false, Output.Multi);
            }
            #endregion

            #region Multiple inputs, void output
            public static DelegateParameterizedModifier<TEntity> Create<T1>(
                Action<GeneratorContext, List<TEntity>, T1> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2>(
                Action<GeneratorContext, List<TEntity>, T1, T2> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Void);
            }
            #endregion

            #region Multiple inputs, single output
            public static DelegateParameterizedModifier<TEntity> Create<T1>(
                Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2>(
                Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }

            public static DelegateParameterizedModifier<TEntity> Create<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Single);
            }
            #endregion

            #region Multiple inputs, multiple outputs
            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1>(
                Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2>(
                Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }

            public static DelegateParameterizedModifier<TEntity> CreateMulti<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
                Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
            {
                return new DelegateParameterizedModifier<TEntity>(modifyFunc, true, Output.Multi);
            }
            #endregion
        }


        //Invoke modify method
        public virtual IList Modify(GeneratorContext context, IList entities)
        {
            //order required types according to delegate parameters order
            int[] argsOrder = GetRequiredTypesOrder(context);
            object[] requiredValues = context.RequiredEntities.Values.ToArray();
            Array.Sort(argsOrder, requiredValues);

            object[] arguments = new object[argsOrder.Length + 2];
            Array.Copy(requiredValues, 0, arguments, 2, requiredValues.Length);

            return _isMultiInput
                ? InvokeMultiIn(context, entities, arguments)
                : InvokeSingleIn(context, entities, arguments);
        }

        protected virtual List<TEntity> InvokeSingleIn(GeneratorContext context, IList entities, object[] arguments)
        {
            var inputList = (List<TEntity>)entities;
            var outputList = new List<TEntity>();

            for (int i = 0; i < inputList.Count; i++)
            {
                TEntity entity = inputList[i];
                GeneratorContext contextClone = context.Clone();
                contextClone.CurrentCount += i;

                arguments[0] = contextClone;
                arguments[1] = entity;
                object response = _delegateInvokeMethod.Invoke(_modifyFunc, arguments);

                if (response == null)
                {
                    continue;
                }
                if (_output == Output.Single)
                {
                    outputList.Add(response as TEntity);
                }
                if (_output == Output.Multi)
                {
                    outputList.AddRange(response as List<TEntity>);
                }
            }

            return _output == Output.Void
                ? inputList
                : outputList;
        }

        protected virtual List<TEntity> InvokeMultiIn(GeneratorContext context, IList entities, object[] arguments)
        {
            List<TEntity> entitiesTyped = (List<TEntity>)entities;

            arguments[0] = context;
            arguments[1] = entitiesTyped;

            object response = _delegateInvokeMethod.Invoke(_modifyFunc, arguments);

            if (_output == Output.Void)
            {
                return entitiesTyped;
            }
            if (_output == Output.Single)
            {
                return response == null
                    ? new List<TEntity>()
                    : new List<TEntity> { response as TEntity };
            }
            if (_output == Output.Multi)
            {
                return response as List<TEntity> ?? new List<TEntity>();
            }
            throw new NotImplementedException($"Unexpected value {_output} received for type {nameof(Output)}");
        }

        protected virtual MethodInfo GetDelegateInvokeMethod()
        {
            Type delegateType = _modifyFunc.GetType();
            return delegateType.GetMethod("Invoke");
        }

        protected virtual int[] GetRequiredTypesOrder(GeneratorContext context)
        {
            Type delegateType = _modifyFunc.GetType();

            if (!_delegateMapping.ContainsKey(delegateType))
            {
                List<Type> requiredParametersTypes = GetRequiredEntitiesFuncArguments();

                List<Type> requiredTypes = context.RequiredEntities.Keys.ToList();
                int[] requiredTypesOrder = requiredTypes
                    .Select(x => requiredParametersTypes.IndexOf(x))
                    .ToArray();

                _delegateMapping.Add(delegateType, requiredTypesOrder);
            }

            return _delegateMapping[delegateType].ToArray();
        }

        public virtual List<Type> GetRequiredEntitiesFuncArguments()
        {
            Type delegateType = _modifyFunc.GetType();
            Type[] genericArgs = delegateType.GetGenericArguments();

            List<Type> requiredParametersTypes = genericArgs
                //1 parameter is GeneratorContext
                //2 parameter is TEntity or List<TEntity> list of instances
                .Skip(2)   
                .Take(genericArgs.Length - 3)   //last is returned instnace  
                .ToList();

            return requiredParametersTypes;
        }
    }
}
