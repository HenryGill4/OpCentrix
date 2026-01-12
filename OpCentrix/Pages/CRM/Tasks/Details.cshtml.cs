using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Tasks;

public class DetailsModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;

    public DetailsModel(ICrmTaskService taskService, SchedulerContext context)
    {
        _taskService = taskService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmTask? TaskEntity { get; set; }

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

        [Required]
        public string Status { get; set; } = "Open";

        public int? AssignedToUserId { get; set; }
        public int? AccountId { get; set; }
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadListsAsync(ct);
        TaskEntity = await _taskService.GetByIdAsync(Id, ct);
        if (TaskEntity == null) return NotFound();

        Input = new InputModel
        {
            Title = TaskEntity.Title,
            Description = TaskEntity.Description,
            Priority = TaskEntity.Priority,
            DueAt = TaskEntity.DueAt?.Date,
            Status = TaskEntity.Status,
            AssignedToUserId = TaskEntity.AssignedToUserId,
            AccountId = TaskEntity.AccountId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        await LoadListsAsync(ct);
        if (!ModelState.IsValid) return Page();

        await _taskService.UpdateAsync(
            id: Id,
            title: Input.Title,
            description: Input.Description,
            priority: Input.Priority,
            dueAt: Input.DueAt,
            status: Input.Status,
            assignedToUserId: Input.AssignedToUserId,
            accountId: Input.AccountId,
            contactId: null,
            ct: ct);

        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id, CancellationToken ct)
    {
        await _taskService.CompleteAsync(id, ct);
        return RedirectToPage(new { id });
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
}
