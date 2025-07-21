using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.Models.ViewModels
{
    public class SchedulerPageViewModel
    {
        public List<DateTime> Dates { get; set; }
        public List<string> Machines { get; set; }
        public List<Job> Jobs { get; set; }
    }
}
