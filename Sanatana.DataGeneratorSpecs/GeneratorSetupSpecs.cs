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
        [TestMethod]
        public void GeneratorSetup_WhenSubscribedToProgressChange_ReturnsProgress()
        {
            //Arrange
            using GeneratorSetup generatorSetup = GetGeneratorSetup();
            var completionPercents = new List<decimal>();
            generatorSetup.Progress.Changed += new Action<GeneratorSetup, decimal>(
                (GeneratorSetup _, decimal completionPercent) => completionPercents.Add(completionPercent));

            //Act
            generatorSetup.Generate();

            //Assert
            completionPercents.Should().NotBeEmpty();
            completionPercents.Should().EndWith(100);
        }

        [TestMethod]
        public void GeneratorSetup_WhenInsertingSameEntityInParallel_InsertsExpectedCountOfInstances()
        {
            //Arrange
            var slowStorage = new SlowStorage();
            int targetCount = 100;
            int instancesPerRequest = 10;

            using var generatorSetup = new GeneratorSetup();
            generatorSetup.TemporaryStorage.MaxTasksRunning = 4;
            generatorSetup.Defaults.PersistentStorages.Add(slowStorage);
            generatorSetup.RegisterEntity<Post>()
                .SetTargetCount(targetCount)
                .SetRequestCapacityProvider(10) //will make 10 parallel db requests
                .SetGenerator((x) => new Post() { Id = (int)x.CurrentCount });

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

            using var generatorSetup = new GeneratorSetup();
            generatorSetup.TemporaryStorage.MaxTasksRunning = 4;
            generatorSetup.Defaults.PersistentStorages.Add(slowStorage);
            generatorSetup.RegisterEntity<Post>()
                .SetTargetCount(targetCount)
                .SetRequestCapacityProvider(10) //will make 10 parallel db requests
                .SetMultiGenerator((x) => postsToGenerate);

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

            var generatorSetup = new GeneratorSetup();
            generatorSetup.Defaults.PersistentStorages.Add(new InMemoryStorage());

            generatorSetup.RegisterEntity<Category>()
                .SetTargetCount(targetCount)
                .SetGenerator(x => new Category());
            generatorSetup.RegisterEntity<Post>()
                .SetTargetCount(targetCount)
                .SetGenerator(x => new Post());
            generatorSetup.RegisterEntity<Comment>()
                .SetTargetCount(targetCount)
                .SetGenerator(x => new Comment());

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

