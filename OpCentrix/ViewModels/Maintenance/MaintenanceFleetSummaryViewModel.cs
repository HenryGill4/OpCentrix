using System;
using System.Collections.Generic;

namespace OpCentrix.ViewModels.Maintenance
{
    public class MaintenanceFleetSummaryViewModel
    {
        public int MachinesWithIssues { get; set; }
        public int OverdueRules { get; set; }
        public int DueSoonRules { get; set; }
        public List<string> MachinesOverdue { get; set; } = new();
        public List<string> MachinesDueSoon { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
