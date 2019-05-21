using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples
{
    public class SampleDbContext : DbContext
    {
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            optionsBuilder.UseSqlServer($@"Data Source=(LocalDb)\MSSQLLocalDB;
                 AttachDbFilename={basePath}GeneratorEfCoreContext-20190501.mdf;
                 Initial Catalog=GeneratorEfCoreContext-20190501;
                 Integrated Security=True");
        }
    }
}
