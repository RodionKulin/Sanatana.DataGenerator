using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGeneratorSpecs.TestTools.Entities
{
    public class Attachment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string FileUrl { get; set; }
    }
}
