using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Storages;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sanatana.DataGeneratorSpecs.Internals
{
    [TestClass]
    public class TemporaryStorageSpecs
    {
        [TestMethod]
        [ExpectedException(typeof(AggregateException))]
        public void FlushToPersistent_WhenStorageThrows_ThenExceptionIsPropagated()
        {
            //Arrange
            var generatorSetup = new GeneratorSetup();
            TemporaryStorage target = generatorSetup.GetGeneratorServices().TemporaryStorage;
            var entityContext = new EntityContext() { 
                Type = typeof(Post),
                Description = new EntityDescription() { Type = typeof(Post) }
            };
            var flushRange = new FlushRange(0, 1);
            var storages = new List<IPersistentStorage> { new ThrowingStorage() };
            target.InsertToTemporary(entityContext, new List<Post>() { new Post() });

            //Act
            target.FlushToPersistent(entityContext, flushRange, storages);
            target.WaitAllTasks();
        }


        //helper classes
        class ThrowingStorage : IPersistentStorage
        {
            public async Task Insert<TEntity>(List<TEntity> entities) 
                where TEntity : class
            {
                throw new NotImplementedException();
            }

            public void Dispose(){}

            public void Setup(){}
        }
    }
}
