using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using OpCentrix.Models;
using System.Security.Claims;

namespace OpCentrix.Pages.Tasks;

[Authorize] // Allow any authenticated user
public class ProgressModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<ProgressModel> _logger;

    public ProgressModel(SchedulerContext context, ILogger<ProgressModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<CrmTaskProgress> RecentProgressUpdates { get; set; } = new();
    public List<CrmTaskViewModel> TasksNeedingUpdates { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogWarning("User ID not found in claims or invalid format");
                return RedirectToPage("/Account/Login");
            }

            await LoadRecentProgressUpdatesAsync(userId);
            await LoadTasksNeedingUpdatesAsync(userId);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task progress for user");
            ErrorMessage = "An error occurred while loading task progress.";
            return Page();
        }
    }

    private async Task LoadRecentProgressUpdatesAsync(int userId)
    {
        RecentProgressUpdates = await _context.CrmTaskProgress
            .Include(p => p.Task)
                .ThenInclude(t => t.Account)
            .Include(p => p.CreatedBy)
            .Where(p => p.Task.AssignedToUserId == userId)
            .OrderByDescending(p => p.CreatedDate)
            .Take(10)
            .ToListAsync();
    }

    private async Task LoadTasksNeedingUpdatesAsync(int userId)
    {
        var activeTasks = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.ProgressEntries)
            .Where(t => t.AssignedToUserId == userId && t.Status != "Completed")
            .ToListAsync();

        var tasksNeedingUpdates = new List<CrmTaskViewModel>();

        foreach (var task in activeTasks)
        {
            bool needsUpdate = false;
            int? daysSinceLastUpdate = null;

            // Check if task is overdue
            bool isOverdue = task.DueAt.HasValue && task.DueAt < DateTime.UtcNow;

            // Check if task hasn't been updated recently (more than 3 days)
            var lastUpdate = task.ProgressEntries
                ?.OrderByDescending(p => p.CreatedDate)
                .FirstOrDefault();

            if (lastUpdate != null)
            {
                daysSinceLastUpdate = (DateTime.UtcNow - lastUpdate.CreatedDate).Days;
                needsUpdate = daysSinceLastUpdate > 3;
            }
            else
            {
                // No progress updates yet and task is more than 1 day old
                needsUpdate = (DateTime.UtcNow - task.CreatedDate).Days > 1;
            }

            // Always show overdue tasks
            if (isOverdue)
            {
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                tasksNeedingUpdates.Add(new CrmTaskViewModel
                {
                    Task = task,
                    IsOverdue = isOverdue,
                    CompletionPercentage = CalculateCompletionPercentage(task),
                    DaysSinceLastUpdate = daysSinceLastUpdate
                });
            }
        }

        // Sort by priority (overdue first, then by priority level)
        TasksNeedingUpdates = tasksNeedingUpdates
            .OrderByDescending(t => t.IsOverdue)
            .ThenByDescending(t => t.Task.Priority)
            .ThenBy(t => t.Task.DueAt ?? DateTime.MaxValue)
            .ToList();
    }

    private int? CalculateCompletionPercentage(CrmTask task)
    {
        if (task.Status == "Completed")
            return 100;

        if (task.ProgressEntries?.Any() != true)
            return task.Status == "InProgress" ? 10 : 0;

        var latestProgress = task.ProgressEntries
            .OrderByDescending(p => p.CreatedDate)
            .FirstOrDefault();

        return latestProgress?.PercentComplete ?? (task.Status == "InProgress" ? 25 : 0);
    }

    public class CrmTaskViewModel
    {
        public CrmTask Task { get; set; } = null!;
        public bool IsOverdue { get; set; }
        public int? CompletionPercentage { get; set; }
        public int? DaysSinceLastUpdate { get; set; }
    }
}