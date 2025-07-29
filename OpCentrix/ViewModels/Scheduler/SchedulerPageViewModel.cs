using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.Scheduler
{
    public class SchedulerPageViewModel
    {
        public List<DateTime> Dates { get; set; } = new();
        public List<string> Machines { get; set; } = new();
        public List<Job> Jobs { get; set; } = new();
        public string Zoom { get; set; } = "day";
        public DateTime StartDate { get; set; } = DateTime.Today;
        public int SlotsPerDay { get; set; } = 1;
        public int SlotMinutes { get; set; } = 1440;
        public Dictionary<string, int> MachineRowHeights { get; set; } = new();
    }
}
