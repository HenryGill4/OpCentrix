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

public class DetailsModel : PageModel
{
    private readonly ICrmTaskService _taskService;
    private readonly ICrmNotificationService _notificationService;
    private readonly SchedulerContext _context;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ICrmTaskService taskService, ICrmNotificationService notificationService, SchedulerContext context, ILogger<DetailsModel> logger)
    {
        _taskService = taskService;
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public CrmTask? TaskEntity { get; set; }
    public List<CrmTaskProgress> ProgressEntries { get; set; } = new();

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

        // Reminder settings
        public bool HasReminder { get; set; }
        public int ReminderMinutesBefore { get; set; } = 15;
        public string ReminderType { get; set; } = "Email";

        // Notification preferences
        public bool NotifyOnStatusChange { get; set; } = true;
        public bool NotifyAssignee { get; set; } = true;
        public bool NotifyCreator { get; set; } = true;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    
    [BindProperty]
    public int? DeleteProgressId { get; set; }
    
    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadListsAsync(ct);
        TaskEntity = await _taskService.GetByIdAsync(Id, ct);
        if (TaskEntity == null) return NotFound();

        // Load progress entries
        ProgressEntries = await _taskService.GetTaskProgressAsync(Id, ct);

        Input = new InputModel
        {
            Title = TaskEntity.Title,
            Description = TaskEntity.Description,
            Priority = TaskEntity.Priority,
            DueAt = TaskEntity.DueAt?.Date,
            Status = TaskEntity.Status,
            AssignedToUserId = TaskEntity.AssignedToUserId,
            AccountId = TaskEntity.AccountId,
            HasReminder = TaskEntity.HasReminder,
            ReminderMinutesBefore = TaskEntity.ReminderMinutesBefore,
            ReminderType = TaskEntity.ReminderType,
            NotifyOnStatusChange = TaskEntity.NotifyOnStatusChange,
            NotifyAssignee = TaskEntity.NotifyAssignee,
            NotifyCreator = TaskEntity.NotifyCreator
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
            hasReminder: Input.HasReminder,
            reminderMinutesBefore: Input.ReminderMinutesBefore,
            reminderType: Input.ReminderType,
            notifyOnStatusChange: Input.NotifyOnStatusChange,
            notifyAssignee: Input.NotifyAssignee,
            notifyCreator: Input.NotifyCreator,
            ct: ct);

        StatusMessage = "Task updated successfully!";
        return RedirectToPage(new { id = Id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id, CancellationToken ct)
    {
        await _taskService.CompleteAsync(id, ct);
        StatusMessage = "Task marked as completed!";
        return RedirectToPage(new { id });
    }
    
    public async Task<IActionResult> OnPostTestReminderAsync(CancellationToken ct)
    {
        var task = await _taskService.GetByIdAsync(Id, ct);
        if (task != null)
        {
            await _notificationService.SendTaskReminderAsync(task, ct);
            StatusMessage = "Test reminder sent! Check the alerts page to see it.";
        }
        return RedirectToPage(new { id = Id });
    }
    
    public async Task<IActionResult> OnPostDeleteProgressAsync(CancellationToken ct)
    {
        if (!User.IsInRole("Admin") && !User.IsInRole("Manager"))
        {
            return Forbid();
        }

        if (DeleteProgressId.HasValue)
        {
            try
            {
                var progressEntry = await _taskService.GetProgressEntryAsync(DeleteProgressId.Value, ct);
                if (progressEntry != null && progressEntry.TaskId == Id)
                {
                    await _taskService.DeleteProgressEntryAsync(DeleteProgressId.Value, ct);
                    StatusMessage = "Progress entry deleted successfully.";
                }
                else
                {
                    StatusMessage = "Progress entry not found.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting progress entry {ProgressId}", DeleteProgressId.Value);
                StatusMessage = "Error deleting progress entry. Please try again.";
            }
        }

        return RedirectToPage(new { id = Id });
    }
    
    public async Task<IActionResult> OnPostDeleteTaskAsync(CancellationToken ct)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            var task = await _taskService.GetByIdAsync(Id, ct);
            var taskTitle = task?.Title ?? "Unknown Task";
            
            await _taskService.DeleteTaskAsync(Id, ct);
            StatusMessage = $"Task '{taskTitle}' has been permanently deleted.";
            return RedirectToPage("/CRM/Tasks/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", Id);
            StatusMessage = "Error deleting task. Please try again.";
            return RedirectToPage(new { id = Id });
        }
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
