using Microsoft.EntityFrameworkCore;
using SmarthStore.Models;

namespace SmarthStore.Services
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base (options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
    }
}
