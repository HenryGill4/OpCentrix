using System.ComponentModel.DataAnnotations;
using OpCentrix.Models.Enums;

namespace OpCentrix.Models.Maintenance;

public class MaintenanceWorkOrder
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string WorkOrderNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string MachineId { get; set; } = string.Empty;

    public int? MachineComponentId { get; set; }

    public int? MaintenanceRuleId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public MaintenanceWorkOrderType WorkOrderType { get; set; } = MaintenanceWorkOrderType.Preventive;

    [Required]
    public MaintenanceWorkOrderPriority Priority { get; set; } = MaintenanceWorkOrderPriority.Normal;

    [Required]
    public MaintenanceWorkOrderStatus Status { get; set; } = MaintenanceWorkOrderStatus.Open;

    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    [StringLength(100)]
    public string? AssignedTechnician { get; set; }

    public int? AssignedTechnicianUserId { get; set; }

    public double EstimatedHours { get; set; }
    public double? ActualHours { get; set; }

    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }

    [StringLength(2000)]
    public string? WorkPerformed { get; set; }

    [StringLength(1000)]
    public string? PartsUsed { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? CompletionNotes { get; set; }

    public bool RequiresShutdown { get; set; }

    public int? ShutdownDurationMinutes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    public DateTime? UpdatedAt { get; set; }

    [StringLength(100)]
    public string? UpdatedBy { get; set; }

    // Navigation
    public virtual Machine? Machine { get; set; }
    public virtual MachineComponent? MachineComponent { get; set; }
    public virtual MaintenanceRule? MaintenanceRule { get; set; }
    public virtual User? AssignedTechnicianUser { get; set; }
}
