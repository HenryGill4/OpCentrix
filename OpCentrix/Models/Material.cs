using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Material model for manufacturing processes
/// Enhanced for Task 6: Machine Material Management
/// </summary>
public class Material
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Material code/identifier (e.g., "TI64-G5", "IN718")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// Full material name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>
    /// Material type category
    /// </summary>
    [Required]
    [StringLength(50)]
    public string MaterialType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the material
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Density in g/cm³
    /// </summary>
    [Range(0.1, 50.0)]
    public double Density { get; set; } = 4.43; // Ti-6Al-4V default

    /// <summary>
    /// Melting point in Celsius
    /// </summary>
    [Range(100, 5000)]
    public double MeltingPointC { get; set; } = 1660; // Ti-6Al-4V default

    /// <summary>
    /// Whether this material is active/available
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Cost per gram
    /// </summary>
    [Range(0, 1000)]
    public decimal CostPerGram { get; set; } = 0.50m;

    /// <summary>
    /// Default layer thickness in microns
    /// </summary>
    [Range(10, 200)]
    public double DefaultLayerThicknessMicrons { get; set; } = 30;

    /// <summary>
    /// Default laser power percentage
    /// </summary>
    [Range(10, 100)]
    public double DefaultLaserPowerPercent { get; set; } = 85;

    /// <summary>
    /// Default scan speed in mm/s
    /// </summary>
    [Range(500, 10000)]
    public double DefaultScanSpeedMmPerSec { get; set; } = 1200;

    /// <summary>
    /// Material properties as JSON string
    /// </summary>
    [StringLength(2000)]
    public string MaterialProperties { get; set; } = "{}";

    /// <summary>
    /// Compatible machine types (comma-separated)
    /// </summary>
    [StringLength(500)]
    public string CompatibleMachineTypes { get; set; } = "SLS";

    /// <summary>
    /// Safety notes and warnings
    /// </summary>
    [StringLength(1000)]
    public string SafetyNotes { get; set; } = string.Empty;

    #region Audit Fields
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [Required]
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";
    #endregion

    #region Helper Methods
    /// <summary>
    /// Get material display name for UI
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{MaterialCode} - {MaterialName}";

    /// <summary>
    /// Get material type color for UI
    /// </summary>
    [NotMapped]
    public string MaterialTypeColor => MaterialType.ToLower() switch
    {
        "titanium" => "#8B5CF6", // purple
        "steel" => "#6B7280", // gray
        "aluminum" => "#10B981", // emerald
        "nickel" => "#F59E0B", // amber
        "cobalt" => "#EF4444", // red
        "ceramic" => "#3B82F6", // blue
        _ => "#6B7280" // gray default
    };

    /// <summary>
    /// Check if material is compatible with machine type
    /// </summary>
    public bool IsCompatibleWithMachineType(string machineType)
    {
        if (string.IsNullOrEmpty(CompatibleMachineTypes) || string.IsNullOrEmpty(machineType))
            return true;

        return CompatibleMachineTypes.Contains(machineType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parse material properties from JSON
    /// </summary>
    public Dictionary<string, object> GetMaterialProperties()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(MaterialProperties) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Set material properties as JSON
    /// </summary>
    public void SetMaterialProperties(Dictionary<string, object> properties)
    {
        MaterialProperties = System.Text.Json.JsonSerializer.Serialize(properties);
    }
    #endregion

    #region Static Material Definitions
    /// <summary>
    /// Get common SLS materials
    /// </summary>
    public static List<Material> GetCommonSlsMaterials()
    {
        return new List<Material>
        {
            new()
            {
                MaterialCode = "TI64-G5",
                MaterialName = "Ti-6Al-4V Grade 5",
                MaterialType = "Titanium",
                Description = "Aerospace grade titanium alloy with excellent strength-to-weight ratio",
                Density = 4.43,
                MeltingPointC = 1660,
                CostPerGram = 0.45m,
                DefaultLayerThicknessMicrons = 30,
                DefaultLaserPowerPercent = 85,
                DefaultScanSpeedMmPerSec = 1200,
                CompatibleMachineTypes = "SLS",
                SafetyNotes = "Handle with care. May cause respiratory irritation if inhaled."
            },
            new()
            {
                MaterialCode = "TI64-G23",
                MaterialName = "Ti-6Al-4V Grade 23 (ELI)",
                MaterialType = "Titanium",
                Description = "Extra Low Interstitial grade for medical implants",
                Density = 4.43,
                MeltingPointC = 1660,
                CostPerGram = 0.65m,
                DefaultLayerThicknessMicrons = 25,
                DefaultLaserPowerPercent = 80,
                DefaultScanSpeedMmPerSec = 1100,
                CompatibleMachineTypes = "SLS",
                SafetyNotes = "Medical grade material. Handle with care."
            },
            new()
            {
                MaterialCode = "IN718",
                MaterialName = "Inconel 718",
                MaterialType = "Nickel",
                Description = "High-temperature nickel-chromium superalloy",
                Density = 8.19,
                MeltingPointC = 1336,
                CostPerGram = 0.75m,
                DefaultLayerThicknessMicrons = 40,
                DefaultLaserPowerPercent = 90,
                DefaultScanSpeedMmPerSec = 1000,
                CompatibleMachineTypes = "SLS",
                SafetyNotes = "High-temperature alloy. Use appropriate safety equipment."
            },
            new()
            {
                MaterialCode = "SS316L",
                MaterialName = "Stainless Steel 316L",
                MaterialType = "Steel",
                Description = "Austenitic stainless steel with low carbon content",
                Density = 8.0,
                MeltingPointC = 1400,
                CostPerGram = 0.15m,
                DefaultLayerThicknessMicrons = 30,
                DefaultLaserPowerPercent = 75,
                DefaultScanSpeedMmPerSec = 1500,
                CompatibleMachineTypes = "SLS",
                SafetyNotes = "General purpose stainless steel. Standard safety precautions apply."
            },
            new()
            {
                MaterialCode = "ALSI10MG",
                MaterialName = "AlSi10Mg",
                MaterialType = "Aluminum",
                Description = "Aluminum-silicon alloy for lightweight applications",
                Density = 2.67,
                MeltingPointC = 570,
                CostPerGram = 0.25m,
                DefaultLayerThicknessMicrons = 30,
                DefaultLaserPowerPercent = 70,
                DefaultScanSpeedMmPerSec = 1800,
                CompatibleMachineTypes = "SLS",
                SafetyNotes = "Aluminum powder - fire hazard. Keep away from ignition sources."
            }
        };
    }
    #endregion
}