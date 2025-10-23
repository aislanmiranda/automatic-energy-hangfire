using Microsoft.EntityFrameworkCore;
using service.Models;

namespace service.Repository.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }
        
        public DbSet<Equipament> Equipaments => Set<Equipament>();
        public DbSet<Monitoring> Monitorings => Set<Monitoring>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

