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

    [BindProperty]
    public int? DeleteTaskId { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public List<User> Assignees { get; set; } = new();
    public List<CrmTask> Tasks { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Assignees = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Take(200)
            .ToListAsync(ct);

        Tasks = await _taskService.ListAsync(StatusFilter, AssigneeFilter, accountId: null, ct);
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

        return RedirectToPage(new { status = StatusFilter, assignee = AssigneeFilter });
    }
}
