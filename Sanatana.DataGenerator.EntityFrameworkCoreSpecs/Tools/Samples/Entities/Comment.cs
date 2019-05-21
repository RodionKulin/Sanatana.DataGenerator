using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities
{
    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string CommentText { get; set; }
        public string MarkerText { get; set; }
    }
}
