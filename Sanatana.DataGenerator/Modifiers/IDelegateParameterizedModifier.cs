using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    public interface IDelegateParameterizedModifier : IModifier
    {
        List<Type> GetRequiredEntitiesFuncParameters();
    }
}
