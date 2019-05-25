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
            CompleteFlushCandidatesRegistry target =
                SetupCompleteFlushCandidatesRegistry(entityContexts);

            //simulate that Comment generation is finished
            entityContexts[typeof(Comment)].EntityProgress.CurrentCount = 100;


            //invoke
            EntityContext entityContext = entityContexts[typeof(Comment)];
            List<ICommand> flushCommands = target.GetNextFlushCommands(entityContext);


            //assert
            flushCommands.Should().AllBeOfType<FlushCommand>();

            var actualFlushActions = flushCommands
                .Select(x => ((FlushCommand)x).EntityContext.Description.Type);
            var expectedActions = new Type[]
                {
                    typeof(Comment),
                    typeof(Post),
                    typeof(Category)
                };
            actualFlushActions.Should().BeEquivalentTo(expectedActions);
        }



        //Setup helpers
        private Dictionary<Type, EntityContext> GetEntityContexts(
            Dictionary<Type, long> entityCurrentCount, long targetCount)
        {
            var generatorSetup = new GeneratorSetup();

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
            {
                category,
                post,
                comment
            }.ToDictionary(x => x.Type, x => x);

            Dictionary<Type, EntityContext> contexts =
                generatorSetup.SetupEntityContexts(dictDescriptions);
            contexts.Values.ToList().ForEach(x =>
            {
                x.EntityProgress.AddFlushedCount(0);
                x.EntityProgress.NextFlushCount = 0;
            });

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
            var target = new CompleteFlushCandidatesRegistry(generatorSetup,
                contexts, progressState);

            //Add flush candidates
            target.CheckIsFlushRequired(contexts[typeof(Post)]);
            target.CheckIsFlushRequired(contexts[typeof(Category)]);
            target.CheckIsFlushRequired(contexts[typeof(Comment)]);

            return target;
        }

    }
}
