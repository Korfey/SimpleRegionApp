using Microsoft.EntityFrameworkCore;
using SimpleRegionApp.Models;

namespace SimpleRegionApp.API.Data
{
    public class SimpleDbContext(DbContextOptions<SimpleDbContext> options) : DbContext(options)
    {
        public DbSet<Metadata> Images { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    } 

}
