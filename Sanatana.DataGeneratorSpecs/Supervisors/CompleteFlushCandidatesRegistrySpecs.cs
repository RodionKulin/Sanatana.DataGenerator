using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGeneratorSpecs.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Internals.Progress;

namespace Sanatana.DataGeneratorSpecs.Supervisors
{
    [TestClass]
    public class CompleteFlushCandidatesRegistrySpecs
    {
        [TestMethod]
        public void GetFlushActions_ReturnsExpectedFlushActions()
        {
            //prepare
            Dictionary<Type, EntityContext> entityContexts = GetEntityContexts(new Dictionary<Type, long>
            {
                { typeof(Category), 100 },
                { typeof(Post), 100 },
                { typeof(Comment), 98 },
            }, 100);
            CompleteFlushCandidatesRegistry target = SetupCompleteFlushCandidatesRegistry(entityContexts);

            //simulate that parent instances were generated up to TargetCount 100 out of 100
            List<ICommand> catCommands = target.GetNextFlushCommands(entityContexts[typeof(Category)]);
            List<ICommand> postCommands = target.GetNextFlushCommands(entityContexts[typeof(Post)]);
            
            //simulate that remaining 2 child instances were generated, also reaching 100 out of 100
            entityContexts[typeof(Comment)].EntityProgress.CurrentCount = 100;
            target.UpdateRequestCapacity(entityContexts[typeof(Comment)]);
            target.UpdateFlushRequired(entityContexts[typeof(Comment)]);

            //invoke
            List<ICommand> flushCommands = target.GetNextFlushCommands(entityContexts[typeof(Comment)]);

            //assert
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
        public void FlushCandidatesHashSet_KeepsUniqueEntities()
        {
            //arrange
            var flushCandidates = new HashSet<EntityContext>();
            Dictionary<Type, EntityContext> entityContexts = GetEntityContexts(new Dictionary<Type, long>
            {
                { typeof(Category), 50 }
            }, 100);
            EntityContext categoryContext = entityContexts[typeof(Category)];
            flushCandidates.Add(categoryContext);
            categoryContext.EntityProgress.CurrentCount++;

            //act
            flushCandidates.Add(categoryContext);

            //assert
            flushCandidates.Count.Should().Be(1);
        }



        //Setup helpers
        private Dictionary<Type, EntityContext> GetEntityContexts(
            Dictionary<Type, long> entityCurrentCount, long targetCount)
        {

            //set TargetCount
            var category = new EntityDescription<Category>()
                .SetTargetCount(targetCount);
            var post = new EntityDescription<Post>()
                .SetTargetCount(targetCount)
                .SetRequired(typeof(Category));
            var comment = new EntityDescription<Comment>()
                .SetTargetCount(targetCount)
                .SetRequired(typeof(Post))
                .SetRequired(typeof(Category));
            Dictionary<Type, IEntityDescription> dictDescriptions = new List<IEntityDescription>
                { category, post, comment }
                .ToDictionary(x => x.Type, x => x);

            //set RequestCapacity
            var generatorSetup = new GeneratorSetup();
            Dictionary<Type, EntityContext> contexts = generatorSetup.SetupEntityContexts(dictDescriptions);
            foreach (EntityContext entityContext in contexts.Values)
            {
                FlushRange firstFlushRange = entityContext.EntityProgress.CreateNewRangeIfRequired();
                firstFlushRange.UpdateCapacity(100);
            }

            //set CurrentCount
            foreach (KeyValuePair<Type, long> entity in entityCurrentCount)
            {
                contexts[entity.Key].EntityProgress.CurrentCount = entity.Value;
            }

            return contexts;
        }

        private CompleteFlushCandidatesRegistry SetupCompleteFlushCandidatesRegistry(
            Dictionary<Type, EntityContext> contexts)
        {
            var generatorSetup = new GeneratorSetup();
            var progressState = new CompleteProgressState(contexts);
            var target = new CompleteFlushCandidatesRegistry(generatorSetup, contexts, progressState);

            //Add flush candidates
            foreach (Type entityType in contexts.Keys)
            {
                target.UpdateRequestCapacity(contexts[entityType]);
                target.UpdateFlushRequired(contexts[entityType]);
            }

            return target;
        }

    }
}
