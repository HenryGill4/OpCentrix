using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Tasks;

public class IndexModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ICrmTaskService taskService, SchedulerContext context, ILogger<IndexModel> logger)
    {
        _taskService = taskService;
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true, Name = "status")]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "assignee")]
    public int? AssigneeFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "assigneeId")]
    public int? AssigneeIdFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "priority")]
    public int? PriorityFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "account")]
    public int? AccountFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "overdue")]
    public bool? OverdueFilter { get; set; }

    [BindProperty]
    public int? DeleteTaskId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public List<User> Assignees { get; set; } = new();
    public List<CrmAccount> Accounts { get; set; } = new();
    public List<EnhancedTaskViewModel> Tasks { get; set; } = new();
    
    // Statistics for dashboard-like view
    public TaskStatistics Statistics { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Support both 'assignee' and 'assigneeId' query parameters for flexibility
        var effectiveAssigneeFilter = AssigneeFilter ?? AssigneeIdFilter;

        await LoadDropdownDataAsync(ct);
        await LoadTasksAsync(effectiveAssigneeFilter, ct);
        await LoadStatisticsAsync(ct);
        
        // Update AssigneeFilter to reflect the actual filter being used for UI consistency
        if (effectiveAssigneeFilter.HasValue)
        {
            AssigneeFilter = effectiveAssigneeFilter;
        }
    }

    private async Task LoadDropdownDataAsync(CancellationToken ct)
    {
        Assignees = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Take(200)
            .ToListAsync(ct);

        Accounts = await _context.CrmAccounts
            .Where(a => a.Status == "Active")
            .OrderBy(a => a.Name)
            .Take(200)
            .ToListAsync(ct);
    }

    private async Task LoadTasksAsync(int? effectiveAssigneeFilter, CancellationToken ct)
    {
        var query = _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query = query.Where(t => t.Status == StatusFilter);
        }

        if (effectiveAssigneeFilter.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == effectiveAssigneeFilter);
        }

        if (PriorityFilter.HasValue)
        {
            query = query.Where(t => t.Priority == PriorityFilter);
        }

        if (AccountFilter.HasValue)
        {
            query = query.Where(t => t.AccountId == AccountFilter);
        }

        if (OverdueFilter == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t => t.DueAt.HasValue && t.DueAt < now && t.Status != "Completed");
        }

        var tasks = await query
            .OrderBy(t => t.Status == "Completed" ? 1 : 0) // Non-completed first
            .ThenByDescending(t => t.Priority) // Higher priority first (5, 4, 3, 2, 1)
            .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
            .ThenByDescending(t => t.Id)
            .Take(500)
            .ToListAsync(ct);

        // Load additional user information
        var userIds = tasks.Select(t => t.AssignedToUserId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Concat(tasks.Select(t => t.CreatedByUserId))
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u, ct);

        // Load progress information
        var taskIds = tasks.Select(t => t.Id).ToList();
        var progressData = await _context.CrmTaskProgress
            .Where(p => taskIds.Contains(p.TaskId))
            .GroupBy(p => p.TaskId)
            .Select(g => new {
                TaskId = g.Key,
                ProgressCount = g.Count(),
                LatestProgress = g.OrderByDescending(p => p.CreatedDate).First(),
                LatestPercentComplete = g.Where(p => p.PercentComplete.HasValue)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(p => p.PercentComplete)
                    .FirstOrDefault()
            })
            .ToDictionaryAsync(x => x.TaskId, x => x, ct);

        // Create enhanced view models
        Tasks = tasks.Select(t => new EnhancedTaskViewModel
        {
            Task = t,
            AssignedUser = t.AssignedToUserId.HasValue && users.ContainsKey(t.AssignedToUserId.Value) 
                ? users[t.AssignedToUserId.Value] : null,
            CreatedByUser = users.ContainsKey(t.CreatedByUserId) 
                ? users[t.CreatedByUserId] : null,
            ProgressCount = progressData.ContainsKey(t.Id) ? progressData[t.Id].ProgressCount : 0,
            LatestProgressNote = progressData.ContainsKey(t.Id) ? progressData[t.Id].LatestProgress?.ProgressNote : null,
            LatestProgressDate = progressData.ContainsKey(t.Id) ? progressData[t.Id].LatestProgress?.CreatedDate : null,
            CompletionPercentage = progressData.ContainsKey(t.Id) ? progressData[t.Id].LatestPercentComplete : null,
            IsOverdue = t.DueAt.HasValue && t.DueAt < DateTime.UtcNow && t.Status != "Completed",
            DaysUntilDue = t.DueAt.HasValue ? (int)(t.DueAt.Value.Date - DateTime.Now.Date).TotalDays : null
        }).ToList();
    }

    private async Task LoadStatisticsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        
        var allTasks = await _context.CrmTasks
            .Where(t => AssigneeFilter == null || t.AssignedToUserId == AssigneeFilter)
            .ToListAsync(ct);

        Statistics = new TaskStatistics
        {
            TotalTasks = allTasks.Count,
            OpenTasks = allTasks.Count(t => t.Status == "Open"),
            InProgressTasks = allTasks.Count(t => t.Status == "InProgress"),
            CompletedTasks = allTasks.Count(t => t.Status == "Completed"),
            OverdueTasks = allTasks.Count(t => t.DueAt.HasValue && t.DueAt < now && t.Status != "Completed"),
            HighPriorityTasks = allTasks.Count(t => t.Priority >= 4 && t.Status != "Completed"),
            TasksDueToday = allTasks.Count(t => t.DueAt.HasValue && t.DueAt.Value.Date == now.Date && t.Status != "Completed"),
            TasksDueThisWeek = allTasks.Count(t => t.DueAt.HasValue && 
                t.DueAt.Value.Date >= now.Date && 
                t.DueAt.Value.Date <= now.Date.AddDays(7) && 
                t.Status != "Completed")
        };
    }

    public async Task<IActionResult> OnPostDeleteTaskAsync(CancellationToken ct)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        if (DeleteTaskId.HasValue)
        {
            try
            {
                var task = await _taskService.GetByIdAsync(DeleteTaskId.Value, ct);
                var taskTitle = task?.Title ?? "Unknown Task";
                
                await _taskService.DeleteTaskAsync(DeleteTaskId.Value, ct);
                StatusMessage = $"Task '{taskTitle}' has been permanently deleted.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", DeleteTaskId.Value);
                StatusMessage = "Error deleting task. Please try again.";
            }
        }

        return RedirectToPage(new { 
            status = StatusFilter, 
            assignee = AssigneeFilter,
            priority = PriorityFilter,
            account = AccountFilter,
            overdue = OverdueFilter
        });
    }
}

public class EnhancedTaskViewModel
{
    public CrmTask Task { get; set; } = new();
    public User? AssignedUser { get; set; }
    public User? CreatedByUser { get; set; }
    public int ProgressCount { get; set; }
    public string? LatestProgressNote { get; set; }
    public DateTime? LatestProgressDate { get; set; }
    public int? CompletionPercentage { get; set; }
    public bool IsOverdue { get; set; }
    public int? DaysUntilDue { get; set; }
}

public class TaskStatistics
{
    public int TotalTasks { get; set; }
    public int OpenTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int HighPriorityTasks { get; set; }
    public int TasksDueToday { get; set; }
    public int TasksDueThisWeek { get; set; }
}
