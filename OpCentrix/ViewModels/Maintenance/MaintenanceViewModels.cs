using OpCentrix.Models;
using OpCentrix.Models.Maintenance;

namespace OpCentrix.ViewModels.Maintenance
{
    public class MaintenanceDashboardViewModel
    {
        public List<RuleStatusDto> OverdueItems { get; set; } = new();
        public List<RuleStatusDto> DueItems { get; set; } = new();
        public List<MaintenanceWorkOrder> ActiveWorkOrders { get; set; } = new();
        public List<MaintenanceWorkOrder> UpcomingWorkOrders { get; set; } = new();
        public List<MaintenanceNotification> ActiveNotifications { get; set; } = new();
        public List<MaintenanceService> CriticalServices { get; set; } = new();
        public List<MaintenanceSchedule> DueSchedules { get; set; } = new();
        
        // Summary statistics
        public int TotalMachines { get; set; }
        public int MachinesWithIssues { get; set; }
        public int OpenWorkOrders { get; set; }
        public int OverdueWorkOrders { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public double AverageUptime { get; set; }
        
        // Charts data
        public Dictionary<string, int> WorkOrdersByStatus { get; set; } = new();
        public Dictionary<string, decimal> CostByMonth { get; set; } = new();
        public Dictionary<string, double> UptimeByMachine { get; set; } = new();
        public List<MaintenanceTrendData> MaintenanceTrends { get; set; } = new();
    }

    public class MaintenanceTrendData
    {
        public DateTime Date { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class MaintenanceMetricsDto
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public double TotalOperatingHours { get; set; }
        public double MaintenanceHours { get; set; }
        public double UptimePercentage { get; set; }
        public int TotalWorkOrders { get; set; }
        public int CompletedWorkOrders { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public DateTime LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public List<MaintenanceService> Services { get; set; } = new();
    }

    public class MaintenanceCostAnalysisDto
    {
        public decimal TotalCost { get; set; }
        public decimal PreventiveCost { get; set; }
        public decimal CorrectiveCost { get; set; }
        public decimal EmergencyCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal OverheadCost { get; set; }
        
        public Dictionary<string, decimal> CostByMachine { get; set; } = new();
        public Dictionary<string, decimal> CostByMonth { get; set; } = new();
        public Dictionary<string, decimal> CostByWorkOrderType { get; set; } = new();
        
        public double PreventiveVsCorrectiveRatio { get; set; }
        public decimal AverageCostPerWorkOrder { get; set; }
        public decimal CostPerOperatingHour { get; set; }
    }

    public class MaintenanceHistoryDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string? ComponentName { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ActionType { get; set; } = string.Empty; // Service, WorkOrder, Rule, etc.
        public string Status { get; set; } = string.Empty;
        public string? TechnicianName { get; set; }
        public double? Hours { get; set; }
        public decimal? Cost { get; set; }
        public string? Notes { get; set; }
    }

    public class CreateMaintenanceServiceViewModel
    {
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public MaintenanceServiceType ServiceType { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public int? MachineComponentId { get; set; }
        public int UpdateIntervalMinutes { get; set; } = 60;
        public string? DataSource { get; set; }
        public double? WarningThreshold { get; set; }
        public double? CriticalThreshold { get; set; }
        public string? Unit { get; set; }
        
        // Available options for dropdowns
        public List<Machine> AvailableMachines { get; set; } = new();
        public List<MachineComponent> AvailableComponents { get; set; } = new();
    }

    public class CreateWorkOrderViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public int? MachineComponentId { get; set; }
        public MaintenanceWorkOrderType WorkOrderType { get; set; }
        public MaintenanceWorkOrderPriority Priority { get; set; }
        public DateTime? ScheduledStartDate { get; set; }
        public DateTime? ScheduledEndDate { get; set; }
        public string? AssignedTechnician { get; set; }
        public double EstimatedHours { get; set; }
        public decimal EstimatedCost { get; set; }
        public bool RequiresShutdown { get; set; }
        public int? ShutdownDurationMinutes { get; set; }
        public string? Notes { get; set; }
        
        // Available options for dropdowns
        public List<Machine> AvailableMachines { get; set; } = new();
        public List<MachineComponent> AvailableComponents { get; set; } = new();
        public List<User> AvailableTechnicians { get; set; } = new();
    }
}