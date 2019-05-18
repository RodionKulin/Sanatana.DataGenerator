using AutoBogus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.AutoBogusSpecs.Samples
{
    public class PostOverride : AutoGeneratorOverride
    {
        public override bool CanOverride(AutoGenerateContext context)
        {
            return context.GenerateType == typeof(Post);
        }

        public override void Generate(AutoGenerateOverrideContext context)
        {
            var post = context.Instance as Post;
            post.CategoryId = 5;
            post.PostText = context.Faker.Lorem.Lines();
        }
    }
}
