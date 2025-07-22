using System;
using System.Collections.Generic;
using OpCentrix.Models;

namespace OpCentrix.Models.ViewModels
{
    public class DayCellViewModel
    {
        public DateTime Date { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public Job? Job { get; set; }
        public int ColSpan { get; set; } = 1;
        public bool ShowAdd { get; set; }
        public bool IsChangeover { get; set; }
    }
}
