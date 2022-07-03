using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using Sanatana.DataGenerator.Internals.Validators.BeforeSetup;

namespace Sanatana.DataGeneratorSpecs.Internals
{
    [TestClass]
    public class ValidatorSpecs
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EntityDescriptionSetupValidator_WhenNotComplete_ThenThrowsException()
        {
            //Arrange
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            Dictionary<Type, IEntityDescription> entityDescriptions = descriptions.ToDictionary(x => x.Type, x => x);
            var generatorSetup = new GeneratorSetup()
                .RegisterEntity(descriptions);
            var target = new EntityDescriptionSetupValidator();

            //Act
            target.ValidateSetup(generatorSetup.GetGeneratorServices());
        }

        [TestMethod]
        public void CheckGeneratorSetupComplete_WhenComplete_ThenNotThrowsException()
        {
            //Arrange
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetTargetCount(5)
                .SetGenerator(context => null));
            var generatorSetup = new GeneratorSetup()
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(new InMemoryStorage()))
                .RegisterEntity(descriptions);
            var target = new EntityDescriptionSetupValidator();

            //Act
            target.ValidateSetup(generatorSetup.GetGeneratorServices());
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CircularDependenciesSetupValidator_WhenHasCircleDependencies_ThenThrowsException()
        {
            //Arrange
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            descriptions.Add(new EntityDescription<Comment>());
            descriptions.Add(new EntityDescription<Category>()
                .SetRequired(typeof(Comment))
                .SetRequired(typeof(Attachment)));
            descriptions.Add(new EntityDescription<Attachment>()
                .SetRequired(typeof(Post)));
            var generatorSetup = new GeneratorSetup()
                .RegisterEntity(descriptions);
            var target = new CircularDependenciesSetupValidator();

            //Act
            target.ValidateSetup(generatorSetup.GetGeneratorServices());
        }

        [TestMethod]
        public void CircularDependenciesSetupValidator_WhenNoCircleDependencies_ThenNotThrowsException()
        {
            //Arrange
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            descriptions.Add(new EntityDescription<Comment>());
            descriptions.Add(new EntityDescription<Category>()
                .SetRequired(typeof(Comment)));
            descriptions.Add(new EntityDescription<Attachment>()
                .SetRequired(typeof(Post)));
            var generatorSetup = new GeneratorSetup()
                .RegisterEntity(descriptions);
            var target = new CircularDependenciesSetupValidator();

            //Act
            target.ValidateSetup(generatorSetup.GetGeneratorServices());
        }


    }
}
