using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// Internal type to setup single modifiers set for CombineModifier.
    /// </summary>
    public class CombineModifierSet
    {
        protected List<IModifier> _modifiers;


        //init
        public CombineModifierSet()
        {
            _modifiers = new List<IModifier>();
        }


        //methods
        /// <summary>
        /// Add modifier(s) to modifier set. Each set will modify entity instances in turn.
        /// </summary>
        /// <param name="modifiers"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual CombineModifierSet AddModifier(params IModifier[] modifiers)
        {
            modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
            _modifiers.AddRange(modifiers);
            return this;
        }

        /// <summary>
        /// Internal method to select set modifiers.
        /// </summary>
        /// <returns></returns>
        public virtual List<IModifier> GetModifiers()
        {
            return _modifiers;
        }

        /// <summary>
        /// Remove modifiers from set.
        /// </summary>
        /// <returns></returns>
        public virtual CombineModifierSet RemoveModifiers()
        {
            _modifiers.Clear();
            return this;
        }
    }
}
