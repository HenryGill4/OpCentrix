using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    // Represents a scheduled SLS metal printing job with comprehensive analytics and TruPrint 3000 integration
    public class Job
    {
        public int Id { get; set; }

        #region Core Scheduling Data
        
        // Machine that will run the job (required) - TruPrint 3000 machines
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;

        // Scheduled time range
        [Required]
        public DateTime ScheduledStart { get; set; }
        
        [Required]
        public DateTime ScheduledEnd { get; set; }

        // Actual time tracking for performance analysis
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }

        // Audit trail
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
        
        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastModifiedBy { get; set; } = string.Empty;

        #endregion

        #region Option A: Multi-Stage Workflow Enhancement (4 NEW FIELDS)
        
        /// <summary>
        /// Links to SLS build cohort for parts tracking (20-130 parts per build)
        /// </summary>
        public int? BuildCohortId { get; set; }
        
        /// <summary>
        /// Current workflow stage: "SLS", "CNC", "EDM", "Assembly", "QC"
        /// </summary>
        [StringLength(50)]
        public string? WorkflowStage { get; set; }
        
        /// <summary>
        /// Execution order within workflow: 1, 2, 3, etc.
        /// </summary>
        public int? StageOrder { get; set; }
        
        /// <summary>
        /// Total stages in this job's workflow: 5 total stages typical
        /// </summary>
        public int? TotalStages { get; set; }

        #endregion

        #region Part and Production Data

        // Linked part information
        [Required]
        public int PartId { get; set; }
        
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^\d{2}-\d{4}$", ErrorMessage = "Part number must be in format XX-XXXX (e.g., 14-5396)")]
        public string PartNumber { get; set; } = string.Empty;
        
        // Production details
        [Required]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        public int Quantity { get; set; } = 1;
        
        public int ProducedQuantity { get; set; } = 0;
        public int DefectQuantity { get; set; } = 0;
        public int ReworkQuantity { get; set; } = 0;

        #endregion

        #region Duration and Time Management

        // ADDED: Missing EstimatedDuration property
        public TimeSpan EstimatedDuration { get; set; } = TimeSpan.FromHours(8);

        #endregion

        #region SLS-Specific Manufacturing Data

        // SLS Material Information
        [Required]
        [StringLength(100)]
        public string SlsMaterial { get; set; } = "Ti-6Al-4V Grade 5"; // Ti-6Al-4V, Inconel 718, Inconel 625, etc.
        
        [StringLength(50)]
        public string PowderLotNumber { get; set; } = string.Empty;
        
        public DateTime? PowderExpirationDate { get; set; }
        
        // Build Platform Information
        [StringLength(50)]
        public string BuildPlatformId { get; set; } = string.Empty;
        
        [Range(1, 10)]
        public int BuildLayerNumber { get; set; } = 1; // Layer on build platform (1-10)
        
        // SLS Process Parameters
        [Range(0, 2000)]
        public double LaserPowerWatts { get; set; } = 200;
        
        [Range(0, 5000)]
        public double ScanSpeedMmPerSec { get; set; } = 1200;
        
        [Range(0, 200)]
        public double LayerThicknessMicrons { get; set; } = 30;
        
        [Range(0, 1000)]
        public double HatchSpacingMicrons { get; set; } = 120;
        
        // Build Environment
        [Range(0, 100)]
        public double ArgonPurityPercent { get; set; } = 99.9;
        
        [Range(0, 200)]
        public double OxygenContentPpm { get; set; } = 50;
        
        [Range(0, 500)]
        public double BuildTemperatureCelsius { get; set; } = 180;

        // Powder Usage (kg)
        [Range(0, 50)]
        public double EstimatedPowderUsageKg { get; set; } = 0;
        
        [Range(0, 50)]
        public double ActualPowderUsageKg { get; set; } = 0;
        
        [Range(0, 100)]
        public double PowderRecyclePercentage { get; set; } = 85;

        #endregion

        #region Performance and Time Tracking

        // Time estimates and actuals (in hours)
        public double EstimatedHours { get; set; }
        
        [NotMapped]
        public double ActualHours => ActualStart.HasValue && ActualEnd.HasValue 
            ? (ActualEnd.Value - ActualStart.Value).TotalHours 
            : 0;

        // SLS-specific time tracking
        public double PreheatingTimeMinutes { get; set; } = 60;
        public double BuildTimeMinutes { get; set; } = 0;
        public double CoolingTimeMinutes { get; set; } = 240; // 4 hours typical cooling
        public double PowderChangeoverTimeMinutes { get; set; } = 30;
        public double PostProcessingTimeMinutes { get; set; } = 0;
        
        // Setup and changeover tracking
        public double SetupTimeMinutes { get; set; } = 0;
        public double ChangeoverTimeMinutes { get; set; } = 0;
        public string? PreviousJobPartNumber { get; set; }

        // Performance metrics
        [NotMapped]
        public double EfficiencyPercent => EstimatedHours > 0 && ActualHours > 0 
            ? Math.Round((EstimatedHours / ActualHours) * 100, 2) 
            : 0;

        #endregion

        #region Cost Tracking

        // Cost per hour data
        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCostPerHour { get; set; } = 85.00m; // SLS operator rate
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MaterialCostPerKg { get; set; } = 450.00m; // Titanium powder cost
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal MachineOperatingCostPerHour { get; set; } = 125.00m; // TruPrint 3000 operating cost
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal ArgonCostPerHour { get; set; } = 15.00m; // Inert gas cost
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal PowerCostPerKwh { get; set; } = 0.12m;

        // Calculated costs (NotMapped to avoid EF issues)
        [NotMapped]
        public decimal EstimatedLaborCost => (decimal)EstimatedHours * LaborCostPerHour;
        
        [NotMapped]
        public decimal EstimatedMaterialCost => (decimal)EstimatedPowderUsageKg * MaterialCostPerKg;
        
        [NotMapped]
        public decimal EstimatedMachineCost => (decimal)EstimatedHours * MachineOperatingCostPerHour;
        
        [NotMapped]
        public decimal EstimatedArgonCost => (decimal)EstimatedHours * ArgonCostPerHour;
        
        [NotMapped]
        public decimal EstimatedTotalCost => EstimatedLaborCost + EstimatedMaterialCost + EstimatedMachineCost + EstimatedArgonCost;

        [NotMapped]
        public decimal ActualLaborCost => (decimal)ActualHours * LaborCostPerHour;
        
        [NotMapped]
        public decimal ActualMaterialCost => (decimal)ActualPowderUsageKg * MaterialCostPerKg;

        #endregion

        #region TruPrint 3000 Integration

        // OPC UA Integration Fields
        [StringLength(100)]
        public string? OpcUaJobId { get; set; } // Unique identifier in TruPrint system
        
        [StringLength(50)]
        public string? OpcUaStatus { get; set; } // Real-time status from machine
        
        public DateTime? OpcUaLastUpdate { get; set; } // Last data sync from machine
        
        [Range(0, 100)]
        public double OpcUaBuildProgress { get; set; } = 0; // Build completion percentage from machine
        
        [StringLength(500)]
        public string? OpcUaErrorMessages { get; set; } // Error messages from machine
        
        // Machine telemetry data
        public double CurrentLaserPowerWatts { get; set; } = 0;
        public double CurrentBuildTemperature { get; set; } = 0;
        public double CurrentOxygenLevel { get; set; } = 0;
        public double CurrentArgonFlowRate { get; set; } = 0;
        
        // Build file information
        [StringLength(255)]
        public string? BuildFileName { get; set; } // .slm file name
        
        [StringLength(500)]
        public string? BuildFilePath { get; set; } // Full path to build file
        
        public long BuildFileSizeBytes { get; set; } = 0;
        
        public DateTime? BuildFileCreatedDate { get; set; }

        #endregion

        #region Resource Requirements

        // Skills and certifications required for SLS operations
        [StringLength(500)]
        public string RequiredSkills { get; set; } = "SLS Operation,Powder Handling,Inert Gas Safety";
        
        // SLS-specific tooling and equipment
        [StringLength(500)]
        public string RequiredTooling { get; set; } = "Build Platform,Powder Sieve,Inert Gas Setup";
        
        // Materials and consumables specific to SLS
        [StringLength(500)]
        public string RequiredMaterials { get; set; } = string.Empty;
        
        // Special instructions for SLS operations
        [StringLength(1000)]
        public string SpecialInstructions { get; set; } = string.Empty;

        #endregion

        #region Process and Quality Data

        // Process parameters (stored as JSON) - enhanced for SLS
        [StringLength(2000)]
        public string ProcessParameters { get; set; } = "{}";
        
        // Quality checkpoints and results specific to SLS (stored as JSON)
        [StringLength(2000)]
        public string QualityCheckpoints { get; set; } = "{}";
        
        // Machine utilization during job
        [Range(0, 100)]
        public double MachineUtilizationPercent { get; set; } = 0;
        
        // Energy consumption (kWh) - important for SLS cost calculation
        public double EnergyConsumptionKwh { get; set; } = 0;
        
        // SLS-specific quality metrics
        [Range(0, 100)]
        public double SurfaceRoughnessRa { get; set; } = 0;
        
        [Range(0, 100)]
        public double DensityPercentage { get; set; } = 99.5; // Typical for SLS parts
        
        [Range(0, 1000)]
        public double UltimateTensileStrengthMPa { get; set; } = 0;

        #endregion

        #region Status and Workflow

        // Enhanced status tracking for SLS workflow
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled";
        
        // Priority level (1 = highest, 5 = lowest)
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
        
        // Customer or order information
        [StringLength(100)]
        public string CustomerOrderNumber { get; set; } = string.Empty;
        
        public DateTime? CustomerDueDate { get; set; }
        
        // Rush job indicator
        public bool IsRushJob { get; set; } = false;
        
        // Hold reasons (if job is on hold)
        [StringLength(500)]
        public string HoldReason { get; set; } = string.Empty;
        
        // SLS-specific status flags
        public bool RequiresArgonPurge { get; set; } = true;
        public bool RequiresPreheating { get; set; } = true;
        public bool RequiresPostProcessing { get; set; } = false;
        public bool RequiresPowderSieving { get; set; } = true;

        #endregion

        #region Personnel

        [StringLength(100)]
        public string? Operator { get; set; }
        
        [StringLength(100)]
        public string? QualityInspector { get; set; }
        
        [StringLength(100)]
        public string? Supervisor { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Part information
        /// </summary>
        public virtual Part? Part { get; set; }

        /// <summary>
        /// Job step notes - Task 9: Enhanced scheduler features
        /// </summary>
        public virtual ICollection<JobNote> JobNotes { get; set; } = new List<JobNote>();

        /// <summary>
        /// Job stages for multi-stage manufacturing - Task 11: Modular Multi-Stage Scheduling
        /// </summary>
        public virtual ICollection<JobStage> JobStages { get; set; } = new List<JobStage>();

        #endregion

        #region Computed Properties for Analytics (NotMapped)

        // Duration calculations
        [NotMapped]
        public TimeSpan ScheduledDuration => ScheduledEnd - ScheduledStart;
        
        [NotMapped]
        public TimeSpan? ActualDuration => ActualStart.HasValue && ActualEnd.HasValue 
            ? ActualEnd.Value - ActualStart.Value 
            : null;

        [NotMapped]
        public double DurationHours => ScheduledDuration.TotalHours;
        
        [NotMapped]
        public int DurationMinutes => (int)ScheduledDuration.TotalMinutes;

        // Quality metrics
        [NotMapped]
        public double QualityScore => ProducedQuantity > 0 
            ? Math.Round(((double)(ProducedQuantity - DefectQuantity) / ProducedQuantity) * 100, 2) 
            : 100;
        
        [NotMapped]
        public double DefectRate => ProducedQuantity > 0 
            ? Math.Round(((double)DefectQuantity / ProducedQuantity) * 100, 2) 
            : 0;

        [NotMapped]
        public double ReworkRate => ProducedQuantity > 0 
            ? Math.Round(((double)ReworkQuantity / ProducedQuantity) * 100, 2) 
            : 0;

        // Schedule performance
        [NotMapped]
        public bool IsOnTime => ActualEnd.HasValue && ActualEnd.Value <= ScheduledEnd;
        
        [NotMapped]
        public bool IsStartedOnTime => ActualStart.HasValue && ActualStart.Value <= ScheduledStart.AddMinutes(15); // 15 min tolerance
        
        [NotMapped]
        public double ScheduleVarianceHours => ActualDuration.HasValue 
            ? (ActualDuration.Value - ScheduledDuration).TotalHours 
            : 0;

        // Cost performance
        [NotMapped]
        public decimal CostVariance => EstimatedTotalCost - (ActualLaborCost + ActualMaterialCost);
        
        [NotMapped]
        public double CostEfficiencyPercent => EstimatedTotalCost > 0 
            ? Math.Round((double)((ActualLaborCost + ActualMaterialCost) / EstimatedTotalCost) * 100, 2) 
            : 0;

        // SLS-specific calculations
        [NotMapped]
        public double TotalProcessTimeHours => (PreheatingTimeMinutes + BuildTimeMinutes + CoolingTimeMinutes + PostProcessingTimeMinutes) / 60.0;
        
        [NotMapped]
        public double PowderEfficiency => EstimatedPowderUsageKg > 0 && ActualPowderUsageKg > 0
            ? Math.Round((EstimatedPowderUsageKg / ActualPowderUsageKg) * 100, 2)
            : 100;

        #endregion

        #region Helper Methods

        // Helper method to check if this job overlaps with another
        public bool OverlapsWith(Job other)
        {
            if (other == null || MachineId != other.MachineId)
                return false;

            // Two jobs overlap if neither ends before the other starts
            return !(ScheduledEnd <= other.ScheduledStart || ScheduledStart >= other.ScheduledEnd);
        }

        // Helper method to calculate position in grid
        public double CalculateGridPosition(DateTime gridStartDate, int slotMinutes)
        {
            var totalMinutes = (ScheduledStart - gridStartDate.Date).TotalMinutes;
            return Math.Max(0, totalMinutes / slotMinutes);
        }

        // Helper method to calculate width in grid
        public double CalculateGridWidth(int slotMinutes)
        {
            return Math.Max(0.5, DurationMinutes / (double)slotMinutes);
        }

        // Helper method to get status color based on SLS workflow
        public string GetStatusColor()
        {
            return Status?.ToLower() switch
            {
                "completed" => "#10B981", // green
                "building" => "#F59E0B", // amber - SLS machine is building
                "cooling" => "#8B5CF6", // purple - cooling period
                "post-processing" => "#06B6D4", // cyan - post-processing
                "delayed" => "#EF4444", // red
                "cancelled" => "#6B7280", // gray
                "on-hold" => "#F97316", // orange
                "preheating" => "#FBBF24", // yellow - preheating phase
                _ => "#3B82F6" // blue (scheduled)
            };
        }

        // Get priority color
        public string GetPriorityColor()
        {
            return Priority switch
            {
                1 => "#DC2626", // red (critical)
                2 => "#F59E0B", // orange (high)
                3 => "#10B981", // green (normal)
                4 => "#6B7280", // gray (low)
                5 => "#9CA3AF", // light gray (lowest)
                _ => "#6B7280"
            };
        }

        // Parse SLS process parameters from JSON
        public Dictionary<string, object> GetProcessParameters()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(ProcessParameters) ?? new();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        // Set SLS process parameters as JSON
        public void SetProcessParameters(Dictionary<string, object> parameters)
        {
            ProcessParameters = JsonSerializer.Serialize(parameters);
        }

        // Parse quality checkpoints from JSON
        public Dictionary<string, object> GetQualityCheckpoints()
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(QualityCheckpoints) ?? new();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        // Set quality checkpoints as JSON
        public void SetQualityCheckpoints(Dictionary<string, object> checkpoints)
        {
            QualityCheckpoints = JsonSerializer.Serialize(checkpoints);
        }

        // Validate SLS material compatibility
        public bool IsCompatibleMaterial(string previousMaterial)
        {
            if (string.IsNullOrEmpty(previousMaterial))
                return true;

            // Define material compatibility matrix
            var compatibilityMatrix = new Dictionary<string, string[]>
            {
                ["Ti-6Al-4V Grade 5"] = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                ["Ti-6Al-4V ELI Grade 23"] = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23" },
                ["Inconel 718"] = new[] { "Inconel 718", "Inconel 625" },
                ["Inconel 625"] = new[] { "Inconel 718", "Inconel 625" }
            };

            return compatibilityMatrix.ContainsKey(previousMaterial) &&
                   compatibilityMatrix[previousMaterial].Contains(SlsMaterial);
        }

        // Calculate required powder changeover time based on material compatibility
        public double CalculatePowderChangeoverTime(string? previousMaterial)
        {
            if (string.IsNullOrEmpty(previousMaterial) || previousMaterial == SlsMaterial)
                return 0; // No changeover needed

            if (IsCompatibleMaterial(previousMaterial))
                return 30; // 30 minutes for compatible materials

            return 120; // 2 hours for incompatible materials (full cleaning required)
        }

        #endregion
    }
}

// Enhanced enums for SLS-specific operations
public enum SlsJobStatus
{
    Scheduled,
    Preheating,
    Building,
    Cooling,
    PostProcessing,
    QualityInspection,
    Completed,
    OnHold,
    Cancelled,
    Delayed
}

public enum SlsMaterial
{
    TiGrade5,           // Ti-6Al-4V Grade 5
    TiELIGrade23,       // Ti-6Al-4V ELI Grade 23
    Inconel718,         // Inconel 718
    Inconel625,         // Inconel 625
    Stainless316L,      // 316L Stainless Steel
    AlSi10Mg,          // AlSi10Mg Aluminum
    CoCrMo             // Cobalt Chrome
}

public enum JobPriority
{
    Critical = 1,
    High = 2,
    Normal = 3,
    Low = 4,
    Lowest = 5
}
