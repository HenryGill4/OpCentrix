using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class EditModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly SchedulerContext _context;

    public EditModel(ICrmAccountService accountService, SchedulerContext context)
    {
        _accountService = accountService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmAccount? Account { get; set; }
    
    [TempData]
    public string? StatusMessage { get; set; }
    
    // Statistics for the sidebar
    public int ContactCount { get; set; }
    public int TaskCount { get; set; }

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        [Display(Name = "Account Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Status")]
        public string? Status { get; set; }

        [StringLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Account = await _accountService.GetByIdAsync(Id, ct);
        if (Account == null) return NotFound();

        // Load statistics
        ContactCount = await _context.CrmContacts.CountAsync(c => c.AccountId == Id, ct);
        TaskCount = await _context.CrmTasks.CountAsync(t => t.AccountId == Id, ct);

        // Populate form
        Input = new InputModel
        {
            Name = Account.Name,
            Status = Account.Status,
            Notes = Account.Notes
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        Account = await _accountService.GetByIdAsync(Id, ct);
        if (Account == null) return NotFound();

        if (!ModelState.IsValid)
        {
            // Reload statistics on validation failure
            ContactCount = await _context.CrmContacts.CountAsync(c => c.AccountId == Id, ct);
            TaskCount = await _context.CrmTasks.CountAsync(t => t.AccountId == Id, ct);
            return Page();
        }

        try
        {
            await _accountService.UpdateAsync(Id, Input.Name, Input.Status, Input.Notes, ct);
            StatusMessage = $"Account '{Input.Name}' has been updated successfully!";
            return RedirectToPage("/CRM/Accounts/Details", new { id = Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "An error occurred while updating the account. Please try again.");
            
            // Reload statistics on error
            ContactCount = await _context.CrmContacts.CountAsync(c => c.AccountId == Id, ct);
            TaskCount = await _context.CrmTasks.CountAsync(t => t.AccountId == Id, ct);
            
            return Page();
        }
    }
}