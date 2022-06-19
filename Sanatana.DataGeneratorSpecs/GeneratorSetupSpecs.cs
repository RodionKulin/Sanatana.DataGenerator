using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Storages;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Sanatana.DataGeneratorSpecs
{
    [TestClass]
    public class GeneratorSetupSpecs
    {
        //Progress specs
        [TestMethod]
        public void GeneratorSetup_WhenSubscribedToProgressChange_ReturnsProgress()
        {
            //Arrange
            GeneratorSetup generatorSetup = GetGeneratorSetup();
            var completionPercents = new List<decimal>();
            generatorSetup = generatorSetup.SetProgressHandler(
                (decimal completionPercent) => completionPercents.Add(completionPercent)
            );

            //Act
            generatorSetup.Generate();

            //Assert
            completionPercents.Should().NotBeEmpty();
            completionPercents.Should().EndWith(100);
        }


        //Generate specs
        [TestMethod]
        public void GeneratorSetup_WhenInsertingSameEntityInParallel_InsertsExpectedCountOfInstances()
        {
            //Arrange
            var slowStorage = new SlowStorage();
            int targetCount = 100;
            int instancesPerRequest = 10;

            var generatorSetup = new GeneratorSetup()
                .SetMaxParallelInserts(4)
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(slowStorage))
                .RegisterEntity<Post>(entity => entity
                    .SetTargetCount(targetCount)
                    .SetRequestCapacityProvider(10) //will make 10 parallel db requests
                    .SetGenerator((x) => new Post() { Id = (int)x.CurrentCount })
                );

            //Act
            generatorSetup.Generate();

            //Assert
            slowStorage.Select<Post>().Should().HaveCount(targetCount);
            slowStorage.Select<Post>().Select(x => x.Id).Distinct().Should().HaveCount(targetCount);

            int expectedRequestsCount = targetCount / instancesPerRequest;
            slowStorage.InsertTime.Should().HaveCount(expectedRequestsCount);
        }

        [TestMethod]
        public void GeneratorSetup_WhenInsertingSameEntityInParallelFromSingleGeneratorInvocation_InsertsExpectedCountOfInstances()
        {
            //Arrange
            var slowStorage = new SlowStorage();
            int targetCount = 100;
            int instancesPerRequest = 10;
            List<Post> postsToGenerate = Enumerable.Range(0, targetCount)
                .Select(id => new Post { Id = id })
                .ToList();

            var generatorSetup = new GeneratorSetup()
                .SetMaxParallelInserts(4)
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(slowStorage))
                .RegisterEntity<Post>(entity => entity
                    .SetTargetCount(targetCount)
                    .SetRequestCapacityProvider(10) //will make 10 parallel db requests
                    .SetMultiGenerator((x) => postsToGenerate)
                );

            //Act
            generatorSetup.Generate();

            //Assert
            slowStorage.Select<Post>().Should().HaveCount(targetCount);
            slowStorage.Select<Post>().Select(x => x.Id).Distinct().Should().HaveCount(targetCount);

            int expectedRequestsCount = targetCount / instancesPerRequest;
            slowStorage.InsertTime.Should().HaveCount(expectedRequestsCount);
        }


        //Setup helpers
        private GeneratorSetup GetGeneratorSetup()
        {
            long targetCount = 100;

            var generatorSetup = new GeneratorSetup()
                .SetDefaultSettings(defaults => defaults.AddPersistentStorage(new InMemoryStorage()))
                .RegisterEntity<Category>(entity => entity
                    .SetTargetCount(targetCount)
                    .SetGenerator(x => new Category())
                )
                .RegisterEntity<Post>(entity => entity
                    .SetTargetCount(targetCount)
                    .SetGenerator(x => new Post())
                )
                .RegisterEntity<Comment>(entity => entity
                    .SetTargetCount(targetCount)
                    .SetGenerator(x => new Comment())
                );

            return generatorSetup;
        }

        private class SlowStorage : InMemoryStorage
        {
            public ConcurrentQueue<DateTime> InsertTime { get; set; } = new ConcurrentQueue<DateTime>();

            public override async Task Insert<TEntity>(List<TEntity> entities) where TEntity : class
            {
                await Task.Delay(1000); //simulate db request
                await base.Insert(entities);
                InsertTime.Enqueue(DateTime.Now);
            }
        }
    }
}

