//MachineViewModel.cs:
namespace OpCentrix.Models
{
    public class MachineViewModel
    {
public string Id { get; set; } = string.Empty;
public string Name { get; set; } = string.Empty;

        public List<ScheduledJob> Jobs { get; set; } = new();
    }
}
