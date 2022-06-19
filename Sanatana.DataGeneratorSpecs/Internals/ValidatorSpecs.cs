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

namespace Sanatana.DataGeneratorSpecs.Internals
{
    [TestClass]
    public class ValidatorSpecs
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckGeneratorSetupComplete_WhenNotComplete_ThrowsException()
        {
            //Arrange
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            Dictionary<Type, IEntityDescription> entityDescriptions = descriptions.ToDictionary(x => x.Type, x => x);

            //Act
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup.GetGeneratorServices());
            target.CheckGeneratorSetupComplete(entityDescriptions);
        }

        [TestMethod]
        public void CheckGeneratorSetupComplete_WhenComplete_NotThrowsException()
        {
            //Arrange
            var generatorSetup = new GeneratorSetup()
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(new InMemoryStorage()));

            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetTargetCount(5)
                .SetGenerator(context => null));
            Dictionary<Type, IEntityDescription> entityDescriptions = descriptions.ToDictionary(x => x.Type, x => x);

            //Act
            var target = new Validator(generatorSetup.GetGeneratorServices());
            target.CheckGeneratorSetupComplete(entityDescriptions);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CheckCircularDependencies_WhenHasCircleDependencies_ThrowsException()
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
            Dictionary<Type, IEntityDescription> entityDescriptions = descriptions.ToDictionary(x => x.Type, x => x);

            //Act
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup.GetGeneratorServices());
            target.CheckCircularDependencies(entityDescriptions);
        }

        [TestMethod]
        public void CheckCircularDependencies_WhenNoCircleDependencies_NotThrowsException()
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
            Dictionary<Type, IEntityDescription> entityDescriptions = descriptions.ToDictionary(x => x.Type, x => x);

            //Act
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup.GetGeneratorServices());
            target.CheckCircularDependencies(entityDescriptions);
        }


    }
}
