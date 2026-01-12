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

    public IndexModel(ICrmTaskService taskService, SchedulerContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    [BindProperty(SupportsGet = true, Name = "status")]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true, Name = "assignee")]
    public int? AssigneeFilter { get; set; }

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
}
