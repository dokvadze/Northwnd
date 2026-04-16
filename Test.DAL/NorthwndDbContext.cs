using Microsoft.EntityFrameworkCore;
using Northwnd.DAL.Models;

namespace Test.DAL
{
    public class NorthwndDbContext : DbContext
    {
        public NorthwndDbContext() { }

        public NorthwndDbContext(DbContextOptions<NorthwndDbContext> options) : base(options)
        { 
        
        }

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Region> Regions { get; set; }

    }
}