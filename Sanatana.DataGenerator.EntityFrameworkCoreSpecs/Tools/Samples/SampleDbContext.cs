using Microsoft.EntityFrameworkCore;
using Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sanatana.DataGenerator.EntityFrameworkCoreSpecs.Tools.Samples
{
    public class SampleDbContext : DbContext
    {
        //tables
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }


        //init
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Data Source=.\\;Initial Catalog=TestDataGenerator-SampleDb;integrated security=true;MultipleActiveResultSets=True;");
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().HasKey(x => x.Id);
        }
    }
}
