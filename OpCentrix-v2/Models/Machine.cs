using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using OpCentrix.Models.Enums;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.Models;

public class Machine
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MachineType { get; set; } = "SLS";

    [StringLength(100)]
    public string MachineModel { get; set; } = string.Empty;

    [StringLength(50)]
    public string? SerialNumber { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(50)]
    public string? Department { get; set; }

    public MachineStatus Status { get; set; } = MachineStatus.Idle;

    public bool IsActive { get; set; } = true;

    public bool IsAvailableForScheduling { get; set; } = true;

    [Range(1, 10)]
    public int Priority { get; set; } = 5;

    [StringLength(1000)]
    public string? SupportedMaterials { get; set; }

    [StringLength(100)]
    public string? CurrentMaterial { get; set; }

    public double MaintenanceIntervalHours { get; set; } = 500;

    public double HoursSinceLastMaintenance { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }

    public DateTime? NextMaintenanceDate { get; set; }

    public double TotalOperatingHours { get; set; }

    // SLS-specific
    public double BuildLengthMm { get; set; } = 250;
    public double BuildWidthMm { get; set; } = 250;
    public double BuildHeightMm { get; set; } = 300;
    public double MaxLaserPowerWatts { get; set; } = 400;

    // OPC UA
    [StringLength(200)]
    public string? OpcUaEndpointUrl { get; set; }
    public bool OpcUaEnabled { get; set; }

    [Column(TypeName = "decimal(8,2)")]
    public decimal HourlyRate { get; set; } = 150.00m;

    // Audit
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = string.Empty;

    // Navigation
    public virtual ICollection<MachineComponent> Components { get; set; } = new List<MachineComponent>();
    public virtual ICollection<MaintenanceWorkOrder> MaintenanceWorkOrders { get; set; } = new List<MaintenanceWorkOrder>();
}
