using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class CreateModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly SchedulerContext _context;

    public CreateModel(ICrmAccountService accountService, SchedulerContext context)
    {
        _accountService = accountService;
        _context = context;
    }

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; } = "Prospect";

        [StringLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    [TempData]
    public string? StatusMessage { get; set; }
    
    // Statistics for the sidebar
    public int TotalAccounts { get; set; }
    public int ActiveAccounts { get; set; }
    public int ProspectAccounts { get; set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadStatisticsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) 
        {
            await LoadStatisticsAsync(ct);
            return Page();
        }

        try
        {
            var created = await _accountService.CreateAsync(Input.Name, Input.Status, Input.Notes, ct);
            StatusMessage = $"Account '{Input.Name}' has been created successfully!";
            return RedirectToPage("/CRM/Accounts/Details", new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while creating the account. Please try again.");
            await LoadStatisticsAsync(ct);
            return Page();
        }
    }
    
    private async Task LoadStatisticsAsync(CancellationToken ct)
    {
        TotalAccounts = await _context.CrmAccounts.CountAsync(ct);
        ActiveAccounts = await _context.CrmAccounts.CountAsync(a => a.Status == "Active", ct);
        ProspectAccounts = await _context.CrmAccounts.CountAsync(a => a.Status == "Prospect", ct);
    }
}
