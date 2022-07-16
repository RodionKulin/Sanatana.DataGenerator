using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    public interface IModifier
    {
        IList Modify(GeneratorContext context, IList instances);
    }
}
