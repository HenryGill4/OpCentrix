using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceSchedule
    {
        public int Id { get; set; }
        
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        public int? MachineComponentId { get; set; }
        
        [Required, MaxLength(100)] public string ScheduleName { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
        
        [Required] public MaintenanceScheduleType ScheduleType { get; set; } = MaintenanceScheduleType.TimeInterval;
        
        // Time-based scheduling
        public int? IntervalDays { get; set; }
        public int? IntervalWeeks { get; set; }
        public int? IntervalMonths { get; set; }
        
        // Usage-based scheduling
        public double? OperatingHoursInterval { get; set; }
        public int? CyclesInterval { get; set; }
        public int? UnitsProducedInterval { get; set; }
        
        // Condition-based scheduling
        public double? ConditionThreshold { get; set; }
        [MaxLength(100)] public string? ConditionMetric { get; set; }
        
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        
        public double EstimatedDurationHours { get; set; }
        public decimal EstimatedCost { get; set; }
        
        [MaxLength(100)] public string? DefaultTechnician { get; set; }
        public int? DefaultTechnicianUserId { get; set; }
        
        public bool IsActive { get; set; } = true;
        public bool AutoCreateWorkOrders { get; set; } = true;
        public int? LeadTimeDays { get; set; } = 7;
        
        [MaxLength(1000)] public string? Instructions { get; set; }
        [MaxLength(500)] public string? RequiredTools { get; set; }
        [MaxLength(500)] public string? RequiredParts { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Machine? Machine { get; set; }
        public virtual MachineComponent? MachineComponent { get; set; }
        public virtual User? DefaultTechnicianUser { get; set; }
    }

    public enum MaintenanceScheduleType
    {
        TimeInterval = 0,
        OperatingHours = 1,
        CycleCount = 2,
        UnitsProduced = 3,
        ConditionBased = 4,
        Calendar = 5
    }
}