using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceRule
    {
        public int Id { get; set; }

        // If null, rule applies to all machines (global rule)
        [MaxLength(50)]
        public string? MachineId { get; set; }

        // OPTIONAL: Target specific machine component
        public int? MachineComponentId { get; set; }

        // OPTIONAL: Target specific production stage (stage level maintenance like Sieve Station)
        public int? ProductionStageId { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public MaintenanceTriggerType TriggerType { get; set; }

        [Required]
        public MaintenanceSeverity Severity { get; set; } = MaintenanceSeverity.Info;

        // Primary numeric threshold (hours, builds count, meter value etc.)
        public double ThresholdValue { get; set; }

        // For DateInterval rules (days between services)
        public int? IntervalDays { get; set; }

        public bool IsActive { get; set; } = true;

        // Early warning customization (percent of threshold reached)
        public int? EarlyWarningPercent { get; set; }

        // Early warning by days remaining (for DateInterval)
        public int? EarlyWarningDays { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(100)] public string CreatedBy { get; set; } = "System";
        public DateTime? UpdatedAt { get; set; }
        [MaxLength(100)] public string? UpdatedBy { get; set; }
    }
}
