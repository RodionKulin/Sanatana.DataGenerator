using Sanatana.DataGenerator.CombineStrategies;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Sanatana.DataGenerator.Generators
{
    /// <summary>
    /// CombineModifier uses multiple IModifier sets in turn to modify entity instances.
    /// </summary>
    public class CombineModifier : IModifier
    {
        //fields
        private readonly List<IModifier[]> _modifiers;
        private ICombineStrategy _combineStrategy;


        //init
        public CombineModifier(ICombineStrategy combineStrategy = null)
        {
            _modifiers = new List<IModifier[]>();
            _combineStrategy = combineStrategy ?? new RoundRobinCombineStrategy();
        }

        public CombineModifier(List<IModifier[]> modifiers, ICombineStrategy combineStrategy = null)
        {
            _modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
            _combineStrategy = combineStrategy ?? new RoundRobinCombineStrategy();
        }


        #region IModifier methods
        public virtual IList Modify(GeneratorContext context, IList instances)
        {
            int nextModifierSetIndex = _combineStrategy.GetNext(_modifiers.Count, context.CurrentCount);
            IModifier[] nextModifierSet = _modifiers[nextModifierSetIndex];

            foreach (IModifier modifier in nextModifierSet)
            {
                instances = modifier.Modify(context, instances);
            }
            return instances;
        }
        #endregion


        #region Configure methods
        /// <summary>
        /// Add modifier set to list of modifier sets that will modify entity instances in turn.
        /// </summary>
        /// <param name="modifierSet"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifier AddModifierSet(params IModifier[] modifierSet)
        {
            modifierSet = modifierSet ?? throw new ArgumentNullException(nameof(modifierSet));
            _modifiers.Add(modifierSet);
            return this;
        }

        /// <summary>
        /// Add multiple modifier sets to list of modifier sets that will modify entity instances in turn.
        /// </summary>
        /// <param name="modifierSets"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifier AddModifierSets(List<IModifier[]> modifierSets)
        {
            modifierSets = modifierSets ?? throw new ArgumentNullException(nameof(modifierSets));
            _modifiers.AddRange(modifierSets);
            return this;
        }

        /// <summary>
        /// Remove all Modifiers.
        /// </summary>
        /// <returns></returns>
        public virtual CombineModifier RemoveModifiers()
        {
            _modifiers.Clear();
            return this;
        }

        /// <summary>
        /// Set ICombineStrategy to assign each modifiers set some portion of instances to modify.
        /// By default, if not set, will use RoundRobinCombineStrategy.
        /// </summary>
        /// <param name="combineStrategy"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifier SetCombineStrategy(ICombineStrategy combineStrategy)
        {
            _combineStrategy = combineStrategy ?? throw new ArgumentNullException(nameof(combineStrategy));
            return this;
        }

        #endregion
    }
}
