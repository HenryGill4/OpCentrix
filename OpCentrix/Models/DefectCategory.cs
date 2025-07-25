using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents defect categories used in quality control and inspection
/// Provides standardized defect classification for consistent reporting
/// </summary>
public class DefectCategory
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Defect category name
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the defect category
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Defect category code for quick reference
    /// </summary>
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Severity level (1 = Critical, 5 = Minor)
    /// </summary>
    [Range(1, 5)]
    public int SeverityLevel { get; set; } = 3;

    /// <summary>
    /// Whether this defect category is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Category group for organizing defects (Surface, Dimensional, Material, etc.)
    /// </summary>
    [StringLength(50)]
    public string CategoryGroup { get; set; } = "General";

    /// <summary>
    /// Applicable to specific processes or materials
    /// </summary>
    [StringLength(200)]
    public string ApplicableProcesses { get; set; } = string.Empty;

    /// <summary>
    /// Standard corrective actions for this defect
    /// </summary>
    [StringLength(1000)]
    public string StandardCorrectiveActions { get; set; } = string.Empty;

    /// <summary>
    /// Prevention methods or root cause information
    /// </summary>
    [StringLength(1000)]
    public string PreventionMethods { get; set; } = string.Empty;

    /// <summary>
    /// Whether this defect requires immediate notification
    /// </summary>
    public bool RequiresImmediateNotification { get; set; } = false;

    /// <summary>
    /// Cost impact of this defect (Low, Medium, High, Critical)
    /// </summary>
    [StringLength(20)]
    public string CostImpact { get; set; } = "Medium";

    /// <summary>
    /// Average time to resolve this defect in minutes
    /// </summary>
    public int AverageResolutionTimeMinutes { get; set; } = 30;

    /// <summary>
    /// Sort order for display purposes
    /// </summary>
    public int SortOrder { get; set; } = 100;

    /// <summary>
    /// Color hex code for visual identification
    /// </summary>
    [StringLength(7)]
    public string ColorCode { get; set; } = "#6B7280";

    /// <summary>
    /// Icon or symbol for this defect category
    /// </summary>
    [StringLength(50)]
    public string Icon { get; set; } = "exclamation-triangle";

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
    /// Navigation property - inspection checkpoints that use this defect category
    /// </summary>
    public virtual ICollection<InspectionCheckpoint> InspectionCheckpoints { get; set; } = new List<InspectionCheckpoint>();

    /// <summary>
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayName => string.IsNullOrEmpty(Code) ? Name : $"{Code} - {Name}";

    /// <summary>
    /// Helper property to get severity text
    /// </summary>
    [NotMapped]
    public string SeverityText => SeverityLevel switch
    {
        1 => "Critical",
        2 => "High",
        3 => "Medium",
        4 => "Low",
        5 => "Minor",
        _ => "Unknown"
    };

    /// <summary>
    /// Helper property to get severity color
    /// </summary>
    [NotMapped]
    public string SeverityColor => SeverityLevel switch
    {
        1 => "#DC2626", // red (critical)
        2 => "#F59E0B", // orange (high)
        3 => "#FBBF24", // yellow (medium)
        4 => "#10B981", // green (low)
        5 => "#6B7280", // gray (minor)
        _ => "#6B7280"
    };

    /// <summary>
    /// Check if this defect category can be safely deleted
    /// </summary>
    public bool CanBeDeleted()
    {
        // Cannot delete if referenced by inspection checkpoints or other records
        return !InspectionCheckpoints.Any();
    }

    /// <summary>
    /// Get applicable processes as a list
    /// </summary>
    public List<string> GetApplicableProcessesList()
    {
        if (string.IsNullOrEmpty(ApplicableProcesses))
            return new List<string>();

        return ApplicableProcesses.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                  .Select(p => p.Trim())
                                  .ToList();
    }

    /// <summary>
    /// Set applicable processes from a list
    /// </summary>
    public void SetApplicableProcessesList(List<string> processes)
    {
        ApplicableProcesses = string.Join(", ", processes.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}

/// <summary>
/// Common defect category groups
/// </summary>
public static class DefectCategoryGroups
{
    public const string Surface = "Surface";
    public const string Dimensional = "Dimensional";
    public const string Material = "Material";
    public const string Functional = "Functional";
    public const string Cosmetic = "Cosmetic";
    public const string Assembly = "Assembly";
    public const string Process = "Process";
    public const string Handling = "Handling";
    public const string Documentation = "Documentation";
    public const string Other = "Other";
}

/// <summary>
/// Common cost impact levels
/// </summary>
public static class CostImpactLevels
{
    public const string Low = "Low";
    public const string Medium = "Medium";
    public const string High = "High";
    public const string Critical = "Critical";
}