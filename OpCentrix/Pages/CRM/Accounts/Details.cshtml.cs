using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class DetailsModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly ICrmContactService _contactService;
    private readonly ICrmTaskService _taskService;

    public DetailsModel(ICrmAccountService accountService, ICrmContactService contactService, ICrmTaskService taskService)
    {
        _accountService = accountService;
        _contactService = contactService;
        _taskService = taskService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmAccount? Account { get; set; }
    public List<CrmContact> Contacts { get; set; } = new();
    public List<CrmTask> Tasks { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Account = await _accountService.GetByIdAsync(Id, ct);
        if (Account == null) return NotFound();

        Contacts = await _contactService.GetByAccountIdAsync(Id, ct);
        Tasks = await _taskService.ListAsync(status: null, assignedToUserId: null, accountId: Id, ct);

        return Page();
    }
}
