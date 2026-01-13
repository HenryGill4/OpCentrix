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
public class MyTasksModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MyTasksModel> _logger;

    public MyTasksModel(SchedulerContext context, ILogger<MyTasksModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<CrmTaskViewModel> Tasks { get; set; } = new();
    public TaskStatistics Statistics { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool OverdueOnly { get; set; }

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

            await LoadTasksAsync(userId);
            await LoadStatisticsAsync(userId);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user tasks for user");
            ErrorMessage = "An error occurred while loading your tasks.";
            return Page();
        }
    }

    private async Task LoadTasksAsync(int userId)
    {
        var query = _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .Include(t => t.AssignedToUser)
            .Include(t => t.ProgressEntries)
                .ThenInclude(p => p.CreatedBy)
            .Where(t => t.AssignedToUserId == userId);

        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            query = query.Where(t => t.Status == StatusFilter);
        }

        // Apply overdue filter
        if (OverdueOnly)
        {
            query = query.Where(t => t.DueAt.HasValue && t.DueAt < DateTime.UtcNow && t.Status != "Completed");
        }

        var tasks = await query
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
            .ToListAsync();

        Tasks = tasks.Select(task => new CrmTaskViewModel
        {
            Task = task,
            IsOverdue = task.DueAt.HasValue && task.DueAt < DateTime.UtcNow && task.Status != "Completed",
            CompletionPercentage = CalculateCompletionPercentage(task),
            ProgressCount = task.ProgressEntries?.Count ?? 0
        }).ToList();
    }

    private async Task LoadStatisticsAsync(int userId)
    {
        var tasks = await _context.CrmTasks
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync();

        var activeTasks = tasks.Where(t => t.Status != "Completed").ToList();
        
        Statistics = new TaskStatistics
        {
            Total = tasks.Count,
            Open = tasks.Count(t => t.Status == "Open"),
            InProgress = activeTasks.Count, // Now represents active tasks
            Completed = tasks.Count(t => t.Status == "Completed"),
            Overdue = tasks.Count(t => t.DueAt.HasValue && t.DueAt < DateTime.UtcNow && t.Status != "Completed")
        };
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
        public int ProgressCount { get; set; }
    }

    public class TaskStatistics
    {
        public int Total { get; set; }
        public int Open { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Overdue { get; set; }
    }
}