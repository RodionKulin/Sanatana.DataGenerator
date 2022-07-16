using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Demo.Entities
{
    public class RefundEvent
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        public bool Accepted { get; set; }
        public DateTime RefundTime { get; set; }


        //navigation properties
        public PurchaseOrder PurchaseOrder { get; set; }
    }
}
