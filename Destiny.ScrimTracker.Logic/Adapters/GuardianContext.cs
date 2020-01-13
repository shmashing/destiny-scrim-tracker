using Destiny.ScrimTracker.Logic.Models;
using Microsoft.EntityFrameworkCore;

namespace Destiny.ScrimTracker.Logic.Adapters
{
    public class GuardianContext : DbContext
    {
        public DbSet<Guardian> Guardians { get; set; }
        
        public GuardianContext(DbContextOptions<GuardianContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guardian>().HasIndex(g => g.GamerTag).IsUnique();
        }
    }
}