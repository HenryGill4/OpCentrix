using System.Collections.Generic;

namespace OpCentrix.Models.ViewModels
{
    public class FooterSummaryViewModel
    {
        public Dictionary<string, double> MachineHours { get; set; } = new();
    }
}
