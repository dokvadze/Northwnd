using Microsoft.EntityFrameworkCore;
using Northwnd.DAL.Models;

namespace Test.DAL
{
    public class NorthwndDbContext : DbContext
    {

        public NorthwndDbContext(DbContextOptions<NorthwndDbContext> options) : base(options)
        { 
        
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Region> Regions { get; set; }

    }
}