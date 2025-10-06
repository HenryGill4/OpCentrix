using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.MaintenanceV2;
using System.Text.Json;

namespace OpCentrix.Services;

public interface IOperationalTaskService
{
    Task<OperationalTask> CreateAsync(OperationalTask task);
    Task<List<OperationalTask>> GetOpenAsync(int max = 25);
    Task<bool> CompleteAsync(int id, int userId, bool early = false);
    Task<bool> ResetAsync(int id, int userId); // NEW: early reset without completion
}

public class DuplicateOperationalTaskException : Exception
{ public DuplicateOperationalTaskException(string message) : base(message) { } }

public class OperationalTaskService : IOperationalTaskService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<OperationalTaskService> _logger;
    private static bool _tableChecked;
    private static bool _configOk;
    private static bool _overdueOk;
    private static bool _lastResetOk;

    public OperationalTaskService(SchedulerContext context, ILogger<OperationalTaskService> logger)
    { _context = context; _logger = logger; }

    private bool IsInMemory => _context.Database.ProviderName?.Contains("InMemory", StringComparison.OrdinalIgnoreCase) == true;

    private async Task EnsureSchemaAsync()
    {
        // Always attempt column healing even if table already checked (idempotent & cheap)
        await EnsureTableAsync();
        if (!_configOk) await EnsureColumnAsync("ConfigJson", "TEXT NULL", c => _configOk = c);
        if (!_overdueOk) await EnsureColumnAsync("OverdueFlag", "INTEGER NULL", c => _overdueOk = c, createIndex: "CREATE INDEX IF NOT EXISTS IX_OperationalTask_OverdueFlag ON OperationalTasks(OverdueFlag)");
        if (!_lastResetOk) await EnsureColumnAsync("LastResetAt", "TEXT NULL", c => _lastResetOk = c);
    }

    private async Task EnsureTableAsync()
    {
        if (IsInMemory) { _tableChecked = true; return; }
        if (_tableChecked) return;
        try
        {
            // If plural table already exists, we're done
            bool pluralExists = false;
            try { await _context.Database.ExecuteSqlRawAsync("PRAGMA table_info('OperationalTasks')"); pluralExists = true; } catch { }
            if (pluralExists) { _tableChecked = true; return; }

            // Plural missing. Check for legacy singular table.
            bool singularExists = false;
            try { await _context.Database.ExecuteSqlRawAsync("PRAGMA table_info('OperationalTask')"); singularExists = true; } catch { }

            if (singularExists)
            {
                // Try simple rename first (SQLite >=3.25). If fails, fallback to copy method.
                bool renamed = false;
                try
                {
                    await _context.Database.ExecuteSqlRawAsync("ALTER TABLE OperationalTask RENAME TO OperationalTasks");
                    renamed = true;
                    _logger.LogInformation("[MAINT][INIT] Renamed legacy OperationalTask -> OperationalTasks");
                }
                catch (Exception rex)
                {
                    _logger.LogWarning(rex, "[MAINT][WARN] Rename legacy table failed, attempting copy migration");
                }

                if (!renamed)
                {
                    // Create new plural table with full schema then copy overlapping columns
                    var createSql = @"CREATE TABLE IF NOT EXISTS OperationalTasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT NULL,
                        TaskType TEXT NULL,
                        MachineId TEXT NULL,
                        AssetId INTEGER NULL,
                        Status TEXT NULL,
                        Priority INTEGER NOT NULL,
                        CreatedByUserId INTEGER NOT NULL,
                        AssignedUserId INTEGER NULL,
                        DueAt TEXT NULL,
                        CompletedAt TEXT NULL,
                        Category TEXT NULL,
                        Tags TEXT NULL,
                        CreatedAt TEXT NOT NULL,
                        ConfigJson TEXT NULL,
                        OverdueFlag INTEGER NULL,
                        LastResetAt TEXT NULL
                    );";
                    try { await _context.Database.ExecuteSqlRawAsync(createSql); } catch { }

                    // Determine which legacy columns exist (fallback minimal copy)
                    // Attempt copy only for columns present in legacy table to avoid errors
                    var copyCols = new[] { "Id","Title","Description","TaskType","MachineId","AssetId","Status","Priority","CreatedByUserId","AssignedUserId","DueAt","CompletedAt","Category","Tags","CreatedAt" };
                    var colsCsv = string.Join(",", copyCols);
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"INSERT INTO OperationalTasks({colsCsv}) SELECT {colsCsv} FROM OperationalTask");
                        _logger.LogInformation("[MAINT][INIT] Copied legacy data from OperationalTask -> OperationalTasks");
                        try { await _context.Database.ExecuteSqlRawAsync("DROP TABLE OperationalTask"); } catch { }
                    }
                    catch (Exception cex)
                    {
                        _logger.LogWarning(cex, "[MAINT][WARN] Failed copying legacy OperationalTask data");
                    }
                }
            }
            else
            {
                // Neither table present – create fresh plural table
                var sql = @"CREATE TABLE IF NOT EXISTS OperationalTasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT NULL,
                        TaskType TEXT NULL,
                        MachineId TEXT NULL,
                        AssetId INTEGER NULL,
                        Status TEXT NULL,
                        Priority INTEGER NOT NULL,
                        CreatedByUserId INTEGER NOT NULL,
                        AssignedUserId INTEGER NULL,
                        DueAt TEXT NULL,
                        CompletedAt TEXT NULL,
                        Category TEXT NULL,
                        Tags TEXT NULL,
                        CreatedAt TEXT NOT NULL,
                        ConfigJson TEXT NULL,
                        OverdueFlag INTEGER NULL,
                        LastResetAt TEXT NULL
                    );
                    CREATE INDEX IF NOT EXISTS IX_OperationalTasks_Status ON OperationalTasks(Status);
                    CREATE INDEX IF NOT EXISTS IX_OperationalTasks_Priority ON OperationalTasks(Priority);
                    CREATE INDEX IF NOT EXISTS IX_OperationalTask_Status_Priority ON OperationalTasks(Status, Priority);
                    CREATE INDEX IF NOT EXISTS IX_OperationalTask_OverdueFlag ON OperationalTasks(OverdueFlag);";
                try { await _context.Database.ExecuteSqlRawAsync(sql); _logger.LogInformation("[MAINT][INIT] Created OperationalTasks table (new)"); } catch (Exception ex) { _logger.LogError(ex, "[MAINT][ERR] Failed creating OperationalTasks table"); }
            }
        }
        finally { _tableChecked = true; }
    }

    private async Task EnsureColumnAsync(string column, string definition, Action<bool> setFlag, string? createIndex = null)
    {
        if (IsInMemory) { setFlag(true); return; }
        // Try plural then singular; attempt select to detect existence.
        bool ok = false;
        foreach (var table in new[] { "OperationalTasks", "OperationalTask" })
        {
            try { await _context.Database.ExecuteSqlRawAsync($"SELECT {column} FROM {table} LIMIT 1"); ok = true; break; }
            catch
            {
                // attempt patch
                try
                {
                    await _context.Database.ExecuteSqlRawAsync($"ALTER TABLE {table} ADD COLUMN {column} {definition}");
                    _logger.LogInformation("[MAINT][INIT] Added column {Column} on {Table}", column, table);
                    if (!string.IsNullOrEmpty(createIndex) && table == "OperationalTasks")
                    {
                        try { await _context.Database.ExecuteSqlRawAsync(createIndex); } catch { }
                    }
                    ok = true; break;
                }
                catch { /* swallow and continue */ }
            }
        }
        setFlag(ok);
        if (!ok) _logger.LogWarning("[MAINT][WARN] Column {Column} still missing (OperationalTask/OperationalTasks)", column);
    }

    public async Task<OperationalTask> CreateAsync(OperationalTask task)
    {
        await EnsureSchemaAsync();

        task.Title = task.Title.Trim();
        if (task.Title.Length < 3) throw new ArgumentException("Title too short");
        if (task.Title.Length > 160) throw new ArgumentException("Title too long");
        if (task.Priority < 1 || task.Priority > 5) task.Priority = 3;
        if (task.DueAt.HasValue && task.DueAt.Value < DateTime.UtcNow.AddMinutes(-5)) task.DueAt = DateTime.UtcNow.AddHours(1);

        bool suppressIfOpen = false;
        if (!string.IsNullOrWhiteSpace(task.ConfigJson))
        {
            if (task.ConfigJson.Length > 8000) task.ConfigJson = task.ConfigJson[..8000];
            try
            {
                using var doc = JsonDocument.Parse(task.ConfigJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("options", out var opts))
                {
                    if (opts.TryGetProperty("suppressIfOpen", out var s) && s.ValueKind == JsonValueKind.True) suppressIfOpen = true;
                }
                if (suppressIfOpen)
                {
                    var existing = await _context.OperationalTasks
                        .Where(t => (t.Status == "Open" || t.Status == "InProgress")
                               && t.MachineId == task.MachineId
                               && t.Title.ToLower() == task.Title.ToLower())
                        .FirstOrDefaultAsync();
                    if (existing != null) throw new DuplicateOperationalTaskException("A similar open task already exists for this machine (duplicate suppression)");
                }
            }
            catch (DuplicateOperationalTaskException) { throw; }
            catch (Exception ex) { _logger.LogWarning(ex, "[MAINT][WARN] ConfigJson parse failed during create"); }
        }

        // Initialize OverdueFlag
        task.OverdueFlag = (task.DueAt.HasValue && task.DueAt.Value < DateTime.UtcNow) ? 1 : 0;

        _context.OperationalTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<List<OperationalTask>> GetOpenAsync(int max = 25)
    {
        await EnsureSchemaAsync();
        try
        {
            var cutoff = DateTime.UtcNow;
            var overdue = await _context.OperationalTasks
                .Where(t => (t.Status == "Open" || t.Status == "InProgress") && t.DueAt != null && t.DueAt < cutoff && (t.OverdueFlag == null || t.OverdueFlag == 0))
                .ToListAsync();
            if (overdue.Count > 0)
            {
                foreach (var t in overdue) t.OverdueFlag = 1;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex) { _logger.LogWarning(ex, "[MAINT][WARN] Overdue refresh failed"); }

        var list = await _context.OperationalTasks
            .Where(t => t.Status == "Open" || t.Status == "InProgress")
            .OrderByDescending(t => t.Priority).ThenBy(t => t.DueAt ?? DateTime.MaxValue)
            .Take(max)
            .ToListAsync();

        var recentlyCompleted = await _context.OperationalTasks
            .Where(t => t.Status == "Completed" && t.ConfigJson != null && t.CompletedAt != null)
            .OrderByDescending(t => t.CompletedAt)
            .Take(10)
            .ToListAsync();
        list.AddRange(recentlyCompleted);

        try
        {
            var includeAllMachines = list.Any(t => string.IsNullOrEmpty(t.MachineId));
            var machines = list.Where(t => !string.IsNullOrEmpty(t.MachineId)).Select(t => t.MachineId!).Distinct().ToList();

            var buildQuery = _context.BuildJobs
                .Include(b => b.Part)
                .Where(b => b.Status == "Completed" && b.CompletedAt != null);
            if (!includeAllMachines && machines.Any())
                buildQuery = buildQuery.Where(b => machines.Contains(b.PrinterName));

            var builds = await buildQuery
                .Select(b => new { b.BuildId, b.PrinterName, CompletedAt = b.CompletedAt!.Value, Mat = TaskTriggerHelpers.NormalizeMaterial(TaskTriggerHelpers.ResolveEffectiveMaterial(b)) })
                .ToListAsync();

            bool anyUpdates = false; // NEW: track if auto-due modifications applied

            foreach (var t in list.Where(t => !string.IsNullOrEmpty(t.ConfigJson)))
            {
                try
                {
                    using var doc = JsonDocument.Parse(t.ConfigJson!);
                    var root = doc.RootElement;
                    if (!root.TryGetProperty("intervals", out var intervals) || intervals.ValueKind != JsonValueKind.Array) continue;
                    var baseline = t.LastResetAt ?? (t.Status == "Completed" ? (t.CompletedAt ?? t.CreatedAt) : t.CreatedAt);
                    foreach (var interval in intervals.EnumerateArray())
                    {
                        var type = interval.TryGetProperty("type", out var tp) ? tp.GetString() : null;
                        if (string.IsNullOrEmpty(type)) continue;
                        switch (type)
                        {
                            case "BuildCount":
                                int threshold = interval.TryGetProperty("threshold", out var thEl) && thEl.TryGetInt32(out var thv) ? thv : 0;
                                if (threshold <= 0) break;
                                var rawMat = interval.TryGetProperty("material", out var mEl) ? (mEl.GetString() ?? "Any") : "Any";
                                var normMat = TaskTriggerHelpers.NormalizeMaterial(rawMat);
                                var candidates = builds
                                    .Where(b => (string.IsNullOrEmpty(t.MachineId) || b.PrinterName == t.MachineId) && b.CompletedAt > baseline)
                                    .ToList();
                                int count = normMat.Equals("Any", StringComparison.OrdinalIgnoreCase)
                                    ? candidates.Count
                                    : candidates.Count(c => c.Mat == normMat);
                                t.ProgressSummaries.Add($"Builds({normMat}) {count}/{threshold}");

                                // NEW: Auto-due / overdue escalation when threshold met or exceeded
                                if (t.Status != "Completed" && count >= threshold)
                                {
                                    // If not previously due, mark due NOW so UI shows status immediately
                                    if (!t.DueAt.HasValue)
                                    {
                                        t.DueAt = DateTime.UtcNow;
                                    }
                                    // Mark overdue immediately (business rule: any exceed = needs action now)
                                    t.OverdueFlag = 1;
                                    anyUpdates = true;
                                }
                                break;
                            case "MachineHours":
                                double hoursThreshold = interval.TryGetProperty("hours", out var hEl) && hEl.TryGetDouble(out var hv) ? hv : 0;
                                if (hoursThreshold > 0 && !string.IsNullOrEmpty(t.MachineId))
                                {
                                    var machineBuilds = builds.Where(b => b.PrinterName == t.MachineId && b.CompletedAt > baseline).ToList();
                                    double hours = 0; // placeholder until durations added here if required
                                    t.ProgressSummaries.Add($"Hours {(int)Math.Round(hours)}/{hoursThreshold}");
                                }
                                break;
                            case "CalendarWeeks":
                                int every = interval.TryGetProperty("every", out var evEl) && evEl.TryGetInt32(out var evv) ? evv : 0;
                                var weeks = (DateTime.UtcNow - baseline).TotalDays / 7.0;
                                t.ProgressSummaries.Add($"Weeks {weeks:F1}/{every}");
                                if (every > 0 && weeks >= every && t.Status != "Completed")
                                {
                                    if (!t.DueAt.HasValue) t.DueAt = DateTime.UtcNow;
                                    t.OverdueFlag = 1; anyUpdates = true;
                                }
                                break;
                            case "DayOfWeek":
                                var dow = interval.TryGetProperty("dayOfWeek", out var dEl) ? (dEl.GetString() ?? "?") : "?";
                                t.ProgressSummaries.Add($"Weekly {dow}");
                                break;
                            case "FixedDate":
                                var fd = interval.TryGetProperty("fixedDate", out var fdEl) ? (fdEl.GetString() ?? "?") : "?";
                                t.ProgressSummaries.Add($"Fixed {fd}");
                                if (DateTime.TryParse(fd, out var fixedDateUtc) && fixedDateUtc <= DateTime.UtcNow && t.Status != "Completed")
                                {
                                    if (!t.DueAt.HasValue) t.DueAt = fixedDateUtc;
                                    t.OverdueFlag = fixedDateUtc < DateTime.UtcNow ? 1 : t.OverdueFlag; anyUpdates = true;
                                }
                                break;
                        }
                    }
                }
                catch { }
            }
            if (anyUpdates)
            {
                try { await _context.SaveChangesAsync(); } catch (Exception ex) { _logger.LogWarning(ex, "[MAINT][WARN] Failed saving auto-due updates"); }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[MAINT][WARN] Failed computing progress summaries");
        }

        return list;
    }

    public async Task<bool> CompleteAsync(int id, int userId, bool early = false)
    {
        await EnsureSchemaAsync();
        var task = await _context.OperationalTasks.FirstOrDefaultAsync(t => t.Id == id);
        if (task == null) return false;
        if (!early)
        {
            if (task.Status == "Completed") return true;
            task.Status = "Completed";
            task.CompletedAt = DateTime.UtcNow;
        }
        if (early && task.Status != "Completed")
        {
            task.LastResetAt = DateTime.UtcNow;
            task.DueAt = null;
            task.OverdueFlag = 0;
        }
        if (!task.AssignedUserId.HasValue) task.AssignedUserId = userId;
        if (!early) task.OverdueFlag = null; else task.OverdueFlag = 0;

        OperationalTask? clone = null;
        if (!early && !string.IsNullOrWhiteSpace(task.ConfigJson))
        {
            try
            {
                using var doc = JsonDocument.Parse(task.ConfigJson);
                if (doc.RootElement.TryGetProperty("intervals", out var intervalsEl) && intervalsEl.ValueKind == JsonValueKind.Array && intervalsEl.GetArrayLength() > 0)
                {
                    clone = new OperationalTask
                    {
                        Title = task.Title,
                        Description = task.Description,
                        Priority = task.Priority,
                        MachineId = task.MachineId,
                        CreatedByUserId = userId,
                        Status = "Open",
                        ConfigJson = task.ConfigJson,
                        CreatedAt = DateTime.UtcNow,
                        LastResetAt = task.CompletedAt, // baseline inclusive so completed build counts if needed
                        OverdueFlag = 0,
                        Category = task.Category,
                        Tags = task.Tags
                    };
                    _context.OperationalTasks.Add(clone);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MAINT][WARN] Failed cloning task {TaskId} for repeat", task.Id);
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetAsync(int id, int userId)
    { return await CompleteAsync(id, userId, early: true); }
}
