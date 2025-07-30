using OpCentrix.Models;

namespace OpCentrix.ViewModels.Admin.Prototypes
{
    /// <summary>
    /// View model for the prototype dashboard showing active prototypes and pipeline metrics
    /// </summary>
    public class PrototypeDashboardViewModel
    {
        public List<PrototypeJobSummary> ActivePrototypes { get; set; } = new List<PrototypeJobSummary>();
        public PipelineMetrics Metrics { get; set; } = new PipelineMetrics();
        public List<BottleneckAlert> Bottlenecks { get; set; } = new List<BottleneckAlert>();
        public List<PrototypeJob> RecentlyCompleted { get; set; } = new List<PrototypeJob>();
    }

    /// <summary>
    /// Summary information about a prototype job for dashboard display
    /// </summary>
    public class PrototypeJobSummary
    {
        public int Id { get; set; }
        public string PrototypeNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string CurrentStage { get; set; } = string.Empty;
        public int CurrentStageOrder { get; set; }
        public int TotalStages { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public int DaysInProcess { get; set; }
        public DateTime RequestDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Pipeline metrics for the last 30 days
    /// </summary>
    public class PipelineMetrics
    {
        public int PrototypesStarted { get; set; }
        public int PrototypesCompleted { get; set; }
        public decimal AverageLeadTimeDays { get; set; }
        public decimal AverageCostVariancePercent { get; set; }
        public decimal AverageTimeVariancePercent { get; set; }
        public decimal TotalCostSavings { get; set; }
        public int QualityIssuesCount { get; set; }
    }

    /// <summary>
    /// Bottleneck and alert information
    /// </summary>
    public class BottleneckAlert
    {
        public string Type { get; set; } = string.Empty; // Stage, Component, Quality
        public string StageName { get; set; } = string.Empty;
        public int JobsWaiting { get; set; }
        public decimal AverageWaitTimeDays { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical
    }
}