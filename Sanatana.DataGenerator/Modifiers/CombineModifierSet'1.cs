using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// Internal type to setup single modifiers set for CombineModifier&lt;TEntity&gt;.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class CombineModifierSet<TEntity> : CombineModifierSet
        where TEntity : class
    {
        //fields
        protected EntityDescription<TEntity> _entityDescription;


        //init
        public CombineModifierSet(EntityDescription<TEntity> entityDescription)
        {
            _entityDescription = entityDescription;
        }


        #region Modifier with single input, void output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier(
            Action<GeneratorContext, TEntity> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1>(
            Action<GeneratorContext, TEntity, T1> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2>(
            Action<GeneratorContext, TEntity, T1, T2> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3>(
            Action<GeneratorContext, TEntity, T1, T2, T3> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Action<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with single input, single output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier(
            Func<GeneratorContext, TEntity, TEntity> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1>(
            Func<GeneratorContext, TEntity, T1, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2>(
            Func<GeneratorContext, TEntity, T1, T2, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3>(
            Func<GeneratorContext, TEntity, T1, T2, T3, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with single input, multiple outputs
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier(
            Func<GeneratorContext, TEntity, List<TEntity>> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1>(
            Func<GeneratorContext, TEntity, T1, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2>(
            Func<GeneratorContext, TEntity, T1, T2, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3>(
            Func<GeneratorContext, TEntity, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, TEntity, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, void output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier(
            Action<GeneratorContext, List<TEntity>> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1>(
            Action<GeneratorContext, List<TEntity>, T1> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2>(
            Action<GeneratorContext, List<TEntity>, T1, T2> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Action<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, single output
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier(
            Func<GeneratorContext, List<TEntity>, TEntity> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddSingleModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.Create(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }
        #endregion


        #region Modifier with multi inputs, multi outputs
        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier(
            Func<GeneratorContext, List<TEntity>, List<TEntity>> modifyFunc)
        {
            _modifiers.Add(DelegateModifier<TEntity>.Factory.Create(modifyFunc));
            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1>(
            Func<GeneratorContext, List<TEntity>, T1, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2>(
            Func<GeneratorContext, List<TEntity>, T1, T2, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        /// <summary>
        /// Add modifier that is triggered after generation. Can be used to apply additional customization to existing entity instance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <typeparam name="T9"></typeparam>
        /// <typeparam name="T10"></typeparam>
        /// <typeparam name="T11"></typeparam>
        /// <typeparam name="T12"></typeparam>
        /// <typeparam name="T13"></typeparam>
        /// <typeparam name="T14"></typeparam>
        /// <param name="modifyFunc"></param>
        /// <returns></returns>
        public virtual CombineModifierSet<TEntity> AddMultiModifier<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, List<TEntity>, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> modifyFunc)
        {
            var modifier = DelegateParameterizedModifier<TEntity>.Factory.CreateMulti(modifyFunc);
            _modifiers.Add(modifier);
            _entityDescription.SetRequiredFromModifier(modifier);

            return this;
        }

        #endregion
    }

}
