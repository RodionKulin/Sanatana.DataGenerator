using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.Demo.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.Demo.Database
{
    public class ProcurementDbContext : DbContext
    {
        public DbSet<Buyer> Buyers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<RefundEvent> RefundEvents { get; set; }

        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=.\\;Initial Catalog=TestDataGenerator-SampleDbDemo;integrated security=true;MultipleActiveResultSets=True;");

        }
    }
}
