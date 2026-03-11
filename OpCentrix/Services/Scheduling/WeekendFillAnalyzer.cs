using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Scheduling;

/// <summary>
/// Analyzes schedule gaps, particularly for weekends and overnight periods.
/// Suggests jobs from backlog that could fill underutilized time.
/// </summary>
public class WeekendFillAnalyzer
{
    private readonly SchedulerContext _context;
    private readonly ILogger _logger;

    public WeekendFillAnalyzer(SchedulerContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Find gaps in the schedule based on request parameters
    /// </summary>
    public async Task<List<ScheduleGapSuggestion>> FindGapsAsync(
        ScheduleOptimizationRequest request,
        CancellationToken ct = default)
    {
        var gaps = new List<ScheduleGapSuggestion>();

        // Get machines to analyze
        var machineQuery = _context.Machines
            .Where(m => m.IsActive)
            .Where(m => m.MachineType == "SLS" || m.MachineType == "DMLS" || m.MachineType == "Metal3D");

        if (request.MachineIds.Any())
            machineQuery = machineQuery.Where(m => request.MachineIds.Contains(m.Id));

        var machines = await machineQuery
            .Select(m => new { m.Id, m.MachineId, m.Name })
            .ToListAsync(ct);

        foreach (var machine in machines)
        {
            var machineGaps = await FindMachineGapsAsync(
                machine.Id,
                machine.MachineId ?? machine.Name ?? $"Machine-{machine.Id}",
                request.StartDate,
                request.EndDate,
                request.MinGapHours,
                request.IncludeWeekends,
                ct);

            gaps.AddRange(machineGaps);
        }

        // Sort by gap duration (largest first)
        return gaps.OrderByDescending(g => g.GapDurationHours).ToList();
    }

    /// <summary>
    /// Find gaps for a specific machine
    /// </summary>
    private async Task<List<ScheduleGapSuggestion>> FindMachineGapsAsync(
        int machineId,
        string machineCode,
        DateTime startDate,
        DateTime endDate,
        double minGapHours,
        bool includeWeekends,
        CancellationToken ct)
    {
        var gaps = new List<ScheduleGapSuggestion>();

        // Get machine's MachineId (string) to match jobs
        var machine = await _context.Machines.FindAsync(new object[] { machineId }, ct);
        if (machine == null) return gaps;

        var machineIdString = machine.MachineId ?? machineId.ToString();

        // Get scheduled jobs for this machine in the date range
        var scheduledJobs = await _context.Jobs
            .Where(j => j.MachineId == machineIdString)
            .Where(j => j.ScheduledStart >= startDate && j.ScheduledStart <= endDate)
            .Where(j => j.Status != "Completed" && j.Status != "Cancelled")
            .OrderBy(j => j.ScheduledStart)
            .Select(j => new
            {
                StartDate = j.ScheduledStart,
                EndDate = j.ScheduledEnd
            })
            .ToListAsync(ct);

        // Find gaps between jobs
        var currentTime = startDate;

        foreach (var job in scheduledJobs)
        {
            if (job.StartDate > currentTime)
            {
                var gapDuration = (job.StartDate - currentTime).TotalHours;

                if (gapDuration >= minGapHours)
                {
                    var isWeekend = IsWeekendPeriod(currentTime, job.StartDate);

                    if (includeWeekends || !isWeekend)
                    {
                        var gap = new ScheduleGapSuggestion
                        {
                            MachineId = machineId,
                            MachineCode = machineCode,
                            GapStart = currentTime,
                            GapEnd = job.StartDate,
                            IsWeekend = isWeekend,
                            SuggestedJobs = await FindJobsForGapAsync(gapDuration, machineId, ct),
                            UtilizationImprovement = CalculateUtilizationImprovement(gapDuration, startDate, endDate)
                        };

                        gaps.Add(gap);
                    }
                }
            }

            currentTime = job.EndDate > currentTime ? job.EndDate : currentTime;
        }

        // Check for gap at the end
        if (currentTime < endDate)
        {
            var gapDuration = (endDate - currentTime).TotalHours;

            if (gapDuration >= minGapHours)
            {
                var isWeekend = IsWeekendPeriod(currentTime, endDate);

                if (includeWeekends || !isWeekend)
                {
                    var gap = new ScheduleGapSuggestion
                    {
                        MachineId = machineId,
                        MachineCode = machineCode,
                        GapStart = currentTime,
                        GapEnd = endDate,
                        IsWeekend = isWeekend,
                        SuggestedJobs = await FindJobsForGapAsync(gapDuration, machineId, ct),
                        UtilizationImprovement = CalculateUtilizationImprovement(gapDuration, startDate, endDate)
                    };

                    gaps.Add(gap);
                }
            }
        }

        return gaps;
    }

    /// <summary>
    /// Find jobs from backlog that could fit in a gap
    /// </summary>
    private async Task<List<GapFillJob>> FindJobsForGapAsync(
        double gapHours,
        int machineId,
        CancellationToken ct)
    {
        var suggestions = new List<GapFillJob>();

        // Look for pending jobs that could fit
        var pendingJobs = await _context.Jobs
            .Where(j => j.Status == "Pending" || j.Status == "Queued")
            .Where(j => j.EstimatedHours <= gapHours)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CustomerDueDate)
            .Take(5)
            .Include(j => j.Part)
            .ToListAsync(ct);

        foreach (var job in pendingJobs)
        {
            var duration = job.EstimatedHours > 0 ? job.EstimatedHours : 8;
            var fitScore = CalculateFitScore(duration, gapHours);

            suggestions.Add(new GapFillJob
            {
                PartId = job.PartId,
                PartNumber = job.Part?.PartNumber ?? job.PartNumber ?? "Unknown",
                StackLevel = job.StackLevel ?? 1,
                Quantity = job.Quantity,
                EstimatedDurationHours = duration,
                FitScore = fitScore,
                Source = "Pending"
            });
        }

        // Look for parts that have been printed before (repeats)
        var recentParts = await _context.Parts
            .OrderByDescending(p => p.LastModifiedDate)
            .Take(10)
            .ToListAsync(ct);

        foreach (var part in recentParts)
        {
            // Try to find a MasterPart with matching PartNumber
            var masterPart = await _context.MasterParts
                .FirstOrDefaultAsync(mp => mp.PartNumber == part.PartNumber, ct);
            if (masterPart == null) continue;

            var duration = masterPart.EffectiveSingleDuration;

            if (duration <= gapHours && !suggestions.Any(s => s.PartId == part.Id))
            {
                var fitScore = CalculateFitScore(duration, gapHours);

                suggestions.Add(new GapFillJob
                {
                    PartId = part.Id,
                    PartNumber = part.PartNumber ?? "",
                    StackLevel = 1,
                    Quantity = masterPart.PartsPerBuildSingle,
                    EstimatedDurationHours = duration,
                    FitScore = fitScore,
                    Source = "Repeat"
                });
            }
        }

        return suggestions
            .OrderByDescending(s => s.FitScore)
            .Take(5)
            .ToList();
    }

    /// <summary>
    /// Check if a time period includes a weekend
    /// </summary>
    private static bool IsWeekendPeriod(DateTime start, DateTime end)
    {
        var current = start;
        while (current < end)
        {
            if (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday)
                return true;
            current = current.AddHours(12);
        }
        return false;
    }

    /// <summary>
    /// Calculate how well a job fits a gap (0-100)
    /// </summary>
    private static double CalculateFitScore(double jobHours, double gapHours)
    {
        if (jobHours > gapHours) return 0;

        // Perfect fit = 100, leaving gaps = lower score
        var utilization = jobHours / gapHours;

        // 90-100% utilization = full score
        // 70-90% = good
        // <70% = okay but not ideal
        if (utilization >= 0.9) return 100;
        if (utilization >= 0.7) return 70 + (utilization - 0.7) * 100;
        return utilization * 100;
    }

    /// <summary>
    /// Calculate utilization improvement percentage
    /// </summary>
    private static double CalculateUtilizationImprovement(double gapHours, DateTime periodStart, DateTime periodEnd)
    {
        var totalHours = (periodEnd - periodStart).TotalHours;
        if (totalHours <= 0) return 0;

        return (gapHours / totalHours) * 100;
    }
}
