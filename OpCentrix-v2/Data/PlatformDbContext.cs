using Microsoft.EntityFrameworkCore;
using OpCentrix.Models.Platform;

namespace OpCentrix.Data;

public class PlatformDbContext : DbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<PlatformUser> PlatformUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<PlatformUser>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }
}
