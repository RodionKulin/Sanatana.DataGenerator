using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.AutoBogusSpecs.Samples
{
    public class Post
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string PostText { get; set; }
        public string ValueToIgnore { get; set; }
        public int CommentId { get; set; }
        public DateTime CreatedDate { get; set; }


        //foreign keys
        public Comment Comment { get; set; }
    }
}
