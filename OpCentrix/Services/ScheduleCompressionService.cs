using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Services;

/// <summary>
/// Service that "compresses" / reflows Scheduled SLS jobs on a single machine forward
/// honoring a base 3h setup/changeover plus material changeover delays, operating shifts, and (Phase B.1) single predecessor dependency.
/// </summary>
public interface IScheduleCompressionService
{
    Task<CompressionResult> CompressMachineAsync(
        string machineId,
        DateTime? horizonStartUtc = null,
        DateTime? horizonEndUtc = null,
        CompressionOptions? options = null,
        string? userName = null);
}

public class ScheduleCompressionService : IScheduleCompressionService
{
    private readonly SchedulerContext _context;
    private readonly IOperatingShiftService _shiftService;
    private readonly ILogger<ScheduleCompressionService> _logger;

    private const double BaseSetupHours = 3.0; // mandatory baseline setup/changeover between jobs (mirrors create job logic)
    private static readonly TimeSpan BaseSetup = TimeSpan.FromHours(BaseSetupHours);

    public ScheduleCompressionService(
        SchedulerContext context,
        IOperatingShiftService shiftService,
        ILogger<ScheduleCompressionService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _shiftService = shiftService ?? throw new ArgumentNullException(nameof(shiftService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CompressionResult> CompressMachineAsync(
        string machineId,
        DateTime? horizonStartUtc = null,
        DateTime? horizonEndUtc = null,
        CompressionOptions? options = null,
        string? userName = null)
    {
        var opId = Guid.NewGuid().ToString("N")[..8];
        options ??= CompressionOptions.Default;
        var startUtc = (horizonStartUtc ?? DateTime.UtcNow).ToUniversalTime();
        var endUtc = horizonEndUtc?.ToUniversalTime();
        var result = new CompressionResult { MachineId = machineId };
        try
        {
            if (string.IsNullOrWhiteSpace(machineId))
            {
                result.Warnings.Add("Machine id required");
                return result;
            }

            // Preload all jobs on machine in horizon *plus* potential predecessor jobs (phase B.1 simple: single predecessor possibly off-machine)
            var query = _context.Jobs
                .Where(j => j.MachineId == machineId && j.ScheduledEnd > startUtc);
            if (endUtc.HasValue)
                query = query.Where(j => j.ScheduledStart < endUtc.Value);

            var jobs = await query
                .OrderBy(j => j.ScheduledStart)
                .AsTracking()
                .ToListAsync();

            // Load predecessor references (could be on other machines) into lookup for earliest-ready calculation
            var predecessorIds = jobs.Where(j => j.PredecessorJobId.HasValue).Select(j => j.PredecessorJobId!.Value).Distinct().ToList();
            Dictionary<int, Job> predecessors = new();
            if (predecessorIds.Any())
            {
                predecessors = await _context.Jobs
                    .Where(j => predecessorIds.Contains(j.Id))
                    .ToDictionaryAsync(j => j.Id, j => j);
            }

            result.JobsConsidered = jobs.Count;
            if (!jobs.Any())
            {
                result.Warnings.Add("No jobs found in horizon");
                return result;
            }

            var mutable = jobs
                .Where(j => string.Equals(j.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (!mutable.Any())
            {
                result.Warnings.Add("No Scheduled jobs eligible for compression");
                return result;
            }

            DateTime cursor = startUtc;
            Job? previousJob = null;
            int guard = 0;

            foreach (var job in mutable)
            {
                var originalStart = job.ScheduledStart;
                var originalEnd = job.ScheduledEnd;
                var duration = originalEnd - originalStart;

                if (duration <= TimeSpan.Zero)
                {
                    previousJob = job;
                    continue;
                }

                if (options.SkipPastJobs && originalEnd <= startUtc)
                {
                    cursor = cursor < originalEnd ? originalEnd : cursor;
                    previousJob = job;
                    continue;
                }

                DateTime candidate = cursor;

                // Base setup after previous same-machine job
                if (previousJob != null)
                {
                    candidate = previousJob.ScheduledEnd > candidate ? previousJob.ScheduledEnd : candidate;
                    candidate = candidate.Add(BaseSetup);

                    if (!string.IsNullOrEmpty(previousJob.SlsMaterial) && !string.Equals(previousJob.SlsMaterial, job.SlsMaterial, StringComparison.OrdinalIgnoreCase))
                    {
                        var changeMinutes = job.CalculatePowderChangeoverTime(previousJob.SlsMaterial);
                        if (changeMinutes > 0)
                            candidate = candidate.Add(TimeSpan.FromMinutes(changeMinutes));
                    }
                }
                else
                {
                    if (candidate < startUtc)
                        candidate = startUtc;
                }

                if (candidate < startUtc)
                    candidate = startUtc;

                // PHASE B.1: Single predecessor earliest-ready enforcement (may be on different machine)
                if (job.PredecessorJobId.HasValue && predecessors.TryGetValue(job.PredecessorJobId.Value, out var pred))
                {
                    var gapHours = job.UpstreamGapHours.HasValue && job.UpstreamGapHours.Value > 0 ? job.UpstreamGapHours.Value : 0;
                    var earliestReady = pred.ScheduledEnd.AddHours(gapHours);
                    if (earliestReady > candidate)
                    {
                        candidate = earliestReady; // allow push later due to dependency
                    }
                }

                // Shift alignment (still enforce inside shift + shiftStart + base setup rule)
                candidate = await AlignWithShiftSetupAsync(candidate, duration, machineId);

                var newStart = candidate;
                var newEnd = newStart.Add(duration);

                if (!options.KeepCrossShiftAllowed && !await _shiftService.IsTimeWithinOperatingHoursAsync(newEnd, machineId))
                {
                    newStart = await AlignWithShiftSetupAsync(newEnd, duration, machineId, forceNextShift: true);
                    newEnd = newStart.Add(duration);
                }

                if (newStart != originalStart || newEnd != originalEnd)
                {
                    var minutesPulled = (originalStart - newStart).TotalMinutes;
                    if (minutesPulled > 0)
                        result.TotalMinutesPulledForward += minutesPulled;

                    job.ScheduledStart = newStart;
                    job.ScheduledEnd = newEnd;
                    job.LastModifiedDate = DateTime.UtcNow;
                    job.LastModifiedBy = userName ?? "System-Compress";

                    result.Changes.Add(new ChangedJobDto
                    {
                        Id = job.Id,
                        OldStart = originalStart,
                        OldEnd = originalEnd,
                        NewStart = newStart,
                        NewEnd = newEnd,
                        MinutesPulledForward = minutesPulled
                    });
                }

                cursor = job.ScheduledEnd;
                previousJob = job;

                if (++guard > 5000)
                {
                    result.Warnings.Add("Compression aborted: guard limit reached");
                    break;
                }
            }

            if (result.Changes.Any())
            {
                await _context.SaveChangesAsync();
            }
            else
            {
                result.Warnings.Add("No jobs moved");
            }

            result.JobsMoved = result.Changes.Count;
            _logger.LogInformation("[COMPRESS-{OpId}] Machine {MachineId} considered {Considered} jobs; moved {Moved}; pulled {Pulled} min (baseSetup=3h, predSupport=1)" , opId, machineId, result.JobsConsidered, result.JobsMoved, (int)result.TotalMinutesPulledForward);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[COMPRESS-{OpId}] Error compressing machine {MachineId}", opId, machineId);
            result.Warnings.Add("Compression failed: " + ex.Message);
        }
        return result;
    }

    /// <summary>
    /// Align a proposed start so that it lies within an operating shift and not before shiftStart + BaseSetup.
    /// If forceNextShift = true we skip the shift containing the supplied time (used when previous attempt crossed out of shift).
    /// </summary>
    private async Task<DateTime> AlignWithShiftSetupAsync(DateTime candidateUtc, TimeSpan duration, string machineId, bool forceNextShift = false)
    {
        candidateUtc = RoundUp15(candidateUtc);

        for (int dayOffset = 0; dayOffset < 14; dayOffset++)
        {
            var probe = candidateUtc.AddDays(dayOffset == 0 ? 0 : dayOffset).Date;
            var shifts = await _shiftService.GetShiftsForDayAsync(probe.DayOfWeek, machineId) ?? new List<OperatingShift>();
            List<OperatingShift>? prevDayShifts = null;
            if (dayOffset == 0)
            {
                prevDayShifts = await _shiftService.GetShiftsForDayAsync(probe.AddDays(-1).DayOfWeek, machineId);
            }

            foreach (var window in EnumerateShiftWindows(probe, shifts).Concat(EnumerateShiftWindows(probe.AddDays(-1), prevDayShifts)))
            {
                var shiftStart = window.start;
                var shiftEnd = window.end;
                if (forceNextShift && candidateUtc < shiftEnd)
                {
                    if (candidateUtc >= shiftStart)
                        continue;
                }

                if (candidateUtc < shiftStart)
                    candidateUtc = shiftStart;

                if (candidateUtc >= shiftStart && candidateUtc < shiftEnd)
                {
                    var minOperational = shiftStart.Add(BaseSetup);
                    if (candidateUtc < minOperational)
                        candidateUtc = minOperational;
                    return RoundUp15(candidateUtc);
                }
            }
        }
        return RoundUp15(candidateUtc.Add(BaseSetup));
    }

    private static DateTime RoundUp15(DateTime dt)
    {
        var minutes = ((dt.Minute + 14) / 15) * 15;
        if (minutes == 60) dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 0, 0, dt.Kind).AddHours(1);
        else dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, minutes, 0, dt.Kind);
        return dt;
    }

    private static IEnumerable<(DateTime start, DateTime end)> EnumerateShiftWindows(DateTime day, IEnumerable<OperatingShift>? shifts)
    {
        if (shifts == null) yield break;
        foreach (var s in shifts)
        {
            var start = day + s.StartTime;
            var end = day + s.EndTime;
            if (s.EndTime < s.StartTime)
                end = end.AddDays(1);
            yield return (start, end);
        }
    }
}

#region DTOs / Options
public class CompressionOptions
{
    public bool SkipInProgress { get; set; } = true; // placeholder for future (not used in MVP)
    public bool SkipPastJobs { get; set; } = false;  // do not move already completed past jobs
    public bool AlignStartsToShiftBoundary { get; set; } = false; // kept for compatibility (not used directly after refactor)
    public bool KeepCrossShiftAllowed { get; set; } = true;
    public static CompressionOptions Default => new();
}

public class CompressionResult
{
    public string MachineId { get; set; } = string.Empty;
    public int JobsConsidered { get; set; }
    public int JobsMoved { get; set; }
    public double TotalMinutesPulledForward { get; set; }
    public List<ChangedJobDto> Changes { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public IEnumerable<int> ChangedIds => Changes.Select(c => c.Id);
}

public class ChangedJobDto
{
    public int Id { get; set; }
    public DateTime OldStart { get; set; }
    public DateTime OldEnd { get; set; }
    public DateTime NewStart { get; set; }
    public DateTime NewEnd { get; set; }
    public double MinutesPulledForward { get; set; }
}
#endregion
