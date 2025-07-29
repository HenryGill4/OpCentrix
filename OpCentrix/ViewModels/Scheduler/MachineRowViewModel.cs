using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.Scheduler
{
    public class MachineRowViewModel
    {
        public string MachineId { get; set; } = string.Empty;
        public List<DateTime> Dates { get; set; } = new();
        public List<Job> Jobs { get; set; } = new();
        public int RowHeight { get; set; } = 160;
        public int SlotsPerDay { get; set; } = 1;
        public int SlotMinutes { get; set; } = 1440;
        public string Zoom { get; set; } = "day";
        public List<List<Job>> JobLayers { get; set; } = new();
    }
}
