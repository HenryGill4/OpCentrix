using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;
using OpCentrix.Services;
using OpCentrix.Models.MaintenanceV2;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Printing
{
    [PrintingAccess]
    public class IndexModel : PageModel
    {
        private readonly IOperationalTaskService _taskService;
        private readonly ILogger<IndexModel> _logger;

        public List<OperationalTask> OperationalTasks { get; set; } = new();

        public int HighPriorityCount { get; private set; }
        public int OverdueCount { get; private set; }
        public DateTime SnapshotUtc { get; private set; }

        private static readonly string[] PrintingMachines = { "TI1", "TI2", "INC", "TI3", "TI4" };

        public IndexModel(IOperationalTaskService taskService, ILogger<IndexModel> logger)
        {
            _taskService = taskService;
            _logger = logger;
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

                // Signal front-end (htmx) to refresh and close modal (listeners already in modal script)
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
                    (task.MachineId != null && PrintingMachines.Contains(task.MachineId, StringComparer.OrdinalIgnoreCase)) ||
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