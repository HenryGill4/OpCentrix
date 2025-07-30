using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Tracks the execution of a specific production stage for a prototype job
    /// Captures actual time, cost, and quality data for each stage
    /// </summary>
    public class ProductionStageExecution
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PrototypeJobId { get; set; }

        [Required]
        public int ProductionStageId { get; set; }

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

        // Audit Fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(PrototypeJobId))]
        public virtual PrototypeJob? PrototypeJob { get; set; }

        [ForeignKey(nameof(ProductionStageId))]
        public virtual ProductionStage? ProductionStage { get; set; }

        public virtual ICollection<PrototypeTimeLog> TimeLogs { get; set; } = new List<PrototypeTimeLog>();
    }
}