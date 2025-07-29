using OpCentrix.Models;

namespace OpCentrix.ViewModels.Shared
{
    /// <summary>
    /// View model for embedded scheduler display in print tracking dashboard
    /// </summary>
    public class EmbeddedSchedulerViewModel
    {
        public List<Job> Jobs { get; set; } = new();
        public List<string> Machines { get; set; } = new();
        public List<DateTime> Dates { get; set; } = new();
        public DateTime StartDate { get; set; }
    }
}