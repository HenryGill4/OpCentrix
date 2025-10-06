using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.Text.Json;

namespace OpCentrix.Services;

public interface IOperationalTaskEvaluationService
{
    Task EvaluateAsync(CancellationToken cancellationToken = default);
}

public class OperationalTaskEvaluationService : IOperationalTaskEvaluationService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<OperationalTaskEvaluationService> _logger;
    private const string LOG_PREFIX = "[MAINT][EVAL]";

    public OperationalTaskEvaluationService(SchedulerContext context, ILogger<OperationalTaskEvaluationService> logger)
    { _context = context; _logger = logger; }

    private static string NormalizeMaterial(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "Any";
        raw = raw.Trim();
        return raw.ToLower() switch
        {
            "inc" or "inco" or "inconel" or "in718" or "in625" => "Inconel",
            "ti" or "ti64" or "ti-6al-4v" or "titanium" => "Titanium",
            "pa" or "pa12" or "pa-12" => "PA12",
            _ => raw
        };
    }

    public async Task EvaluateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var start = DateTime.UtcNow;
            var now = start;

            // Snapshot completed builds with effective material
            var completedBuilds = await _context.BuildJobs
                .Include(b => b.Part)
                .Where(b => b.Status == "Completed" && b.CompletedAt != null)
                .Select(b => new { b.PrinterName, CompletedAt = b.CompletedAt!.Value, Start = b.ActualStartTime, Mat = TaskTriggerHelpers.NormalizeMaterial(TaskTriggerHelpers.ResolveEffectiveMaterial(b)), b.BuildId })
                .ToListAsync(cancellationToken);

            // Pre-group by machine
            var buildsByMachine = completedBuilds
                .GroupBy(b => b.PrinterName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.CompletedAt).ToList());

            var allOrdered = completedBuilds.OrderBy(b => b.CompletedAt).ToList();

            var open = await _context.OperationalTasks
                .Where(t => (t.Status == "Open" || t.Status == "InProgress") && t.ConfigJson != null)
                .OrderBy(t => t.Id)
                .Take(500)
                .ToListAsync(cancellationToken);

            int updated = 0, scanned = 0, skippedInvalid = 0, alreadyDue = 0, parseErrors = 0;

            foreach (var task in open)
            {
                scanned++;
                if (task.DueAt != null) { alreadyDue++; continue; }
                try
                {
                    using var doc = JsonDocument.Parse(task.ConfigJson!);
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("intervals", out var intervals) || intervals.ValueKind != JsonValueKind.Array || intervals.GetArrayLength() == 0)
                    { skippedInvalid++; continue; }
                    var logic = root.TryGetProperty("logic", out var logicProp) ? (logicProp.GetString() ?? "ANY") : "ANY";
                    bool requireAll = logic.Equals("ALL", StringComparison.OrdinalIgnoreCase);
                    bool anyTriggered = false; bool allTriggered = true;

                    // Baseline improved: LastResetAt (explicit) else CreatedAt else CompletedAt
                    var baseline = task.LastResetAt ?? task.CreatedAt;

                    foreach (var interval in intervals.EnumerateArray())
                    {
                        if (interval.ValueKind != JsonValueKind.Object) { allTriggered = false; continue; }
                        var type = interval.TryGetProperty("type", out var tp) ? tp.GetString() : null;
                        if (string.IsNullOrWhiteSpace(type)) { allTriggered = false; continue; }
                        bool triggered = false;
                        switch (type)
                        {
                            case "FixedDate":
                                if (interval.TryGetProperty("fixedDate", out var fd) && DateTime.TryParse(fd.GetString(), out var fixedDateUtc) && fixedDateUtc <= now)
                                    triggered = true; else allTriggered = false;
                                break;
                            case "DayOfWeek":
                                if (interval.TryGetProperty("dayOfWeek", out var dow) && Enum.TryParse<DayOfWeek>(dow.GetString() ?? string.Empty, out var day) && day == now.DayOfWeek)
                                    triggered = true; else allTriggered = false;
                                break;
                            case "CalendarWeeks":
                                if (interval.TryGetProperty("every", out var ev) && ev.TryGetInt32(out var weeks) && weeks > 0)
                                {
                                    var diffWeeks = (now - baseline).TotalDays / 7.0;
                                    if (diffWeeks >= weeks) triggered = true; else allTriggered = false;
                                }
                                else allTriggered = false;
                                break;
                            case "BuildCount":
                                if (interval.TryGetProperty("threshold", out var th) && th.TryGetInt32(out var threshold) && threshold > 0)
                                {
                                    var rawMat = interval.TryGetProperty("material", out var matProp) ? (matProp.GetString() ?? "Any") : "Any";
                                    var normMat = TaskTriggerHelpers.NormalizeMaterial(rawMat);
                                    int count = 0;
                                    if (!string.IsNullOrEmpty(task.MachineId))
                                    {
                                        if (buildsByMachine.TryGetValue(task.MachineId, out var list))
                                        {
                                            var filtered = list.Where(b => b.CompletedAt > baseline).ToList();
                                            count = normMat.Equals("Any", StringComparison.OrdinalIgnoreCase) ? filtered.Count : filtered.Count(b => b.Mat == normMat);
                                        }
                                    }
                                    else
                                    {
                                        var filtered = allOrdered.Where(b => b.CompletedAt > baseline).ToList();
                                        count = normMat.Equals("Any", StringComparison.OrdinalIgnoreCase) ? filtered.Count : filtered.Count(b => b.Mat == normMat);
                                    }
                                    if (count >= threshold) triggered = true; else allTriggered = false;
                                }
                                else allTriggered = false;
                                break;
                            case "MachineHours":
                                if (interval.TryGetProperty("hours", out var hr) && hr.TryGetDouble(out var hourThreshold) && hourThreshold > 0.01 && !string.IsNullOrEmpty(task.MachineId))
                                {
                                    if (buildsByMachine.TryGetValue(task.MachineId, out var list))
                                    {
                                        double hours = 0;
                                        foreach (var b in list)
                                        {
                                            if (b.CompletedAt <= baseline) continue;
                                            var effectiveStart = b.Start < baseline ? baseline : b.Start;
                                            hours += (b.CompletedAt - effectiveStart).TotalHours;
                                        }
                                        if (hours >= hourThreshold) triggered = true; else allTriggered = false;
                                    }
                                    else allTriggered = false;
                                }
                                else allTriggered = false;
                                break;
                            default:
                                allTriggered = false;
                                break;
                        }
                        if (triggered)
                        {
                            anyTriggered = true;
                            if (!requireAll) break; // ANY
                        }
                    }

                    bool shouldDue = requireAll ? allTriggered : anyTriggered;
                    if (shouldDue)
                    {
                        task.DueAt = now;
                        task.OverdueFlag = 0;
                        updated++;
                        _logger.LogInformation($"{LOG_PREFIX} Task {task.Id} due (logic={logic}, machine={task.MachineId ?? "*"})");
                    }
                }
                catch (Exception exInterval)
                {
                    parseErrors++;
                    _logger.LogWarning(exInterval, $"{LOG_PREFIX} Parse/eval error for task {{TaskId}}", task.Id);
                }
            }

            if (updated > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            var elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
            _logger.LogInformation($"{LOG_PREFIX} Scan={scanned} Updated={updated} AlreadyDue={alreadyDue} SkippedInvalid={skippedInvalid} ParseErrors={parseErrors} ElapsedMs={elapsedMs:F1}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{LOG_PREFIX} Evaluation failure");
        }
    }
}

public class OperationalTaskEvaluationHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OperationalTaskEvaluationHostedService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
    private const string LOG_PREFIX = "[MAINT][EVAL]";

    public OperationalTaskEvaluationHostedService(IServiceProvider services, ILogger<OperationalTaskEvaluationHostedService> logger)
    { _services = services; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var flag = Environment.GetEnvironmentVariable("OP_TASK_EVAL_ENABLED");
        if (!(flag == "1" || (flag?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false)))
        {
            _logger.LogInformation($"{LOG_PREFIX} Disabled (set OP_TASK_EVAL_ENABLED=1 to enable)");
            return;
        }
        _logger.LogInformation($"{LOG_PREFIX} Started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IOperationalTaskEvaluationService>();
                await svc.EvaluateAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{LOG_PREFIX} Loop failure");
            }
            await Task.Delay(_interval, stoppingToken);
        }
        _logger.LogInformation($"{LOG_PREFIX} Stopped");
    }
}
