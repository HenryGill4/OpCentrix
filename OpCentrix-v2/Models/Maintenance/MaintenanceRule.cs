using System.ComponentModel.DataAnnotations;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models.Maintenance;

public class MaintenanceRule
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? MachineId { get; set; }

    public int? MachineComponentId { get; set; }

    public int? ProductionStageId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public MaintenanceTriggerType TriggerType { get; set; }

    [Required]
    public MaintenanceSeverity Severity { get; set; } = MaintenanceSeverity.Info;

    public double ThresholdValue { get; set; }

    public int? IntervalDays { get; set; }

    public bool IsActive { get; set; } = true;

    public int? EarlyWarningPercent { get; set; }

    public int? EarlyWarningDays { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation
    public virtual MachineComponent? MachineComponent { get; set; }
    public virtual ProductionStage? ProductionStage { get; set; }
}
