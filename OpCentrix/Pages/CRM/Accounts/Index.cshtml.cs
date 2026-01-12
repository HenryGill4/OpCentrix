using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class IndexModel : PageModel
{
    private readonly ICrmAccountService _svc;

    public IndexModel(ICrmAccountService svc)
    {
        _svc = svc;
    }

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public List<CrmAccount> Accounts { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Accounts = await _svc.SearchAsync(Query, ct);
    }
}
