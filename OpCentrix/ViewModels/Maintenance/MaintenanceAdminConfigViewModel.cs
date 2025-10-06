using System.Collections.Generic;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.ViewModels.Maintenance
{
    public class MaintenanceAdminConfigViewModel
    {
        public List<MaintenanceRule> Rules { get; set; } = new();
        public MaintenanceRule? Editing { get; set; }
    }
}
