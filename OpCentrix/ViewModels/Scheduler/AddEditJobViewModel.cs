using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.Scheduler
{
    public class AddEditJobViewModel
    {
        public Job Job { get; set; } = new();
        public List<Part> Parts { get; set; } = new();
        public List<string> Errors { get; set; } = new();

        public bool IsEdit => Job.Id > 0;
        public bool IsEditing => Job.Id > 0;
        public string Title => IsEditing ? "Edit Job" : "Add New Job";
    }
}
