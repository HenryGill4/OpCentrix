using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;

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

        // Task 11: Multi-stage manufacturing entities (JobStage-based)
        public DbSet<JobStage> JobStages { get; set; }
        public DbSet<JobStageDependency> StageDependencies { get; set; }
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

        // EDM Operations tracking
        public DbSet<EDMLog> EDMLogs { get; set; }

        // Bug Reporting System
        public DbSet<BugReport> BugReports { get; set; }

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
        public DbSet<PartStageRequirement> PartStageRequirements { get; set; } = null!;

        // Option A: Multi-Stage Workflow Enhancement
        public DbSet<BuildCohort> BuildCohorts { get; set; }
        public DbSet<JobStageHistory> JobStageHistories { get; set; }

        // PHASE 4: Enhanced Build Time Tracking and Learning
        public DbSet<PartCompletionLog> PartCompletionLogs { get; set; }
        public DbSet<OperatorEstimateLog> OperatorEstimateLogs { get; set; }
        public DbSet<BuildTimeLearningData> BuildTimeLearningData { get; set; }

        // Advanced Stage Management: Workflow Templates and Dependencies (ProductionStage-based)
        public DbSet<WorkflowTemplate> WorkflowTemplates { get; set; }
        public DbSet<ResourcePool> ResourcePools { get; set; }
        public DbSet<OpCentrix.Models.StageDependency> ProductionStageDependencies { get; set; }

        // PHASE 3: Part Form Refactor - New lookup tables
        public DbSet<ComponentType> ComponentTypes { get; set; }
        public DbSet<ComplianceCategory> ComplianceCategories { get; set; }
        public DbSet<PartAssetLink> PartAssetLinks { get; set; }
        public DbSet<LegacyFlagToStageMap> LegacyFlagToStageMaps { get; set; }

        // PHASE 6: Advanced Stage Management - Stage Templates
        public DbSet<StageTemplate> StageTemplates { get; set; }
        public DbSet<StageTemplateStep> StageTemplateSteps { get; set; }
        public DbSet<StageTemplateCategory> StageTemplateCategories { get; set; }

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

            // PHASE 3: Configure Part Form Refactor entities
            ConfigurePartFormRefactorEntities(modelBuilder);

            // PHASE 6: Configure Stage Template entities
            ConfigureStageTemplateEntities(modelBuilder);

            // Note: Other configuration methods temporarily disabled to prevent build errors
            // These can be added back when the respective features are implemented:
            // - ConfigureBTEntities(modelBuilder);
            // - ConfigurePrototypeTrackingEntities(modelBuilder);
            // - ConfigureOptionAWorkflowEntities(modelBuilder);
            // - ConfigurePhase4TrackingEntities(modelBuilder);
            // - ConfigureAdvancedStageManagementEntities(modelBuilder);
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

                // CRITICAL FIX: Make CustomerOrderNumber nullable and provide default
                entity.Property(e => e.CustomerOrderNumber).HasMaxLength(100).HasDefaultValue("");

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

                // B&T Specialization properties - FIXED: Use correct property names
                entity.Property(e => e.ExportClassification).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.BTComponentType).HasMaxLength(50).HasDefaultValue("General"); // FIXED: Use BTComponentType not ComponentType
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
                entity.HasIndex(e => e.BTComponentType); // FIXED: Use BTComponentType not ComponentType
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

            // Task 11: Configure StageDependency entity (JobStage-based)
            modelBuilder.Entity<JobStageDependency>(entity =>
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
                entity.ToTable(t => t.HasCheckConstraint("CK_JobStageDependency_NoSelfReference",
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
            modelBuilder.Entity<JobStageDependency>().HasKey(e => e.Id);
            modelBuilder.Entity<StageNote>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<UserSettings>().HasKey(e => e.Id);
            modelBuilder.Entity<JobLogEntry>().HasKey(e => e.Id);

            // Configure BugReport entity
            modelBuilder.Entity<BugReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BugId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("New");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.PageUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PageArea).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PageController).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PageAction).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ReportedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserRole).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.UserEmail).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ReportedDate).IsRequired().HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UserAgent).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BrowserName).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.BrowserVersion).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.OperatingSystem).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ScreenResolution).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.IpAddress).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ErrorType).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ErrorMessage).HasDefaultValue("");
                entity.Property(e => e.StackTrace).HasDefaultValue("");
                entity.Property(e => e.OperationId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.StepsToReproduce).HasDefaultValue("");
                entity.Property(e => e.ExpectedBehavior).HasDefaultValue("");
                entity.Property(e => e.ActualBehavior).HasDefaultValue("");
                entity.Property(e => e.AdditionalNotes).HasDefaultValue("");
                entity.Property(e => e.AttachedFiles).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.FormData).HasDefaultValue("");
                entity.Property(e => e.NetworkRequests).HasDefaultValue("");
                entity.Property(e => e.ConsoleErrors).HasDefaultValue("");
                entity.Property(e => e.AssignedTo).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ResolvedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ResolutionNotes).HasDefaultValue("");
                entity.Property(e => e.ResolutionType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.VoteCount).HasDefaultValue(0);
                entity.Property(e => e.LastViewedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.IsReproduced).HasDefaultValue(false);
                entity.Property(e => e.ReproducedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsPublic).HasDefaultValue(false);
                entity.Property(e => e.NotifyReporter).HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.RelatedBugIds).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.DuplicateOf).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.PerformanceImpact).HasMaxLength(20).HasDefaultValue("None");
                entity.Property(e => e.PageLoadTime).HasPrecision(8, 2);
                entity.Property(e => e.MemoryUsage).HasPrecision(8, 2);
                entity.Property(e => e.CpuUsage).HasPrecision(5, 2);
                entity.Property(e => e.Tags).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CustomMetadata).HasDefaultValue("{}");

                // Unique constraint on BugId
                entity.HasIndex(e => e.BugId).IsUnique();

                // Performance indexes
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.PageArea);
                entity.HasIndex(e => e.ReportedBy);
                entity.HasIndex(e => e.AssignedTo);
                entity.HasIndex(e => e.ReportedDate);
                entity.HasIndex(e => e.ResolvedDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsPublic);
                entity.HasIndex(e => e.OperationId);
                entity.HasIndex(e => new { e.PageArea, e.Status });
                entity.HasIndex(e => new { e.Severity, e.Priority });
                entity.HasIndex(e => new { e.ReportedDate, e.Status });
            });
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
            modelBuilder.Entity<JobNote>().HasKey(e => e.Id);
            modelBuilder.Entity<JobStage>().HasKey(e => e.Id);
            modelBuilder.Entity<JobStageDependency>().HasKey(e => e.Id);
            modelBuilder.Entity<StageNote>().HasKey(e => e.Id);
            modelBuilder.Entity<User>().HasKey(e => e.Id);
            modelBuilder.Entity<UserSettings>().HasKey(e => e.Id);
            modelBuilder.Entity<JobLogEntry>().HasKey(e => e.Id);

            // Configure BugReport entity
            modelBuilder.Entity<BugReport>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BugId).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Severity).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20).HasDefaultValue("New");
                entity.Property(e => e.Category).IsRequired().HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.PageUrl).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PageArea).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PageController).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.PageAction).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ReportedBy).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserRole).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.UserEmail).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ReportedDate).IsRequired().HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.UserAgent).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.BrowserName).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.BrowserVersion).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.OperatingSystem).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ScreenResolution).HasMaxLength(20).HasDefaultValue("");
                entity.Property(e => e.IpAddress).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ErrorType).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ErrorMessage).HasDefaultValue("");
                entity.Property(e => e.StackTrace).HasDefaultValue("");
                entity.Property(e => e.OperationId).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.StepsToReproduce).HasDefaultValue("");
                entity.Property(e => e.ExpectedBehavior).HasDefaultValue("");
                entity.Property(e => e.ActualBehavior).HasDefaultValue("");
                entity.Property(e => e.AdditionalNotes).HasDefaultValue("");
                entity.Property(e => e.AttachedFiles).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.FormData).HasDefaultValue("");
                entity.Property(e => e.NetworkRequests).HasDefaultValue("");
                entity.Property(e => e.ConsoleErrors).HasDefaultValue("");
                entity.Property(e => e.AssignedTo).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ResolvedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.ResolutionNotes).HasDefaultValue("");
                entity.Property(e => e.ResolutionType).HasMaxLength(50).HasDefaultValue("");
                entity.Property(e => e.ViewCount).HasDefaultValue(0);
                entity.Property(e => e.VoteCount).HasDefaultValue(0);
                entity.Property(e => e.LastViewedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.IsReproduced).HasDefaultValue(false);
                entity.Property(e => e.ReproducedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsPublic).HasDefaultValue(false);
                entity.Property(e => e.NotifyReporter).HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedBy).HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.RelatedBugIds).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.DuplicateOf).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.PerformanceImpact).HasMaxLength(20).HasDefaultValue("None");
                entity.Property(e => e.PageLoadTime).HasPrecision(8, 2);
                entity.Property(e => e.MemoryUsage).HasPrecision(8, 2);
                entity.Property(e => e.CpuUsage).HasPrecision(5, 2);
                entity.Property(e => e.Tags).HasMaxLength(500).HasDefaultValue("");
                entity.Property(e => e.CustomMetadata).HasDefaultValue("{}");

                // Unique constraint on BugId
                entity.HasIndex(e => e.BugId).IsUnique();

                // Performance indexes
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.Severity);
                entity.HasIndex(e => e.Priority);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.PageArea);
                entity.HasIndex(e => e.ReportedBy);
                entity.HasIndex(e => e.AssignedTo);
                entity.HasIndex(e => e.ReportedDate);
                entity.HasIndex(e => e.ResolvedDate);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.IsPublic);
                entity.HasIndex(e => e.OperationId);
                entity.HasIndex(e => new { e.PageArea, e.Status });
                entity.HasIndex(e => new { e.Severity, e.Priority });
                entity.HasIndex(e => new { e.ReportedDate, e.Status });
            });
        }

        private void ConfigurePartFormRefactorEntities(ModelBuilder modelBuilder)
        {
            // Configure ComponentType entity
            modelBuilder.Entity<ComponentType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");

                // Indexes
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SortOrder);
            });

            // Configure ComplianceCategory entity
            modelBuilder.Entity<ComplianceCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RegulatoryLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.RequiresSpecialHandling).HasDefaultValue(false);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");

                // Indexes
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.RegulatoryLevel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => new { e.RegulatoryLevel, e.SortOrder });
            });

            // Configure PartAssetLink entity
            modelBuilder.Entity<PartAssetLink>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PartId).IsRequired();
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Source).IsRequired().HasMaxLength(50).HasDefaultValue("Upload");
                entity.Property(e => e.AssetType).IsRequired().HasMaxLength(50).HasDefaultValue("3DModel");
                entity.Property(e => e.LastCheckedUtc).HasMaxLength(50);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");

                // Foreign key relationship
                entity.HasOne(e => e.Part)
                    .WithMany(p => p.AssetLinks)
                    .HasForeignKey(e => e.PartId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Indexes
                entity.HasIndex(e => e.PartId);
                entity.HasIndex(e => e.AssetType);
                entity.HasIndex(e => e.IsActive);
            });

            // Configure LegacyFlagToStageMap entity
            modelBuilder.Entity<LegacyFlagToStageMap>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LegacyFieldName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ProductionStageName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ExecutionOrder).HasDefaultValue(1);
                entity.Property(e => e.DefaultSetupMinutes).HasDefaultValue(30);
                entity.Property(e => e.DefaultTeardownMinutes).HasDefaultValue(0);
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                // Indexes
                entity.HasIndex(e => e.LegacyFieldName).IsUnique();
                entity.HasIndex(e => e.ProductionStageName);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ExecutionOrder);
            });

            // Update Part entity configuration for Part Form Refactor relationships
            modelBuilder.Entity<Part>(entity =>
            {
                // Configure relationships to new lookup tables
                entity.HasOne(e => e.ComponentType)
                    .WithMany(ct => ct.Parts)
                    .HasForeignKey(e => e.ComponentTypeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.ComplianceCategory)
                    .WithMany(cc => cc.Parts)
                    .HasForeignKey(e => e.ComplianceCategoryId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Indexes for new foreign keys
                entity.HasIndex(e => e.ComponentTypeId);
                entity.HasIndex(e => e.ComplianceCategoryId);
                entity.HasIndex(e => e.IsLegacyForm);
            });
        }

        private void ConfigureStageTemplateEntities(ModelBuilder modelBuilder)
        {
            // Configure StageTemplate entity
            modelBuilder.Entity<StageTemplate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Industry).HasMaxLength(50).HasDefaultValue("General");
                entity.Property(e => e.MaterialType).HasMaxLength(50).HasDefaultValue("Metal");
                entity.Property(e => e.ComplexityLevel).HasMaxLength(50).HasDefaultValue("Medium");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsDefault).HasDefaultValue(false);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);
                entity.Property(e => e.TemplateConfiguration).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.EstimatedTotalHours).HasPrecision(8, 2);
                entity.Property(e => e.EstimatedTotalCost).HasPrecision(12, 2);
                entity.Property(e => e.UsageCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.LastModifiedBy).IsRequired().HasMaxLength(100).HasDefaultValue("System");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("datetime('now')");

                // Indexes
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Industry);
                entity.HasIndex(e => e.MaterialType);
                entity.HasIndex(e => e.ComplexityLevel);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.UsageCount);
                entity.HasIndex(e => new { e.Industry, e.MaterialType, e.ComplexityLevel });
            });

            // Configure StageTemplateStep entity
            modelBuilder.Entity<StageTemplateStep>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StageTemplateId).IsRequired();
                entity.Property(e => e.ProductionStageId).IsRequired();
                entity.Property(e => e.ExecutionOrder).HasDefaultValue(1);
                entity.Property(e => e.EstimatedHours).HasPrecision(8, 2).HasDefaultValue(1.0);
                entity.Property(e => e.HourlyRate).HasPrecision(8, 2).HasDefaultValue(85.00);
                entity.Property(e => e.MaterialCost).HasPrecision(10, 2).HasDefaultValue(0.00);
                entity.Property(e => e.SetupTimeMinutes).HasDefaultValue(30);
                entity.Property(e => e.TeardownTimeMinutes).HasDefaultValue(0);
                entity.Property(e => e.IsRequired).HasDefaultValue(true);
                entity.Property(e => e.IsParallel).HasDefaultValue(false);
                entity.Property(e => e.StageConfiguration).HasMaxLength(2000).HasDefaultValue("{}");
                entity.Property(e => e.QualityRequirements).HasMaxLength(1000).HasDefaultValue("");
                entity.Property(e => e.SpecialInstructions).HasMaxLength(1000).HasDefaultValue("");

                // Foreign key relationships
                entity.HasOne(e => e.StageTemplate)
                    .WithMany(st => st.TemplateSteps)
                    .HasForeignKey(e => e.StageTemplateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ProductionStage)
                    .WithMany()
                    .HasForeignKey(e => e.ProductionStageId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Indexes
                entity.HasIndex(e => e.StageTemplateId);
                entity.HasIndex(e => e.ProductionStageId);
                entity.HasIndex(e => new { e.StageTemplateId, e.ExecutionOrder });

                // Unique constraint for template + stage combination
                entity.HasIndex(e => new { e.StageTemplateId, e.ProductionStageId }).IsUnique();
            });

            // Configure StageTemplateCategory entity
            modelBuilder.Entity<StageTemplateCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200).HasDefaultValue("");
                entity.Property(e => e.Icon).HasMaxLength(50).HasDefaultValue("fas fa-cogs");
                entity.Property(e => e.ColorCode).HasMaxLength(7).HasDefaultValue("#007bff");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.SortOrder).HasDefaultValue(100);

                // Indexes
                entity.HasIndex(e => e.Name).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.SortOrder);
            });

            // Update Part entity configuration to include template reference
            modelBuilder.Entity<Part>(entity =>
            {
                // Add foreign key for applied template (optional)
                entity.Property(e => e.AppliedTemplateId).IsRequired(false);
                
                entity.HasOne<StageTemplate>()
                    .WithMany()
                    .HasForeignKey(e => e.AppliedTemplateId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.AppliedTemplateId);
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
