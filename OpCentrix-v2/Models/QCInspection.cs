using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

public class QCInspection
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string InspectionNumber { get; set; } = string.Empty;

    public int? JobId { get; set; }

    public int? BuildJobId { get; set; }

    public int? PartInstanceId { get; set; }

    [Required]
    public int PartId { get; set; }

    [Required]
    [StringLength(50)]
    public string InspectionType { get; set; } = "PostPrint";

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending";

    #region Quantities

    [Required]
    [Range(1, 1000)]
    public int TotalQuantity { get; set; } = 1;

    [Range(0, 1000)]
    public int PassedQuantity { get; set; }

    [Range(0, 1000)]
    public int FailedQuantity { get; set; }

    [Range(0, 1000)]
    public int ReworkQuantity { get; set; }

    [Range(0, 1000)]
    public int ScrappedQuantity { get; set; }

    #endregion

    #region Inspection Details

    public int? InspectorUserId { get; set; }

    [StringLength(100)]
    public string? InspectorName { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [Range(0, 100)]
    public double? QualityScore { get; set; }

    #endregion

    #region Measurements and Results

    [Column(TypeName = "TEXT")]
    public string? DimensionalResultsJson { get; set; }

    public double? SurfaceRoughnessRa { get; set; }
    public double? DensityPercent { get; set; }

    [StringLength(2000)]
    public string? VisualInspectionNotes { get; set; }

    [Column(TypeName = "TEXT")]
    public string? DefectsJson { get; set; }

    [StringLength(2000)]
    public string? CorrectiveActions { get; set; }

    #endregion

    #region Audit

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    public DateTime? LastModifiedAt { get; set; }

    [StringLength(100)]
    public string? LastModifiedBy { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    #endregion

    #region Navigation

    public virtual Job? Job { get; set; }
    public virtual BuildJob? BuildJob { get; set; }
    public virtual Part Part { get; set; } = null!;
    public virtual PartInstance? PartInstance { get; set; }
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
    public TimeSpan? InspectionDuration => StartedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;

    #endregion
}

public class QCChecklistItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int QCInspectionId { get; set; }

    [Required]
    [StringLength(200)]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(50)]
    public string Category { get; set; } = "Visual";

    [StringLength(200)]
    public string? Specification { get; set; }

    [StringLength(200)]
    public string? ActualValue { get; set; }

    [StringLength(20)]
    public string Result { get; set; } = "NA";

    public bool IsCritical { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    // Navigation
    public virtual QCInspection QCInspection { get; set; } = null!;
}
