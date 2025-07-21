//SchedulerPageViewModel.cs:
using System;
namespace OpCentrix.Models
{
    public class SchedulerPageViewModel
    {
        public List<string> Dates { get; set; } = new();
        public List<MachineViewModel> Machines { get; set; } = new();

        /// <summary>
        /// Starting date of the week being displayed.
        /// </summary>
        public DateTime StartOfWeek { get; set; }
    }
}
