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
        public DbSet<Machine> Machines { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<JobLogEntry> JobLogEntries { get; set; }

        // Task 9: Enhanced scheduler features
        public DbSet<JobNote> JobNotes { get; set; }

        // Task 11: Multi-stage manufacturing entities
        public DbSet<JobStage> JobStages { get; set; }
        public DbSet<StageDependency> StageDependencies { get; set; }
        public DbSet<StageNote> StageNotes { get; set; }

        // Enhanced admin entities from Task 2
        public DbSet<OperatingShift> OperatingShifts { get; set; }
        public DbSet<MachineCapability> MachineCapabilities { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<InspectionCheckpoint> InspectionCheckpoints { get; set; }
        public DbSet<DefectCategory> DefectCategories { get; set; }
        public DbSet<ArchivedJob> ArchivedJobs { get; set; }
        public DbSet<AdminAlert> AdminAlerts { get; set; }
        public DbSet<FeatureToggle> FeatureToggles { get; set; }

        // Print tracking tables
        public DbSet<BuildJob> BuildJobs { get; set; }
        public DbSet<BuildJobPart> BuildJobParts { get; set; }
        public DbSet<DelayLog> DelayLogs { get; set; }

        // Task 6: Enhanced machine management
        public DbSet<Material> Materials { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite(connectionString =>
                {
                    connectionString.CommandTimeout(30);
                });

                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MachineId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).HasDefaultValue(3);
                // FIXED: Use ScheduledDuration instead of EstimatedDuration
                entity.Property(e => e.LaserPowerWatts).HasDefaultValue(170);
                entity.Property(e => e.ScanSpeedMmPerSec).HasDefaultValue(1000);
                entity.Property(e => e.LayerThicknessMicrons).HasDefaultValue(30);
                entity.Property(e => e.HatchSpacingMicrons).HasDefaultValue(120);
                entity.Property(e => e.BuildTemperatureCelsius).HasDefaultValue(180);
                entity.Property(e => e.ArgonPurityPercent).HasDefaultValue(99.5);
                entity.Property(e => e.OxygenContentPpm).HasDefaultValue(50);
                entity.Property(e => e.EstimatedPowderUsageKg).HasDefaultValue(0.5);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Index for better query performance
                entity.HasIndex(e => new { e.MachineId, e.ScheduledStart });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Priority);
            });

            // Configure Part entity
            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(20);
                // FIXED: Use correct property names
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Unique constraint on part number
                entity.HasIndex(e => e.PartNumber).IsUnique();
            });

            // Configure Machine entity
            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MachineId).IsRequired().HasMaxLength(50);
                // FIXED: Use correct property names
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MachineName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MachineType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Idle");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Unique constraint on machine ID
                entity.HasIndex(e => e.MachineId).IsUnique();
                entity.HasIndex(e => e.MachineType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
            });
            
            // Task 9: Configure JobNote entity
            modelBuilder.Entity<JobNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Step).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Note).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.NoteType).HasMaxLength(20).HasDefaultValue("Info");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationship to Job
                entity.HasOne(e => e.Job)
                    .WithMany(j => j.JobNotes)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Indexes for performance
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => new { e.JobId, e.Step });
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.IsCompleted);
            });

            // Task 11: Configure JobStage entity
            modelBuilder.Entity<JobStage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.StageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MachineId).HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Scheduled");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.EstimatedDurationHours).HasDefaultValue(1.0);
                entity.Property(e => e.AssignedOperator).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.QualityRequirements).HasMaxLength(1000);
                entity.Property(e => e.RequiredMaterials).HasMaxLength(1000);
                entity.Property(e => e.RequiredTooling).HasMaxLength(1000);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationships
                entity.HasOne(e => e.Job)
                    .WithMany(j => j.JobStages)
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Machine)
                    .WithMany()
                    .HasForeignKey(e => e.MachineId)
                    .HasPrincipalKey(m => m.MachineId) // Use Machine.MachineId instead of Machine.Id
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Indexes for performance
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.StageType);
                entity.HasIndex(e => e.Department);
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ScheduledStart);
                entity.HasIndex(e => e.ScheduledEnd);
                entity.HasIndex(e => new { e.JobId, e.ExecutionOrder });
            });

            // Task 11: Configure StageDependency entity
            modelBuilder.Entity<StageDependency>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DependencyType).IsRequired().HasMaxLength(50).HasDefaultValue("FinishToStart");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationships
                entity.HasOne(e => e.DependentStage)
                    .WithMany(s => s.Dependencies)
                    .HasForeignKey(e => e.DependentStageId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                entity.HasOne(e => e.RequiredStage)
                    .WithMany(s => s.Dependents)
                    .HasForeignKey(e => e.RequiredStageId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // Indexes for performance
                entity.HasIndex(e => e.DependentStageId);
                entity.HasIndex(e => e.RequiredStageId);
                entity.HasIndex(e => e.DependencyType);
                
                // Prevent circular dependencies
                entity.HasCheckConstraint("CK_StageDependency_NoSelfReference", 
                    "DependentStageId != RequiredStageId");
            });

            // Task 11: Configure StageNote entity
            modelBuilder.Entity<StageNote>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Note).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.NoteType).HasMaxLength(20).HasDefaultValue("Info");
                entity.Property(e => e.Priority).HasDefaultValue(3);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationship to JobStage
                entity.HasOne(e => e.Stage)
                    .WithMany(s => s.StageNotes)
                    .HasForeignKey(e => e.StageId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Indexes for performance
                entity.HasIndex(e => e.StageId);
                entity.HasIndex(e => e.NoteType);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.CreatedDate);
            });

            // Enhanced admin entities from Task 2
            ConfigureAdminEntities(modelBuilder);
        }

        /// <summary>
        /// Configure admin entities from Task 2
        /// </summary>
        private void ConfigureAdminEntities(ModelBuilder modelBuilder)
        {
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
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.CapabilityType);
                entity.HasIndex(e => e.IsAvailable);
                entity.HasIndex(e => new { e.MachineId, e.CapabilityType });

                // Relationships
                entity.HasOne(e => e.Machine)
                      .WithMany()
                      .HasForeignKey(e => e.MachineId)
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
                entity.Property(e => e.Notes).HasMaxLength(1000).HasDefaultValue("");

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
                entity.Property(e => e.Notes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.HoldReason).HasMaxLength(500).HasDefaultValue("");
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
        }
    }
}
