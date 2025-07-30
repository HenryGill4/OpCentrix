using OpCentrix.Models;

namespace OpCentrix.ViewModels.Admin.Prototypes
{
    /// <summary>
    /// View model for admin review and approval of completed prototypes
    /// </summary>
    public class PrototypeReviewViewModel
    {
        public PrototypeJob PrototypeJob { get; set; } = new PrototypeJob();
        public PerformanceSummary Performance { get; set; } = new PerformanceSummary();
        public List<StagePerformanceAnalysis> StageAnalysis { get; set; } = new List<StagePerformanceAnalysis>();
        public LessonsLearned LessonsLearned { get; set; } = new LessonsLearned();
        public List<string> ValidationIssues { get; set; } = new List<string>();
        public AdminDecisionForm AdminDecision { get; set; } = new AdminDecisionForm();
        
        // Supporting data
        public List<ProductionStageExecution> CompletedStages { get; set; } = new List<ProductionStageExecution>();
        public List<AssemblyComponent> Components { get; set; } = new List<AssemblyComponent>();
        public decimal TotalComponentCost { get; set; }
    }

    /// <summary>
    /// Performance summary comparing estimated vs actual
    /// </summary>
    public class PerformanceSummary
    {
        public TimePerformance Time { get; set; } = new TimePerformance();
        public CostPerformance Cost { get; set; } = new CostPerformance();
        public QualityPerformance Quality { get; set; } = new QualityPerformance();
        public MaterialPerformance Material { get; set; } = new MaterialPerformance();
        
        public int LeadTimeDays { get; set; }
        public int EstimatedLeadTimeDays { get; set; }
        public decimal LeadTimeVariancePercent { get; set; }
        public string OverallRating { get; set; } = "Good"; // Excellent, Good, Fair, Poor
    }

    /// <summary>
    /// Time performance details
    /// </summary>
    public class TimePerformance
    {
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal VarianceHours { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; } = string.Empty; // On Time, Behind, Ahead
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Cost performance details
    /// </summary>
    public class CostPerformance
    {
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public decimal VarianceCost { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; } = string.Empty; // Under Budget, Over Budget, On Budget
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Quality performance details
    /// </summary>
    public class QualityPerformance
    {
        public int TotalQualityChecks { get; set; }
        public int PassedQualityChecks { get; set; }
        public int FailedQualityChecks { get; set; }
        public decimal QualityScore { get; set; }
        public int ReworkHours { get; set; }
        public List<QualityIssue> Issues { get; set; } = new List<QualityIssue>();
    }

    /// <summary>
    /// Material performance details
    /// </summary>
    public class MaterialPerformance
    {
        public decimal EstimatedMaterialKg { get; set; }
        public decimal ActualMaterialKg { get; set; }
        public decimal MaterialWasteKg { get; set; }
        public decimal WastePercent { get; set; }
        public string WasteStatus { get; set; } = string.Empty; // Low, Normal, High, Excessive
    }

    /// <summary>
    /// Quality issue details
    /// </summary>
    public class QualityIssue
    {
        public string StageName { get; set; } = string.Empty;
        public string IssueType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Resolution { get; set; } = string.Empty;
        public decimal ReworkHours { get; set; }
        public string Severity { get; set; } = string.Empty; // Minor, Major, Critical
    }

    /// <summary>
    /// Stage performance analysis
    /// </summary>
    public class StagePerformanceAnalysis
    {
        public string StageName { get; set; } = string.Empty;
        public decimal EstimatedHours { get; set; }
        public decimal ActualHours { get; set; }
        public decimal VarianceHours { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; } = string.Empty; // Completed, Failed, Skipped
        public string Performance { get; set; } = string.Empty; // Excellent, Good, Fair, Poor
        public string Notes { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new List<string>();
        public List<string> Improvements { get; set; } = new List<string>();
    }

    /// <summary>
    /// Lessons learned and improvements
    /// </summary>
    public class LessonsLearned
    {
        public List<DesignImprovement> DesignImprovements { get; set; } = new List<DesignImprovement>();
        public List<ProcessImprovement> ProcessImprovements { get; set; } = new List<ProcessImprovement>();
        public List<CostReduction> CostReductions { get; set; } = new List<CostReduction>();
        public string GeneralNotes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Design improvement suggestion
    /// </summary>
    public class DesignImprovement
    {
        public string Category { get; set; } = string.Empty; // Geometry, Material, Tolerances
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedTimeSavings { get; set; }
        public decimal EstimatedCostSavings { get; set; }
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
    }

    /// <summary>
    /// Process improvement suggestion
    /// </summary>
    public class ProcessImprovement
    {
        public string StageName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedTimeSavings { get; set; }
        public decimal EstimatedCostSavings { get; set; }
        public string ImplementationDifficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
    }

    /// <summary>
    /// Cost reduction opportunity
    /// </summary>
    public class CostReduction
    {
        public string Category { get; set; } = string.Empty; // Material, Labor, Component, Overhead
        public string Description { get; set; } = string.Empty;
        public decimal EstimatedSavings { get; set; }
        public decimal SavingsPercent { get; set; }
        public string ImplementationDifficulty { get; set; } = "Medium"; // Easy, Medium, Hard
        public string Priority { get; set; } = "Medium"; // High, Medium, Low
    }

    /// <summary>
    /// Admin decision form
    /// </summary>
    public class AdminDecisionForm
    {
        public string Decision { get; set; } = string.Empty; // Approve, ApproveWithModifications, Reject, Archive
        public string AdminNotes { get; set; } = string.Empty;
        public bool UseActualTimeData { get; set; } = true;
        public bool UseActualCostData { get; set; } = true;
        public decimal? AdjustedEstimatedHours { get; set; }
        public decimal? AdjustedEstimatedCost { get; set; }
        public string AdjustmentReason { get; set; } = string.Empty;
        public List<string> RequiredChanges { get; set; } = new List<string>();
        public DateTime? ScheduledReviewDate { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
    }
}