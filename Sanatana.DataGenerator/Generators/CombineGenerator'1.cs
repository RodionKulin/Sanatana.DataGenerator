using Sanatana.DataGenerator.CombineStrategies;
using Sanatana.DataGenerator.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Generators
{
    public class CombineGenerator<TEntity> : CombineGenerator
        where TEntity : class
    {
        //fields
        protected EntityDescription<TEntity> _entityDescription;


        //init
        public CombineGenerator(EntityDescription<TEntity> entityDescription, IGeneratorsCombiner combineStrategy = null)
            : base(combineStrategy)
        {
            _entityDescription = entityDescription;
        }

        public CombineGenerator(EntityDescription<TEntity> entityDescription, 
            List<IGenerator> generators, IGeneratorsCombiner combineStrategy = null)
            : base(generators, combineStrategy)
        {
            _entityDescription = entityDescription;
        }


        #region Generator with single output
        /// <summary>
        /// Add generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator(
            Func<GeneratorContext, TEntity> generateFunc)
        {
            if (generateFunc == null)
            {
                throw new ArgumentNullException(nameof(generateFunc));
            }
            _generators.Add(DelegateGenerator<TEntity>.Factory.Create(generateFunc));
            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1>(
            Func<GeneratorContext, T1, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3>(
            Func<GeneratorContext, T1, T2, T3, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4>(
            Func<GeneratorContext, T1, T2, T3, T4, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <typeparam name="T15"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TEntity> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.Create(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }
        #endregion


        #region Generator with multi outputs
        /// <summary>
        /// Add generator Func that will create new TEntity instances.
        /// </summary>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator(
            Func<GeneratorContext, List<TEntity>> generateFunc)
        {
            _generators.Add(DelegateGenerator<TEntity>.Factory.CreateMulti(generateFunc));
            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1>(
            Func<GeneratorContext, T1, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2>(
            Func<GeneratorContext, T1, T2, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);


            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3>(
            Func<GeneratorContext, T1, T2, T3, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4>(
            Func<GeneratorContext, T1, T2, T3, T4, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <typeparam name="T4"></typeparam>
        /// <typeparam name="T5"></typeparam>
        /// <typeparam name="T6"></typeparam>
        /// <typeparam name="T7"></typeparam>
        /// <typeparam name="T8"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        /// <summary>
        /// Add generator Func that will receive Required type instances as parameters and create new TEntity instances.
        /// Will also register generic arguments as Required types.
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
        /// <typeparam name="T15"></typeparam>
        /// <param name="generateFunc"></param>
        /// <returns></returns>
        public virtual CombineGenerator<TEntity> AddMultiGenerator<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
            Func<GeneratorContext, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, List<TEntity>> generateFunc)
        {
            var generator = DelegateParameterizedGenerator<TEntity>.Factory.CreateMulti(generateFunc);
            _entityDescription.SetRequiredFromGenerator(generator);
            _generators.Add(generator);

            return this;
        }

        #endregion

    }
}
