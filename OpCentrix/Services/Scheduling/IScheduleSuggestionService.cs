namespace OpCentrix.Services.Scheduling;

/// <summary>
/// Service for generating intelligent scheduling suggestions.
/// Analyzes stacking configurations, identifies schedule gaps, and recommends optimizations.
/// </summary>
public interface IScheduleSuggestionService
{
    /// <summary>
    /// Get stacking recommendation for a specific part and quantity
    /// </summary>
    /// <param name="partId">Part to analyze</param>
    /// <param name="quantity">Number of parts needed</param>
    /// <param name="preferredStartDate">When the job should ideally start</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Optimal stacking recommendation</returns>
    Task<StackingRecommendation> GetStackingRecommendationAsync(
        int partId,
        int quantity,
        DateTime? preferredStartDate = null,
        CancellationToken ct = default);

    /// <summary>
    /// Get stacking recommendations for multiple parts (batch planning)
    /// </summary>
    Task<List<StackingRecommendation>> GetBatchStackingRecommendationsAsync(
        IEnumerable<(int PartId, int Quantity)> parts,
        CancellationToken ct = default);

    /// <summary>
    /// Find gaps in the schedule that could be filled
    /// </summary>
    /// <param name="request">Optimization request parameters</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of identified gaps with fill suggestions</returns>
    Task<List<ScheduleGapSuggestion>> FindScheduleGapsAsync(
        ScheduleOptimizationRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Get weekend fill suggestions for a specific weekend
    /// </summary>
    /// <param name="weekendStart">Friday evening or Saturday morning</param>
    /// <param name="machineIds">Machine IDs to analyze (empty = all SLS)</param>
    /// <param name="ct">Cancellation token</param>
    Task<List<ScheduleGapSuggestion>> GetWeekendFillSuggestionsAsync(
        DateTime weekendStart,
        IEnumerable<int>? machineIds = null,
        CancellationToken ct = default);

    /// <summary>
    /// Run full schedule optimization analysis
    /// </summary>
    /// <param name="request">Optimization parameters</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Complete optimization result with all suggestions</returns>
    Task<ScheduleOptimizationResult> OptimizeScheduleAsync(
        ScheduleOptimizationRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Calculate efficiency score for a stacking configuration
    /// </summary>
    /// <param name="partId">Part ID</param>
    /// <param name="stackLevel">Stack level (1, 2, or 3)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Efficiency score 0-100</returns>
    Task<double> CalculateStackingEfficiencyAsync(
        int partId,
        int stackLevel,
        CancellationToken ct = default);

    /// <summary>
    /// Get machines suitable for a part configuration
    /// </summary>
    Task<List<int>> GetSuitableMachinesAsync(
        int partId,
        int stackLevel,
        CancellationToken ct = default);
}
