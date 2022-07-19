using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.SpreadStrategies;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.SpreadStrategies
{
    [TestClass]
    internal class CombinatoricsSpreadStrategySpecs
    {
        //Progress specs
        [TestMethod]
        public void GeneratorSetup_WhenSubscribedToProgressChange_ReturnsProgress()
        {
            
            //Arrange
            GeneratorSetup generatorSetup = new GeneratorSetup()
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(new InMemoryStorage()))
                .RegisterEntity<Post>(entity => entity
                    .SetGenerator((ctx) => new Post())
                )
                .RegisterEntity<Attachment>(entity => entity
                    .SetGenerator((ctx) => new Attachment())
                )
                .RegisterEntity<Category>(entity => entity
                    .SetGenerator((ctx) => new Category())
                )
                .RegisterEntity<Comment>(entity => entity
                    .SetGenerator<Post, Attachment>((ctx, post, attachment) => new Comment())
                    .SetSpreadStrategyAndTargetCount(new CartesianProductSpreadStrategy())
                );


            //Act
            generatorSetup.Generate();

            //Assert

        }

    }
}
