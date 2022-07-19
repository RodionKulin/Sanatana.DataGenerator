using Sanatana.DataGenerator.CombineStrategies;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// CombineGenerator uses multiple inner generators in turn to produce entity instances.
    /// </summary>
    public class CombineGenerator : IGenerator
    {
        //fields
        protected List<IGenerator> _generators;
        protected IGeneratorsCombiner _combineStrategy;


        //init
        public CombineGenerator(IGeneratorsCombiner combineStrategy = null)
        {
            _generators = new List<IGenerator>();
            _combineStrategy = combineStrategy ?? new RoundRobinGeneratorsCombiner();
        }

        public CombineGenerator(List<IGenerator> generators, IGeneratorsCombiner combineStrategy = null)
        {
            _generators = generators ?? throw new ArgumentNullException(nameof(generators));
            _combineStrategy = combineStrategy ?? new RoundRobinGeneratorsCombiner();
        }

        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
            _combineStrategy.Setup(generatorServices);
        }


        #region IGenerator methods
        public virtual IList Generate(GeneratorContext context)
        {
            IGenerator nextGenerator = _combineStrategy.GetNext(_generators, context);
            return nextGenerator.Generate(context);
        }

        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
            if(_generators.Count == 0)
            {
                throw new ArgumentException($"No inner generators provided in {nameof(CombineGenerator)} for type {entity.Type.FullName}. Expected at least 1 inner generator. {nameof(CombineGenerator)} should use multiple generators in turn to produce entity instances.");
            }
            if (_combineStrategy == null)
            {
                throw new ArgumentNullException(nameof(IGeneratorsCombiner));
            }

            foreach (IGenerator generator in _generators)
            {
                generator.ValidateBeforeSetup(entity, defaults);
            }
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
            if (_generators.Count == 0)
            {
                throw new ArgumentException($"No inner generators provided in {nameof(CombineGenerator)} for type {entityContext.Type.FullName}. Expected at least 1 inner generator. {nameof(CombineGenerator)} should use multiple generators in turn to produce entity instances.");
            }
            if (_combineStrategy == null)
            {
                throw new ArgumentNullException(nameof(IGeneratorsCombiner));
            }

            foreach (IGenerator generator in _generators)
            {
                generator.ValidateAfterSetup(entityContext, defaults);
            }
        }
        #endregion


        #region Configure methods
        /// <summary>
        /// Add generator to list of generators that will produce entity instances in turn.
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineGenerator AddGenerator(IGenerator generator)
        {
            generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _generators.Add(generator);
            return this;
        }
        
        /// <summary>
        /// Add multiple generators to list of generators that will produce entity instances in turn.
        /// </summary>
        /// <param name="generators"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineGenerator AddGenerator(params IGenerator[] generators)
        {
            generators = generators ?? throw new ArgumentNullException(nameof(generators));
            _generators.AddRange(generators);
            return this;
        }

        /// <summary>
        /// Remove all Generators.
        /// </summary>
        /// <returns></returns>
        public virtual CombineGenerator RemoveGenerator()
        {
            _generators.Clear();
            return this;
        }

        /// <summary>
        /// Set ICombineStrategy to assign each generator some portion of instances to generate.
        /// By default, if not set, will use RoundRobinGeneratorsCombiner.
        /// </summary>
        /// <param name="combineStrategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineGenerator SetCombineStrategy(IGeneratorsCombiner combineStrategy)
        {
            _combineStrategy = combineStrategy ?? throw new ArgumentNullException(nameof(combineStrategy));
            return this;
        }
        #endregion

    }
}
