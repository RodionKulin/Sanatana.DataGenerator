using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Sanatana.DataGeneratorSpecs.Samples;
using Sanatana.DataGenerator.Storages;

namespace Sanatana.DataGeneratorSpecs.Internals
{
    [TestClass]
    public class ValidatorSpecs
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CheckGeneratorSetupComplete_WhenNotComplete()
        {
            //Prepare
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            Dictionary<Type, IEntityDescription> entityDescriptions =
                descriptions.ToDictionary(x => x.Type, x => x);

            //Invoke
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup);
            target.CheckGeneratorSetupComplete(entityDescriptions);
        }

        [TestMethod]
        public void CheckGeneratorSetupComplete_WhenComplete()
        {
            //Prepare
            var generatorSetup = new GeneratorSetup();
            generatorSetup.Defaults.PersistentStorages.Add(new InMemoryStorage());

            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetTargetCount(5)
                .SetGenerator(context => null));
            Dictionary<Type, IEntityDescription> entityDescriptions =
                descriptions.ToDictionary(x => x.Type, x => x);

            //Invoke
            var target = new Validator(generatorSetup);
            target.CheckGeneratorSetupComplete(entityDescriptions);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void CheckCircularDependencies_WhenHasCircleDependencies()
        {
            //Prepare
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            descriptions.Add(new EntityDescription<Comment>());
            descriptions.Add(new EntityDescription<Category>()
                .SetRequired(typeof(Comment))
                .SetRequired(typeof(Attachment)));
            descriptions.Add(new EntityDescription<Attachment>()
                .SetRequired(typeof(Post)));
            Dictionary<Type, IEntityDescription> entityDescriptions =
                descriptions.ToDictionary(x => x.Type, x => x);

            //Invoke
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup);
            target.CheckCircularDependencies(entityDescriptions);
        }

        [TestMethod]
        public void CheckCircularDependencies_WhenNoCircleDependencies()
        {
            //Prepare
            var descriptions = new List<IEntityDescription>();
            descriptions.Add(new EntityDescription<Post>()
                .SetRequired(typeof(Category)));
            descriptions.Add(new EntityDescription<Comment>());
            descriptions.Add(new EntityDescription<Category>()
                .SetRequired(typeof(Comment)));
            descriptions.Add(new EntityDescription<Attachment>()
                .SetRequired(typeof(Post)));
            Dictionary<Type, IEntityDescription> entityDescriptions =
                descriptions.ToDictionary(x => x.Type, x => x);

            //Invoke
            var generatorSetup = new GeneratorSetup();
            var target = new Validator(generatorSetup);
            target.CheckCircularDependencies(entityDescriptions);
        }


        //Setup helpers
        private Dictionary<Type, IEntityDescription> SetupCompleteOrderProvider(
            List<IEntityDescription> descriptions)
        {

            Dictionary<Type, IEntityDescription> entityDescriptions = 
                descriptions.ToDictionary(x => x.Type, x => x);
            return entityDescriptions;
        }


    }
}
