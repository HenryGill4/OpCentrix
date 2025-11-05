using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;
using OpCentrix.Services;
using OpCentrix.Models.MaintenanceV2;
using System.ComponentModel.DataAnnotations;
using OpCentrix.Data; // added
using Microsoft.EntityFrameworkCore; // added
using OpCentrix.Models; // added

namespace OpCentrix.Pages.Printing
{
    [PrintingAccess]
    public class IndexModel : PageModel
    {
        private readonly IOperationalTaskService _taskService;
        private readonly ILogger<IndexModel> _logger;
        private readonly SchedulerContext _context; // added

        public List<OperationalTask> OperationalTasks { get; set; } = new();

        public int HighPriorityCount { get; private set; }
        public int OverdueCount { get; private set; }
        public DateTime SnapshotUtc { get; private set; }

        private static readonly string[] PrintingMachines = { "TI1", "TI2", "INC", "TI3", "TI4" };

        public IndexModel(IOperationalTaskService taskService, ILogger<IndexModel> logger, SchedulerContext context)
        {
            _taskService = taskService;
            _logger = logger;
            _context = context; // added
        }

        [BindProperty]
        public CreateTaskInput NewTask { get; set; } = new();

        public class CreateTaskInput
        {
            [Required, StringLength(120)] public string Title { get; set; } = string.Empty;
            [StringLength(500)] public string? Description { get; set; }
            [Range(1,5)] public int Priority { get; set; } = 3;
            [StringLength(20)] public string? MachineId { get; set; }
            public DateTime? DueAt { get; set; }
            [StringLength(50)] public string? Category { get; set; }
            [StringLength(200)] public string? Tags { get; set; }
        }

        public async Task OnGetAsync() => await LoadTaskSnapshotAsync();

        // NEW: ViewModel for dynamic printer card
        public sealed class PrinterCardViewModel
        {
            public string MachineId { get; init; } = string.Empty;
            public string MachineName { get; init; } = string.Empty;
            public string Status { get; init; } = "Idle"; // Idle | Printing | Cooling | Completed | Maintenance | Offline
            public BuildJob? ActiveBuild { get; init; }
            public Job? CurrentJob { get; init; }
            public List<Job> UpcomingJobs { get; init; } = new();
            public double? ProgressPercent { get; init; }
            public BuildJob? RecentlyCompleted { get; init; }
            public int? CoolingMinutesTotal { get; init; }
            public int? CoolingMinutesRemaining { get; init; }
            public int? DowntimeMinutes { get; init; }
        }

        // NEW: HTMX handler that returns a single printer card partial
        public async Task<IActionResult> OnGetPrinterCardAsync(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return BadRequest("machineId required");
            try
            {
                machineId = machineId.Trim();
                var now = DateTime.UtcNow;

                // Persisted machine (for manual overrides)
                var machine = await _context.Machines.AsNoTracking().FirstOrDefaultAsync(m => m.MachineId == machineId);

                // Active build on machine
                var activeBuild = await _context.BuildJobs
                    .Include(b => b.Part)
                    .Where(b => b.PrinterName == machineId && b.Status == "In Progress")
                    .OrderByDescending(b => b.ActualStartTime)
                    .FirstOrDefaultAsync();

                // Most recent completed build (no short time window)
                var recentlyCompleted = await _context.BuildJobs
                    .Include(b => b.Part)
                    .Where(b => b.PrinterName == machineId && b.Status == "Completed" && b.CompletedAt.HasValue)
                    .OrderByDescending(b => b.CompletedAt)
                    .FirstOrDefaultAsync();

                // Near-term scheduled jobs
                var upcomingJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId && (j.Status == "Scheduled" || j.Status == "Building" || j.Status == "In Progress") && j.ScheduledStart >= DateTime.UtcNow.AddDays(-1))
                    .OrderBy(j => j.ScheduledStart)
                    .Take(3)
                    .ToListAsync();

                Job? currentJob = null;
                if (activeBuild != null)
                {
                    if (activeBuild.AssociatedScheduledJobId.HasValue)
                    {
                        currentJob = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == activeBuild.AssociatedScheduledJobId.Value);
                    }
                    if (currentJob == null)
                    {
                        currentJob = upcomingJobs.FirstOrDefault(j => j.Status == "Building" || j.Status == "In Progress");
                    }
                }

