using OpCentrix.Models;

namespace OpCentrix.ViewModels.Admin.Prototypes
{
    /// <summary>
    /// View model for detailed prototype job tracking
    /// </summary>
    public class PrototypeDetailsViewModel
    {
        public PrototypeJob PrototypeJob { get; set; } = new PrototypeJob();
        public List<StageExecutionDetail> StageExecutions { get; set; } = new List<StageExecutionDetail>();
        public List<AssemblyComponent> AssemblyComponents { get; set; } = new List<AssemblyComponent>();
        public CostAnalysisDetail CostAnalysis { get; set; } = new CostAnalysisDetail();
        public TimeAnalysisDetail TimeAnalysis { get; set; } = new TimeAnalysisDetail();
        public ComponentReadinessSummary ComponentReadiness { get; set; } = new ComponentReadinessSummary();
        public List<PrototypeTimeLog> RecentTimeLogs { get; set; } = new List<PrototypeTimeLog>();
        
        // Current stage details
        public ProductionStageExecution? CurrentStageExecution { get; set; }
        public string CurrentStageStatus { get; set; } = string.Empty;
        public bool CanStartNextStage { get; set; }
        public bool CanCompleteCurrentStage { get; set; }
        public bool IsReadyForAdminReview { get; set; }
    }

    /// <summary>
    /// Detailed stage execution information
    /// </summary>
    public class StageExecutionDetail
    {
        public ProductionStageExecution Execution { get; set; } = new ProductionStageExecution();
        public string StageName { get; set; } = string.Empty;
        public int StageOrder { get; set; }
        public string StatusIcon { get; set; } = string.Empty; // ?, ??, ?, ?
        public string StatusColor { get; set; } = string.Empty; // success, warning, info, danger
        public string TimeVarianceStatus { get; set; } = string.Empty; // On Schedule, Behind, Ahead
        public decimal TimeVarianceHours { get; set; }
        public List<PrototypeTimeLog> TimeLogs { get; set; } = new List<PrototypeTimeLog>();
        public bool CanStart { get; set; }
        public bool CanComplete { get; set; }
        public bool CanSkip { get; set; }
    }

    /// <summary>
    /// Cost analysis details
    /// </summary>
    public class CostAnalysisDetail
    {
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal CostVariance { get; set; }
        public decimal CostVariancePercent { get; set; }
        public string VarianceStatus { get; set; } = string.Empty; // Under Budget, Over Budget, On Budget
        
        public List<StageCostBreakdown> StageCosts { get; set; } = new List<StageCostBreakdown>();
        public decimal MaterialCost { get; set; }
        public decimal LaborCost { get; set; }
        public decimal OverheadCost { get; set; }
        public decimal ComponentsCost { get; set; }
    }

    /// <summary>
    /// Time analysis details
    /// </summary>
    public class TimeAnalysisDetail
    {
        public decimal TotalEstimatedHours { get; set; }
        public decimal TotalActualHours { get; set; }
        public decimal TimeVariance { get; set; }
        public decimal TimeVariancePercent { get; set; }
        public string VarianceStatus { get; set; } = string.Empty; // On Schedule, Behind, Ahead
        
        public List<StageTimeBreakdown> StageTimes { get; set; } = new List<StageTimeBreakdown>();
        public decimal SetupTime { get; set; }
        public decimal ProductionTime { get; set; }
        public decimal QualityTime { get; set; }
        public decimal ReworkTime { get; set; }
    }

    /// <summary>
    /// Stage cost breakdown
    /// </summary>
    public class StageCostBreakdown
    {
        public string StageName { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; } = string.Empty; // Completed, InProgress, Pending
    }

    /// <summary>
    /// Stage time breakdown
    /// </summary>
    public class StageTimeBreakdown
    {
        public string StageName { get; set; } = string.Empty;
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; } = string.Empty; // Completed, InProgress, Pending
    }

    /// <summary>
    /// Component readiness summary from AssemblyComponentService
    /// </summary>
    public class ComponentReadinessSummary
    {
        public int PrototypeJobId { get; set; }
        public int TotalComponents { get; set; }
        public int NeededComponents { get; set; }
        public int OrderedComponents { get; set; }
        public int ReceivedComponents { get; set; }
        public int UsedComponents { get; set; }
        public decimal ReadinessPercentage { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public List<string> ComponentsAwaitingOrder { get; set; } = new List<string>();
        public List<ComponentDeliveryInfo> ComponentsAwaitingDelivery { get; set; } = new List<ComponentDeliveryInfo>();
    }

    /// <summary>
    /// Component delivery information
    /// </summary>
    public class ComponentDeliveryInfo
    {
        public string Description { get; set; } = string.Empty;
        public string Supplier { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
    }
}