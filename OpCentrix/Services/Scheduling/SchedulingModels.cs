namespace OpCentrix.Services.Scheduling;

/// <summary>
/// Recommendation for optimal stacking configuration for a part/job.
/// </summary>
public class StackingRecommendation
{
    /// <summary>
    /// Part ID this recommendation applies to
    /// </summary>
    public int PartId { get; init; }

    /// <summary>
    /// Part number for display
    /// </summary>
    public string PartNumber { get; init; } = "";

    /// <summary>
    /// Recommended stack level (1, 2, or 3)
    /// </summary>
    public int RecommendedStackLevel { get; init; } = 1;

    /// <summary>
    /// Number of parts per build at this stack level
    /// </summary>
    public int PartsPerBuild { get; init; } = 1;

    /// <summary>
    /// Estimated duration in hours for this configuration
    /// </summary>
    public double EstimatedDurationHours { get; init; }

    /// <summary>
    /// Efficiency score (0-100) - higher is better
    /// </summary>
    public double EfficiencyScore { get; init; }

    /// <summary>
    /// Throughput: parts per hour
    /// </summary>
    public double PartsPerHour => EstimatedDurationHours > 0 
        ? PartsPerBuild / EstimatedDurationHours 
        : 0;

    /// <summary>
    /// Human-readable reasoning for this recommendation
    /// </summary>
    public string Reasoning { get; init; } = "";

    /// <summary>
    /// Alternative configurations that were considered
    /// </summary>
    public List<StackingAlternative> Alternatives { get; init; } = new();

    /// <summary>
    /// Whether this is optimal for weekend/overnight runs
    /// </summary>
    public bool OptimalForWeekend { get; init; }

    /// <summary>
    /// Suggested machine IDs that can handle this configuration
    /// </summary>
    public List<int> SuggestedMachineIds { get; init; } = new();
}

/// <summary>
/// An alternative stacking configuration that was considered
/// </summary>
public class StackingAlternative
{
    public int StackLevel { get; init; }
    public int PartsPerBuild { get; init; }
    public double EstimatedDurationHours { get; init; }
    public double EfficiencyScore { get; init; }
    public string Note { get; init; } = "";
}

/// <summary>
/// Suggestion for filling a gap in the schedule (e.g., weekend)
/// </summary>
public class ScheduleGapSuggestion
{
    /// <summary>
    /// Machine ID with the gap
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Machine name/code for display
    /// </summary>
    public string MachineCode { get; init; } = "";

    /// <summary>
    /// Start of the gap
    /// </summary>
    public DateTime GapStart { get; init; }

    /// <summary>
    /// End of the gap
    /// </summary>
    public DateTime GapEnd { get; init; }

    /// <summary>
    /// Duration of the gap in hours
    /// </summary>
    public double GapDurationHours => (GapEnd - GapStart).TotalHours;

    /// <summary>
    /// Is this a weekend gap?
    /// </summary>
    public bool IsWeekend { get; init; }

    /// <summary>
    /// Suggested jobs that could fill this gap
    /// </summary>
    public List<GapFillJob> SuggestedJobs { get; init; } = new();

    /// <summary>
    /// Utilization improvement if gap is filled (0-100%)
    /// </summary>
    public double UtilizationImprovement { get; init; }
}

/// <summary>
/// A job suggestion that could fill a schedule gap
/// </summary>
public class GapFillJob
{
    public int? PartId { get; init; }
    public string PartNumber { get; init; } = "";
    public int StackLevel { get; init; } = 1;
    public int Quantity { get; init; } = 1;
    public double EstimatedDurationHours { get; init; }
    public double FitScore { get; init; } // How well it fits the gap (0-100)
    public string Source { get; init; } = ""; // "Backlog", "Pending", "Repeat"
}

/// <summary>
/// Request for schedule optimization
/// </summary>
public class ScheduleOptimizationRequest
{
    /// <summary>
    /// Machine IDs to consider (empty = all SLS machines)
    /// </summary>
    public List<int> MachineIds { get; init; } = new();

    /// <summary>
    /// Date range start
    /// </summary>
    public DateTime StartDate { get; init; } = DateTime.Today;

    /// <summary>
    /// Date range end
    /// </summary>
    public DateTime EndDate { get; init; } = DateTime.Today.AddDays(14);

    /// <summary>
    /// Include weekend gaps in analysis
    /// </summary>
    public bool IncludeWeekends { get; init; } = true;

    /// <summary>
    /// Minimum gap duration to consider (hours)
    /// </summary>
    public double MinGapHours { get; init; } = 4;

    /// <summary>
    /// Prioritize jobs by urgency
    /// </summary>
    public bool PrioritizeByUrgency { get; init; } = true;
}

/// <summary>
/// Result of schedule optimization analysis
/// </summary>
public class ScheduleOptimizationResult
{
    /// <summary>
    /// Current schedule utilization (0-100%)
    /// </summary>
    public double CurrentUtilization { get; init; }

    /// <summary>
    /// Potential utilization after applying suggestions
    /// </summary>
    public double PotentialUtilization { get; init; }

    /// <summary>
    /// Identified gaps that could be filled
    /// </summary>
    public List<ScheduleGapSuggestion> Gaps { get; init; } = new();

    /// <summary>
    /// Stacking recommendations for pending jobs
    /// </summary>
    public List<StackingRecommendation> StackingRecommendations { get; init; } = new();

    /// <summary>
    /// Total potential hours that could be utilized
    /// </summary>
    public double TotalGapHours => Gaps.Sum(g => g.GapDurationHours);

    /// <summary>
    /// Analysis timestamp
    /// </summary>
    public DateTime AnalyzedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Summary message
    /// </summary>
    public string Summary { get; init; } = "";
}
