using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGeneratorSpecs.TestTools.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;
using Sanatana.DataGenerator.Internals.Commands;

namespace Sanatana.DataGeneratorSpecs.Supervisors
{
    [TestClass]
    public class CompleteFlushCandidatesRegistrySpecs
    {
        [TestMethod]
        public void GetNextFlushCommands_WhenFlushCandidatesGetReleased_ThenReturnsExpectedFlushCommands()
        {
            //Arrange
            Dictionary<Type, IEntityDescription> entityDescriptions = EntityDescriptionProvider.GetAllEntities(targetCount: 100);
            var provider = new CompleteSupervisorProvider(entityDescriptions);
            provider.SetEntityCurrentCount(new Dictionary<Type, long>
            {
                { typeof(Category), 100 },  //flush candidate 1
                { typeof(Post), 100 },      //flush candidate 2
                { typeof(Comment), 98 },    //child that prevents flush candidates from flushing
            });
            provider.SetEntityRequestCapacity(100);
            provider.UpdateFlushCandidates();

            var target = (CompleteFlushCandidatesRegistry)provider.FlushCandidatesRegistry;
            List<ICommand> catCommands = target.GetNextFlushCommands(provider.EntityContexts[typeof(Category)]);
            List<ICommand> postCommands = target.GetNextFlushCommands(provider.EntityContexts[typeof(Post)]);

            //simulate that remaining 2 child instances were generated, also reaching 100 out of 100
            provider.EntityContexts[typeof(Comment)].EntityProgress.CurrentCount = 100;
            provider.UpdateFlushCandidates(typeof(Comment));

            //Act
            List<ICommand> flushCommands = target.GetNextFlushCommands(provider.EntityContexts[typeof(Comment)]);

            //Assert
            //parent can not be flushed before all child entities generated
            catCommands.Should().BeEmpty();
            postCommands.Should().BeEmpty();

            //parents can now be flushed when all child entities generated
            flushCommands.Should().AllBeOfType<FlushCommand>();
            IEnumerable<Type> actualFlushTypes = flushCommands
                .Select(x => ((FlushCommand)x).EntityContext.Description.Type);
            actualFlushTypes.Should().BeEquivalentTo(new Type[]
            {
                typeof(Comment),
                typeof(Post),
                typeof(Category)
            });
        }


        [TestMethod]
        public void AddOnFlushCandidatesHashSet_WhenAddedDuplicates_ThenKeepsUniqueEntitiesOnly()
        {
            //Arrange
            var flushCandidates = new HashSet<EntityContext>();
            Dictionary<Type, IEntityDescription> entityDescriptions = EntityDescriptionProvider.GetAllEntities(targetCount: 100);
            Dictionary<Type, EntityContext> entityContexts = CompleteSupervisorProvider.ToEntityContexts(entityDescriptions);
            EntityContext categoryContext = entityContexts[typeof(Category)];
            flushCandidates.Add(categoryContext);

            //Act
            flushCandidates.Add(categoryContext);

            //Assert
            flushCandidates.Count.Should().Be(1);
        }

    }
}
