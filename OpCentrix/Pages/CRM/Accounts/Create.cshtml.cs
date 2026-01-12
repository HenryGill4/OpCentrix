using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Accounts;

public class CreateModel : PageModel
{
    private readonly ICrmAccountService _svc;

    public CreateModel(ICrmAccountService svc)
    {
        _svc = svc;
    }

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Status { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        var created = await _svc.CreateAsync(Input.Name, Input.Status, Input.Notes, ct);
        return RedirectToPage("/CRM/Accounts/Details", new { id = created.Id });
    }
}
