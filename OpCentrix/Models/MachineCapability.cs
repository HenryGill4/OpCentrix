using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents machine capabilities and configurations for any machine type
/// Links machines to their supported materials, processes, and operational parameters
/// Task 6: Machine Status and Dynamic Machine Management - REDESIGNED
/// </summary>
public class MachineCapability
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Reference to the machine (FIXED: Generic Machine instead of SlsMachine)
    /// </summary>
    [Required]
    public int MachineId { get; set; }

    /// <summary>
    /// Navigation property to Machine (FIXED: Generic Machine)
    /// </summary>
    public virtual Machine Machine { get; set; } = null!;

    /// <summary>
    /// Capability type (e.g., "Material", "Process", "Quality", "Tooling")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string CapabilityType { get; set; } = string.Empty;

    /// <summary>
    /// Capability name (e.g., "Ti-6Al-4V Grade 5", "High Precision", "Wire EDM")
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
                return $">= {MinValue} {Unit}".Trim();

            if (MaxValue.HasValue)
                return $"<= {MaxValue} {Unit}".Trim();

            return CapabilityValue;
        }
    }

    /// <summary>
    /// Helper property for machine type-specific capability validation
    /// </summary>
    [NotMapped]
    public bool IsValidForMachineType => Machine?.MachineType switch
    {
        "SLS" => IsValidForSls(),
        "EDM" => IsValidForEdm(),
        "CNC" => IsValidForCnc(),
        "Coating" => IsValidForCoating(),
        _ => true // Generic capabilities are always valid
    };

    private bool IsValidForSls()
    {
        // SLS-specific capability validation
        var validTypes = new[] { "Material", "Process Parameter", "Quality Metric", "Build Configuration" };
        return validTypes.Contains(CapabilityType, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidForEdm()
    {
        // EDM-specific capability validation
        var validTypes = new[] { "Material", "Tooling", "Process Parameter", "Surface Finish" };
        return validTypes.Contains(CapabilityType, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidForCnc()
    {
        // CNC-specific capability validation
        var validTypes = new[] { "Material", "Tooling", "Spindle Speed", "Feed Rate", "Accuracy" };
        return validTypes.Contains(CapabilityType, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidForCoating()
    {
        // Coating-specific capability validation
        var validTypes = new[] { "Coating Type", "Substrate Material", "Thickness Range", "Curing Process" };
        return validTypes.Contains(CapabilityType, StringComparer.OrdinalIgnoreCase);
    }
}