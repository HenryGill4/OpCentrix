using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Tasks;

[Authorize] // Allow any authenticated user to access, detailed permissions checked in methods
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
        try
        {
            // Load task with full details
            TaskEntity = await _context.CrmTasks
                .Include(t => t.Account)
                .Include(t => t.Contact)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == Id, ct);

            if (TaskEntity == null) 
            {
                _logger.LogWarning("Task {TaskId} not found", Id);
                return NotFound();
            }

            // Check if user has permission to add progress to this task
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                _logger.LogWarning("Unable to determine current user ID");
                return Forbid();
            }

            if (!CanUserAddProgress(currentUserId.Value))
            {
                _logger.LogWarning("User {UserId} denied access to add progress to task {TaskId}. Task assigned to: {AssignedUserId}, Created by: {CreatedByUserId}", 
                    currentUserId, Id, TaskEntity.AssignedToUserId, TaskEntity.CreatedByUserId);
                return Forbid();
            }

            _logger.LogInformation("User {UserId} granted access to add progress to task {TaskId}", currentUserId, Id);

            // Pre-populate form with current task status
            Input.Status = TaskEntity.Status;

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading task {TaskId} for progress entry", Id);
            return NotFound();
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        try
        {
            // Load task with full details
            TaskEntity = await _context.CrmTasks
                .Include(t => t.Account)
                .Include(t => t.Contact)
                .Include(t => t.AssignedToUser)
                .FirstOrDefaultAsync(t => t.Id == Id, ct);

            if (TaskEntity == null) 
            {
                _logger.LogWarning("Task {TaskId} not found", Id);
                return NotFound();
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                _logger.LogWarning("Unable to determine current user ID during POST");
                return Forbid();
            }

            if (!CanUserAddProgress(currentUserId.Value))
            {
                _logger.LogWarning("User {UserId} denied access to add progress to task {TaskId} during POST", currentUserId, Id);
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Create progress entry directly since we have the context
            var progressEntry = new CrmTaskProgress
            {
                TaskId = Id,
                ProgressNote = Input.ProgressNote,
                PercentComplete = Input.PercentComplete,
                Status = Input.Status ?? TaskEntity.Status,
                ProgressType = Input.ProgressType,
                IsVisibleToClient = Input.IsVisibleToClient,
                CreatedByUserId = currentUserId.Value,
                CreatedDate = DateTime.UtcNow
            };

            _context.CrmTaskProgress.Add(progressEntry);

            // Update task status if provided and different
            if (!string.IsNullOrEmpty(Input.Status) && Input.Status != TaskEntity.Status)
            {
                TaskEntity.Status = Input.Status;
                TaskEntity.LastModifiedDate = DateTime.UtcNow;
            }

            // Auto-complete task if 100% complete
            if (Input.PercentComplete == 100 && TaskEntity.Status != "Completed")
            {
                TaskEntity.Status = "Completed";
                TaskEntity.CompletedAt = DateTime.UtcNow;
                TaskEntity.LastModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Progress entry added successfully for task {TaskId} by user {UserId}", Id, currentUserId);

            StatusMessage = "Progress entry added successfully!";
            
            // Redirect to My Tasks page instead of Details page
            return RedirectToPage("/Tasks/MyTasks");
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
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var id)) return id;
        return null;
    }

    private bool CanUserAddProgress(int userId)
    {
        if (TaskEntity == null) 
        {
            _logger.LogWarning("TaskEntity is null when checking permissions for user {UserId}", userId);
            return false;
        }

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var userName = User.Identity?.Name ?? "Unknown";

        _logger.LogInformation("Checking permissions for user {UserId} ({UserName}, Role: {Role}) to add progress to task {TaskId}. Task assigned to: {AssignedUserId}, Created by: {CreatedByUserId}", 
            userId, userName, userRole, TaskEntity.Id, TaskEntity.AssignedToUserId, TaskEntity.CreatedByUserId);

        // Allow admins and managers full access
        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
        {
            _logger.LogInformation("User {UserId} granted access as {Role}", userId, userRole);
            return true;
        }

        // Allow task assignee to add progress
        if (TaskEntity.AssignedToUserId.HasValue && TaskEntity.AssignedToUserId == userId)
        {
            _logger.LogInformation("User {UserId} granted access as task assignee", userId);
            return true;
        }

        // Allow task creator to add progress
        if (TaskEntity.CreatedByUserId == userId)
        {
            _logger.LogInformation("User {UserId} granted access as task creator", userId);
            return true;
        }

        _logger.LogWarning("User {UserId} denied access - not assignee, creator, admin, or manager", userId);
        return false;
    }
}