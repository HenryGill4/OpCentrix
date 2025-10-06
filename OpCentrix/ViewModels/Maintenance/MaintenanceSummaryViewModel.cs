using System.Collections.Generic;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.ViewModels.Maintenance
{
    public class MaintenanceSummaryViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public MaintenanceSeverity HighestSeverity { get; set; }
        public int OverdueCount { get; set; }
        public int DueSoonCount { get; set; }
        public List<RuleStatusDto> Rules { get; set; } = new();
    }
}
