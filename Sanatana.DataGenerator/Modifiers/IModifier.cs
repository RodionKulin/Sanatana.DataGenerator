using System;
using System.Collections.Generic;
using System.Collections;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals.EntitySettings;

namespace Sanatana.DataGenerator.Modifiers
{
    public interface IModifier
    {
        IList Modify(GeneratorContext context, IList instances);
        void ValidateBeforeSetup(IEntityDescription entity, DefaultSettings defaults);
        void ValidateAfterSetup(EntityContext entityContext, DefaultSettings defaults);
    }
}
