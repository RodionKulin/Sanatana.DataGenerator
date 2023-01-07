using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanatana.DataGenerator.Demo.Entities
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        public int BuyerId { get; set; }
        public bool Accepted { get; set; }
        public DateTime RegisteredTime { get; set; }


        //navigation properties
        public Supplier Supplier { get; set; }
        public Buyer Buyer { get; set; }
    }
}
