using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.Maintenance
{
    public class MaintenanceActionLog
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        [Required, MaxLength(50)] public string MachineId { get; set; } = string.Empty;
        public int PerformedByUserId { get; set; }
        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
        [MaxLength(1000)] public string? Notes { get; set; }
        public bool ResetPerformed { get; set; }
    }
}
