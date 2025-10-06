using System;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.ViewModels.Maintenance
{
    public class RuleStatusDto
    {
        public int RuleId { get; set; }
        public string? MachineId { get; set; }
        public string Title { get; set; } = string.Empty;
        public MaintenanceSeverity Severity { get; set; }
        public double CurrentValue { get; set; }
        public double Threshold { get; set; }
        public double Percent { get; set; }
        public bool IsDue { get; set; }
        public bool IsOverdue { get; set; }
        public DateTime? NextDueDate { get; set; }
        public string DueInText { get; set; } = string.Empty;
        // NEW: Component context (null for machine/global rules)
        public string? ComponentName { get; set; }
    }
}
