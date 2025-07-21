using System.Collections.Generic;

namespace OpCentrix.Models
{
    public class AddEditJobViewModel
    {
        public Job Job { get; set; } = new();
        public List<Part> Parts { get; set; } = new();
    }
}
