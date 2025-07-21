//SchedulerPageViewModel.cs:
namespace OpCentrix.Models
{
    public class SchedulerPageViewModel
    {
        public List<string> Dates { get; set; } = new();
        public List<MachineViewModel> Machines { get; set; } = new();
    }
}
