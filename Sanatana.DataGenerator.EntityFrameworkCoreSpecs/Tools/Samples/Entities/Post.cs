using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string MarkerText { get; set; }
    }
}
