using Sanatana.DataGenerator.Generators;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Validators.BeforeSetup;
using Sanatana.DataGenerator.Internals.Validators.Contracts;
using Sanatana.DataGenerator.Modifiers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGenerator.Internals.Validators
{
    public class ValidatorsSetup
    {
        public List<IValidator> Validators { get; protected set; }


        //init
        public ValidatorsSetup()
        {
            Validators = new List<IValidator>()
            {
                //IBeforeSetupValidator
                new EntityDescriptionSetupValidator(),
                new CircularDependenciesSetupValidator(),
                new RequiredCountSetupValidator(),
                new RequiredEntitiesExistSetupValidator(),
                new InsertToPersistentStorageBeforeUseSetupValidator(),
                new GeneratorSettingsBeforeSetupValidator(),
                
                //IAfterSetupValidator
                new GeneratorSettingsAfterSetupValidator(),

                //IGenerateValidator
                new InstancesCountGeneratedValidator(),

                //IModifyValidator
                new InstancesCountModifiedValidator(),
            };
        }

        public ValidatorsSetup(List<IValidator> validators)
        {
            Validators = validators;
        }

        public virtual ValidatorsSetup Clone()
        {
            return new ValidatorsSetup(new List<IValidator>(Validators));
        }


        //configure methods
        public virtual ValidatorsSetup EnsureAddValidator(IValidator validator)
        {
            validator = validator ?? throw new ArgumentNullException(nameof(validator));
            if (Validators.All(x => x.GetType() != validator.GetType()))
            {
                Validators.Add(validator);
            }
            return this;
        }

        public virtual ValidatorsSetup AddValidator(IValidator validator)
        {
            Validators.Add(validator ?? throw new ArgumentNullException(nameof(validator)));
            return this;
        }

        public virtual IValidator GetValidator<TValidator>()
            where TValidator : IValidator
        {
            return Validators.OfType<TValidator>().FirstOrDefault();
        }

        public virtual ValidatorsSetup RemoveValidator<TValidator>()
            where TValidator : IValidator
        {
            Validators = Validators.Where(x => x.GetType() != typeof(TValidator)).ToList();
            return this;
        }


        //validation methods
        public virtual void ValidateBeforeSetup(GeneratorServices generatorServices)
        {
            List<IBeforeSetupValidator> validators = Validators.OfType<IBeforeSetupValidator>().ToList();
            validators.ForEach(v => v.ValidateSetup(generatorServices));
        }

        public virtual void ValidateAfterSetup(GeneratorServices generatorServices)
        {
            List<IAfterSetupValidator> validators = Validators.OfType<IAfterSetupValidator>().ToList();
            validators.ForEach(v => v.ValidateSetup(generatorServices));
        }

        public virtual void ValidateGenerated(IList entities, Type entityType, IGenerator generator)
        {
            List<IGenerateValidator> validators = Validators.OfType<IGenerateValidator>().ToList();
            validators.ForEach(v => v.ValidateGenerated(entities, entityType, generator));
        }

        public virtual void ValidateModified(IList entities, Type entityType, IModifier modifier)
        {
            List<IModifyValidator> validators = Validators.OfType<IModifyValidator>().ToList();
            validators.ForEach(v => v.ValidateModified(entities, entityType, modifier));
        }
    }
}
