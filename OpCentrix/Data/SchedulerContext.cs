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

        // Segment 7: B&T Industry Specialization
        public DbSet<PartClassification> PartClassifications { get; set; }
        public DbSet<ComplianceRequirement> ComplianceRequirements { get; set; }
        public DbSet<SerialNumber> SerialNumbers { get; set; }
        public DbSet<ComplianceDocument> ComplianceDocuments { get; set; }

        // Phase 0.5: Prototype Tracking System
        public DbSet<PrototypeJob> PrototypeJobs { get; set; }
        public DbSet<ProductionStage> ProductionStages { get; set; }
        public DbSet<ProductionStageExecution> ProductionStageExecutions { get; set; }
        public DbSet<AssemblyComponent> AssemblyComponents { get; set; }
        public DbSet<PrototypeTimeLog> PrototypeTimeLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Basic SQLite configuration
                optionsBuilder.UseSqlite(options =>
                {
                    options.CommandTimeout(30);
                });

                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(true);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure core entities for Parts page functionality
            ConfigureCoreEntities(modelBuilder);
            
            // Configure admin entities
            ConfigureAdminEntities(modelBuilder);

            // Configure B&T industry specialization entities
            ConfigureBTEntities(modelBuilder);

            // Configure prototype tracking entities  
            ConfigurePrototypeTrackingEntities(modelBuilder);
        }

        private void ConfigureCoreEntities(ModelBuilder modelBuilder)
        {
            // Configure Job entity
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(20);
                entity.Property(e => e.MachineId).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Priority).HasDefaultValue(3);
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
                
                // Indexes for performance
                entity.HasIndex(e => new { e.MachineId, e.ScheduledStart });
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.PartNumber);
                entity.HasIndex(e => e.Priority);
            });

            // Configure Part entity - ENHANCED for Parts page and B&T specialization
            modelBuilder.Entity<Part>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Material).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SlsMaterial).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Industry).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Application).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure decimal properties with proper precision
                entity.Property(e => e.MaterialCostPerKg).HasPrecision(12, 2);
                entity.Property(e => e.StandardLaborCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.SetupCost).HasPrecision(10, 2);
                entity.Property(e => e.PostProcessingCost).HasPrecision(10, 2);
                entity.Property(e => e.QualityInspectionCost).HasPrecision(10, 2);
                entity.Property(e => e.MachineOperatingCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.ArgonCostPerHour).HasPrecision(10, 2);
                entity.Property(e => e.AverageCostPerUnit).HasPrecision(10, 2);
                entity.Property(e => e.StandardSellingPrice).HasPrecision(10, 2);
                
                // Configure string properties with defaults
                entity.Property(e => e.PowderSpecification).HasMaxLength(100).HasDefaultValue("15-45 micron particle size");
                entity.Property(e => e.Dimensions).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.SurfaceFinishRequirement).HasMaxLength(100).HasDefaultValue("As-built");
                entity.Property(e => e.ProcessType).HasMaxLength(50).HasDefaultValue("SLS Metal");
                entity.Property(e => e.RequiredMachineType).HasMaxLength(100).HasDefaultValue("TruPrint 3000");
                entity.Property(e => e.PreferredMachines).HasMaxLength(200).HasDefaultValue("TI1,TI2");
                entity.Property(e => e.QualityStandards).HasMaxLength(500).HasDefaultValue("ASTM F3001, ISO 17296");
                entity.Property(e => e.ToleranceRequirements).HasMaxLength(500).HasDefaultValue("±0.1mm typical");
                entity.Property(e => e.RequiredSkills).HasMaxLength(500).HasDefaultValue("SLS Operation,Powder Handling");
                entity.Property(e => e.RequiredCertifications).HasMaxLength(500).HasDefaultValue("SLS Operation Certification");
                entity.Property(e => e.RequiredTooling).HasMaxLength(500).HasDefaultValue("Build Platform,Powder Sieve");
                entity.Property(e => e.ConsumableMaterials).HasMaxLength(500).HasDefaultValue("Argon Gas,Build Platform Coating");
                entity.Property(e => e.SupportStrategy).HasMaxLength(200).HasDefaultValue("Minimal supports on overhangs > 45°");
                entity.Property(e => e.CustomerPartNumber).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.PartCategory).HasMaxLength(100).HasDefaultValue("Prototype");
                entity.Property(e => e.PartClass).HasMaxLength(10).HasDefaultValue("B");
                entity.Property(e => e.ProcessParameters).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.QualityCheckpoints).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.BuildFileTemplate).HasMaxLength(255).HasDefaultValue("");
                entity.Property(e => e.CadFilePath).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CadFileVersion).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.AvgDuration).HasMaxLength(50).HasDefaultValue("8h 0m");
                entity.Property(e => e.AdminOverrideReason).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.AdminOverrideBy).HasMaxLength(100).HasDefaultValue("");
                
                // B&T Specialization properties
                entity.Property(e => e.ExportClassification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ComponentType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.FirearmType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.BTTestingRequirements).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BTQualityStandards).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BTRegulatoryNotes).HasMaxLength(200).HasDefaultValue("");
                
                // Foreign key relationships for B&T
                entity.HasOne(e => e.PartClassification)
                    .WithMany(pc => pc.Parts)
                    .HasForeignKey(e => e.PartClassificationId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Unique constraint on part number
                entity.HasIndex(e => e.PartNumber).IsUnique();
                
                // Performance indexes for Parts page
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Material);
                entity.HasIndex(e => e.SlsMaterial);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.Industry);
                entity.HasIndex(e => e.PartCategory);
                entity.HasIndex(e => e.ProcessType);
                entity.HasIndex(e => e.RequiredMachineType);
                entity.HasIndex(e => e.CreatedDate);
                entity.HasIndex(e => e.CustomerPartNumber);
                
                // B&T specific indexes
                entity.HasIndex(e => e.PartClassificationId);
                entity.HasIndex(e => e.ComponentType);
                entity.HasIndex(e => e.FirearmType);
                entity.HasIndex(e => e.RequiresATFCompliance);
                entity.HasIndex(e => e.RequiresITARCompliance);
                entity.HasIndex(e => e.RequiresSerialization);
            });

            // Configure Machine entity
            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MachineId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MachineName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MachineType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("Idle");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Constraints and indexes
                entity.HasIndex(e => e.MachineId).IsUnique();
                entity.HasIndex(e => e.MachineType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsActive);
            });
            
            // FIXED: Configure MachineCapability entity with correct foreign key
            modelBuilder.Entity<MachineCapability>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                // Uses integer MachineId referencing Machine.Id (primary key)
                entity.Property(e => e.MachineId).IsRequired();
                
                // Relationship to Machine using integer primary key
                entity.HasOne(e => e.Machine)
                      .WithMany(m => m.Capabilities)
                      .HasForeignKey(e => e.MachineId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                // Indexes
                entity.HasIndex(e => e.MachineId);
                entity.HasIndex(e => e.CapabilityType);
                entity.HasIndex(e => e.IsAvailable);
                entity.HasIndex(e => new { e.MachineId, e.CapabilityType });
                
                // Properties
                entity.Property(e => e.CapabilityType).HasMaxLength(50);
                entity.Property(e => e.CapabilityName).HasMaxLength(100);
                entity.Property(e => e.CapabilityValue).HasMaxLength(500);
                entity.Property(e => e.Unit).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.Notes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.RequiredCertification).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsAvailable).HasDefaultValue(true);
                entity.Property(e => e.Priority).HasDefaultValue(3);
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
                    
                // Foreign key relationship to Part (optional)
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.JobNotes)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Indexes for performance
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.PartId);
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
                    
                // NOTE: JobStage.MachineId is a string reference to Machine.MachineId
                // We don't configure a navigation property here to avoid confusion
                
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
                entity.ToTable(t => t.HasCheckConstraint("CK_StageDependency_NoSelfReference", 
                    "DependentStageId != RequiredStageId"));
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

            // Configure admin entities with basic setup
            modelBuilder.Entity<RolePermission>().HasKey(e => e.Id);
            modelBuilder.Entity<InspectionCheckpoint>().HasKey(e => e.Id);
            modelBuilder.Entity<DefectCategory>().HasKey(e => e.Id);
            modelBuilder.Entity<ArchivedJob>().HasKey(e => e.Id);
            modelBuilder.Entity<AdminAlert>().HasKey(e => e.Id);
            modelBuilder.Entity<FeatureToggle>().HasKey(e => e.Id);
            modelBuilder.Entity<BuildJob>().HasKey(e => e.BuildId);
            modelBuilder.Entity<BuildJobPart>().HasKey(e => e.PartEntryId);
            modelBuilder.Entity<DelayLog>().HasKey(e => e.DelayId);
            modelBuilder.Entity<Material>().HasKey(e => e.Id);
            modelBuilder.Entity<JobNote>().HasKey(e => e.Id);
            modelBuilder.Entity<JobStage>().HasKey(e => e.Id);
            modelBuilder.Entity<StageDependency>().HasKey(e => e.Id);
            modelBuilder.Entity<StageNote>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<UserSettings>().HasKey(e => e.Id);
            modelBuilder.Entity<JobLogEntry>().HasKey(e => e.Id);
        }
        
        private void ConfigureAdminEntities(ModelBuilder modelBuilder)
        {
            // Configure OperatingShift entity
            modelBuilder.Entity<OperatingShift>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Indexes
                entity.HasIndex(e => e.DayOfWeek);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsHoliday);
                entity.HasIndex(e => e.SpecificDate);
                entity.HasIndex(e => new { e.DayOfWeek, e.IsActive });
            });

            // Configure SystemSetting entity
            modelBuilder.Entity<SystemSetting>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SettingKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SettingValue).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.DataType).HasMaxLength(50).HasDefaultValue("String");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DefaultValue).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.ValidationRules).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Unique constraint
                entity.HasIndex(e => e.SettingKey).IsUnique();
                
                // Indexes
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.Category, e.DisplayOrder });
            });

            // Configure RolePermission entity
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PermissionKey).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PermissionLevel).HasMaxLength(20).HasDefaultValue("Read");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Constraints).HasMaxLength(1000).HasDefaultValue("{}");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Unique constraint
                entity.HasIndex(e => new { e.RoleName, e.PermissionKey }).IsUnique();
                
                // Indexes
                entity.HasIndex(e => e.RoleName);
                entity.HasIndex(e => e.PermissionKey);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure InspectionCheckpoint entity
            modelBuilder.Entity<InspectionCheckpoint>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CheckpointName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.InspectionType).HasMaxLength(50).HasDefaultValue("Visual");
                entity.Property(e => e.AcceptanceCriteria).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.MeasurementMethod).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RequiredEquipment).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RequiredSkills).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ReferenceDocuments).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.Unit).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FailureAction).HasMaxLength(100).HasDefaultValue("Hold for review");
                entity.Property(e => e.SamplingMethod).HasMaxLength(50).HasDefaultValue("All");
                entity.Property(e => e.Category).HasMaxLength(50).HasDefaultValue("Quality");
                entity.Property(e => e.Notes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationships
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.InspectionCheckpoints)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.DefectCategory)
                    .WithMany()
                    .HasForeignKey(e => e.DefectCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.DefectCategoryId);
                entity.HasIndex(e => e.InspectionType);
                entity.HasIndex(e => e.IsRequired);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.PartId, e.SortOrder });
            });

            // Configure DefectCategory entity
            modelBuilder.Entity<DefectCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CategoryGroup).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.ApplicableProcesses).IsRequired().HasMaxLength(500);
                entity.Property(e => e.StandardCorrectiveActions).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.PreventionMethods).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CostImpact).HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.ColorCode).HasMaxLength(7).HasDefaultValue("#6B7280");
                entity.Property(e => e.Icon).HasMaxLength(50).HasDefaultValue("exclamation-triangle");
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Code);
                entity.HasIndex(e => e.CategoryGroup);
                entity.HasIndex(e => e.SeverityLevel);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure remaining entities with basic setup
            modelBuilder.Entity<ArchivedJob>().HasKey(e => e.Id);
            modelBuilder.Entity<AdminAlert>().HasKey(e => e.Id);
            modelBuilder.Entity<FeatureToggle>().HasKey(e => e.Id);
            modelBuilder.Entity<BuildJob>().HasKey(e => e.BuildId);
            modelBuilder.Entity<BuildJobPart>().HasKey(e => e.PartEntryId);
            modelBuilder.Entity<DelayLog>().HasKey(e => e.DelayId);
            modelBuilder.Entity<Material>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<UserSettings>().HasKey(e => e.Id);
            modelBuilder.Entity<JobLogEntry>().HasKey(e => e.Id);
        }

        private void ConfigureBTEntities(ModelBuilder modelBuilder)
        {
            // Configure PartClassification entity - Segment 7.1
            modelBuilder.Entity<PartClassification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClassificationCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ClassificationName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IndustryType).IsRequired().HasMaxLength(50).HasDefaultValue("Firearms");
                entity.Property(e => e.ComponentCategory).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SuppressorType).HasMaxLength(50);
                entity.Property(e => e.BafflePosition).HasMaxLength(50);
                entity.Property(e => e.FirearmType).HasMaxLength(50);
                entity.Property(e => e.RecommendedMaterial).IsRequired().HasMaxLength(100).HasDefaultValue("Ti-6Al-4V Grade 5");
                entity.Property(e => e.AlternativeMaterials).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.MaterialGrade).HasMaxLength(50).HasDefaultValue("Aerospace");
                entity.Property(e => e.RequiredProcess).HasMaxLength(100).HasDefaultValue("SLS Metal Printing");
                entity.Property(e => e.PostProcessingRequired).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.SpecialInstructions).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.TestingRequirements).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.QualityStandards).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.ExportClassification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.RegulatoryNotes).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Indexes
                entity.HasIndex(e => e.ClassificationCode).IsUnique();
                entity.HasIndex(e => e.ClassificationName);
                entity.HasIndex(e => e.IndustryType);
                entity.HasIndex(e => e.ComponentCategory);
                entity.HasIndex(e => e.SuppressorType);
                entity.HasIndex(e => e.FirearmType);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.RequiresATFCompliance);
                entity.HasIndex(e => e.RequiresITARCompliance);
                entity.HasIndex(e => e.RequiresSerialization);
            });

            // Configure ComplianceRequirement entity - Segment 7.1
            modelBuilder.Entity<ComplianceRequirement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RequirementCode).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequirementName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ComplianceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RegulatoryAuthority).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequirementDetails).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.DocumentationRequired).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.FormsRequired).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.RecordKeepingRequirements).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.ApplicableIndustries).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ApplicablePartTypes).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.ApplicableProcesses).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.EnforcementLevel).HasMaxLength(20).HasDefaultValue("Mandatory");
                entity.Property(e => e.PenaltyType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PenaltyDescription).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.RenewalProcess).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.ImplementationSteps).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.RequiredTraining).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.RequiredCertifications).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.SystemRequirements).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.ReferenceDocuments).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.WebResources).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.ContactInformation).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.AdditionalNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure decimal properties
                entity.Property(e => e.MaxPenaltyAmount).HasPrecision(12, 2);
                entity.Property(e => e.EstimatedImplementationCost).HasPrecision(10, 2);
                
                // Foreign key relationships
                entity.HasOne(e => e.PartClassification)
                    .WithMany(pc => pc.ComplianceRequirements)
                    .HasForeignKey(e => e.PartClassificationId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => e.RequirementCode).IsUnique();
                entity.HasIndex(e => e.ComplianceType);
                entity.HasIndex(e => e.RegulatoryAuthority);
                entity.HasIndex(e => e.EnforcementLevel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsCurrentVersion);
                entity.HasIndex(e => e.PartClassificationId);
                entity.HasIndex(e => e.EffectiveDate);
                entity.HasIndex(e => e.ExpirationDate);
            });

            // Configure SerialNumber entity - Segment 7.2
            modelBuilder.Entity<SerialNumber>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SerialNumberValue).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SerialNumberFormat).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ManufacturerCode).IsRequired().HasMaxLength(50).HasDefaultValue("BT");
                entity.Property(e => e.AssignedJobId).HasMaxLength(100);
                entity.Property(e => e.PartNumber).HasMaxLength(50);
                entity.Property(e => e.ComponentName).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ComponentType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ManufacturingMethod).HasMaxLength(50).HasDefaultValue("SLS Metal Printing");
                entity.Property(e => e.MaterialUsed).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.MaterialLotNumber).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.MachineUsed).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.Operator).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.QualityInspector).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ATFComplianceStatus).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.ATFClassification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.FFLDealer).HasMaxLength(100);
                entity.Property(e => e.FFLNumber).HasMaxLength(50);
                entity.Property(e => e.ATFFormNumbers).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.TaxStampNumber).HasMaxLength(50);
                entity.Property(e => e.TransferStatus).HasMaxLength(20).HasDefaultValue("In Manufacturing");
                entity.Property(e => e.TransferTo).HasMaxLength(200);
                entity.Property(e => e.TransferDocument).HasMaxLength(100);
                entity.Property(e => e.TransferNotes).HasMaxLength(500);
                entity.Property(e => e.DestructionMethod).HasMaxLength(500);
                entity.Property(e => e.ExportClassification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ExportLicense).HasMaxLength(100);
                entity.Property(e => e.DestinationCountry).HasMaxLength(100);
                entity.Property(e => e.EndUser).HasMaxLength(200);
                entity.Property(e => e.QualityStatus).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.QualityCertificateNumber).HasMaxLength(100);
                entity.Property(e => e.TestResultsSummary).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.QualityNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.ManufacturingHistory).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.ComponentGenealogy).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.AssemblyComponents).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BatchNumber).HasMaxLength(200);
                entity.Property(e => e.BuildPlatformId).HasMaxLength(50);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationships
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.SerialNumbers)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Job)
                    .WithMany()
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.ComplianceRequirement)
                    .WithMany(cr => cr.SerialNumbers)
                    .HasForeignKey(e => e.ComplianceRequirementId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => e.SerialNumberValue).IsUnique();
                entity.HasIndex(e => e.ManufacturerCode);
                entity.HasIndex(e => e.ComponentType);
                entity.HasIndex(e => e.ATFComplianceStatus);
                entity.HasIndex(e => e.TransferStatus);
                entity.HasIndex(e => e.QualityStatus);
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.ComplianceRequirementId);
                entity.HasIndex(e => e.AssignedDate);
                entity.HasIndex(e => e.ManufacturedDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsLocked);
            });

            // Configure ComplianceDocument entity - Segment 7.2
            modelBuilder.Entity<ComplianceDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DocumentTitle).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ComplianceCategory).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DocumentClassification).HasMaxLength(50).HasDefaultValue("Unclassified");
                entity.Property(e => e.RegulatoryAuthority).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.FormNumber).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Draft");
                entity.Property(e => e.ApprovalNumber).HasMaxLength(100);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
                entity.Property(e => e.FilePath).HasMaxLength(500);
                entity.Property(e => e.FileName).HasMaxLength(100);
                entity.Property(e => e.FileType).HasMaxLength(20);
                entity.Property(e => e.FileHash).HasMaxLength(100);
                entity.Property(e => e.DocumentContent).HasMaxLength(2000).HasDefaultValue("");
                entity.Property(e => e.AssociatedSerialNumbers).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.AssociatedPartNumbers).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.AssociatedJobNumbers).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.Customer).HasMaxLength(100);
                entity.Property(e => e.Vendor).HasMaxLength(100);
                entity.Property(e => e.PreparedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ReviewedBy).HasMaxLength(100);
                entity.Property(e => e.ApprovedBy).HasMaxLength(100);
                entity.Property(e => e.ReviewComments).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.ApprovalComments).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.RetentionPeriod).IsRequired().HasMaxLength(20).HasDefaultValue("Permanent");
                entity.Property(e => e.ArchiveLocation).HasMaxLength(50);
                entity.Property(e => e.DisposalMethod).HasMaxLength(500);
                entity.Property(e => e.NotificationRecipients).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastAccessedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.AuditNotes).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastAccessedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure decimal properties
                entity.Property(e => e.FileSizeMB).HasPrecision(8, 2);
                
                // Foreign key relationships
                entity.HasOne(e => e.SerialNumber)
                    .WithMany(sn => sn.ComplianceDocuments)
                    .HasForeignKey(e => e.SerialNumberId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.ComplianceRequirement)
                    .WithMany(cr => cr.ComplianceDocuments)
                    .HasForeignKey(e => e.ComplianceRequirementId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.ComplianceDocuments)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Job)
                    .WithMany()
                    .HasForeignKey(e => e.JobId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                // Indexes
                entity.HasIndex(e => e.DocumentNumber).IsUnique();
                entity.HasIndex(e => e.DocumentType);
                entity.HasIndex(e => e.ComplianceCategory);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.SerialNumberId);
                entity.HasIndex(e => e.ComplianceRequirementId);
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.JobId);
                entity.HasIndex(e => e.DocumentDate);
                entity.HasIndex(e => e.EffectiveDate);
                entity.HasIndex(e => e.ExpirationDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsArchived);
            });
        }
        
        private void ConfigurePrototypeTrackingEntities(ModelBuilder modelBuilder)
        {
            // Configure PrototypeJob entity
            modelBuilder.Entity<PrototypeJob>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrototypeNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequestedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Standard");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("InProgress");
                entity.Property(e => e.AdminReviewStatus).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Configure decimal properties
                entity.Property(e => e.TotalActualCost).HasPrecision(12, 2);
                entity.Property(e => e.TotalEstimatedCost).HasPrecision(12, 2);
                entity.Property(e => e.CostVariancePercent).HasPrecision(5, 2);
                entity.Property(e => e.TotalActualHours).HasPrecision(8, 2);
                entity.Property(e => e.TotalEstimatedHours).HasPrecision(8, 2);
                entity.Property(e => e.TimeVariancePercent).HasPrecision(5, 2);
                
                // Foreign key relationships
                entity.HasOne(e => e.Part)
                    .WithMany()
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Indexes
                entity.HasIndex(e => e.PrototypeNumber).IsUnique();
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.AdminReviewStatus);
                entity.HasIndex(e => e.RequestedBy);
                entity.HasIndex(e => e.RequestDate);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure ProductionStage entity
            modelBuilder.Entity<ProductionStage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DisplayOrder).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.DefaultSetupMinutes).HasDefaultValue(30);
                entity.Property(e => e.DefaultHourlyRate).HasPrecision(8, 2).HasDefaultValue(85.00m);
                entity.Property(e => e.RequiresQualityCheck).HasDefaultValue(true);
                entity.Property(e => e.RequiresApproval).HasDefaultValue(false);
                entity.Property(e => e.AllowSkip).HasDefaultValue(false);
                entity.Property(e => e.IsOptional).HasDefaultValue(false);
                entity.Property(e => e.RequiredRole).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.DisplayOrder);
                entity.HasIndex(e => e.RequiredRole);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure ProductionStageExecution entity  
            modelBuilder.Entity<ProductionStageExecution>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("NotStarted");
                entity.Property(e => e.QualityCheckRequired).HasDefaultValue(true);
                entity.Property(e => e.ProcessParameters).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.ExecutedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Configure decimal properties
                entity.Property(e => e.EstimatedHours).HasPrecision(8, 2);
                entity.Property(e => e.ActualHours).HasPrecision(8, 2);
                entity.Property(e => e.SetupHours).HasPrecision(8, 2);
                entity.Property(e => e.RunHours).HasPrecision(8, 2);
                entity.Property(e => e.EstimatedCost).HasPrecision(10, 2);
                entity.Property(e => e.ActualCost).HasPrecision(10, 2);
                entity.Property(e => e.MaterialCost).HasPrecision(10, 2);
                entity.Property(e => e.LaborCost).HasPrecision(10, 2);
                entity.Property(e => e.OverheadCost).HasPrecision(10, 2);
                
                // Foreign key relationships
                entity.HasOne(e => e.PrototypeJob)
                    .WithMany(pj => pj.StageExecutions)
                    .HasForeignKey(e => e.PrototypeJobId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.ProductionStage)
                    .WithMany(ps => ps.StageExecutions)
                    .HasForeignKey(e => e.ProductionStageId)
                    .OnDelete(DeleteBehavior.Restrict);
                
                // Unique constraint - one execution per stage per prototype job
                entity.HasIndex(e => new { e.PrototypeJobId, e.ProductionStageId }).IsUnique();
                
                // Performance indexes
                entity.HasIndex(e => e.PrototypeJobId);
                entity.HasIndex(e => e.ProductionStageId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ExecutedBy);
                entity.HasIndex(e => e.StartDate);
                entity.HasIndex(e => e.CompletionDate);
            });

            // Configure AssemblyComponent entity
            modelBuilder.Entity<AssemblyComponent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ComponentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ComponentDescription).IsRequired().HasMaxLength(200);
                entity.Property(e => e.QuantityRequired).HasDefaultValue(1);
                entity.Property(e => e.QuantityUsed).HasDefaultValue(0);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Needed");
                entity.Property(e => e.InspectionRequired).HasDefaultValue(false);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                
                // Configure decimal properties
                entity.Property(e => e.UnitCost).HasPrecision(8, 2);
                entity.Property(e => e.TotalCost).HasPrecision(10, 2);
                
                // Foreign key relationships
                entity.HasOne(e => e.PrototypeJob)
                    .WithMany(pj => pj.AssemblyComponents)
                    .HasForeignKey(e => e.PrototypeJobId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes
                entity.HasIndex(e => e.PrototypeJobId);
                entity.HasIndex(e => e.ComponentType);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Supplier);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure PrototypeTimeLog entity
            modelBuilder.Entity<PrototypeTimeLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActivityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.ActivityDescription).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Employee).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                
                // Foreign key relationships
                entity.HasOne(e => e.ProductionStageExecution)
                    .WithMany(pse => pse.TimeLogs)
                    .HasForeignKey(e => e.ProductionStageExecutionId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                // Indexes
                entity.HasIndex(e => e.ProductionStageExecutionId);
                entity.HasIndex(e => e.ActivityType);
                entity.HasIndex(e => e.Employee);
                entity.HasIndex(e => e.LogDate);
                entity.HasIndex(e => e.StartTime);
            });
        }
        
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }
        
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }
        
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetProperty("LastModifiedDate") != null && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));
                           
            foreach (var entry in entries)
            {
                var entity = entry.Entity;
                var entityType = entity.GetType();
                
                if (entry.State == EntityState.Added)
                {
                    var createdDateProp = entityType.GetProperty("CreatedDate");
                    var createdByProp = entityType.GetProperty("CreatedBy");
                    
                    if (createdDateProp != null)
                        createdDateProp.SetValue(entity, DateTime.UtcNow);
                    if (createdByProp != null && createdByProp.GetValue(entity)?.ToString() == "")
                        createdByProp.SetValue(entity, "System");
                }
                
                var lastModifiedDateProp = entityType.GetProperty("LastModifiedDate");
                var lastModifiedByProp = entityType.GetProperty("LastModifiedBy");
                
                if (lastModifiedDateProp != null)
                    lastModifiedDateProp.SetValue(entity, DateTime.UtcNow);
                if (lastModifiedByProp != null && lastModifiedByProp.GetValue(entity)?.ToString() == "")
                    lastModifiedByProp.SetValue(entity, "System");
            }
        }
    }
}
