using System.Collections.Generic;

namespace OpCentrix.Models.ViewModels
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
