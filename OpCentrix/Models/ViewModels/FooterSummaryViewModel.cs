using System.Collections.Generic;
using System.Linq;

namespace OpCentrix.Models.ViewModels
{
    public class FooterSummaryViewModel
    {
        public Dictionary<string, double> MachineHours { get; set; } = new();
        public Dictionary<string, int> JobCounts { get; set; } = new();
        public double TotalHours => MachineHours.Values.Sum();
        public int TotalJobs => JobCounts.Values.Sum();
    }
}
