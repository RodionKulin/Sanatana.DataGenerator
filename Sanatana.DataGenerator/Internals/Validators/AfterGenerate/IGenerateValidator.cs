using Sanatana.DataGenerator.Generators;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Validators.AfterGenerate
{
    public interface IGenerateValidator : IValidator
    {
        void ValidateGenerated(IList entities, Type entityType, IGenerator generator);
    }
}
