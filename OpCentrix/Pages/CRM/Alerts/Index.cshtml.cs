using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using OpCentrix.Services.CRM;

namespace OpCentrix.Pages.CRM.Alerts;

public class IndexModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ICrmNotificationService _notificationService;
    private readonly ICrmTaskService _taskService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(SchedulerContext context, ICrmNotificationService notificationService, ICrmTaskService taskService, ILogger<IndexModel> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _taskService = taskService;
        _logger = logger;
    }

    public List<CrmAlert> Alerts { get; set; } = new();
    public List<CrmTask> OverdueTasks { get; set; } = new();
    public List<CrmTask> UpcomingTasks { get; set; } = new();
    public int TotalActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int WarningAlerts { get; set; }

    [BindProperty]
    public int? DismissAlertId { get; set; }

    [BindProperty]
    public int? CompleteTaskId { get; set; }

    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            await LoadAlertsAsync();
            await LoadTaskSummariesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading alerts page");
            StatusMessage = "Error loading alerts. Please try again.";
        }
    }

    public async Task<IActionResult> OnPostDismissAsync()
    {
        if (DismissAlertId.HasValue)
        {
            try
            {
                await _notificationService.DismissAlertAsync(DismissAlertId.Value);
                StatusMessage = "Alert dismissed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dismissing alert {AlertId}", DismissAlertId.Value);
                StatusMessage = "Error dismissing alert. Please try again.";
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDismissAllAsync()
    {
        try
        {
            var activeAlerts = await _context.CrmAlerts
                .Where(a => !a.IsDismissed)
                .ToListAsync();

            foreach (var alert in activeAlerts)
            {
                alert.IsDismissed = true;
                alert.LastModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            StatusMessage = $"Successfully dismissed {activeAlerts.Count} alerts.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dismissing all alerts");
            StatusMessage = "Error dismissing all alerts. Please try again.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostProcessRemindersAsync()
    {
        try
        {
            await _notificationService.ProcessPendingRemindersAsync();
            StatusMessage = "Successfully processed pending reminders.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing reminders");
            StatusMessage = "Error processing reminders. Please try again.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCompleteTaskAsync()
    {
        if (CompleteTaskId.HasValue)
        {
            try
            {
                await _taskService.CompleteAsync(CompleteTaskId.Value);
                StatusMessage = "Task marked as completed successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing task {TaskId}", CompleteTaskId.Value);
                StatusMessage = "Error completing task. Please try again.";
            }
        }

        return RedirectToPage();
    }

    private async Task LoadAlertsAsync()
    {
        Alerts = await _notificationService.GetActiveAlertsAsync();
        TotalActiveAlerts = Alerts.Count;
        CriticalAlerts = Alerts.Count(a => a.Severity == "Critical");
        WarningAlerts = Alerts.Count(a => a.Severity == "Warning");
    }

    private async Task LoadTaskSummariesAsync()
    {
        var now = DateTime.UtcNow;

        // Get overdue tasks
        OverdueTasks = await _context.CrmTasks
            .Where(t => t.DueAt.HasValue && 
                       t.DueAt.Value < now && 
                       t.Status != "Completed" && 
                       t.Status != "On Hold")
            .OrderBy(t => t.DueAt)
            .Take(10)
            .ToListAsync();

        // Get upcoming tasks (due in next 24 hours)
        var tomorrow = now.AddDays(1);
        UpcomingTasks = await _context.CrmTasks
            .Where(t => t.DueAt.HasValue && 
                       t.DueAt.Value >= now && 
                       t.DueAt.Value <= tomorrow && 
                       t.Status != "Completed")
            .OrderBy(t => t.DueAt)
            .Take(10)
            .ToListAsync();
    }
}