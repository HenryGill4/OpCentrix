using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a QC inspection record for parts after manufacturing stages
    /// </summary>
    public class QCInspection
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Unique inspection reference number (e.g., "QC-2024-001")
        /// </summary>
        [Required]
        [StringLength(50)]
        public string InspectionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Link to the job being inspected
        /// </summary>
        public int? JobId { get; set; }

        /// <summary>
        /// Link to the build job (for SLS post-print inspection)
        /// </summary>
        public int? BuildJobId { get; set; }

        /// <summary>
        /// Link to the build cohort
        /// </summary>
        public int? BuildCohortId { get; set; }

        /// <summary>
        /// Part being inspected
        /// </summary>
        [Required]
        public int PartId { get; set; }

        /// <summary>
        /// Inspection type: PostPrint, PostCNC, PostEDM, PostCoating, Final
        /// </summary>
        [Required]
        [StringLength(50)]
        public string InspectionType { get; set; } = "PostPrint";

        /// <summary>
        /// Current status: Pending, InProgress, Passed, Failed, ConditionalPass
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        #region Quantities

        /// <summary>
        /// Total quantity to inspect
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int TotalQuantity { get; set; } = 1;

        /// <summary>
        /// Quantity that passed inspection
        /// </summary>
        [Range(0, 1000)]
        public int PassedQuantity { get; set; }

        /// <summary>
        /// Quantity that failed inspection
        /// </summary>
        [Range(0, 1000)]
        public int FailedQuantity { get; set; }

        /// <summary>
        /// Quantity requiring rework
        /// </summary>
        [Range(0, 1000)]
        public int ReworkQuantity { get; set; }

        /// <summary>
        /// Quantity scrapped
        /// </summary>
        [Range(0, 1000)]
        public int ScrappedQuantity { get; set; }

        #endregion

        #region Inspection Details

        /// <summary>
        /// Inspector user ID
        /// </summary>
        public int? InspectorUserId { get; set; }

        /// <summary>
        /// Inspector name
        /// </summary>
        [StringLength(100)]
        public string? InspectorName { get; set; }

        /// <summary>
        /// When inspection started
        /// </summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>
        /// When inspection completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Overall quality score (0-100)
        /// </summary>
        [Range(0, 100)]
        public double? QualityScore { get; set; }

        #endregion

        #region Measurements and Results

        /// <summary>
        /// Dimensional inspection results as JSON
        /// </summary>
        public string? DimensionalResultsJson { get; set; }

        /// <summary>
        /// Surface finish measurements
        /// </summary>
        public double? SurfaceRoughnessRa { get; set; }

        /// <summary>
        /// Material density percentage
        /// </summary>
        public double? DensityPercent { get; set; }

        /// <summary>
        /// Visual inspection notes
        /// </summary>
        [StringLength(2000)]
        public string? VisualInspectionNotes { get; set; }

        /// <summary>
        /// Defects found (JSON array)
        /// </summary>
        public string? DefectsJson { get; set; }

        /// <summary>
        /// Corrective actions required
        /// </summary>
        [StringLength(2000)]
        public string? CorrectiveActions { get; set; }

        #endregion

        #region Audit Trail

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime? LastModifiedAt { get; set; }

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        #endregion

        #region Navigation Properties

        public virtual Job? Job { get; set; }
        public virtual BuildJob? BuildJob { get; set; }
        public virtual Part Part { get; set; } = null!;
        public virtual User? Inspector { get; set; }
        public virtual ICollection<QCChecklistItem> ChecklistItems { get; set; } = new List<QCChecklistItem>();

        #endregion

        #region Computed Properties

        [NotMapped]
        public double PassRate => TotalQuantity > 0 
            ? Math.Round((double)PassedQuantity / TotalQuantity * 100, 2) 
            : 0;

        [NotMapped]
        public double FailRate => TotalQuantity > 0 
            ? Math.Round((double)FailedQuantity / TotalQuantity * 100, 2) 
            : 0;

        [NotMapped]
        public int InspectedQuantity => PassedQuantity + FailedQuantity + ReworkQuantity + ScrappedQuantity;

        [NotMapped]
        public int PendingQuantity => TotalQuantity - InspectedQuantity;

        [NotMapped]
        public bool IsComplete => InspectedQuantity >= TotalQuantity;

        [NotMapped]
        public string StatusColor => Status switch
        {
            "Pending" => "#6B7280",
            "InProgress" => "#F59E0B",
            "Passed" => "#10B981",
            "Failed" => "#EF4444",
            "ConditionalPass" => "#8B5CF6",
            _ => "#6B7280"
        };

        [NotMapped]
        public TimeSpan? InspectionDuration => StartedAt.HasValue && CompletedAt.HasValue
            ? CompletedAt.Value - StartedAt.Value
            : null;

        #endregion
    }

    /// <summary>
    /// Individual checklist item for QC inspection
    /// </summary>
    public class QCChecklistItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QCInspectionId { get; set; }

        /// <summary>
        /// Checklist item name
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>
        /// Category: Dimensional, Visual, Surface, Material, Documentation
        /// </summary>
        [StringLength(50)]
        public string Category { get; set; } = "Visual";

        /// <summary>
        /// Expected value or specification
        /// </summary>
        [StringLength(200)]
        public string? Specification { get; set; }

        /// <summary>
        /// Actual measured/observed value
        /// </summary>
        [StringLength(200)]
        public string? ActualValue { get; set; }

        /// <summary>
        /// Result: Pass, Fail, NA
        /// </summary>
        [StringLength(20)]
        public string Result { get; set; } = "NA";

        /// <summary>
        /// Is this a critical check point?
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Notes for this check item
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Order in the checklist
        /// </summary>
        public int SortOrder { get; set; }

        #region Navigation Properties

        public virtual QCInspection QCInspection { get; set; } = null!;

        #endregion

        #region Computed Properties

        [NotMapped]
        public string ResultColor => Result switch
        {
            "Pass" => "#10B981",
            "Fail" => "#EF4444",
            "NA" => "#6B7280",
            _ => "#6B7280"
        };

        #endregion
    }
}
