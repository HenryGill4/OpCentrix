using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceService
    {
        public int Id { get; set; }
        
        [Required, MaxLength(100)] public string ServiceName { get; set; } = string.Empty;
        [MaxLength(500)] public string? Description { get; set; }
        
        [Required] public MaintenanceServiceType ServiceType { get; set; } = MaintenanceServiceType.HourTracking;
        
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        public int? MachineComponentId { get; set; }
        
        // Service configuration
        public bool IsEnabled { get; set; } = true;
        public int UpdateIntervalMinutes { get; set; } = 60;
        
        [MaxLength(200)] public string? DataSource { get; set; } // OPC-UA tag, database query, API endpoint
        [MaxLength(1000)] public string? Configuration { get; set; } // JSON configuration
        
        // Current values
        public double CurrentValue { get; set; }
        public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
        public DateTime? LastResetTime { get; set; }
        
        // Thresholds and alerts
        public double? WarningThreshold { get; set; }
        public double? CriticalThreshold { get; set; }
        public double? MaxValue { get; set; }
        
        [MaxLength(20)] public string? Unit { get; set; } // hours, cycles, degrees, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Machine? Machine { get; set; }
        public virtual MachineComponent? MachineComponent { get; set; }
    }

    public enum MaintenanceServiceType
    {
        HourTracking = 0,      // Track machine operating hours from build jobs
        CycleTracking = 1,     // Track number of cycles/builds
        TemperatureMonitoring = 2,  // Monitor temperature levels
        VibrationMonitoring = 3,    // Monitor vibration levels
        PressureMonitoring = 4,     // Monitor pressure levels
        PowerConsumption = 5,       // Track power consumption
        CustomMeter = 6,           // Custom metric tracking
        TimeElapsed = 7,           // Simple time elapsed since last reset
        PartCounter = 8,           // Count parts produced
        ErrorCounter = 9,          // Count errors/failures
        UsagePercentage = 10       // Track usage percentage
    }
}