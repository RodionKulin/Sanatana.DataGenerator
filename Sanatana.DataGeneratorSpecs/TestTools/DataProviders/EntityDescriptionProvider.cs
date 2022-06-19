using Sanatana.DataGenerator;
using Sanatana.DataGenerator.Commands;
using Sanatana.DataGenerator.Entities;
using Sanatana.DataGenerator.Supervisors.Complete;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanatana.DataGenerator.Internals.Progress;
using Sanatana.DataGenerator.Internals.EntitySettings;
using Sanatana.DataGenerator.Internals;
using Sanatana.DataGeneratorSpecs.TestTools.DataProviders;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    internal class EntityDescriptionProvider
    {
        public static Dictionary<Type, IEntityDescription> GetAllEntityContexts(long targetCount)
        {
            var category = new EntityDescription<Category>()
                .SetTargetCount(targetCount);

            var post = new EntityDescription<Post>()
                .SetTargetCount(targetCount)
                .SetRequired(typeof(Category));

            var comment = new EntityDescription<Comment>()
                .SetTargetCount(targetCount)
                .SetRequired(typeof(Post))
                .SetRequired(typeof(Category));

            return new List<IEntityDescription>
                { category, post, comment }
                .ToDictionary(x => x.Type, x => x);
        }

    }
}
