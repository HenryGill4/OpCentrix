using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.Models.ViewModels
{
    public class DayCellViewModel
    {
        public DateTime Date { get; set; }
        public string MachineId { get; set; }
        public List<Job> Jobs { get; set; }
    }
}
