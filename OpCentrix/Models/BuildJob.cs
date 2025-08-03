using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a single build job for 3D printer tracking (TI1, TI2, INC)
    /// Focused on actual print time tracking, delays, and operator accountability
    /// </summary>
    public class BuildJob
    {
        [Key] // FIXED: Add explicit Key attribute to resolve primary key issue
        public int BuildId { get; set; }

        #region Core Print Tracking

        [Required]
        [StringLength(10)]
        public string PrinterName { get; set; } = string.Empty; // TI1, TI2, INC

        [Required]
        public DateTime ActualStartTime { get; set; }

        public DateTime? ActualEndTime { get; set; }

        // Optional - for comparison with scheduled times
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress"; // Scheduled, In Progress, Completed, Aborted, Error

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        #endregion

        #region Print Details (from printer summary screen)

        [StringLength(50)]
        public string? LaserRunTime { get; set; }

        public float? GasUsed_L { get; set; }

        public float? PowderUsed_L { get; set; }

        [StringLength(50)]
        public string? ReasonForEnd { get; set; } // Completed, Aborted, Error, etc.

        #endregion

        #region Optional Setup Notes

        [StringLength(1000)]
        public string? SetupNotes { get; set; }

        // Link to existing scheduled job if applicable
        public int? AssociatedScheduledJobId { get; set; }

        // ADDED: Part relationship for Parts-to-Scheduler integration
        public int? PartId { get; set; }
        public virtual Part? Part { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Parts produced in this build job
        /// </summary>
        public virtual ICollection<BuildJobPart> BuildJobParts { get; set; } = new List<BuildJobPart>();

        /// <summary>
        /// Delay logs associated with this build job
        /// </summary>
        public virtual ICollection<DelayLog> DelayLogs { get; set; } = new List<DelayLog>();

        #endregion

        #region Audit Trail

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        #endregion

        #region Computed Properties

        [NotMapped]
        public TimeSpan? PrintDuration => ActualEndTime.HasValue 
            ? ActualEndTime.Value - ActualStartTime 
            : null;

        [NotMapped]
        public double PrintHours => PrintDuration?.TotalHours ?? 0;

        [NotMapped]
        public bool IsCompleted => Status == "Completed" && ActualEndTime.HasValue;

        [NotMapped]
        public bool IsActive => Status == "In Progress";

        [NotMapped]
        public string StatusColor => Status?.ToLower() switch
        {
            "completed" => "#10B981", // green
            "in progress" => "#F59E0B", // amber
            "aborted" => "#EF4444", // red
            "error" => "#DC2626", // dark red
            _ => "#6B7280" // gray (scheduled)
        };

        // ADDED: Part display properties for Parts-to-Scheduler integration
        [NotMapped]
        public string PartDisplayName => Part?.Name ?? "Unknown Part";

        [NotMapped]
        public string JobLifecycleStatus => Status switch
        {
            "In Progress" => "?? Printing",
            "Completed" => "? Complete", 
            "Aborted" => "? Aborted",
            "Error" => "?? Error",
            _ => "? Unknown"
        };

        [NotMapped]
        public int TotalPartsProduced => BuildJobParts?.Sum(p => p.Quantity) ?? 0;

        [NotMapped]
        public string PrimaryPartNumber => BuildJobParts?.FirstOrDefault(p => p.IsPrimary)?.PartNumber ?? "Unknown";

        [NotMapped]
        public bool HasDelays => DelayLogs?.Any() ?? false;

        [NotMapped]
        public int TotalDelayMinutes => DelayLogs?.Sum(d => d.DelayDuration) ?? 0;

        #endregion

        #region Enhanced Build Time Tracking Fields (Phase 2)

        // Operator Build Time Tracking
        public decimal? OperatorEstimatedHours { get; set; }  // Operator's estimate at start  
        public decimal? OperatorActualHours { get; set; }    // Operator's logged actual time
        public int TotalPartsInBuild { get; set; }           // Count of all parts in build
        public string? BuildFileHash { get; set; }           // Track unique build files  
        public bool IsLearningBuild { get; set; }            // Mark builds for ML learning
        public string? OperatorBuildAssessment { get; set; } // "faster", "expected", "slower"
        public string? TimeFactors { get; set; }             // JSON array of factors affecting time

        // Machine Performance Tracking
        public string? MachinePerformanceNotes { get; set; } // TI1 vs TI2 specific notes
        public decimal? PowerConsumption { get; set; }       // Track power usage if available
        public decimal? LaserOnTime { get; set; }            // Actual laser time from machine
        public int? LayerCount { get; set; }                 // Number of layers in build
        public decimal? BuildHeight { get; set; }            // Height of tallest part
        public string? SupportComplexity { get; set; }       // "Low", "Medium", "High", "None"

        // Quality and Learning Data  
        public string? PartOrientations { get; set; }        // JSON array of part orientations
        public string? PostProcessingNeeded { get; set; }    // Required post-processing steps
        public int? DefectCount { get; set; }                // Number of defective parts
        public string? LessonsLearned { get; set; }          // Operator notes for future builds

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate if this build started late compared to scheduled time
        /// </summary>
        public bool IsStartedLate()
        {
            return ScheduledStartTime.HasValue && ActualStartTime > ScheduledStartTime.Value;
        }

        /// <summary>
        /// Get delay in minutes if started late
        /// </summary>
        public int GetDelayMinutes()
        {
            if (!IsStartedLate()) return 0;
            return (int)(ActualStartTime - ScheduledStartTime!.Value).TotalMinutes;
        }

        /// <summary>
        /// Complete the build job
        /// </summary>
        public void Complete()
        {
            Status = "Completed";
            ActualEndTime = DateTime.UtcNow;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Abort the build job with a reason
        /// </summary>
        public void Abort(string reason)
        {
            Status = "Aborted";
            ReasonForEnd = reason;
            ActualEndTime = DateTime.UtcNow;
            CompletedAt = DateTime.UtcNow;
        }

        #endregion
    }

    /// <summary>
    /// PHASE 4: Tracks completion data for individual parts within a build job
    /// </summary>
    public class PartCompletionLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BuildJobId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string PartNumber { get; set; } = string.Empty;
        
        [Required]
        [Range(1, 1000)]
        public int Quantity { get; set; }
        
        [Required]
        [Range(0, 1000)]
        public int GoodParts { get; set; }
        
        [Required]
        [Range(0, 1000)]
        public int DefectiveParts { get; set; }
        
        [Required]
        [Range(0, 1000)]
        public int ReworkParts { get; set; }
        
        [Range(0, 100)]
        public decimal QualityRate { get; set; }
        
        public bool IsPrimary { get; set; }
        
        [StringLength(500)]
        public string? InspectionNotes { get; set; }
        
        public DateTime CompletedAt { get; set; }
        
        // Navigation property
        public virtual BuildJob BuildJob { get; set; } = null!;
    }

    /// <summary>
    /// PHASE 4: Tracks operator time estimates at the start of builds for learning
    /// </summary>
    public class OperatorEstimateLog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BuildJobId { get; set; }
        
        [Required]
        [Range(0.1, 100)]
        public decimal EstimatedHours { get; set; }
        
        [StringLength(500)]
        public string? TimeFactors { get; set; }
        
        [StringLength(500)]
        public string? OperatorNotes { get; set; }
        
        public DateTime LoggedAt { get; set; }
        
        // Navigation property
        public virtual BuildJob BuildJob { get; set; } = null!;
    }

    /// <summary>
    /// PHASE 4: Machine learning data for build time estimation and optimization
    /// </summary>
    public class BuildTimeLearningData
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int BuildJobId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;
        
        [StringLength(32)]
        public string? BuildFileHash { get; set; }
        
        [Required]
        [Range(0.1, 100)]
        public decimal OperatorEstimatedHours { get; set; }
        
        [Required]
        [Range(0.1, 100)]
        public decimal ActualHours { get; set; }
        
        [Required]
        [Range(-1000, 1000)]
        public decimal VariancePercent { get; set; }
        
        [StringLength(20)]
        public string? SupportComplexity { get; set; }
        
        [StringLength(1000)]
        public string? TimeFactors { get; set; }
        
        [Required]
        [Range(0, 100)]
        public decimal QualityScore { get; set; }
        
        public int DefectCount { get; set; } = 0;
        
        [Range(0, 300)]
        public decimal? BuildHeight { get; set; }
        
        [Range(10, 5000)]
        public int? LayerCount { get; set; }
        
        public int TotalParts { get; set; } = 1;
        
        [StringLength(500)]
        public string? PartOrientations { get; set; }
        
        public DateTime RecordedAt { get; set; }
        
        // Navigation property
        public virtual BuildJob BuildJob { get; set; } = null!;
    }
}