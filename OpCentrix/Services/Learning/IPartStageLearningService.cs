namespace OpCentrix.Services.Learning;

/// <summary>
/// Service for learning and refining stage duration estimates from actual completions.
/// Uses Exponential Moving Average (EMA) with ?=0.3 for stable, responsive estimates.
/// </summary>
public interface IPartStageLearningService
{
    /// <summary>
    /// Records a stage completion and triggers estimate refinement.
    /// Called from MultiStageJobService.CompleteStageAsync after SaveChangesAsync.
    /// </summary>
    /// <param name="jobStageId">The completed JobStage ID</param>
    /// <param name="ct">Cancellation token</param>
    Task RecordCompletionAsync(int jobStageId, CancellationToken ct = default);

    /// <summary>
    /// Refines the estimated duration for a specific part-stage combination.
    /// Uses EMA: newAvg = ? * newActual + (1-?) * oldAvg where ? = 0.3
    /// </summary>
    /// <param name="partId">The Part ID</param>
    /// <param name="stageType">The stage type name (e.g., "SLS", "CNC", "EDM")</param>
    /// <param name="ct">Cancellation token</param>
    Task RefineEstimateAsync(int partId, string stageType, CancellationToken ct = default);

    /// <summary>
    /// Gets performance analytics for all stages of a specific part.
    /// Shows estimated vs actual durations and learning metrics.
    /// </summary>
    /// <param name="partId">The Part ID</param>
    /// <param name="ct">Cancellation token</param>
    Task<StagePerformanceReport> GetStagePerformanceAsync(int partId, CancellationToken ct = default);

    /// <summary>
    /// Resets a stage estimate to manual mode with an optional new value.
    /// Clears auto-learning until next completion.
    /// </summary>
    /// <param name="partStageRequirementId">The PartStageRequirement ID</param>
    /// <param name="manualHours">Optional new manual estimate (null keeps current)</param>
    /// <param name="ct">Cancellation token</param>
    Task ResetToManualAsync(int partStageRequirementId, double? manualHours, CancellationToken ct = default);
}

/// <summary>
/// Performance report comparing estimated vs actual stage durations for a part.
/// </summary>
public class StagePerformanceReport
{
    /// <summary>
    /// The Part ID this report is for
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Part number for display
    /// </summary>
    public string PartNumber { get; set; } = string.Empty;

    /// <summary>
    /// Part name for display
    /// </summary>
    public string PartName { get; set; } = string.Empty;

    /// <summary>
    /// Performance data for each stage
    /// </summary>
    public List<StagePerformanceItem> Stages { get; set; } = new();

    /// <summary>
    /// Total estimated hours across all stages
    /// </summary>
    public double TotalEstimatedHours => Stages.Sum(s => s.EstimatedHours);

    /// <summary>
    /// Total actual average hours across all stages (where data exists)
    /// </summary>
    public double TotalActualAverageHours => Stages.Where(s => s.ActualAverageHours.HasValue)
                                                   .Sum(s => s.ActualAverageHours!.Value);

    /// <summary>
    /// Overall variance percentage (positive = taking longer than estimated)
    /// </summary>
    public double? OverallVariancePercent => TotalEstimatedHours > 0 && Stages.Any(s => s.ActualAverageHours.HasValue)
        ? ((TotalActualAverageHours - TotalEstimatedHours) / TotalEstimatedHours) * 100
        : null;

    /// <summary>
    /// Report generation timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance data for a single stage.
/// </summary>
public class StagePerformanceItem
{
    /// <summary>
    /// The PartStageRequirement ID
    /// </summary>
    public int PartStageRequirementId { get; set; }

    /// <summary>
    /// Stage name (e.g., "SLS Printing", "CNC Machining")
    /// </summary>
    public string StageName { get; set; } = string.Empty;

    /// <summary>
    /// Stage type for matching (e.g., "SLS", "CNC")
    /// </summary>
    public string StageType { get; set; } = string.Empty;

    /// <summary>
    /// Execution order within the workflow
    /// </summary>
    public int ExecutionOrder { get; set; }

    /// <summary>
    /// Current estimated hours (may be manual or auto-calculated)
    /// </summary>
    public double EstimatedHours { get; set; }

    /// <summary>
    /// Rolling average of actual durations (null if no data)
    /// </summary>
    public double? ActualAverageHours { get; set; }

    /// <summary>
    /// Most recent actual duration (null if no data)
    /// </summary>
    public double? LastActualHours { get; set; }

    /// <summary>
    /// Number of completions used in the average
    /// </summary>
    public int SampleCount { get; set; }

    /// <summary>
    /// Source of estimate: "Manual", "Auto", or "Default"
    /// </summary>
    public string EstimateSource { get; set; } = "Manual";

    /// <summary>
    /// When the estimate was last updated automatically
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Variance from estimate (positive = taking longer)
    /// </summary>
    public double? VariancePercent => ActualAverageHours.HasValue && EstimatedHours > 0
        ? ((ActualAverageHours.Value - EstimatedHours) / EstimatedHours) * 100
        : null;

    /// <summary>
    /// Whether there's enough data to trust the average (>=3 samples)
    /// </summary>
    public bool HasSufficientData => SampleCount >= 3;

    /// <summary>
    /// Recommendation based on variance
    /// </summary>
    public string Recommendation
    {
        get
        {
            if (!HasSufficientData) return "Insufficient data";
            if (!VariancePercent.HasValue) return "No variance data";
            
            return VariancePercent.Value switch
            {
                > 20 => "Consider increasing estimate",
                < -20 => "Consider decreasing estimate",
                _ => "Estimate is accurate"
            };
        }
    }
}
