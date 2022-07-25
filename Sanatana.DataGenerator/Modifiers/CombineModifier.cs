using Sanatana.DataGenerator.CombineStrategies;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// CombineModifier uses multiple IModifier sets in turn to modify entity instances.
    /// </summary>
    public class CombineModifier : IModifier
    {
        //fields
        protected List<List<IModifier>> _modifiers;
        protected IModifiersCombiner _combineStrategy;


        //init
        public CombineModifier(IModifiersCombiner combineStrategy = null)
        {
            _modifiers = new List<List<IModifier>>();
            _combineStrategy = combineStrategy ?? new RoundRobinModifiersCombiner();
        }

        public CombineModifier(List<List<IModifier>> modifiers, IModifiersCombiner combineStrategy = null)
        {
            _modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
            _combineStrategy = combineStrategy ?? new RoundRobinModifiersCombiner();
        }
        
        /// <summary>
        /// Internal method to reset variables when starting new generation.
        /// </summary>
        public virtual void Setup(GeneratorServices generatorServices)
        {
            _combineStrategy.Setup(_modifiers, generatorServices);
        }



        #region IModifier methods
        public virtual IList Modify(GeneratorContext context, IList instances)
        {
            List<IModifier> nextModifierSet = _combineStrategy.GetNext(_modifiers, context);

            foreach (IModifier modifier in nextModifierSet)
            {
                instances = modifier.Modify(context, instances);
            }
            return instances;
        }

        public virtual void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults)
        {
            if (_modifiers.Count == 0)
            {
                throw new ArgumentException($"No inner modifiers provided in {nameof(CombineModifier)} for type {entity.Type.FullName}. Expected at least 1 inner modifier. {nameof(CombineModifier)} should use multiple modifiers in turn to produce entity instances.");
            }
            if (_combineStrategy == null)
            {
                throw new ArgumentNullException(nameof(IModifiersCombiner));
            }

            foreach (List<IModifier> modifierSet in _modifiers)
                foreach (IModifier modifier in modifierSet)
                {
                    modifier.ValidateBeforeSetup(entity, defaults);
                }

            _combineStrategy.ValidateBeforeSetup(_modifiers, entity, defaults);
        }

        public virtual void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults)
        {
            if (_modifiers.Count == 0)
            {
                throw new ArgumentException($"No inner modifiers provided in {nameof(CombineModifier)} for type {entityContext.Type.FullName}. Expected at least 1 inner modifier. {nameof(CombineModifier)} should use multiple modifiers in turn to produce entity instances.");
            }
            if (_combineStrategy == null)
            {
                throw new ArgumentNullException(nameof(IModifiersCombiner));
            }

            foreach (List<IModifier> modifierSet in _modifiers)
                foreach (IModifier modifier in modifierSet)
                {
                    modifier.ValidateAfterSetup(entityContext, defaults);
                }

            _combineStrategy.ValidateAfterSetup(_modifiers, entityContext, defaults);
        }
        #endregion


        #region Configure methods
        /// <summary>
        /// Add modifiers set for CombineModifier.
        /// CombineModifier uses multiple IModifier sets in turn to modify entity instances.
        /// </summary>
        /// <param name="setSetup"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifier AddModifiersSet(Func<CombineModifierSet, CombineModifierSet> setSetup)
        {
            setSetup = setSetup ?? throw new ArgumentNullException(nameof(setSetup));
            var combineModifiersSet = new CombineModifierSet();
            combineModifiersSet = setSetup(combineModifiersSet) ?? throw new ArgumentNullException(nameof(combineModifiersSet));
            
            _modifiers.Add(combineModifiersSet.GetModifiers());
            
            return this;
        }

        /// <summary>
        /// Remove all Modifiers.
        /// </summary>
        /// <returns></returns>
        public virtual CombineModifier RemoveModifiersSets()
        {
            _modifiers.Clear();
            return this;
        }

        /// <summary>
        /// Set ICombineStrategy to assign each modifiers set some portion of instances to modify.
        /// By default, if not set, will use RoundRobinModifiersCombiner.
        /// </summary>
        /// <param name="combineStrategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifier SetCombineStrategy(IModifiersCombiner combineStrategy)
        {
            _combineStrategy = combineStrategy ?? throw new ArgumentNullException(nameof(combineStrategy));
            return this;
        }

        #endregion

    }
}