                // Simplified, deterministic status & progress
                string status;
                double? progress = null;
                int? coolingTotal = null;
                int? coolingRemaining = null;
                int? downtimeMinutes = null;

                // Manual overrides take precedence for demo (any canonical status)
                var manualRaw = (machine?.Status ?? string.Empty).Trim();
                var manualNorm = NormalizeStatus(manualRaw);
                bool manualOverride = !string.IsNullOrEmpty(manualNorm);

                if (manualOverride)
                {
                    status = manualNorm; // keep operator-selected state for demo
                    // Still compute progress for display if an active build exists
                    if (activeBuild != null)
                    {
                        var elapsedHrs = (now - activeBuild.ActualStartTime).TotalHours;
                        double estHrs = 0;
                        if (activeBuild.OperatorEstimatedHours.HasValue && activeBuild.OperatorEstimatedHours.Value > 0)
                        {
                            estHrs = (double)activeBuild.OperatorEstimatedHours.Value;
                        }
                        else if (currentJob != null && currentJob.EstimatedHours > 0)
                        {
                            estHrs = currentJob.EstimatedHours;
                        }
                        else if (activeBuild.Part?.EstimatedHours > 0)
                        {
                            estHrs = activeBuild.Part.EstimatedHours;
                        }
                        else
                        {
                            estHrs = Math.Max(1.0, elapsedHrs);
                        }
                        if (estHrs > 0)
                        {
                            progress = Math.Min(110, Math.Max(0, (elapsedHrs / estHrs) * 100.0));
                        }
                    }

                    // Ensure Cooling timer is available when manually set to Cooling
                    if (string.Equals(status, "Cooling", StringComparison.OrdinalIgnoreCase))
                    {
                        coolingTotal = DetermineCoolingMinutes(machineId);
                        if (recentlyCompleted?.CompletedAt.HasValue == true)
                        {
                            var minutesSinceComplete = (int)Math.Max(0, Math.Round((now - recentlyCompleted.CompletedAt.Value).TotalMinutes));
                            if (minutesSinceComplete < coolingTotal)
                            {
                                coolingRemaining = Math.Max(0, coolingTotal.Value - minutesSinceComplete);
                            }
                            else
                            {
                                // Past cooling; treat as urgent
                                status = "Urgent";
                                var dt = minutesSinceComplete - coolingTotal.Value;
                                downtimeMinutes = dt;
                                await UpsertPostCooldownDowntimeAsync(
                                    recentlyCompleted,
                                    recentlyCompleted.CompletedAt.Value.AddMinutes((double)coolingTotal.Value),
                                    now,
                                    dt);
                                coolingRemaining = 0;
                            }
                        }
                        else
                        {
                            // No completion time available; show full cooling as remaining to make timer visible
                            coolingRemaining = coolingTotal;
                        }
                    }
                }
                else if (activeBuild != null)
                {
                    status = "Printing";

                    // Progress: elapsed / estimate (operator -> current job -> part -> elapsed)
                    var elapsedHrs = (now - activeBuild.ActualStartTime).TotalHours;
                    double estHrs = 0;
                    if (activeBuild.OperatorEstimatedHours.HasValue && activeBuild.OperatorEstimatedHours.Value > 0)
                    {
                        estHrs = (double)activeBuild.OperatorEstimatedHours.Value;
                    }
                    else if (currentJob != null && currentJob.EstimatedHours > 0)
                    {
                        estHrs = currentJob.EstimatedHours;
                    }
                    else if (activeBuild.Part?.EstimatedHours > 0)
                    {
                        estHrs = activeBuild.Part.EstimatedHours;
                    }
                    else
                    {
                        estHrs = Math.Max(1.0, elapsedHrs); // minimal safe fallback
                    }
                    if (estHrs > 0)
                    {
                        progress = Math.Min(110, Math.Max(0, (elapsedHrs / estHrs) * 100.0));
                    }
                }
                else if (recentlyCompleted != null && recentlyCompleted.CompletedAt.HasValue)
                {
                    int machineCooling = DetermineCoolingMinutes(machineId);
                    coolingTotal = machineCooling;
                    var minutesSinceComplete = (int)Math.Max(0, Math.Round((now - recentlyCompleted.CompletedAt.Value).TotalMinutes));
                    if (minutesSinceComplete < machineCooling)
                    {
                        status = "Cooling";
                        coolingRemaining = Math.Max(0, machineCooling - minutesSinceComplete);
                    }
                    else
                    {
                        status = "Urgent";
                        downtimeMinutes = minutesSinceComplete - machineCooling;
                        // Persist/Upsert to DelayLog as Post-Cooldown Downtime against the completed build
                        await UpsertPostCooldownDowntimeAsync(recentlyCompleted, recentlyCompleted.CompletedAt.Value.AddMinutes(machineCooling), now, downtimeMinutes.Value);
                        coolingRemaining = 0;
                    }
                }
                else
                {
                    status = "Idle";
                }

