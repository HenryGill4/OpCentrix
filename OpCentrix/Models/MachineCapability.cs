using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents machine capabilities and configurations
/// Links machines to their supported materials, processes, and operational parameters
/// </summary>
public class MachineCapability
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the machine
    /// </summary>
    [Required]
    public int SlsMachineId { get; set; }

    /// <summary>
    /// Navigation property to SlsMachine
    /// </summary>
    public virtual SlsMachine SlsMachine { get; set; } = null!;

    /// <summary>
    /// Capability type (e.g., "Material", "Process", "Quality")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string CapabilityType { get; set; } = string.Empty;

    /// <summary>
    /// Capability name (e.g., "Ti-6Al-4V Grade 5", "High Precision")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string CapabilityName { get; set; } = string.Empty;

    /// <summary>
    /// Capability value or specification
    /// </summary>
    [StringLength(500)]
    public string CapabilityValue { get; set; } = string.Empty;

    /// <summary>
    /// Whether this capability is currently available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Priority level for this capability (1 = highest, 5 = lowest)
    /// </summary>
    [Range(1, 5)]
    public int Priority { get; set; } = 3;

    /// <summary>
    /// Minimum value for numeric capabilities
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// Maximum value for numeric capabilities
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// Unit of measurement for numeric capabilities
    /// </summary>
    [StringLength(20)]
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Additional notes or requirements
    /// </summary>
    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Certification or qualification required for this capability
    /// </summary>
    [StringLength(200)]
    public string RequiredCertification { get; set; } = string.Empty;

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
    /// Helper method to check if a value is within capability range
    /// </summary>
    public bool IsValueInRange(double value)
    {
        if (!MinValue.HasValue && !MaxValue.HasValue)
            return true;

        if (MinValue.HasValue && value < MinValue.Value)
            return false;

        if (MaxValue.HasValue && value > MaxValue.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{CapabilityType}: {CapabilityName}";

    /// <summary>
    /// Helper property for range display
    /// </summary>
    [NotMapped]
    public string RangeDisplay
    {
        get
        {
            if (!MinValue.HasValue && !MaxValue.HasValue)
                return CapabilityValue;

            if (MinValue.HasValue && MaxValue.HasValue)
                return $"{MinValue} - {MaxValue} {Unit}".Trim();

            if (MinValue.HasValue)
                return $"? {MinValue} {Unit}".Trim();

            if (MaxValue.HasValue)
                return $"? {MaxValue} {Unit}".Trim();

            return CapabilityValue;
        }
    }
}