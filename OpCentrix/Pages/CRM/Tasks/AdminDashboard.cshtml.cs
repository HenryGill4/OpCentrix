using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Tasks;

public class AdminDashboardModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly ICrmNotificationService _notificationService;
    private readonly SchedulerContext _context;
    private readonly ILogger<AdminDashboardModel> _logger;

    public AdminDashboardModel(ICrmTaskService taskService, ICrmNotificationService notificationService, SchedulerContext context, ILogger<AdminDashboardModel> logger)
    {
        _taskService = taskService;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    // Dashboard summary data
    public int TotalActiveTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int TasksNeedingAttention { get; set; }

    // Task lists
    public List<CrmTask> RecentTasks { get; set; } = new();
    public List<CrmTask> OverdueTasksList { get; set; } = new();
    public List<CrmTask> HighPriorityTasksList { get; set; } = new();
    public List<TaskProgressSummary> TasksWithRecentProgress { get; set; } = new();

    // User assignment data
    public List<UserTaskSummary> UserAssignments { get; set; } = new();

    [BindProperty]
    public int? SendReminderTaskId { get; set; }

    [BindProperty]
    public int? ReassignTaskId { get; set; }

    [BindProperty]
    public int? ReassignToUserId { get; set; }

    [BindProperty]
    public int? DeleteTaskId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(string? status = null, int? assigneeId = null)
    {
        // Check admin permissions
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        await LoadDashboardDataAsync(status, assigneeId);
        return Page();
    }

    public async Task<IActionResult> OnPostSendReminderAsync()
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        if (SendReminderTaskId.HasValue)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(SendReminderTaskId.Value);
                if (task != null)
                {
                    await _notificationService.SendTaskReminderAsync(task);
                    StatusMessage = $"Reminder sent for task: {task.Title}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending reminder for task {TaskId}", SendReminderTaskId.Value);
                StatusMessage = "Error sending reminder. Please try again.";
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReassignTaskAsync()
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        if (ReassignTaskId.HasValue && ReassignToUserId.HasValue)
        {
            try
            {
                await _taskService.AssignAsync(ReassignTaskId.Value, ReassignToUserId.Value);
                var task = await _taskService.GetByIdAsync(ReassignTaskId.Value);
                StatusMessage = $"Task '{task?.Title}' reassigned successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reassigning task {TaskId} to user {UserId}", ReassignTaskId.Value, ReassignToUserId.Value);
                StatusMessage = "Error reassigning task. Please try again.";
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteTaskAsync()
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (DeleteTaskId.HasValue)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(DeleteTaskId.Value);
                var taskTitle = task?.Title ?? "Unknown Task";
                
                await _taskService.DeleteTaskAsync(DeleteTaskId.Value);
                StatusMessage = $"Task '{taskTitle}' has been permanently deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", DeleteTaskId.Value);
                StatusMessage = "Error deleting task. Please try again.";
            }
        }

        return RedirectToPage();
    }

    private async Task LoadDashboardDataAsync(string? status, int? assigneeId)
    {
        var now = DateTime.UtcNow;

        // Load summary statistics
        var allActiveTasks = await _context.CrmTasks
            .Where(t => t.Status != "Completed")
            .ToListAsync();

        TotalActiveTasks = allActiveTasks.Count;
        OverdueTasks = allActiveTasks.Count(t => t.DueAt.HasValue && t.DueAt.Value < now);
        HighPriorityTasks = allActiveTasks.Count(t => t.Priority >= 4);
        TasksNeedingAttention = allActiveTasks.Count(t => 
            (t.DueAt.HasValue && t.DueAt.Value < now.AddDays(1)) || 
            t.Priority >= 4 || 
            t.Status == "Open");

        // Load task lists with filters
        var tasksQuery = _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            tasksQuery = tasksQuery.Where(t => t.Status == status);
        }

        if (assigneeId.HasValue)
        {
            tasksQuery = tasksQuery.Where(t => t.AssignedToUserId == assigneeId.Value);
        }

        // Recent tasks (last 30 days)
        RecentTasks = await tasksQuery
            .Where(t => t.CreatedDate >= now.AddDays(-30))
            .OrderByDescending(t => t.CreatedDate)
            .Take(10)
            .ToListAsync();

        // Overdue tasks with user information
        OverdueTasksList = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .Include(t => t.AssignedToUser)
            .Where(t => t.DueAt.HasValue && t.DueAt.Value < now && t.Status != "Completed")
            .OrderBy(t => t.DueAt)
            .Take(15)
            .ToListAsync();

        // High priority tasks with user information
        HighPriorityTasksList = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .Include(t => t.AssignedToUser)
            .Where(t => t.Priority >= 4 && t.Status != "Completed")
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueAt)
            .Take(15)
            .ToListAsync();

        // Tasks with recent progress
        TasksWithRecentProgress = await _context.CrmTaskProgress
            .Include(p => p.Task)
            .Include(p => p.CreatedBy)
            .Where(p => p.CreatedDate >= now.AddDays(-7))
            .GroupBy(p => p.TaskId)
            .Select(g => new TaskProgressSummary
            {
                TaskId = g.Key,
                TaskTitle = g.First().Task!.Title,
                TaskStatus = g.First().Task!.Status,
                LastProgressDate = g.Max(p => p.CreatedDate),
                ProgressCount = g.Count(),
                LastProgressNote = g.OrderByDescending(p => p.CreatedDate).First().ProgressNote,
                LastUpdatedBy = g.OrderByDescending(p => p.CreatedDate).First().CreatedBy!.FullName
            })
            .OrderByDescending(t => t.LastProgressDate)
            .Take(10)
            .ToListAsync();

        // User assignment summary
        UserAssignments = await _context.CrmTasks
            .Where(t => t.AssignedToUserId.HasValue && t.Status != "Completed")
            .GroupBy(t => t.AssignedToUserId)
            .Select(g => new UserTaskSummary
            {
                UserId = g.Key!.Value,
                TaskCount = g.Count(),
                OverdueCount = g.Count(t => t.DueAt.HasValue && t.DueAt.Value < now),
                HighPriorityCount = g.Count(t => t.Priority >= 4),
                LastActivityDate = g.Max(t => t.LastModifiedDate)
            })
            .ToListAsync();

        // Load user names for assignments
        foreach (var assignment in UserAssignments)
        {
            var user = await _context.Users.FindAsync(assignment.UserId);
            assignment.UserName = user?.FullName ?? "Unknown User";
        }
    }

    public class TaskProgressSummary
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public string TaskStatus { get; set; } = string.Empty;
        public DateTime LastProgressDate { get; set; }
        public int ProgressCount { get; set; }
        public string LastProgressNote { get; set; } = string.Empty;
        public string LastUpdatedBy { get; set; } = string.Empty;
    }

    public class UserTaskSummary
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int TaskCount { get; set; }
        public int OverdueCount { get; set; }
        public int HighPriorityCount { get; set; }
        public DateTime LastActivityDate { get; set; }
    }
}