                var vm = new PrinterCardViewModel
                {
                    MachineId = machineId,
                    MachineName = machineId, // could map to friendly name later
                    Status = status,
                    ActiveBuild = activeBuild,
                    CurrentJob = currentJob,
                    UpcomingJobs = upcomingJobs,
                    ProgressPercent = progress,
                    RecentlyCompleted = recentlyCompleted,
                    CoolingMinutesTotal = coolingTotal,
                    CoolingMinutesRemaining = coolingRemaining,
                    DowntimeMinutes = downtimeMinutes
                };
                return Partial("_PrinterCard", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load printer card for {MachineId}", machineId);
                Response.StatusCode = 500;
                return Content($"<div class='p-4 text-red-600'>Error loading {machineId} card</div>", "text/html");
            }
        }

        private static int DetermineCoolingMinutes(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return 90;
            var id = machineId.Trim().ToUpperInvariant();
            if (id.StartsWith("INC")) return 120; // 2 hours for Inconel machine
            if (id.StartsWith("TI")) return 60;   // 1 hour for Titanium machines
            return 90; // default fallback
        }

        private static string NormalizeStatus(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var sl = raw.Trim().ToLowerInvariant();
            return sl switch
            {
                "started" or "start" or "running" or "building" or "printing" => "Printing",
                "preheat" or "pre-heating" or "pre heating" or "preheating" => "Preheating",
                "cool" or "cooling" => "Cooling",
                "down" or "downtime" or "urgent" => "Urgent",
                "maint" or "maintenance" => "Maintenance",
                "idle" => "Idle",
                "offline" => "Offline",
                "error" or "fault" or "alarm" => "Error",
                _ => string.Empty
            };
        }

