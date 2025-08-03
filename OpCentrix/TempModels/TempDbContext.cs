using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ProductionStage> ProductionStages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionStage>(entity =>
        {
            entity.HasIndex(e => e.Department, "IX_ProductionStages_Department");

            entity.HasIndex(e => e.DisplayOrder, "IX_ProductionStages_DisplayOrder");

            entity.HasIndex(e => new { e.DisplayOrder, e.IsActive }, "IX_ProductionStages_DisplayOrder_IsActive");

            entity.HasIndex(e => e.IsActive, "IX_ProductionStages_IsActive");

            entity.HasIndex(e => e.Name, "IX_ProductionStages_Name");

            entity.HasIndex(e => e.RequiredRole, "IX_ProductionStages_RequiredRole");

            entity.HasIndex(e => e.RequiresMachineAssignment, "IX_ProductionStages_RequiresMachineAssignment");

            entity.HasIndex(e => e.StageColor, "IX_ProductionStages_StageColor");

            entity.Property(e => e.CreatedBy).HasDefaultValue("System");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.CustomFieldsConfig).HasDefaultValue("[]");
            entity.Property(e => e.DefaultDurationHours).HasDefaultValue(1.0);
            entity.Property(e => e.DefaultHourlyRate)
                .HasDefaultValueSql("'85.0'")
                .HasColumnType("decimal(8,2)");
            entity.Property(e => e.DefaultMaterialCost)
                .HasDefaultValueSql("'0.0'")
                .HasColumnType("decimal(10,2)");
            entity.Property(e => e.DefaultSetupMinutes).HasDefaultValue(30);
            entity.Property(e => e.IsActive).HasDefaultValue(1);
            entity.Property(e => e.LastModifiedBy).HasDefaultValue("System");
            entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
            entity.Property(e => e.RequiresQualityCheck).HasDefaultValue(1);
            entity.Property(e => e.StageColor).HasDefaultValue("#007bff");
            entity.Property(e => e.StageIcon).HasDefaultValue("fas fa-cogs");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
