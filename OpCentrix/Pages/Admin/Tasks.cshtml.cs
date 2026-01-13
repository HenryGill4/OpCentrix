using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.MaintenanceV2;
using Microsoft.AspNetCore.Authorization;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class TasksModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<TasksModel> _logger;

        public TasksModel(SchedulerContext context, ILogger<TasksModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<OperationalTask> Tasks { get; set; } = new();
        public int OpenTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int CompletedTodayTasks { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Load all tasks
                Tasks = await _context.Set<OperationalTask>()
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(50) // Limit to recent 50 tasks
                    .ToListAsync();

                // Load statistics
                OpenTasks = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "Open");
                InProgressTasks = await _context.Set<OperationalTask>().CountAsync(t => t.Status == "InProgress");
                OverdueTasks = await _context.Set<OperationalTask>().CountAsync(t => t.OverdueFlag == 1);
                
                var today = DateTime.Today;
                CompletedTodayTasks = await _context.Set<OperationalTask>()
                    .CountAsync(t => t.Status == "Completed" && t.CompletedAt != null && t.CompletedAt >= today);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not load tasks - table may not exist yet");
                Tasks = new List<OperationalTask>();
                OpenTasks = 0;
                InProgressTasks = 0;
                OverdueTasks = 0;
                CompletedTodayTasks = 0;
            }
        }

        // Task action handlers
        public async Task<IActionResult> OnPostStartTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToPage();
                }

                if (task.Status != "Open")
                {
                    TempData["Warning"] = "Task is not in Open status";
                    return RedirectToPage();
                }

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1");
                
                task.Status = "InProgress";
                task.AssignedUserId = userId;
                task.LastResetAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task started successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting task {TaskId}", taskId);
                TempData["Error"] = $"Error starting task: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int taskId, string? completionNotes = null)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToPage();
                }

                if (task.Status != "InProgress")
                {
                    TempData["Warning"] = "Task must be in progress to complete";
                    return RedirectToPage();
                }

                task.Status = "Completed";
                task.CompletedAt = DateTime.UtcNow;
                task.OverdueFlag = 0; // Clear overdue flag
                
                if (!string.IsNullOrEmpty(completionNotes))
                {
                    task.Description += $"\n\nCompletion Notes: {completionNotes}";
                }
                
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task completed successfully!";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", taskId);
                TempData["Error"] = $"Error completing task: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostUpdateTaskStatusAsync(int taskId, string status)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToPage();
                }

                var oldStatus = task.Status;
                task.Status = status;
                
                // Set completion timestamp if marking as completed
                if (status == "Completed" && task.CompletedAt == null)
                {
                    task.CompletedAt = DateTime.UtcNow;
                    task.OverdueFlag = 0;
                }
                
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Task status updated from {oldStatus} to {status}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status for {TaskId}", taskId);
                TempData["Error"] = $"Error updating task status: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteTaskAsync(int taskId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToPage();
                }

                var taskTitle = task.Title;
                _context.Set<OperationalTask>().Remove(task);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Task '{taskTitle}' deleted successfully";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
                TempData["Error"] = $"Error deleting task: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostAssignTaskAsync(int taskId, int userId)
        {
            try
            {
                var task = await _context.Set<OperationalTask>().FindAsync(taskId);
                if (task == null)
                {
                    TempData["Error"] = "Task not found";
                    return RedirectToPage();
                }

                task.AssignedUserId = userId;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Task assigned successfully";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task {TaskId}", taskId);
                TempData["Error"] = $"Error assigning task: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}