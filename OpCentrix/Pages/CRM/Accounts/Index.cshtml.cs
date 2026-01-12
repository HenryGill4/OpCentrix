using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class IndexModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly SchedulerContext _context;

    public IndexModel(ICrmAccountService accountService, SchedulerContext context)
    {
        _accountService = accountService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; } = "name";

    public List<CrmAccount> Accounts { get; set; } = new();
    public Dictionary<int, int> ContactCounts { get; set; } = new();
    public Dictionary<int, int> TaskCounts { get; set; } = new();
    public Dictionary<int, int> ActiveTaskCounts { get; set; } = new();
    public int AccountsWithTasks { get; set; }
    public int AccountsWithContacts { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Get filtered accounts
        var accountsQuery = _context.CrmAccounts.AsQueryable();
        
        // Apply search filter
        if (!string.IsNullOrEmpty(Query))
        {
            accountsQuery = accountsQuery.Where(a => 
                a.Name.Contains(Query) || 
                (a.Status != null && a.Status.Contains(Query)) ||
                (a.Notes != null && a.Notes.Contains(Query)));
        }
        
        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            accountsQuery = accountsQuery.Where(a => a.Status == StatusFilter);
        }
        
        // Apply sorting
        accountsQuery = SortBy?.ToLower() switch
        {
            "name_desc" => accountsQuery.OrderByDescending(a => a.Name),
            "created" => accountsQuery.OrderByDescending(a => a.CreatedDate),
            "updated" => accountsQuery.OrderByDescending(a => a.LastModifiedDate),
            _ => accountsQuery.OrderBy(a => a.Name)
        };
        
        Accounts = await accountsQuery.ToListAsync(ct);
        
        // Load relationship counts
        var accountIds = Accounts.Select(a => a.Id).ToList();
        
        // Contact counts
        var contactCounts = await _context.CrmContacts
            .Where(c => accountIds.Contains(c.AccountId))
            .GroupBy(c => c.AccountId)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count, ct);
        ContactCounts = contactCounts;
        
        // Task counts
        var taskCounts = await _context.CrmTasks
            .Where(t => t.AccountId.HasValue && accountIds.Contains(t.AccountId.Value))
            .GroupBy(t => t.AccountId!.Value)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count, ct);
        TaskCounts = taskCounts;
        
        // Active task counts
        var activeTaskCounts = await _context.CrmTasks
            .Where(t => t.AccountId.HasValue && 
                       accountIds.Contains(t.AccountId.Value) && 
                       t.Status != "Completed")
            .GroupBy(t => t.AccountId!.Value)
            .Select(g => new { AccountId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.AccountId, x => x.Count, ct);
        ActiveTaskCounts = activeTaskCounts;
        
        // Summary statistics
        AccountsWithTasks = TaskCounts.Count(kv => kv.Value > 0);
        AccountsWithContacts = ContactCounts.Count(kv => kv.Value > 0);
    }
}
