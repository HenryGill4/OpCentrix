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
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<JobLogEntry> JobLogEntries { get; set; }
        public DbSet<SlsMachine> SlsMachines { get; set; }

        // NEW: Admin Control System tables (Task 2)
        public DbSet<OperatingShift> OperatingShifts { get; set; }
        public DbSet<MachineCapability> MachineCapabilities { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<InspectionCheckpoint> InspectionCheckpoints { get; set; }
        public DbSet<DefectCategory> DefectCategories { get; set; }
        public DbSet<ArchivedJob> ArchivedJobs { get; set; }
        public DbSet<AdminAlert> AdminAlerts { get; set; }
        public DbSet<FeatureToggle> FeatureToggles { get; set; }

        // NEW: Print tracking tables for the refactored workflow
        public DbSet<BuildJob> BuildJobs { get; set; }
        public DbSet<BuildJobPart> BuildJobParts { get; set; }
        public DbSet<DelayLog> DelayLogs { get; set; }

        // NEW: Department-specific operations tables - TEMPORARILY COMMENTED OUT
        // public DbSet<CoatingOperation> CoatingOperations { get; set; }
        // public DbSet<EDMOperation> EDMOperations { get; set; }
        // public DbSet<MachiningOperation> MachiningOperations { get; set; }
        // public DbSet<QualityInspection> QualityInspections { get; set; }
        // public DbSet<ShippingOperation> ShippingOperations { get; set; }
        // public DbSet<ShippingItem> ShippingItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Enhanced SQLite configuration for better reliability
                optionsBuilder.UseSqlite(connectionString =>
                {
                    connectionString.CommandTimeout(30); // 30 second timeout
                });

                // Additional SQLite optimizations
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Job Configuration Enhanced for SLS

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
                entity.HasIndex(e => e.SlsMaterial); // SLS-specific index
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.CustomerOrderNumber);
                entity.HasIndex(e => e.OpcUaJobId);

                // Relationships with proper error handling
                entity.HasOne(e => e.Part)
                      .WithMany(p => p.Jobs)
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Restrict);

                // String length constraints with safe defaults
                entity.Property(e => e.MachineId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Scheduled");
                entity.Property(e => e.Operator).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.SlsMaterial).HasMaxLength(100).HasDefaultValue("Ti-6Al-4V Grade 5");
                entity.Property(e => e.PowderLotNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.BuildPlatformId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.RequiredSkills).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.RequiredTooling).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.RequiredMaterials).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.SpecialInstructions).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.ProcessParameters).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.HoldReason).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.OpcUaJobId).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.OpcUaStatus).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.OpcUaErrorMessages).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BuildFileName).HasMaxLength(255).HasDefaultValue("");
                entity.Property(e => e.BuildFilePath).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.QualityInspector).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Supervisor).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Notes).HasMaxLength(1000).HasDefaultValue("");

                // Default values with safe fallbacks
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.RequiredSkills).HasDefaultValue("SLS Operation,Powder Handling,Inert Gas Safety");
                entity.Property(e => e.RequiredTooling).HasDefaultValue("Build Platform,Powder Sieve,Inert Gas Setup");
                entity.Property(e => e.ArgonPurityPercent).HasDefaultValue(99.9);
                entity.Property(e => e.OxygenContentPpm).HasDefaultValue(50);
                entity.Property(e => e.BuildTemperatureCelsius).HasDefaultValue(180);
                entity.Property(e => e.LaserPowerWatts).HasDefaultValue(200);
                entity.Property(e => e.ScanSpeedMmPerSec).HasDefaultValue(1200);
                entity.Property(e => e.LayerThicknessMicrons).HasDefaultValue(30);
                entity.Property(e => e.HatchSpacingMicrons).HasDefaultValue(120);
                entity.Property(e => e.PowderRecyclePercentage).HasDefaultValue(85);
                entity.Property(e => e.PreheatingTimeMinutes).HasDefaultValue(60);
                entity.Property(e => e.CoolingTimeMinutes).HasDefaultValue(240);
                entity.Property(e => e.PowderChangeoverTimeMinutes).HasDefaultValue(30);
                entity.Property(e => e.RequiresArgonPurge).HasDefaultValue(true);
                entity.Property(e => e.RequiresPreheating).HasDefaultValue(true);
                entity.Property(e => e.RequiresPowderSieving).HasDefaultValue(true);
                entity.Property(e => e.DensityPercentage).HasDefaultValue(99.5);

                // Decimal precision
                entity.Property(e => e.LaborCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.MaterialCostPerKg).HasPrecision(10, 2);
                entity.Property(e => e.MachineOperatingCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.ArgonCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.PowerCostPerKwh).HasPrecision(10, 2);
            });

            #endregion

            #region Part Configuration Enhanced for SLS

            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.PartNumber).IsUnique();
                entity.HasIndex(e => e.Material);
                entity.HasIndex(e => e.SlsMaterial);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.PartCategory);
                entity.HasIndex(e => e.PartClass);
                entity.HasIndex(e => e.Industry);
                entity.HasIndex(e => e.ProcessType);
                entity.HasIndex(e => e.RequiredMachineType);

                // String length constraints with safe defaults
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Description).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.Material).HasMaxLength(100).HasDefaultValue("Ti-6Al-4V Grade 5");
                entity.Property(e => e.SlsMaterial).HasMaxLength(100).HasDefaultValue("Ti-6Al-4V Grade 5");
                entity.Property(e => e.PowderSpecification).HasMaxLength(100).HasDefaultValue("15-45 μm particle size");
                entity.Property(e => e.AvgDuration).HasMaxLength(50).HasDefaultValue("8h");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.ProcessParameters).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasDefaultValue("{}");
                entity.Property(e => e.ProcessType).HasDefaultValue("SLS Metal");
                entity.Property(e => e.RequiredMachineType).HasDefaultValue("TruPrint 3000");
                entity.Property(e => e.PreferredMachines).HasDefaultValue("TI1,TI2");
                entity.Property(e => e.EstimatedHours).HasDefaultValue(8.0);

                // Decimal precision
                entity.Property(e => e.MaterialCostPerKg).HasPrecision(12, 2);
                entity.Property(e => e.StandardLaborCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.SetupCost).HasPrecision(10, 2);
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

            #region User Configuration

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
                entity.HasIndex(e => e.IsActive);

                // Constraints
                entity.Property(e => e.Username).HasMaxLength(50);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Role).HasMaxLength(50);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(50);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(50);

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Relationship to UserSettings
                entity.HasOne(e => e.Settings)
                      .WithOne(s => s.User)
                      .HasForeignKey<UserSettings>(s => s.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            #endregion

            #region UserSettings Configuration

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.UserId).IsUnique();

                // Constraints
                entity.Property(e => e.Theme).HasMaxLength(20);
                entity.Property(e => e.DefaultPage).HasMaxLength(100);
                entity.Property(e => e.TimeZone).HasMaxLength(50);

                // Default values
                entity.Property(e => e.SessionTimeoutMinutes).HasDefaultValue(120);
                entity.Property(e => e.Theme).HasDefaultValue("Light");
                entity.Property(e => e.EmailNotifications).HasDefaultValue(true);
                entity.Property(e => e.BrowserNotifications).HasDefaultValue(true);
                entity.Property(e => e.DefaultPage).HasDefaultValue("/Scheduler");
                entity.Property(e => e.ItemsPerPage).HasDefaultValue(20);
                entity.Property(e => e.TimeZone).HasDefaultValue("UTC");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
            });

            #endregion

            #region SlsMachine Configuration

            modelBuilder.Entity<SlsMachine>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.MachineId).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsAvailableForScheduling);
                entity.HasIndex(e => e.CurrentMaterial);
                entity.HasIndex(e => e.LastStatusUpdate);

                // Relationships
                entity.HasOne(e => e.CurrentJob)
                      .WithMany()
                      .HasForeignKey(e => e.CurrentJobId)
                      .OnDelete(DeleteBehavior.SetNull);

                // String length constraints
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.MachineName).HasMaxLength(100);
                entity.Property(e => e.MachineModel).HasMaxLength(100);
                entity.Property(e => e.SerialNumber).HasMaxLength(50);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.SupportedMaterials).HasMaxLength(500);
                entity.Property(e => e.CurrentMaterial).HasMaxLength(100);
                entity.Property(e => e.OpcUaEndpointUrl).HasMaxLength(200);
                entity.Property(e => e.OpcUaUsername).HasMaxLength(100);
                entity.Property(e => e.OpcUaPasswordHash).HasMaxLength(100);
                entity.Property(e => e.OpcUaNamespace).HasMaxLength(100);
                entity.Property(e => e.OpcUaConnectionStatus).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.StatusMessage).HasMaxLength(500);
                entity.Property(e => e.MaintenanceNotes).HasMaxLength(1000);
                entity.Property(e => e.OperatorNotes).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100);

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastStatusUpdate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.MachineModel).HasDefaultValue("TruPrint 3000");
                entity.Property(e => e.BuildLengthMm).HasDefaultValue(250);
                entity.Property(e => e.BuildWidthMm).HasDefaultValue(250);
                entity.Property(e => e.BuildHeightMm).HasDefaultValue(300);
                entity.Property(e => e.SupportedMaterials).HasDefaultValue("Ti-6Al-4V Grade 5,Ti-6Al-4V ELI Grade 23");
                entity.Property(e => e.MaxLaserPowerWatts).HasDefaultValue(400);
                entity.Property(e => e.MaxScanSpeedMmPerSec).HasDefaultValue(7000);
                entity.Property(e => e.MinLayerThicknessMicrons).HasDefaultValue(20);
                entity.Property(e => e.MaxLayerThicknessMicrons).HasDefaultValue(60);
                entity.Property(e => e.Status).HasDefaultValue("Offline");
                entity.Property(e => e.OpcUaConnectionStatus).HasDefaultValue("Disconnected");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsAvailableForScheduling).HasDefaultValue(true);
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.MaintenanceIntervalHours).HasDefaultValue(500);
                entity.Property(e => e.QualityScorePercent).HasDefaultValue(100);
            });

            #endregion

            #region MachineDataSnapshot Configuration

            modelBuilder.Entity<MachineDataSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => new { e.MachineId, e.Timestamp });

                // Relationships
                entity.HasOne(e => e.SlsMachine)
                      .WithMany()
                      .HasForeignKey(e => e.SlsMachineId)
                      .OnDelete(DeleteBehavior.Cascade);

                // String length constraints
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.ProcessDataJson).HasMaxLength(2000);
                entity.Property(e => e.QualityDataJson).HasMaxLength(2000);
                entity.Property(e => e.AlarmDataJson).HasMaxLength(2000);

                // Default values
                entity.Property(e => e.Timestamp).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.ProcessDataJson).HasDefaultValue("{}");
                entity.Property(e => e.QualityDataJson).HasDefaultValue("{}");
                entity.Property(e => e.AlarmDataJson).HasDefaultValue("{}");
            });

            #endregion

            #region BuildJob Configuration (NEW - Print Tracking System)

            modelBuilder.Entity<BuildJob>(entity =>
            {
                entity.HasKey(e => e.BuildId);

                // Indexes for performance
                entity.HasIndex(e => e.PrinterName);
                entity.HasIndex(e => e.ActualStartTime);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.PrinterName, e.ActualStartTime });
                entity.HasIndex(e => e.AssociatedScheduledJobId);

                // Relationships
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // String length constraints
                entity.Property(e => e.PrinterName).HasMaxLength(10);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.LaserRunTime).HasMaxLength(50);
                entity.Property(e => e.ReasonForEnd).HasMaxLength(50);
                entity.Property(e => e.SetupNotes).HasMaxLength(1000);

                // Default values
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Status).HasDefaultValue("In Progress");
            });

            #endregion

            #region BuildJobPart Configuration (NEW - Print Tracking System)

            modelBuilder.Entity<BuildJobPart>(entity =>
            {
                entity.HasKey(e => e.PartEntryId);

                // Indexes
                entity.HasIndex(e => e.BuildId);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.IsPrimary);
                entity.HasIndex(e => new { e.BuildId, e.IsPrimary });

                // Relationships
                entity.HasOne(e => e.BuildJob)
                      .WithMany()
                      .HasForeignKey(e => e.BuildId)
                      .OnDelete(DeleteBehavior.Cascade);

                // String length constraints
                entity.Property(e => e.PartNumber).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Material).HasMaxLength(100);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                // Default values
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            });

            #endregion

            #region DelayLog Configuration (NEW - Print Tracking System)

            modelBuilder.Entity<DelayLog>(entity =>
            {
                entity.HasKey(e => e.DelayId);

                // Indexes
                entity.HasIndex(e => e.BuildId);
                entity.HasIndex(e => e.DelayReason);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.DelayDuration);

                // Relationships
                entity.HasOne(e => e.BuildJob)
                      .WithMany()
                      .HasForeignKey(e => e.BuildId)
                      .OnDelete(DeleteBehavior.Cascade);

                // String length constraints
                entity.Property(e => e.DelayReason).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);

                // Default values
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
            });

            #endregion

            #region OperatingShift Configuration (NEW - Task 2)

            modelBuilder.Entity<OperatingShift>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.DayOfWeek);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsHoliday);
                entity.HasIndex(e => e.SpecificDate);
                entity.HasIndex(e => new { e.DayOfWeek, e.IsActive });

                // Constraints
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsHoliday).HasDefaultValue(false);
            });

            #endregion

            #region MachineCapability Configuration (NEW - Task 2)

            modelBuilder.Entity<MachineCapability>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.SlsMachineId);
                entity.HasIndex(e => e.CapabilityType);
                entity.HasIndex(e => e.IsAvailable);
                entity.HasIndex(e => new { e.SlsMachineId, e.CapabilityType });

                // Relationships
                entity.HasOne(e => e.SlsMachine)
                      .WithMany()
                      .HasForeignKey(e => e.SlsMachineId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Constraints
                entity.Property(e => e.CapabilityType).HasMaxLength(50);
                entity.Property(e => e.CapabilityName).HasMaxLength(100);
                entity.Property(e => e.CapabilityValue).HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.RequiredCertification).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
                entity.Property(e => e.Priority).HasDefaultValue(3);
            });

            #endregion

            #region SystemSetting Configuration (NEW - Task 2)

            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.SettingKey).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.Category, e.DisplayOrder });

                // Constraints
                entity.Property(e => e.SettingKey).HasMaxLength(100);
                entity.Property(e => e.SettingValue).HasMaxLength(2000);
                entity.Property(e => e.DataType).HasMaxLength(20).HasDefaultValue("String");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DefaultValue).HasMaxLength(500);
                entity.Property(e => e.ValidationRules).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsReadOnly).HasDefaultValue(false);
                entity.Property(e => e.RequiresRestart).HasDefaultValue(false);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(100);
            });

            #endregion

            #region RolePermission Configuration (NEW - Task 2)

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.RoleName);
                entity.HasIndex(e => e.PermissionKey);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.RoleName, e.PermissionKey }).IsUnique();

                // Constraints
                entity.Property(e => e.RoleName).HasMaxLength(50);
                entity.Property(e => e.PermissionKey).HasMaxLength(100);
                entity.Property(e => e.PermissionLevel).HasMaxLength(20).HasDefaultValue("Read");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Constraints).HasMaxLength(1000).HasDefaultValue("{}");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.HasPermission).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.Priority).HasDefaultValue(100);
            });

            #endregion

            #region InspectionCheckpoint Configuration (NEW - Task 2)

            modelBuilder.Entity<InspectionCheckpoint>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.InspectionType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsRequired);
                entity.HasIndex(e => new { e.PartId, e.SortOrder });

                // Relationships
                entity.HasOne(e => e.Part)
                      .WithMany()
                      .HasForeignKey(e => e.PartId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Constraints
                entity.Property(e => e.CheckpointName).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.InspectionType).HasMaxLength(50).HasDefaultValue("Visual");
                entity.Property(e => e.AcceptanceCriteria).HasMaxLength(500);
                entity.Property(e => e.MeasurementMethod).HasMaxLength(500);
                entity.Property(e => e.RequiredEquipment).HasMaxLength(500);
                entity.Property(e => e.RequiredSkills).HasMaxLength(500);
                entity.Property(e => e.ReferenceDocuments).HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.FailureAction).HasMaxLength(500).HasDefaultValue("Hold for review");
                entity.Property(e => e.SamplingMethod).HasMaxLength(50).HasDefaultValue("All");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("Quality");
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
                entity.Property(e => e.IsRequired).HasDefaultValue(true);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.EstimatedMinutes).HasDefaultValue(5);
                entity.Property(e => e.SampleSize).HasDefaultValue(1);
                entity.Property(e => e.Priority).HasDefaultValue(3);
            });

            #endregion

            #region DefectCategory Configuration (NEW - Task 2)

            modelBuilder.Entity<DefectCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Code);
                entity.HasIndex(e => e.CategoryGroup);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SeverityLevel);

                // Constraints
                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.CategoryGroup).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.ApplicableProcesses).HasMaxLength(200);
                entity.Property(e => e.StandardCorrectiveActions).HasMaxLength(1000);
                entity.Property(e => e.PreventionMethods).HasMaxLength(1000);
                entity.Property(e => e.CostImpact).HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.ColorCode).HasMaxLength(7).HasDefaultValue("#6B7280");
                entity.Property(e => e.Icon).HasMaxLength(50).HasDefaultValue("exclamation-triangle");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.SeverityLevel).HasDefaultValue(3);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.RequiresImmediateNotification).HasDefaultValue(false);
                entity.Property(e => e.AverageResolutionTimeMinutes).HasDefaultValue(30);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
            });

            #endregion

            #region ArchivedJob Configuration (NEW - Task 2)

            modelBuilder.Entity<ArchivedJob>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.OriginalJobId);
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ArchivedDate);
                entity.HasIndex(e => e.ArchivedBy);
                entity.HasIndex(e => new { e.MachineId, e.ScheduledStart });

                // Constraints
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.PartNumber).HasMaxLength(50);
                entity.Property(e => e.PartDescription).HasMaxLength(500);
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Completed");
                entity.Property(e => e.SlsMaterial).HasMaxLength(100);
                entity.Property(e => e.PowderLotNumber).HasMaxLength(50);
                entity.Property(e => e.Operator).HasMaxLength(100);
                entity.Property(e => e.QualityInspector).HasMaxLength(100);
                entity.Property(e => e.Supervisor).HasMaxLength(100);
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.HoldReason).HasMaxLength(500);
                entity.Property(e => e.ProcessParameters).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.ArchivedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.ArchiveReason).HasMaxLength(500).HasDefaultValue("Cleanup");
                entity.Property(e => e.OriginalCreatedBy).HasMaxLength(100);
                entity.Property(e => e.OriginalLastModifiedBy).HasMaxLength(100);

                // Default values
                entity.Property(e => e.ArchivedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.Quantity).HasDefaultValue(1);

                // Decimal precision
                entity.Property(e => e.LaborCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.MaterialCostPerKg).HasPrecision(10, 2);
                entity.Property(e => e.MachineOperatingCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.ArgonCostPerHour).HasPrecision(10, 2);
            });

            #endregion

            #region AdminAlert Configuration (NEW - Task 2)

            modelBuilder.Entity<AdminAlert>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.AlertName);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.TriggerType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SeverityLevel);
                entity.HasIndex(e => e.LastTriggered);

                // Constraints
                entity.Property(e => e.AlertName).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("System");
                entity.Property(e => e.TriggerType).HasMaxLength(100);
                entity.Property(e => e.TriggerConditions).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.EmailRecipients).HasMaxLength(1000);
                entity.Property(e => e.EmailSubject).HasMaxLength(200).HasDefaultValue("OpCentrix Alert: {AlertName}");
                entity.Property(e => e.EmailTemplate).HasMaxLength(2000);
                entity.Property(e => e.SmsRecipients).HasMaxLength(500);
                entity.Property(e => e.SmsTemplate).HasMaxLength(160);
                entity.Property(e => e.EscalationRules).HasMaxLength(1000).HasDefaultValue("{}");
                entity.Property(e => e.BusinessDays).HasMaxLength(20).HasDefaultValue("1,2,3,4,5");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.SeverityLevel).HasDefaultValue(3);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SendBrowserNotification).HasDefaultValue(true);
                entity.Property(e => e.SendSms).HasDefaultValue(false);
                entity.Property(e => e.CooldownMinutes).HasDefaultValue(15);
                entity.Property(e => e.BusinessHoursOnly).HasDefaultValue(false);
                entity.Property(e => e.MaxAlertsPerDay).HasDefaultValue(10);
                entity.Property(e => e.TriggerCount).HasDefaultValue(0);
                entity.Property(e => e.TriggersToday).HasDefaultValue(0);
                entity.Property(e => e.LastDailyReset).HasDefaultValueSql("date('now')");
            });

            #endregion

            #region FeatureToggle Configuration (NEW - Task 2)

            modelBuilder.Entity<FeatureToggle>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Indexes
                entity.HasIndex(e => e.FeatureName).IsUnique();
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsEnabled);
                entity.HasIndex(e => e.Environment);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.RequiredRole);

                // Constraints
                entity.Property(e => e.FeatureName).HasMaxLength(100);
                entity.Property(e => e.DisplayName).HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.Environment).HasMaxLength(20).HasDefaultValue("All");
                entity.Property(e => e.RequiredRole).HasMaxLength(50).HasDefaultValue("User");
                entity.Property(e => e.Dependencies).HasMaxLength(500);
                entity.Property(e => e.Conflicts).HasMaxLength(500);
                entity.Property(e => e.Configuration).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.PerformanceNotes).HasMaxLength(500);
                entity.Property(e => e.SecurityNotes).HasMaxLength(500);
                entity.Property(e => e.IntroducedInVersion).HasMaxLength(20);
                entity.Property(e => e.PlannedRemovalVersion).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Experimental");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");

                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsEnabled).HasDefaultValue(false);
                entity.Property(e => e.RolloutPercentage).HasDefaultValue(100);
                entity.Property(e => e.RequiresRestart).HasDefaultValue(false);
                entity.Property(e => e.UsageCount).HasDefaultValue(0);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
            });

            #endregion

            // NEW: Department-specific table configurations - TEMPORARILY COMMENTED OUT
            /*
            #region CoatingOperation Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<CoatingOperation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Operator);
                entity.HasIndex(e => e.CoatingType);
                
                // String length constraints with defaults
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.CoatingType).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CoatingSpecification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Color).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Thickness).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Operator).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.QualityInspector).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ProcessNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.QualityNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.RejectionReason).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.ProcessTemperature).HasDefaultValue(20.0);
                entity.Property(e => e.Humidity).HasDefaultValue(50.0);
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                
                // Decimal precision
                entity.Property(e => e.MaterialCost).HasPrecision(10, 2);
                entity.Property(e => e.LaborCost).HasPrecision(10, 2);
                entity.Property(e => e.OverheadCost).HasPrecision(10, 2);
            });
            
            #endregion
            
            #region EDMOperation Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<EDMOperation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Operator);
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.EDMType);
                
                // String length constraints with defaults
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.WorkPieceDescription).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Material).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.EDMType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.MachineId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Setup");
                entity.Property(e => e.Operator).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.WireType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.DielectricFluid).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.QualityNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.ProcessNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.HoldReason).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.FluidTemperature).HasDefaultValue(20.0);
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                
                // Decimal precision
                entity.Property(e => e.WireCost).HasPrecision(10, 2);
                entity.Property(e => e.ElectricityCost).HasPrecision(10, 2);
                entity.Property(e => e.LaborCost).HasPrecision(10, 2);
                entity.Property(e => e.MachineCost).HasPrecision(10, 2);
            });
            
            #endregion
            
            #region MachiningOperation Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<MachiningOperation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Operator);
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.WorkOrderNumber);
                
                // String length constraints with defaults
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.WorkOrderNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.MachineId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.MachineType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Operation).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Setup");
                entity.Property(e => e.Operator).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Programmer).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Material).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CuttingTool).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CoolantType).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ProgramNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ToolList).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.SetupNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.ProcessNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.QualityNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.InspectedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.HoldReason).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                
                // Decimal precision
                entity.Property(e => e.ToolingCost).HasPrecision(10, 2);
                entity.Property(e => e.LaborCost).HasPrecision(10, 2);
                entity.Property(e => e.MachineCost).HasPrecision(10, 2);
                entity.Property(e => e.MaterialCost).HasPrecision(10, 2);
            });
            
            #endregion
            
            #region QualityInspection Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<QualityInspection>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.InspectionNumber).IsUnique();
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledDate);
                entity.HasIndex(e => e.Inspector);
                entity.HasIndex(e => e.InspectionType);
                
                // String length constraints with defaults
                entity.Property(e => e.InspectionNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.InspectionType).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.Inspector).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.DimensionalRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.DimensionalResults).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.VisualRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.VisualResults).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.FunctionalRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.FunctionalResults).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.HardnessScale).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.CriticalDimensions).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.MeasuredDimensions).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.ToleranceAnalysis).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.TestProcedure).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.TestEquipment).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CalibrationDate).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.TestResults).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CertificationRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CertificateNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.NonConformanceDescription).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CorrectiveAction).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.DispositionCode).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.DispositionNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.InspectionNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CustomerRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.DrawingRevision).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.SpecificationRevision).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.TraceabilityInfo).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.QuantityToInspect).HasDefaultValue(1);
                entity.Property(e => e.Temperature).HasDefaultValue(20.0);
                entity.Property(e => e.Humidity).HasDefaultValue(50.0);
                entity.Property(e => e.Pressure).HasDefaultValue(1013.25);
                
                // Decimal precision
                entity.Property(e => e.InspectionCost).HasPrecision(10, 2);
                entity.Property(e => e.TestingCost).HasPrecision(10, 2);
                entity.Property(e => e.CertificationCost).HasPrecision(10, 2);
            });
            
            #endregion
            
            #region ShippingOperation Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<ShippingOperation>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.ShipmentNumber).IsUnique();
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledShipDate);
                entity.HasIndex(e => e.CustomerName);
                entity.HasIndex(e => e.CustomerOrderNumber);
                entity.HasIndex(e => e.TrackingNumber);
                
                // String length constraints with defaults
                entity.Property(e => e.ShipmentNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.CustomerName).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.ShippingMethod).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.TrackingNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Priority).HasMaxLength(50).HasDefaultValue("Standard");
                entity.Property(e => e.ShippingAddress).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.City).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.State).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ZipCode).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.Country).HasMaxLength(50).HasDefaultValue("USA");
                entity.Property(e => e.ContactPerson).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ContactPhone).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.ContactEmail).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.PackagingType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PackagingNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.SpecialInstructions).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.PackingSlipNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CustomsDocumentation).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.PackedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.InspectedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.QualityCheckNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                
                // Default values
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Decimal precision
                entity.Property(e => e.ShippingCost).HasPrecision(10, 2);
                entity.Property(e => e.InsuranceValue).HasPrecision(10, 2);
                entity.Property(e => e.CustomsDeclaredValue).HasPrecision(10, 2);
            });
            
            #endregion
            
            #region ShippingItem Configuration (NEW - Dept Ops)
            
            modelBuilder.Entity<ShippingItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Indexes
                entity.HasIndex(e => e.ShippingOperationId);
                entity.HasIndex(e => e.PartNumber);
                
                // Relationships
                entity.HasOne(e => e.ShippingOperation)
                      .WithMany(so => so.ShippingItems)
                      .HasForeignKey(e => e.ShippingOperationId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                // String length constraints with defaults
                entity.Property(e => e.PartNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Description).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.SerialNumbers).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Notes).HasMaxLength(500).HasDefaultValue("");
                
                // Default values
                entity.Property(e => e.Quantity).HasDefaultValue(1);
                
                // Decimal precision
                entity.Property(e => e.UnitValue).HasPrecision(10, 2);
            });
            
            #endregion
            */
        }

        /// <summary>
        /// Test database connectivity with proper error handling
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await Database.OpenConnectionAsync();
                await Database.CloseConnectionAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get database health information
        /// </summary>
        public async Task<(bool IsHealthy, string StatusMessage)> GetHealthStatusAsync()
        {
            try
            {
                var connectionTest = await TestConnectionAsync();
                if (!connectionTest)
                {
                    return (false, "Database connection failed");
                }

                var userCount = await Users.CountAsync();
                var partCount = await Parts.CountAsync();
                var jobCount = await Jobs.CountAsync();

                return (true, $"Healthy - {userCount} users, {partCount} parts, {jobCount} jobs");
            }
            catch (Exception ex)
            {
                return (false, $"Database error: {ex.Message}");
            }
        }
    }
}
