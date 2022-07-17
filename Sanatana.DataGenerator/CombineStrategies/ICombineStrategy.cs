using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.CombineStrategies
{
    /// <summary>
    /// Assign each generator some portion of instances to generate.
    /// </summary>
    public interface ICombineStrategy
    {
        int GetNext(int generatorsCount, long currentCount);
    }
}
