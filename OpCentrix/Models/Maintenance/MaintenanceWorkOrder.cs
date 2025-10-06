using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceWorkOrder
    {
        public int Id { get; set; }
        
        [Required, MaxLength(50)] public string WorkOrderNumber { get; set; } = string.Empty;
        
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        
        public int? MachineComponentId { get; set; }
        
        public int? MaintenanceRuleId { get; set; }
        
        [Required, MaxLength(100)] public string Title { get; set; } = string.Empty;
        
        [MaxLength(1000)] public string? Description { get; set; }
        
        [Required] public MaintenanceWorkOrderType WorkOrderType { get; set; } = MaintenanceWorkOrderType.Preventive;
        
        [Required] public MaintenanceWorkOrderPriority Priority { get; set; } = MaintenanceWorkOrderPriority.Normal;
        
        [Required] public MaintenanceWorkOrderStatus Status { get; set; } = MaintenanceWorkOrderStatus.Open;
        
        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        
        [MaxLength(100)] public string? AssignedTechnician { get; set; }
        public int? AssignedTechnicianUserId { get; set; }
        
        public double EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        
        public decimal EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        
        [MaxLength(2000)] public string? WorkPerformed { get; set; }
        [MaxLength(1000)] public string? PartsUsed { get; set; }
        [MaxLength(1000)] public string? Notes { get; set; }
        [MaxLength(500)] public string? CompletionNotes { get; set; }
        
        public bool RequiresShutdown { get; set; }
        public int? ShutdownDurationMinutes { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Machine? Machine { get; set; }
        public virtual MachineComponent? MachineComponent { get; set; }
        public virtual MaintenanceRule? MaintenanceRule { get; set; }
        public virtual User? AssignedTechnicianUser { get; set; }
    }

    public enum MaintenanceWorkOrderType
    {
        Preventive = 0,
        Corrective = 1,
        Emergency = 2,
        Inspection = 3,
        Calibration = 4,
        Upgrade = 5
    }

    public enum MaintenanceWorkOrderPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3,
        Emergency = 4
    }

    public enum MaintenanceWorkOrderStatus
    {
        Open = 0,
        Assigned = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        OnHold = 5,
        WaitingForParts = 6
    }
}