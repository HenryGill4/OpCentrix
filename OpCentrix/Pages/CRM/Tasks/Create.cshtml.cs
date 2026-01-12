using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Tasks;

public class CreateModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;

    public CreateModel(ICrmTaskService taskService, SchedulerContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    public List<User> Users { get; set; } = new();
    public List<CrmAccount> Accounts { get; set; } = new();

    public class InputModel
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; } = 3;

        public DateTime? DueAt { get; set; }

        public int? AssignedToUserId { get; set; }
        public int? AccountId { get; set; }

        // Reminder settings
        public bool HasReminder { get; set; }
        public int ReminderMinutesBefore { get; set; } = 15;
        public string ReminderType { get; set; } = "Email";

        // Notification preferences
        public bool NotifyOnStatusChange { get; set; } = true;
        public bool NotifyAssignee { get; set; } = true;
        public bool NotifyCreator { get; set; } = true;
    }

    [BindProperty(SupportsGet = true)]
    public InputModel Input { get; set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadListsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadListsAsync(ct);
        if (!ModelState.IsValid) return Page();

        var createdByUserId = GetCurrentUserId();
        if (createdByUserId == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to determine current user.");
            return Page();
        }

        var created = await _taskService.CreateAsync(
            title: Input.Title,
            description: Input.Description,
            priority: Input.Priority,
            dueAt: Input.DueAt,
            createdByUserId: createdByUserId.Value,
            assignedToUserId: Input.AssignedToUserId,
            accountId: Input.AccountId,
            contactId: null,
            hasReminder: Input.HasReminder,
            reminderMinutesBefore: Input.ReminderMinutesBefore,
            reminderType: Input.ReminderType,
            ct: ct);

        return RedirectToPage("/CRM/Tasks/Details", new { id = created.Id });
    }

    private async Task LoadListsAsync(CancellationToken ct)
    {
        Users = await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Take(200)
            .ToListAsync(ct);

        Accounts = await _context.CrmAccounts
            .OrderBy(a => a.Name)
            .Take(200)
            .ToListAsync(ct);
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (int.TryParse(userIdClaim, out var id)) return id;
        return null;
    }
}
