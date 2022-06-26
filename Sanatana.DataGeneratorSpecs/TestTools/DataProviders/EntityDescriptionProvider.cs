using Sanatana.DataGenerator.Entities;
using Sanatana.DataGeneratorSpecs.TestTools.Samples;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanatana.DataGeneratorSpecs.TestTools.DataProviders
{
    internal class EntityDescriptionProvider
    {
        public static Dictionary<Type, IEntityDescription> GetAllEntities(long targetCount)
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


        public static Dictionary<Type, IEntityDescription> GetMixedRequiredOrderEntities(long targetCount)
        {
            var category = new EntityDescription<Category>()
                .SetTargetCount(targetCount)
                .SetGenerator(x => new Category());

            var post = new EntityDescription<Post>()
                .SetTargetCount(targetCount)
                .SetGenerator<Category>((x, category) => new Post());

            var comment = new EntityDescription<Comment>()
                .SetTargetCount(targetCount)
                .SetGenerator<Category, Post>((x, category, post) => new Comment()); //category goes before post in lambda

            return new List<IEntityDescription>
                { category, post, comment }
                .ToDictionary(x => x.Type, x => x);
        }
    }
}
