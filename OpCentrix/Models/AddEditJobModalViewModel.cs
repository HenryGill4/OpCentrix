using System.Collections.Generic;

namespace OpCentrix.Models
{
    public class AddEditJobModalViewModel
    {
        public ScheduledJob Job { get; set; } = new();
        public List<Part> Parts { get; set; } = new();
    }
}
