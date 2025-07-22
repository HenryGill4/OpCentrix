using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.Models.ViewModels
{
    public class MachineRowViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public List<DateTime> Dates { get; set; } = new();
        public List<Job> Jobs { get; set; } = new();
    }
}
