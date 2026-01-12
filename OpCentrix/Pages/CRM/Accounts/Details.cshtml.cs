using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class DetailsModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly ICrmContactService _contactService;
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;

    public DetailsModel(ICrmAccountService accountService, ICrmContactService contactService, 
                       ICrmTaskService taskService, SchedulerContext context)
    {
        _accountService = accountService;
        _contactService = contactService;
        _taskService = taskService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmAccount? Account { get; set; }
    public List<CrmContact> Contacts { get; set; } = new();
    public List<CrmTask> Tasks { get; set; } = new();
    public List<User> Users { get; set; } = new();
    
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Account = await _accountService.GetByIdAsync(Id, ct);
        if (Account == null) return NotFound();

        // Load related data
        Contacts = await _contactService.GetByAccountIdAsync(Id, ct);
        Tasks = await _taskService.ListAsync(status: null, assignedToUserId: null, accountId: Id, ct);
        
        // Load users for task assignment display
        Users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        return Page();
    }
}
