//SchedulerContext.cs:
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;

namespace OpCentrix.Data
{
    public class SchedulerContext : DbContext
    {
        public SchedulerContext(DbContextOptions<SchedulerContext> options)
            : base(options)
        {
        }

        // Core scheduling tables
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Part> Parts { get; set; }
        public DbSet<JobLogEntry> JobLogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Job Configuration
            
            modelBuilder.Entity<Job>(entity =>
            {
                // Primary key
                entity.HasKey(e => e.Id);
                
                // Indexes for performance
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.ScheduledStart);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => new { e.MachineId, e.ScheduledStart });
                
                // Relationships
                entity.HasOne(e => e.Part)
                      .WithMany(p => p.Jobs)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                // Constraints
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.PartNumber).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Operator).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Status).HasDefaultValue("Scheduled");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.ProcessParameters).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasDefaultValue("{}");
            });
            
            #endregion

            #region Part Configuration
            
            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.PartNumber).IsUnique();
                entity.HasIndex(e => e.Material);
                entity.HasIndex(e => e.IsActive);
                
                // Constraints
                entity.Property(e => e.PartNumber).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100);
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ProcessParameters).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasDefaultValue("{}");
            });
            
            #endregion

            #region JobLogEntry Configuration
            
            modelBuilder.Entity<JobLogEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.Action);
                
                // Constraints
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.PartNumber).HasMaxLength(100);
                entity.Property(e => e.Action).HasMaxLength(50);
                entity.Property(e => e.Operator).HasMaxLength(100);
            });
            
            #endregion
        }
    }
}