        private async Task UpsertPostCooldownDowntimeAsync(BuildJob completedBuild, DateTime downtimeStartUtc, DateTime nowUtc, int downtimeMinutes)
        {
            try
            {
                // Store as DelayLog entry tied to the completed build
                var reason = "Post-Cooldown Downtime";
                var existing = await _context.DelayLogs
                    .Where(d => d.BuildId == completedBuild.BuildId && d.DelayReason == reason)
                    .OrderByDescending(d => d.CreatedAt)
                    .FirstOrDefaultAsync();

                if (existing == null)
                {
                    _context.DelayLogs.Add(new DelayLog
                    {
                        BuildId = completedBuild.BuildId,
                        DelayReason = reason,
                        DelayDuration = Math.Max(0, downtimeMinutes),
                        Description = $"Machine {completedBuild.PrinterName} downtime since cooldown end at {downtimeStartUtc:u}",
                        CreatedAt = nowUtc,
                        CreatedBy = "System"
                    });
                }
                else
                {
                    // Update duration to latest value
                    existing.DelayDuration = Math.Max(existing.DelayDuration, downtimeMinutes);
                    existing.CreatedAt = nowUtc;
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upsert Post-Cooldown Downtime for build {BuildId}", completedBuild.BuildId);
            }
        }

        public async Task<IActionResult> OnGetTaskNotificationsAsync(string? format = null)
        {
            await LoadTaskSnapshotAsync();
            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                var payload = OperationalTasks.Select(t => new TaskNotificationDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    MachineId = t.MachineId,
                    Priority = t.Priority,
                    DueAt = t.DueAt,
                    Overdue = t.OverdueFlag == 1,
                    Status = t.Status,
                    Tags = t.Tags,
                    Category = t.Category
                });
                return new JsonResult(new
                {
                    snapshotUtc = SnapshotUtc,
                    highPriority = HighPriorityCount,
                    overdue = OverdueCount,
                    total = OperationalTasks.Count,
                    tasks = payload
                });
            }
            return Partial("_TaskNotifications", this);
        }

        public async Task<IActionResult> OnGetCreateTaskModalAsync()
        {
            NewTask = new CreateTaskInput();
            return Partial("_CreateTaskModal", this);
        }

        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            if (!ModelState.IsValid)
            {
                Response.StatusCode = 400;
                return Partial("_CreateTaskModal", this);
            }
            try
            {
                var task = new OperationalTask
                {
                    Title = NewTask.Title.Trim(),
                    Description = NewTask.Description?.Trim(),
                    Priority = NewTask.Priority,
                    MachineId = string.IsNullOrWhiteSpace(NewTask.MachineId) ? null : NewTask.MachineId.Trim().ToUpperInvariant(),
                    DueAt = NewTask.DueAt?.ToUniversalTime(),
                    Category = NewTask.Category?.Trim(),
                    Tags = NewTask.Tags?.Trim(),
                    Status = "Open",
                    OverdueFlag = 0
                };
                await _taskService.CreateAsync(task);
                await LoadTaskSnapshotAsync();

                Response.Headers["HX-Trigger"] = "{\"taskCreated\":true}";
                // Return empty content so the modal wrapper innerHTML becomes empty -> JS closes it
                return Content(string.Empty, "text/html");
            }
            catch (DuplicateOperationalTaskException)
            {
                ModelState.AddModelError(string.Empty, "A similar task already exists.");
                Response.StatusCode = 409;
                return Partial("_CreateTaskModal", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed creating operational task");
                ModelState.AddModelError(string.Empty, "Unexpected error creating task.");
                Response.StatusCode = 500;
                return Partial("_CreateTaskModal", this);
            }
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskService.CompleteAsync(taskId, userId);
                if (success)
                    _logger.LogInformation("Task {TaskId} completed by user {UserId}", taskId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", taskId);
            }
            return await OnGetTaskNotificationsAsync();
        }

        public async Task<IActionResult> OnPostDismissTaskAsync(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskService.ResetAsync(taskId, userId);
                if (success)
                    _logger.LogInformation("Task {TaskId} dismissed/reset by user {UserId}", taskId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing task {TaskId}", taskId);
            }
            return await OnGetTaskNotificationsAsync();
        }

        private async Task LoadTaskSnapshotAsync()
        {
            try
            {
                SnapshotUtc = DateTime.UtcNow;
                OperationalTasks = await CreateTaskNotificationSnapshotAsync();
                HighPriorityCount = OperationalTasks.Count(t => t.Priority <= 2);
                OverdueCount = OperationalTasks.Count(t => t.OverdueFlag == 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load operational task snapshot");
                OperationalTasks = new();
                HighPriorityCount = 0;
                OverdueCount = 0;
            }
        }

        private async Task<List<OperationalTask>> CreateTaskNotificationSnapshotAsync(int fetchMax = 50, int displayMax = 10)
        {
            var all = await _taskService.GetOpenAsync(fetchMax);
            var filtered = all.Where(task =>
                    ((task.MachineId != null && Array.Exists(PrintingMachines, m => string.Equals(m, task.MachineId, StringComparison.OrdinalIgnoreCase)))) ||
                    string.IsNullOrEmpty(task.MachineId) ||
                    task.OverdueFlag == 1 ||
                    task.Priority <= 2)
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
                .Take(displayMax)
                .ToList();
            return filtered;
        }

        private int GetCurrentUserId()
        {
            if (HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
                return BitConverter.ToInt32(userIdBytes, 0);
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }

        private sealed record TaskNotificationDto
        {
            public int Id { get; init; }
            public string Title { get; init; } = string.Empty;
            public string? Description { get; init; }
            public string? MachineId { get; init; }
            public int Priority { get; init; }
            public DateTime? DueAt { get; init; }
            public bool Overdue { get; init; }
            public string Status { get; init; } = string.Empty;
            public string? Tags { get; init; }
            public string? Category { get; init; }
        }
    }
}