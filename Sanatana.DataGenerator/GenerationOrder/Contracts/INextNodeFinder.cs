using Sanatana.DataGenerator.Internals;
using System.Collections;

namespace Sanatana.DataGenerator.GenerationOrder.Contracts
{
    /// <summary>
    /// Finder that returns next best node to generate, that can be flushed to persistent storage sooner than other entities.
    /// </summary>
    public interface INextNodeFinder
    {
        EntityContext FindNextNode();
    }
}