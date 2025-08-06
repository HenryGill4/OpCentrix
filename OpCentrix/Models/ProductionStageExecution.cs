using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Tracks the execution of a specific production stage for jobs (both regular jobs and prototype jobs)
    /// Captures actual time, cost, and quality data for each stage
    /// Updated to support both regular jobs and prototype jobs per Stage Dashboard Master Plan
    /// </summary>
    public class ProductionStageExecution
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to regular Job (new field for stage dashboard support)
        /// Either JobId OR PrototypeJobId must be set, but not both
        /// </summary>
        public int? JobId { get; set; }

        /// <summary>
        /// Reference to PrototypeJob (existing field, kept for backward compatibility)
        /// Either JobId OR PrototypeJobId must be set, but not both
        /// </summary>
        public int? PrototypeJobId { get; set; }

        [Required]
        public int ProductionStageId { get; set; }

        /// <summary>
        /// Optional workflow template this execution belongs to
        /// </summary>
        public int? WorkflowTemplateId { get; set; }

        // Execution Status
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "NotStarted"; // NotStarted, InProgress, Completed, Skipped, Failed

        public DateTime? StartDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        // Time Tracking
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EstimatedHours { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? ActualHours { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? SetupHours { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RunHours { get; set; }

        // Cost Tracking
        [Column(TypeName = "decimal(10,2)")]
        public decimal? EstimatedCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? ActualCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? MaterialCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? LaborCost { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OverheadCost { get; set; }

        // Quality Tracking
        public bool QualityCheckRequired { get; set; } = true;
        public bool? QualityCheckPassed { get; set; }

        [StringLength(100)]
        public string? QualityCheckBy { get; set; }

        public DateTime? QualityCheckDate { get; set; }

        [StringLength(1000)]
        public string? QualityNotes { get; set; }

        // Process Data
        [StringLength(2000)]
        public string ProcessParameters { get; set; } = "{}"; // JSON for stage-specific data

        [StringLength(1000)]
        public string? Issues { get; set; }

        [StringLength(1000)]
        public string? Improvements { get; set; }

        // Execution Team
        [Required]
        [StringLength(100)]
        public string ExecutedBy { get; set; } = string.Empty;

        [StringLength(100)]
        public string? ReviewedBy { get; set; }

        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        // Stage Dashboard Support Fields (new)
        [StringLength(100)]
        public string? OperatorName { get; set; }

        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        public DateTime? LastModifiedDate { get; set; }

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(JobId))]
        public virtual Job? Job { get; set; }

        [ForeignKey(nameof(PrototypeJobId))]
        public virtual PrototypeJob? PrototypeJob { get; set; }

        [ForeignKey(nameof(ProductionStageId))]
        public virtual ProductionStage? ProductionStage { get; set; }

        public virtual ICollection<PrototypeTimeLog> TimeLogs { get; set; } = new List<PrototypeTimeLog>();

        /// <summary>
        /// Helper property to get the associated job, whether it's a regular job or prototype job
        /// </summary>
        [NotMapped]
        public string JobIdentifier
        {
            get
            {
                if (JobId.HasValue && Job != null)
                    return $"Job-{JobId}-{Job.PartNumber}";
                if (PrototypeJobId.HasValue && PrototypeJob != null)
                    return $"Prototype-{PrototypeJobId}-{PrototypeJob.PrototypeNumber}";
                return "Unknown";
            }
        }

        /// <summary>
        /// Helper property to determine if this is for a regular job or prototype
        /// </summary>
        [NotMapped]
        public bool IsRegularJob => JobId.HasValue;

        /// <summary>
        /// Helper property to determine if this is for a prototype job
        /// </summary>
        [NotMapped]
        public bool IsPrototypeJob => PrototypeJobId.HasValue;
    }
}