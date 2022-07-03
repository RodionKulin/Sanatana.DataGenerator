﻿using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Internals.Validators.Contracts
{
    public interface IBeforeSetupValidator : IValidator
    {
        void ValidateSetup(GeneratorServices generatorServices);
    }
}
