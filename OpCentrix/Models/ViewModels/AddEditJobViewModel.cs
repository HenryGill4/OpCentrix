using System.Collections.Generic;

namespace OpCentrix.Models.ViewModels
{
    public class AddEditJobViewModel
    {
        public Job Job { get; set; }
        public List<Part> Parts { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
