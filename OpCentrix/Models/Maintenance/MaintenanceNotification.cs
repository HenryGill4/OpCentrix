using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceNotification
    {
        public int Id { get; set; }
        
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        public int? MaintenanceRuleId { get; set; }
        public int? MaintenanceServiceId { get; set; }
        public int? WorkOrderId { get; set; }
        
        [Required] public MaintenanceNotificationType NotificationType { get; set; }
        [Required] public MaintenanceNotificationSeverity Severity { get; set; }
        
        [Required, MaxLength(200)] public string Title { get; set; } = string.Empty;
        [MaxLength(1000)] public string? Message { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcknowledgedAt { get; set; }
        [MaxLength(100)] public string? AcknowledgedBy { get; set; }
        
        public bool IsEmailSent { get; set; }
        public bool IsSmsSent { get; set; }
        public bool IsBrowserNotificationSent { get; set; }
        
        [MaxLength(500)] public string? Recipients { get; set; } // comma-separated list
        
        public DateTime? AutoDismissAt { get; set; }
        public bool IsDismissed { get; set; }
        
        // Navigation properties
        public virtual Machine? Machine { get; set; }
        public virtual MaintenanceRule? MaintenanceRule { get; set; }
        public virtual MaintenanceService? MaintenanceService { get; set; }
        public virtual MaintenanceWorkOrder? WorkOrder { get; set; }
    }

    public enum MaintenanceNotificationType
    {
        MaintenanceDue = 0,
        MaintenanceOverdue = 1,
        ServiceAlert = 2,
        WorkOrderCreated = 3,
        WorkOrderCompleted = 4,
        ThresholdExceeded = 5,
        SystemFailure = 6
    }

    public enum MaintenanceNotificationSeverity
    {
        Info = 0,
        Warning = 1,
        Critical = 2,
        Emergency = 3
    }
}