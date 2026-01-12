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

public class AddProgressModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly SchedulerContext _context;
    private readonly ILogger<AddProgressModel> _logger;

    public AddProgressModel(ICrmTaskService taskService, SchedulerContext context, ILogger<AddProgressModel> logger)
    {
        _taskService = taskService;
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmTask? TaskEntity { get; set; }

    public class ProgressInputModel
    {
        [Required]
        [StringLength(2000)]
        [Display(Name = "Progress Note")]
        public string ProgressNote { get; set; } = string.Empty;

        [Range(0, 100)]
        [Display(Name = "Completion %")]
        public int? PercentComplete { get; set; }

        [Display(Name = "Status Change")]
        public string? Status { get; set; }

        [Display(Name = "Progress Type")]
        public string ProgressType { get; set; } = "Update";

        [Display(Name = "Visible to Client")]
        public bool IsVisibleToClient { get; set; } = true;
    }

    [BindProperty]
    public ProgressInputModel Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        TaskEntity = await _taskService.GetByIdAsync(Id, ct);
        if (TaskEntity == null) return NotFound();

        // Check if user has permission to add progress to this task
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null || !CanUserAddProgress(currentUserId.Value))
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        TaskEntity = await _taskService.GetByIdAsync(Id, ct);
        if (TaskEntity == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (currentUserId == null || !CanUserAddProgress(currentUserId.Value))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _taskService.AddProgressEntryAsync(
                Id,
                Input.ProgressNote,
                Input.PercentComplete,
                Input.Status,
                currentUserId.Value,
                Input.ProgressType,
                Input.IsVisibleToClient,
                ct);

            StatusMessage = "Progress entry added successfully!";
            return RedirectToPage("Details", new { id = Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding progress entry for task {TaskId}", Id);
            StatusMessage = "Error adding progress entry. Please try again.";
            return Page();
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue("UserId");
        if (int.TryParse(userIdClaim, out var id)) return id;
        return null;
    }

    private bool CanUserAddProgress(int userId)
    {
        if (TaskEntity == null) return false;

        // Allow task creator, assignee, and admins
        return TaskEntity.CreatedByUserId == userId ||
               TaskEntity.AssignedToUserId == userId ||
               User.IsInRole("Admin") ||
               User.IsInRole("Manager");
    }
}