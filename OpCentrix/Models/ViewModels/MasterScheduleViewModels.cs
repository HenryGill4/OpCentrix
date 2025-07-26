using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models.ViewModels
{
    /// <summary>
    /// Comprehensive view model for the master schedule dashboard
    /// Task 12: Master Schedule View with real-time analytics
    /// </summary>
    public class MasterScheduleViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        public string ViewMode { get; set; } = "weekly"; // daily, weekly, monthly
        public string FilterDepartment { get; set; } = "all";
        public string FilterStatus { get; set; } = "all";
        public string FilterPriority { get; set; } = "all";
        
        // Master data collections
        public List<MasterScheduleJob> Jobs { get; set; } = new();
        public List<MasterScheduleMachine> Machines { get; set; } = new();
        public List<string> Departments { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<string> Priorities { get; set; } = new();
        
        // Real-time metrics
        public MasterScheduleMetrics Metrics { get; set; } = new();
        
        // Timeline data for visualization
        public List<MasterScheduleTimeSlot> Timeline { get; set; } = new();
        
        // Resource utilization data
        public List<ResourceUtilization> ResourceUtilization { get; set; } = new();
        
        // Critical alerts and issues
        public List<ScheduleAlert> Alerts { get; set; } = new();
    }

    /// <summary>
    /// Enhanced job information for master schedule view
    /// </summary>
    public class MasterScheduleJob
    {
        public int Id { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        
        public DateTime ScheduledStart { get; set; }
        public DateTime ScheduledEnd { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        
        public double EstimatedHours { get; set; }
        public double? ActualHours { get; set; }
        public double ProgressPercent { get; set; }
        
        public decimal EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        
        public string SlsMaterial { get; set; } = string.Empty;
        public int Quantity { get; set; }
        
        // Calculated properties
        public TimeSpan Duration => ScheduledEnd - ScheduledStart;
        public bool IsDelayed => ActualStart.HasValue && ActualStart > ScheduledStart;
        public bool IsOverBudget => ActualCost.HasValue && ActualCost > EstimatedCost;
        public bool IsInProgress => Status.Equals("running", StringComparison.OrdinalIgnoreCase) || 
                                   Status.Equals("building", StringComparison.OrdinalIgnoreCase);
        public bool IsCompleted => Status.Equals("completed", StringComparison.OrdinalIgnoreCase);
        public bool IsCritical => Priority.Equals("1", StringComparison.OrdinalIgnoreCase) || 
                                 Priority.Equals("high", StringComparison.OrdinalIgnoreCase);
        
        // Schedule health indicators
        public string HealthStatus => GetHealthStatus();
        public string HealthColor => GetHealthColor();
        
        private string GetHealthStatus()
        {
            if (IsCompleted) return "Completed";
            if (IsDelayed && IsOverBudget) return "Critical";
            if (IsDelayed || IsOverBudget) return "Warning";
            if (IsInProgress) return "In Progress";
            return "On Track";
        }
        
        private string GetHealthColor()
        {
            return GetHealthStatus() switch
            {
                "Completed" => "#10B981",    // green
                "Critical" => "#EF4444",     // red
                "Warning" => "#F59E0B",      // amber
                "In Progress" => "#3B82F6",  // blue
                "On Track" => "#6B7280",     // gray
                _ => "#9CA3AF"               // light gray
            };
        }
    }

    /// <summary>
    /// Machine status and utilization for master schedule
    /// </summary>
    public class MasterScheduleMachine
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // operational, maintenance, down, idle
        public double UtilizationPercent { get; set; }
        public double EfficiencyPercent { get; set; }
        
        public int ActiveJobs { get; set; }
        public int QueuedJobs { get; set; }
        public int CompletedJobs { get; set; }
        
        public TimeSpan TotalScheduledTime { get; set; }
        public TimeSpan TotalActualTime { get; set; }
        
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        
        // Current job information
        public string CurrentJobPartNumber { get; set; } = string.Empty;
        public DateTime? CurrentJobStartTime { get; set; }
        public DateTime? CurrentJobEndTime { get; set; }
        public double? CurrentJobProgress { get; set; }
        
        // Status indicators
        public bool IsOperational => Status.Equals("operational", StringComparison.OrdinalIgnoreCase);
        public bool NeedsMaintenance => NextMaintenanceDate.HasValue && NextMaintenanceDate <= DateTime.Today.AddDays(7);
        public bool IsHighUtilization => UtilizationPercent > 85;
        public bool IsLowEfficiency => EfficiencyPercent < 75;
    }

    /// <summary>
    /// Real-time metrics for master schedule dashboard
    /// </summary>
    public class MasterScheduleMetrics
    {
        // Job metrics
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int DelayedJobs { get; set; }
        public int CriticalJobs { get; set; }
        
        // Performance metrics
        public double OverallEfficiency { get; set; }
        public double OnTimeDeliveryPercent { get; set; }
        public double UtilizationPercent { get; set; }
        public double QualityScorePercent { get; set; }
        
        // Financial metrics
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal CostVariancePercent { get; set; }
        
        // Resource metrics
        public int OperationalMachines { get; set; }
        public int MaintenanceMachines { get; set; }
        public int IdleMachines { get; set; }
        
        // Time metrics
        public double AverageJobDurationHours { get; set; }
        public double AverageSetupTimeHours { get; set; }
        public double AverageDelayHours { get; set; }
        
        // Trend indicators (compared to previous period)
        public double EfficiencyTrend { get; set; }
        public double UtilizationTrend { get; set; }
        public double CostTrend { get; set; }
        public double QualityTrend { get; set; }
        
        // Calculated properties
        public double CompletionRate => TotalJobs > 0 ? (double)CompletedJobs / TotalJobs * 100 : 0;
        public double DelayRate => TotalJobs > 0 ? (double)DelayedJobs / TotalJobs * 100 : 0;
        public bool IsPerformingWell => OverallEfficiency > 80 && OnTimeDeliveryPercent > 90;
        public string PerformanceStatus => GetPerformanceStatus();
        
        private string GetPerformanceStatus()
        {
            if (OverallEfficiency > 90 && OnTimeDeliveryPercent > 95) return "Excellent";
            if (OverallEfficiency > 80 && OnTimeDeliveryPercent > 90) return "Good";
            if (OverallEfficiency > 70 && OnTimeDeliveryPercent > 80) return "Fair";
            return "Poor";
        }
    }

    /// <summary>
    /// Time slot data for timeline visualization
    /// </summary>
    public class MasterScheduleTimeSlot
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public double ProgressPercent { get; set; }
        public bool IsConflict { get; set; }
        public bool IsMaintenanceSlot { get; set; }
        
        public TimeSpan Duration => EndTime - StartTime;
        public string DisplayText => IsMaintenanceSlot ? "Maintenance" : $"{PartNumber} ({Status})";
        public string CssClass => GetCssClass();
        
        private string GetCssClass()
        {
            if (IsMaintenanceSlot) return "maintenance-slot";
            if (IsConflict) return "conflict-slot";
            
            return Status.ToLower() switch
            {
                "completed" => "completed-slot",
                "running" => "running-slot",
                "building" => "running-slot",
                "delayed" => "delayed-slot",
                "scheduled" => "scheduled-slot",
                _ => "default-slot"
            };
        }
    }

    /// <summary>
    /// Resource utilization data for analytics
    /// </summary>
    public class ResourceUtilization
    {
        public string ResourceId { get; set; } = string.Empty;
        public string ResourceName { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty; // machine, operator, material
        public double UtilizationPercent { get; set; }
        public double CapacityHours { get; set; }
        public double UsedHours { get; set; }
        public double AvailableHours { get; set; }
        
        public List<UtilizationDataPoint> DailyUtilization { get; set; } = new();
        
        public bool IsOverUtilized => UtilizationPercent > 100;
        public bool IsUnderUtilized => UtilizationPercent < 60;
        public string UtilizationStatus => GetUtilizationStatus();
        
        private string GetUtilizationStatus()
        {
            return UtilizationPercent switch
            {
                > 100 => "Over-utilized",
                > 90 => "High",
                > 70 => "Optimal",
                > 50 => "Moderate",
                _ => "Low"
            };
        }
    }

    /// <summary>
    /// Daily utilization data point
    /// </summary>
    public class UtilizationDataPoint
    {
        public DateTime Date { get; set; }
        public double UtilizationPercent { get; set; }
        public double Hours { get; set; }
        public int JobCount { get; set; }
    }

    /// <summary>
    /// Schedule alert for critical issues
    /// </summary>
    public class ScheduleAlert
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // delay, conflict, maintenance, quality
        public string Severity { get; set; } = string.Empty; // low, medium, high, critical
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedDate { get; set; }
        public string ResolvedBy { get; set; } = string.Empty;
        public bool IsResolved => ResolvedDate.HasValue;
        
        public string SeverityColor => Severity.ToLower() switch
        {
            "critical" => "#DC2626", // red-600
            "high" => "#EA580C",      // orange-600
            "medium" => "#D97706",    // amber-600
            "low" => "#65A30D",       // lime-600
            _ => "#6B7280"            // gray-500
        };
        
        public string TypeIcon => Type.ToLower() switch
        {
            "delay" => "clock",
            "conflict" => "exclamation-triangle",
            "maintenance" => "wrench",
            "quality" => "shield-check",
            _ => "information-circle"
        };
    }

    /// <summary>
    /// Filter options for master schedule
    /// </summary>
    public class MasterScheduleFilters
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> Departments { get; set; } = new();
        public List<string> Machines { get; set; } = new();
        public List<string> Statuses { get; set; } = new();
        public List<string> Priorities { get; set; } = new();
        public bool ShowCompleted { get; set; } = true;
        public bool ShowDelayed { get; set; } = true;
        public bool ShowCritical { get; set; } = true;
        public string SearchTerm { get; set; } = string.Empty;
    }

    /// <summary>
    /// Master schedule export options
    /// </summary>
    public class MasterScheduleExportOptions
    {
        public string Format { get; set; } = "excel"; // excel, pdf, csv
        public bool IncludeMetrics { get; set; } = true;
        public bool IncludeCharts { get; set; } = true;
        public bool IncludeAlerts { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);
        public string TemplateType { get; set; } = "detailed"; // summary, detailed, executive
    }
}