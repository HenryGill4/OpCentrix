using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceState
    {
        public int Id { get; set; }
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        public int RuleId { get; set; }

        // Calculated current value since last reset/service
        public double CurrentValue { get; set; }

        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextDueDate { get; set; }

        public bool IsDue { get; set; }
        public bool IsOverdue { get; set; }

        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}
