using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Validators.AfterModify
{
    public interface IModifyValidator : IValidator
    {
        void ValidateModified(IList entities, Type entityType, IModifier modifier);
    }
}
