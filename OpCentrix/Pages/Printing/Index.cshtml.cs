using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;
using OpCentrix.Services;
using OpCentrix.Models.MaintenanceV2;

namespace OpCentrix.Pages.Printing
{
    [PrintingAccess]
    public class IndexModel : PageModel
    {
        private readonly IOperationalTaskService _taskService;
        private readonly ILogger<IndexModel> _logger;

        public List<OperationalTask> OperationalTasks { get; set; } = new();

        public IndexModel(IOperationalTaskService taskService, ILogger<IndexModel> logger)
        {
            _taskService = taskService;
            _logger = logger;
        }

        public void OnGet()
        {
        }

        /// <summary>
        /// Get task notifications for operators - filters to relevant tasks only
        /// </summary>
        public async Task<IActionResult> OnGetTaskNotificationsAsync()
        {
            try
            {
                // Get all open operational tasks
                var allTasks = await _taskService.GetOpenAsync(50);
                
                // Filter to tasks that are relevant for printing operators:
                // 1. Machine-specific tasks for printing machines (TI1, TI2, INC, etc.)
                // 2. General maintenance tasks without specific machine assignments
                // 3. Tasks that are overdue or high priority
                var printingMachines = new[] { "TI1", "TI2", "INC", "TI3", "TI4" };
                
                OperationalTasks = allTasks.Where(task => 
                    // Include machine-specific tasks for printing machines
                    (task.MachineId != null && printingMachines.Contains(task.MachineId, StringComparer.OrdinalIgnoreCase)) ||
                    // Include general tasks without machine assignment (could be department-wide)
                    string.IsNullOrEmpty(task.MachineId) ||
                    // Include any overdue tasks that need immediate attention
                    task.OverdueFlag == 1 ||
                    // Include high priority tasks (Priority 1 or 2)
                    task.Priority <= 2
                ).OrderByDescending(t => t.Priority)
                 .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
                 .Take(10) // Limit to top 10 most important
                 .ToList();

                return Partial("_TaskNotifications", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading task notifications for printing operators");
                OperationalTasks = new List<OperationalTask>();
                return Partial("_TaskNotifications", this);
            }
        }

        /// <summary>
        /// Mark an operational task as completed
        /// </summary>
        public async Task<IActionResult> OnPostCompleteTaskAsync(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskService.CompleteAsync(taskId, userId);
                
                if (success)
                {
                    _logger.LogInformation("Task {TaskId} completed by user {UserId}", taskId, userId);
                }

                // Return updated task notifications
                return await OnGetTaskNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", taskId);
                return await OnGetTaskNotificationsAsync();
            }
        }

        /// <summary>
        /// Dismiss/reset a task (early completion/reset cycle)
        /// </summary>
        public async Task<IActionResult> OnPostDismissTaskAsync(int taskId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _taskService.ResetAsync(taskId, userId);
                
                if (success)
                {
                    _logger.LogInformation("Task {TaskId} dismissed/reset by user {UserId}", taskId, userId);
                }

                // Return updated task notifications
                return await OnGetTaskNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing task {TaskId}", taskId);
                return await OnGetTaskNotificationsAsync();
            }
        }

        /// <summary>
        /// Get current user ID from session/claims
        /// </summary>
        private int GetCurrentUserId()
        {
            // Try to get user ID from session first
            if (HttpContext.Session.TryGetValue("UserId", out var userIdBytes))
            {
                return BitConverter.ToInt32(userIdBytes, 0);
            }

            // Fallback to claims if session not available
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            // Default fallback - should ideally not happen in production
            _logger.LogWarning("Could not determine current user ID, using default");
            return 1; // Default admin user ID
        }
    }
}