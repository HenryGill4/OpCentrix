using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OpCentrix.Pages.Tasks;

[Authorize]
public class AddProgressModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<AddProgressModel> _logger;

    public AddProgressModel(SchedulerContext context, ILogger<AddProgressModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmTask? Task { get; set; }

    [BindProperty]
    public ProgressInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    [TempData]
    public string? ErrorMessage { get; set; }

    public class ProgressInput
    {
        [Required(ErrorMessage = "Progress note is required")]
        [StringLength(2000, ErrorMessage = "Progress note cannot exceed 2000 characters")]
        [Display(Name = "Progress Update")]
        public string ProgressNote { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Percent complete must be between 0 and 100")]
        [Display(Name = "Completion %")]
        public int? PercentComplete { get; set; }

        [Display(Name = "Update Status")]
        public string? Status { get; set; }

        [Display(Name = "Update Type")]
        public string ProgressType { get; set; } = "Update";
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await LoadAndValidateTaskAsync();
        if (result != null) return result;

        // Pre-fill current status and estimated completion
        Input.Status = Task!.Status;
        Input.PercentComplete = GetCurrentCompletionPercentage();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var result = await LoadAndValidateTaskAsync();
        if (result != null) return result;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var currentUserId = GetCurrentUserId();

            // Create progress entry
            var progressEntry = new CrmTaskProgress
            {
                TaskId = Id,
                ProgressNote = Input.ProgressNote,
                PercentComplete = Input.PercentComplete,
                Status = Input.Status ?? Task!.Status,
                ProgressType = Input.ProgressType,
                IsVisibleToClient = true, // Employee updates are visible to clients by default
                CreatedByUserId = currentUserId,
                CreatedDate = DateTime.UtcNow
            };

            _context.CrmTaskProgress.Add(progressEntry);

            // Update task status if provided and different
            if (!string.IsNullOrEmpty(Input.Status) && Input.Status != Task!.Status)
            {
                Task.Status = Input.Status;
                Task.LastModifiedDate = DateTime.UtcNow;
            }

            // Auto-complete task if 100% complete
            if (Input.PercentComplete == 100 && Task.Status != "Completed")
            {
                Task.Status = "Completed";
                Task.CompletedAt = DateTime.UtcNow;
                Task.LastModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Progress entry added successfully for task {TaskId} by user {UserId}", Id, currentUserId);

            StatusMessage = "Your progress update has been added successfully!";
            
            // Redirect back to My Tasks
            return RedirectToPage("/Tasks/MyTasks");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding progress entry for task {TaskId}", Id);
            ErrorMessage = "An error occurred while adding your progress update. Please try again.";
            return Page();
        }
    }

    private async Task<IActionResult?> LoadAndValidateTaskAsync()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == null)
        {
            _logger.LogWarning("Unable to determine current user ID");
            return RedirectToPage("/Account/Login");
        }

        // Load task and verify user can access it
        Task = await _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.AssignedToUser)
            .Include(t => t.ProgressEntries.OrderByDescending(p => p.CreatedDate))
                .ThenInclude(p => p.CreatedBy)
            .Where(t => t.Id == Id && t.AssignedToUserId == currentUserId) // Only allow access to own tasks
            .FirstOrDefaultAsync();

        if (Task == null)
        {
            _logger.LogWarning("User {UserId} attempted to access task {TaskId} that doesn't exist or isn't assigned to them", currentUserId, Id);
            ErrorMessage = "Task not found or you don't have permission to update this task.";
            return RedirectToPage("/Tasks/MyTasks");
        }

        _logger.LogInformation("User {UserId} loading progress page for task {TaskId}: {TaskTitle}", currentUserId, Id, Task.Title);

        return null;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var id)) return id;
        return 0;
    }

    private int? GetCurrentCompletionPercentage()
    {
        if (Task?.Status == "Completed")
            return 100;

        if (Task?.ProgressEntries?.Any() != true)
            return Task?.Status == "InProgress" ? 10 : 0;

        var latestProgress = Task.ProgressEntries
            .OrderByDescending(p => p.CreatedDate)
            .FirstOrDefault();

        return latestProgress?.PercentComplete ?? (Task?.Status == "InProgress" ? 25 : 0);
    }
}