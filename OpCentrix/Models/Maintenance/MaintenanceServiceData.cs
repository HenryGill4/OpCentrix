using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceServiceData
    {
        public int Id { get; set; }
        
        public int MaintenanceServiceId { get; set; }
        
        public double Value { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        [MaxLength(100)] public string? Source { get; set; } // Which system/service provided this data
        [MaxLength(500)] public string? Notes { get; set; }
        
        public bool IsCalculated { get; set; } // True if this is a calculated/derived value
        public bool IsReset { get; set; } // True if this represents a reset event
        
        // Navigation properties
        public virtual MaintenanceService? MaintenanceService { get; set; }
    }
}