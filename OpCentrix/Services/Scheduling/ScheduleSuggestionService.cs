using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Scheduling;

/// <summary>
/// Implementation of schedule suggestion service.
/// Analyzes MasterPart stacking configs and Job schedule to generate recommendations.
/// </summary>
public class ScheduleSuggestionService : IScheduleSuggestionService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<ScheduleSuggestionService> _logger;
    private readonly WeekendFillAnalyzer _weekendAnalyzer;

    public ScheduleSuggestionService(
        SchedulerContext context,
        ILogger<ScheduleSuggestionService> logger)
    {
        _context = context;
        _logger = logger;
        _weekendAnalyzer = new WeekendFillAnalyzer(context, logger);
    }

    public async Task<StackingRecommendation> GetStackingRecommendationAsync(
        int partId,
        int quantity,
        DateTime? preferredStartDate = null,
        CancellationToken ct = default)
    {
        var part = await _context.Parts
            .FirstOrDefaultAsync(p => p.Id == partId, ct);

        if (part == null)
        {
            return new StackingRecommendation
            {
                PartId = partId,
                RecommendedStackLevel = 1,
                Reasoning = "Part not found"
            };
        }

        // Try to find a MasterPart with matching PartNumber
        var masterPart = await _context.MasterParts
            .FirstOrDefaultAsync(mp => mp.PartNumber == part.PartNumber, ct);

        if (masterPart == null)
        {
            return new StackingRecommendation
            {
                PartId = partId,
                PartNumber = part.PartNumber ?? "",
                RecommendedStackLevel = 1,
                PartsPerBuild = 1,
                EstimatedDurationHours = part.EstimatedHours,
                EfficiencyScore = 50,
                Reasoning = "No MasterPart configuration - using single stack"
            };
        }

        // Calculate efficiency for each stack level using MasterPart methods
        var alternatives = new List<StackingAlternative>();
        var bestLevel = 1;
        var bestScore = 0.0;

        foreach (var level in masterPart.AvailableStackLevels)
        {
            var partsPerBuild = masterPart.GetPartsPerBuild(level) ?? 1;
            var duration = masterPart.GetStackDuration(level) ?? 8;

            if (partsPerBuild <= 0 || duration <= 0) continue;

            var buildsNeeded = (int)Math.Ceiling((double)quantity / partsPerBuild);
            var totalHours = buildsNeeded * duration;
            var partsPerHour = quantity / totalHours;

            // Efficiency factors
            var throughputScore = partsPerHour * 20;
            var buildCountScore = Math.Max(0, 30 - buildsNeeded * 5);
            var weekendFitBonus = (duration >= 8 && duration <= 48) ? 15 : 0;

            var score = throughputScore + buildCountScore + weekendFitBonus;
            score = Math.Min(100, Math.Max(0, score));

            alternatives.Add(new StackingAlternative
            {
                StackLevel = level,
                PartsPerBuild = partsPerBuild,
                EstimatedDurationHours = duration,
                EfficiencyScore = score,
                Note = $"{buildsNeeded} build(s), {partsPerHour:F2} parts/hr"
            });

            if (score > bestScore)
            {
                bestScore = score;
                bestLevel = level;
            }
        }

        var bestAlt = alternatives.FirstOrDefault(a => a.StackLevel == bestLevel)
                      ?? alternatives.FirstOrDefault()
                      ?? new StackingAlternative { StackLevel = 1, PartsPerBuild = 1 };

        var isWeekendOptimal = bestAlt.EstimatedDurationHours >= 12 && bestAlt.EstimatedDurationHours <= 60;
        var suitableMachines = await GetSuitableMachinesAsync(partId, bestLevel, ct);
        var reasoning = BuildReasoning(bestLevel, quantity, bestAlt, alternatives);

        return new StackingRecommendation
        {
            PartId = partId,
            PartNumber = part.PartNumber ?? masterPart.PartNumber ?? "",
            RecommendedStackLevel = bestLevel,
            PartsPerBuild = bestAlt.PartsPerBuild,
            EstimatedDurationHours = bestAlt.EstimatedDurationHours,
            EfficiencyScore = bestScore,
            Reasoning = reasoning,
            Alternatives = alternatives.Where(a => a.StackLevel != bestLevel).ToList(),
            OptimalForWeekend = isWeekendOptimal,
            SuggestedMachineIds = suitableMachines
        };
    }

    public async Task<List<StackingRecommendation>> GetBatchStackingRecommendationsAsync(
        IEnumerable<(int PartId, int Quantity)> parts,
        CancellationToken ct = default)
    {
        var results = new List<StackingRecommendation>();

        foreach (var (partId, quantity) in parts)
        {
            var rec = await GetStackingRecommendationAsync(partId, quantity, null, ct);
            results.Add(rec);
        }

        return results;
    }

    public async Task<List<ScheduleGapSuggestion>> FindScheduleGapsAsync(
        ScheduleOptimizationRequest request,
        CancellationToken ct = default)
    {
        return await _weekendAnalyzer.FindGapsAsync(request, ct);
    }

    public async Task<List<ScheduleGapSuggestion>> GetWeekendFillSuggestionsAsync(
        DateTime weekendStart,
        IEnumerable<int>? machineIds = null,
        CancellationToken ct = default)
    {
        var friday = weekendStart.Date;
        while (friday.DayOfWeek != DayOfWeek.Friday)
            friday = friday.AddDays(1);

        var weekendStartTime = friday.AddHours(18);
        var weekendEndTime = friday.AddDays(2).AddHours(6);

        var request = new ScheduleOptimizationRequest
        {
            MachineIds = machineIds?.ToList() ?? new List<int>(),
            StartDate = weekendStartTime,
            EndDate = weekendEndTime,
            IncludeWeekends = true,
            MinGapHours = 4
        };

        return await FindScheduleGapsAsync(request, ct);
    }

    public async Task<ScheduleOptimizationResult> OptimizeScheduleAsync(
        ScheduleOptimizationRequest request,
        CancellationToken ct = default)
    {
        _logger.LogInformation("[SCHEDULE] Running optimization for {Start} to {End}",
            request.StartDate, request.EndDate);

        var gaps = await FindScheduleGapsAsync(request, ct);

        // Get pending jobs
        var pendingParts = await _context.Jobs
            .Where(j => j.Status == "Pending" || j.Status == "Queued")
            .Where(j => j.ScheduledStart >= request.StartDate)
            .Select(j => new { j.PartId, j.Quantity })
            .Distinct()
            .Take(20)
            .ToListAsync(ct);

        var stackingRecs = new List<StackingRecommendation>();
        foreach (var pj in pendingParts.Where(p => p.PartId > 0))
        {
            var rec = await GetStackingRecommendationAsync(pj.PartId, pj.Quantity, null, ct);
            stackingRecs.Add(rec);
        }

        var totalHours = (request.EndDate - request.StartDate).TotalHours;
        var gapHours = gaps.Sum(g => g.GapDurationHours);
        var currentUtilization = totalHours > 0 ? ((totalHours - gapHours) / totalHours) * 100 : 0;

        var fillableHours = gaps.Sum(g => g.SuggestedJobs.Sum(j => j.EstimatedDurationHours));
        var potentialUtilization = totalHours > 0 
            ? ((totalHours - gapHours + fillableHours) / totalHours) * 100 
            : currentUtilization;

        var summary = $"Found {gaps.Count} gaps totaling {gapHours:F1} hours. " +
                      $"Current utilization: {currentUtilization:F0}%. " +
                      $"Potential: {potentialUtilization:F0}%.";

        return new ScheduleOptimizationResult
        {
            CurrentUtilization = currentUtilization,
            PotentialUtilization = Math.Min(100, potentialUtilization),
            Gaps = gaps,
            StackingRecommendations = stackingRecs,
            Summary = summary
        };
    }

    public async Task<double> CalculateStackingEfficiencyAsync(
        int partId,
        int stackLevel,
        CancellationToken ct = default)
    {
        var rec = await GetStackingRecommendationAsync(partId, 1, null, ct);
        var alt = rec.Alternatives.FirstOrDefault(a => a.StackLevel == stackLevel);

        if (rec.RecommendedStackLevel == stackLevel)
            return rec.EfficiencyScore;

        return alt?.EfficiencyScore ?? 50;
    }

    public async Task<List<int>> GetSuitableMachinesAsync(
        int partId,
        int stackLevel,
        CancellationToken ct = default)
    {
        var slsMachines = await _context.Machines
            .Where(m => m.IsActive)
            .Where(m => m.MachineType == "SLS" || m.MachineType == "DMLS" || m.MachineType == "Metal3D")
            .Select(m => m.Id)
            .ToListAsync(ct);

        return slsMachines;
    }

    #region Helpers

    private static string BuildReasoning(int level, int quantity, StackingAlternative best, List<StackingAlternative> alts)
    {
        var buildsNeeded = (int)Math.Ceiling((double)quantity / best.PartsPerBuild);
        var parts = new List<string>
        {
            $"{level}x stacking recommended for {quantity} parts",
            $"{best.PartsPerBuild} parts/build × {buildsNeeded} build(s) = {best.PartsPerBuild * buildsNeeded} parts",
            $"Est. {best.EstimatedDurationHours:F1}h per build"
        };

        if (best.EstimatedDurationHours >= 12 && best.EstimatedDurationHours <= 48)
            parts.Add("Suitable for weekend runs");

        if (alts.Count > 1)
        {
            var others = alts.Where(a => a.StackLevel != level).OrderByDescending(a => a.EfficiencyScore).ToList();
            if (others.Any())
                parts.Add($"Alternatives: {string.Join(", ", others.Select(o => $"{o.StackLevel}x ({o.EfficiencyScore:F0}%)"))}");
        }

        return string.Join(". ", parts) + ".";
    }

    #endregion
}
