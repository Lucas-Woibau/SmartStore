using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmarthStore.Models;
using SmartStore.Models;

namespace SmarthStore.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base (options)
        {
            
        }

        public DbSet<Product> Products { get; set; }
    }
}
