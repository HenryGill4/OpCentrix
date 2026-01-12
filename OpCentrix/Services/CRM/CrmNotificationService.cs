using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public class CrmNotificationService : ICrmNotificationService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<CrmNotificationService> _logger;

    public CrmNotificationService(SchedulerContext context, ILogger<CrmNotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SendTaskReminderAsync(CrmTask task, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Sending reminder for task {TaskId}: {Title}", task.Id, task.Title);

            // Create alert for the reminder
            await CreateAlertAsync(
                $"Task Reminder: {task.Title}",
                $"Task '{task.Title}' (ID: {task.Id}) is due at {task.DueAt:f}. Priority: {task.Priority}/5",
                task.Priority >= 4 ? "Warning" : "Info",
                ct);

            // Mark reminder as sent
            task.IsReminderSent = true;
            task.ReminderSentAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync(ct);
            
            _logger.LogInformation("Reminder sent for task {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send reminder for task {TaskId}", task.Id);
        }
    }

    public async Task SendTaskAssignmentNotificationAsync(CrmTask task, CancellationToken ct = default)
    {
        try
        {
            if (!task.NotifyAssignee) return;

            await CreateAlertAsync(
                $"New Task Assignment: {task.Title}",
                $"You have been assigned a new task: {task.Title} (ID: {task.Id}). Due: {task.DueAt:f}",
                "Info",
                ct);

            _logger.LogInformation("Assignment notification sent for task {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send assignment notification for task {TaskId}", task.Id);
        }
    }

    public async Task SendTaskStatusChangeNotificationAsync(CrmTask task, string oldStatus, CancellationToken ct = default)
    {
        try
        {
            if (!task.NotifyOnStatusChange) return;

            var severity = task.Status == "Completed" ? "Info" : 
                          task.Status == "On Hold" ? "Warning" : "Info";

            await CreateAlertAsync(
                $"Task Status Updated: {task.Title}",
                $"Task '{task.Title}' (ID: {task.Id}) status changed from '{oldStatus}' to '{task.Status}'",
                severity,
                ct);

            _logger.LogInformation("Status change notification sent for task {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send status change notification for task {TaskId}", task.Id);
        }
    }

    public async Task SendTaskDueNotificationAsync(CrmTask task, CancellationToken ct = default)
    {
        try
        {
            await CreateAlertAsync(
                $"Task Overdue: {task.Title}",
                $"Task '{task.Title}' (ID: {task.Id}) was due at {task.DueAt:f} and is now overdue!",
                "Critical",
                ct);

            _logger.LogInformation("Due notification sent for task {TaskId}", task.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send due notification for task {TaskId}", task.Id);
        }
    }

    public async Task<List<CrmTask>> GetTasksNeedingRemindersAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        
        return await _context.CrmTasks
            .Where(t => t.HasReminder && 
                       !t.IsReminderSent && 
                       t.ReminderDateTime.HasValue && 
                       t.ReminderDateTime.Value <= now &&
                       t.Status != "Completed")
            .ToListAsync(ct);
    }

    public async Task ProcessPendingRemindersAsync(CancellationToken ct = default)
    {
        try
        {
            var tasksNeedingReminders = await GetTasksNeedingRemindersAsync(ct);
            
            foreach (var task in tasksNeedingReminders)
            {
                await SendTaskReminderAsync(task, ct);
            }

            // Check for overdue tasks
            var overdueTasks = await _context.CrmTasks
                .Where(t => t.DueAt.HasValue && 
                           t.DueAt.Value < DateTime.UtcNow && 
                           t.Status != "Completed" &&
                           t.Status != "On Hold")
                .ToListAsync(ct);

            foreach (var task in overdueTasks)
            {
                await SendTaskDueNotificationAsync(task, ct);
            }

            _logger.LogInformation("Processed {ReminderCount} reminders and {OverdueCount} overdue notifications", 
                tasksNeedingReminders.Count, overdueTasks.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process pending reminders");
        }
    }

    public async Task CreateAlertAsync(string title, string message, string severity = "Info", CancellationToken ct = default)
    {
        try
        {
            var alert = new CrmAlert
            {
                Title = title,
                Message = message,
                Severity = severity,
                IsDismissed = false,
                CreatedDate = DateTime.UtcNow
            };

            _context.CrmAlerts.Add(alert);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Created alert: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create alert: {Title}", title);
        }
    }

    public async Task<List<CrmAlert>> GetActiveAlertsAsync(CancellationToken ct = default)
    {
        return await _context.CrmAlerts
            .Where(a => !a.IsDismissed)
            .OrderByDescending(a => a.CreatedDate)
            .Take(50) // Limit to most recent 50 alerts
            .ToListAsync(ct);
    }

    public async Task DismissAlertAsync(int alertId, CancellationToken ct = default)
    {
        try
        {
            var alert = await _context.CrmAlerts.FindAsync(alertId);
            if (alert != null)
            {
                alert.IsDismissed = true;
                alert.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
                
                _logger.LogInformation("Dismissed alert {AlertId}", alertId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dismiss alert {AlertId}", alertId);
        }
    }
}