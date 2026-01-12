using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Contacts;

public class CreateModel : PageModel
{
    private readonly ICrmAccountService _accountService;
    private readonly ICrmContactService _contactService;

    public CreateModel(ICrmAccountService accountService, ICrmContactService contactService)
    {
        _accountService = accountService;
        _contactService = contactService;
    }

    public string AccountName { get; set; } = string.Empty;

    public class InputModel
    {
        [Required]
        public int AccountId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Title { get; set; }

        [EmailAddress]
        [StringLength(200)]
        public string? Email { get; set; }

        [Phone]
        [StringLength(50)]
        public string? Phone { get; set; }
    }

    [BindProperty(SupportsGet = true)]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var account = await _accountService.GetByIdAsync(Input.AccountId, ct);
        if (account == null) return NotFound();
        AccountName = account.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var account = await _accountService.GetByIdAsync(Input.AccountId, ct);
        if (account == null) return NotFound();
        AccountName = account.Name;

        if (!ModelState.IsValid) return Page();

        await _contactService.CreateAsync(Input.AccountId, Input.Name, Input.Email, Input.Phone, Input.Title, ct);
        return RedirectToPage("/CRM/Accounts/Details", new { id = Input.AccountId });
    }
}
