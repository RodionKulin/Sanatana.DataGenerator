using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    /// <summary>
    /// PassThroughModifier modifier makes not changes to generated entity instances.
    /// Add PassThroughModifier to list of entity modifiers to make it not use list of default modifiers.
    /// </summary>
    public class PassThroughModifier : IModifier
    {
        public virtual IList Modify(GeneratorContext context, IList instances)
        {
            return instances;
        }
    }
}
