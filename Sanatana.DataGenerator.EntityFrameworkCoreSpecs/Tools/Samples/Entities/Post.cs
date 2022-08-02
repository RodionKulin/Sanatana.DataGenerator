using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities
{
    public class Post
    {
        //by default Primary Key will have DatabaseGeneratedOption.Identity so this attribute not required
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string MarkerText { get; set; }


        public List<Comment> Comments { get; set; }
    }
}
