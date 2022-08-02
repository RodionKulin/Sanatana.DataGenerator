using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using System.Collections;

namespace Sanatana.DataGenerator.Supervisors.Contracts
{
    /// <summary>
    /// Finder that returns next best node to generate, that can be flushed to persistent storage sooner than other entities.
    /// </summary>
    public interface INextNodeFinder
    {
        EntityContext FindNextNode();
    }
}