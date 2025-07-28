using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents quality inspection checkpoints for parts
/// Defines the inspection steps and requirements for each part
/// FIXED: Removed duplicate DefectCategoryId1 property to fix database schema error
/// </summary>
public class InspectionCheckpoint
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the part this checkpoint belongs to
    /// </summary>
    [Required]
    public int PartId { get; set; }

    /// <summary>
    /// Navigation property to Part
    /// </summary>
    public virtual Part Part { get; set; } = null!;

    /// <summary>
    /// Optional reference to a defect category
    /// FIXED: Single DefectCategoryId property to prevent schema issues
    /// </summary>
    public int? DefectCategoryId { get; set; }

    /// <summary>
    /// Navigation property to DefectCategory
    /// FIXED: Proper nullable navigation property
    /// </summary>
    public virtual DefectCategory? DefectCategory { get; set; }

    /// <summary>
    /// Checkpoint name or title
    /// </summary>
    [Required]
    [StringLength(200)]
    public string CheckpointName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of what to inspect
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of inspection (Dimensional, Visual, Functional, Destructive, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string InspectionType { get; set; } = "Visual";

    /// <summary>
    /// Checkpoint execution order
    /// </summary>
    public int SortOrder { get; set; } = 100;

    /// <summary>
    /// Whether this checkpoint is mandatory
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Whether this checkpoint is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Expected duration for this checkpoint in minutes
    /// </summary>
    public int EstimatedMinutes { get; set; } = 5;

    /// <summary>
    /// Tolerance or acceptance criteria
    /// </summary>
    [StringLength(500)]
    public string AcceptanceCriteria { get; set; } = string.Empty;

    /// <summary>
    /// Measurement method or procedure
    /// </summary>
    [StringLength(500)]
    public string MeasurementMethod { get; set; } = string.Empty;

    /// <summary>
    /// Required tools or equipment
    /// </summary>
    [StringLength(500)]
    public string RequiredEquipment { get; set; } = string.Empty;

    /// <summary>
    /// Specific skills or certifications required
    /// </summary>
    [StringLength(500)]
    public string RequiredSkills { get; set; } = string.Empty;

    /// <summary>
    /// Reference documents (drawings, specifications, etc.)
    /// </summary>
    [StringLength(500)]
    public string ReferenceDocuments { get; set; } = string.Empty;

    /// <summary>
    /// Target dimension or value (for dimensional checks)
    /// </summary>
    public double? TargetValue { get; set; }

    /// <summary>
    /// Upper tolerance limit
    /// </summary>
    public double? UpperTolerance { get; set; }

    /// <summary>
    /// Lower tolerance limit
    /// </summary>
    public double? LowerTolerance { get; set; }

    /// <summary>
    /// Unit of measurement
    /// </summary>
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Actions to take if checkpoint fails
    /// </summary>
    [StringLength(500)]
    public string FailureAction { get; set; } = "Hold for review";

    /// <summary>
    /// Sample size or quantity to inspect
    /// </summary>
    public int SampleSize { get; set; } = 1;

    /// <summary>
    /// Sampling method (All, Random, First/Last, etc.)
    /// </summary>
    [StringLength(50)]
    public string SamplingMethod { get; set; } = "All";

    /// <summary>
    /// Checkpoint category for grouping
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "Quality";

    /// <summary>
    /// Priority level (1 = Critical, 5 = Minor)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Additional notes or special instructions
    /// </summary>
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Audit fields
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    /// <summary>
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{SortOrder}. {CheckpointName}";

    /// <summary>
    /// Helper property to check if this is a dimensional checkpoint
    /// </summary>
    [NotMapped]
    public bool IsDimensional => InspectionType.ToLower().Contains("dimensional") && TargetValue.HasValue;

    /// <summary>
    /// Helper property to get tolerance range display
    /// </summary>
    [NotMapped]
    public string ToleranceDisplay
    {
        get
        {
            if (!TargetValue.HasValue)
                return AcceptanceCriteria;

            if (UpperTolerance.HasValue && LowerTolerance.HasValue)
            {
                var upper = TargetValue.Value + UpperTolerance.Value;
                var lower = TargetValue.Value - LowerTolerance.Value;
                return $"{lower:F3} - {upper:F3} {Unit}".Trim();
            }

            if (UpperTolerance.HasValue || LowerTolerance.HasValue)
            {
                var tolerance = UpperTolerance ?? LowerTolerance ?? 0;
                return $"{TargetValue:F3} ± {tolerance:F3} {Unit}".Trim();
            }

            return $"{TargetValue:F3} {Unit}".Trim();
        }
    }

    /// <summary>
    /// Check if a measured value is within tolerance
    /// </summary>
    public bool IsValueWithinTolerance(double measuredValue)
    {
        if (!TargetValue.HasValue)
            return true; // No target means any value is acceptable

        if (UpperTolerance.HasValue)
        {
            var upper = TargetValue.Value + UpperTolerance.Value;
            if (measuredValue > upper)
                return false;
        }

        if (LowerTolerance.HasValue)
        {
            var lower = TargetValue.Value - LowerTolerance.Value;
            if (measuredValue < lower)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Get the priority color for UI display
    /// </summary>
    public string GetPriorityColor()
    {
        return Priority switch
        {
            1 => "#DC2626", // red (critical)
            2 => "#F59E0B", // orange (high)
            3 => "#10B981", // green (normal)
            4 => "#6B7280", // gray (low)
            5 => "#9CA3AF", // light gray (minor)
            _ => "#6B7280"
        };
    }
}