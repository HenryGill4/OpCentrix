using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.Scheduler
{
    /// <summary>
    /// Modern view model for job creation/editing with proper machine integration
    /// </summary>
    public class AddEditJobViewModel
    {
        public Job Job { get; set; } = new();
        public List<Part> Parts { get; set; } = new();
        public List<Machine> Machines { get; set; } = new(); // NEW: Include actual machines
        public List<string> Errors { get; set; } = new();

        public bool IsEdit => Job.Id > 0;
        public bool IsEditing => Job.Id > 0;
        public string Title => IsEditing ? "Edit Job" : "Add New Job";
        
        // Helper properties for view
        public int AvailableMachineCount => Machines?.Count ?? 0;
        public int AvailablePartCount => Parts?.Count ?? 0;
        public bool HasMachines => AvailableMachineCount > 0;
        public bool HasParts => AvailablePartCount > 0;
    }
}
