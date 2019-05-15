using Sanatana.DataGenerator.Generators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Modifiers
{
    public interface IModifier
    {
        List<TEntity> Modify<TEntity>(GeneratorContext context, List<TEntity> entities)
            where TEntity : class;
    }
}
