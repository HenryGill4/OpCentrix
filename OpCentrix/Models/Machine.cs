using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Generic machine model supporting all machine types (SLS, EDM, CNC, etc.)
/// Task 6: Machine Status and Dynamic Machine Management - REDESIGNED
/// </summary>
public class Machine
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique machine identifier (e.g., "TI1", "EDM-01", "CNC-02")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable machine name - ADDED: Missing Name property
    /// </summary>
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Machine name for display (keeping existing property name for compatibility)
    /// </summary>
    [Required]
    [StringLength(100)]
    public string MachineName { get; set; } = string.Empty;

    /// <summary>
    /// Machine type (SLS, EDM, CNC, Coating, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string MachineType { get; set; } = "SLS";

    /// <summary>
    /// Machine model/brand (e.g., "TruPrint 3000", "Sodick AP1L", "Haas VF-2")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string MachineModel { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturer serial number
    /// </summary>
    [StringLength(50)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>
    /// Physical location of the machine
    /// </summary>
    [StringLength(100)]
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Department or area (ADDED for missing references)
    /// </summary>
    [StringLength(50)]
    public string Department { get; set; } = string.Empty;

    #region Status and Availability

    /// <summary>
    /// Current machine status
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Idle";

    /// <summary>
    /// Whether the machine is active and operational
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the machine is available for scheduling
    /// </summary>
    public bool IsAvailableForScheduling { get; set; } = true;

    /// <summary>
    /// Machine priority for scheduling (1 = highest, 10 = lowest)
    /// </summary>
    [Range(1, 10)]
    public int Priority { get; set; } = 5;

    /// <summary>
    /// Last status update timestamp
    /// </summary>
    public DateTime LastStatusUpdate { get; set; } = DateTime.UtcNow;

    #endregion

    #region Generic Machine Specifications

    /// <summary>
    /// JSON string storing machine-specific specifications
    /// Different for each machine type
    /// </summary>
    [StringLength(2000)]
    public string TechnicalSpecifications { get; set; } = "{}";

    /// <summary>
    /// Supported materials/processes (comma-separated)
    /// </summary>
    [StringLength(1000)]
    public string SupportedMaterials { get; set; } = string.Empty;

    /// <summary>
    /// Current material loaded in machine
    /// </summary>
    [StringLength(100)]
    public string CurrentMaterial { get; set; } = string.Empty;

    #endregion

    #region Maintenance and Monitoring

    /// <summary>
    /// Maintenance interval in hours
    /// </summary>
    [Range(1, 10000)]
    public double MaintenanceIntervalHours { get; set; } = 500;

    /// <summary>
    /// Hours since last maintenance
    /// </summary>
    public double HoursSinceLastMaintenance { get; set; } = 0;

    /// <summary>
    /// Date of last maintenance
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Next scheduled maintenance date
    /// </summary>
    public DateTime? NextMaintenanceDate { get; set; }

    /// <summary>
    /// Whether machine requires maintenance
    /// </summary>
    [NotMapped]
    public bool RequiresMaintenance => 
        HoursSinceLastMaintenance >= MaintenanceIntervalHours;

    /// <summary>
    /// Average utilization percentage
    /// </summary>
    [Range(0, 100)]
    public double AverageUtilizationPercent { get; set; } = 0;

    /// <summary>
    /// Maintenance notes (ADDED for missing references)
    /// </summary>
    [StringLength(2000)]
    public string MaintenanceNotes { get; set; } = string.Empty;

    /// <summary>
    /// Operator notes (ADDED for missing references)
    /// </summary>
    [StringLength(2000)]
    public string OperatorNotes { get; set; } = string.Empty;

    #endregion

    #region Integration and Communication

    /// <summary>
    /// OPC UA endpoint URL for machine communication
    /// </summary>
    [StringLength(200)]
    public string OpcUaEndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether OPC UA communication is enabled
    /// </summary>
    public bool OpcUaEnabled { get; set; } = false;

    /// <summary>
    /// JSON string for additional communication settings
    /// </summary>
    [StringLength(1000)]
    public string CommunicationSettings { get; set; } = "{}";

    #endregion

    #region Relationships

    /// <summary>
    /// Machine capabilities
    /// </summary>
    public virtual ICollection<MachineCapability> Capabilities { get; set; } = new List<MachineCapability>();

    /// <summary>
    /// Current job running on this machine
    /// </summary>
    public virtual Job? CurrentJob { get; set; }

    #endregion

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

    #region Helper Methods and Properties

    /// <summary>
    /// Get machine type display name
    /// </summary>
    [NotMapped]
    public string MachineTypeDisplay => MachineType switch
    {
        "SLS" => "Selective Laser Sintering",
        "EDM" => "Electrical Discharge Machining",
        "CNC" => "Computer Numerical Control",
        "Coating" => "Coating/Cerakote",
        "Assembly" => "Assembly Station",
        "Inspection" => "Quality Inspection",
        _ => MachineType
    };

    /// <summary>
    /// Get status color for UI display
    /// </summary>
    [NotMapped]
    public string StatusColor => Status?.ToLower() switch
    {
        "idle" => "#10B981", // green
        "running" => "#F59E0B", // amber
        "building" => "#F59E0B", // amber (SLS-specific)
        "machining" => "#F59E0B", // amber (CNC-specific)
        "coating" => "#8B5CF6", // purple (Coating-specific)
        "maintenance" => "#6B7280", // gray
        "error" => "#EF4444", // red
        "offline" => "#6B7280", // gray
        "setup" => "#3B82F6", // blue
        _ => "#6B7280" // gray default
    };

    /// <summary>
    /// Get priority color for UI display
    /// </summary>
    [NotMapped]
    public string PriorityColor => Priority switch
    {
        1 => "#DC2626", // red (critical)
        2 => "#F59E0B", // orange (high)
        3 => "#EAB308", // yellow (elevated)
        4 => "#22C55E", // green (normal)
        5 => "#10B981", // emerald (normal)
        >= 6 => "#6B7280", // gray (low)
        _ => "#6B7280"
    };

    /// <summary>
    /// Check if machine can accommodate a specific part
    /// </summary>
    public bool CanAccommodatePart(Part part)
    {
        if (!IsActive || !IsAvailableForScheduling)
            return false;

        // Check material compatibility
        if (!string.IsNullOrEmpty(part.SlsMaterial) && 
            !string.IsNullOrEmpty(SupportedMaterials) &&
            !SupportedMaterials.Contains(part.SlsMaterial, StringComparison.OrdinalIgnoreCase))
            return false;

        // Add more compatibility checks based on machine type
        return MachineType switch
        {
            "SLS" => CanAccommodateSlsPart(part),
            "EDM" => CanAccommodateEdmPart(part),
            "CNC" => CanAccommodateCncPart(part),
            _ => true // Generic accommodation
        };
    }

    private bool CanAccommodateSlsPart(Part part) 
    {
        // SLS-specific validation logic
        var specs = GetTechnicalSpecifications();
        
        // Check build volume if specified
        if (specs.ContainsKey("BuildLengthMm") && part.LengthMm > 0)
        {
            if (part.LengthMm > Convert.ToDouble(specs["BuildLengthMm"]))
                return false;
        }

        return true;
    }

    private bool CanAccommodateEdmPart(Part part)
    {
        // EDM-specific validation logic
        return true; // Placeholder
    }

    private bool CanAccommodateCncPart(Part part)
    {
        // CNC-specific validation logic
        return true; // Placeholder
    }

    /// <summary>
    /// Parse technical specifications from JSON
    /// </summary>
    public Dictionary<string, object> GetTechnicalSpecifications()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(TechnicalSpecifications) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Set technical specifications as JSON
    /// </summary>
    public void SetTechnicalSpecifications(Dictionary<string, object> specifications)
    {
        TechnicalSpecifications = System.Text.Json.JsonSerializer.Serialize(specifications);
    }

    #endregion

    #region SLS-Specific Properties (for backward compatibility)

    /// <summary>
    /// Build envelope length in mm (SLS machines)
    /// </summary>
    [Range(50, 500)]
    public double BuildLengthMm { get; set; } = 250;

    /// <summary>
    /// Build envelope width in mm (SLS machines)
    /// </summary>
    [Range(50, 500)]
    public double BuildWidthMm { get; set; } = 250;

    /// <summary>
    /// Build envelope height in mm (SLS machines)
    /// </summary>
    [Range(50, 500)]
    public double BuildHeightMm { get; set; } = 300;

    /// <summary>
    /// Build volume in cubic meters - ADDED: Missing BuildVolumeM3 property
    /// </summary>
    [NotMapped]
    public double BuildVolumeM3 => (BuildLengthMm * BuildWidthMm * BuildHeightMm) / 1_000_000_000.0;

    /// <summary>
    /// Maximum laser power in watts (SLS machines)
    /// </summary>
    [Range(100, 1000)]
    public double MaxLaserPowerWatts { get; set; } = 400;

    /// <summary>
    /// Maximum scan speed in mm/s (SLS machines)
    /// </summary>
    [Range(1000, 10000)]
    public double MaxScanSpeedMmPerSec { get; set; } = 7000;

    /// <summary>
    /// Minimum layer thickness in microns (SLS machines)
    /// </summary>
    [Range(10, 100)]
    public double MinLayerThicknessMicrons { get; set; } = 20;

    /// <summary>
    /// Maximum layer thickness in microns (SLS machines)
    /// </summary>
    [Range(10, 100)]
    public double MaxLayerThicknessMicrons { get; set; } = 60;

    /// <summary>
    /// Total operating hours
    /// </summary>
    public double TotalOperatingHours { get; set; } = 0;

    #endregion
}