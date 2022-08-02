using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Generators
{
    public interface IDelegateParameterizedGenerator : IGenerator
    {
        List<Type> GetRequiredEntitiesFuncArguments();
    }
}
