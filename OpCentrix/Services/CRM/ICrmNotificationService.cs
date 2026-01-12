using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public interface ICrmNotificationService
{
    Task SendTaskReminderAsync(CrmTask task, CancellationToken ct = default);
    Task SendTaskAssignmentNotificationAsync(CrmTask task, CancellationToken ct = default);
    Task SendTaskStatusChangeNotificationAsync(CrmTask task, string oldStatus, CancellationToken ct = default);
    Task SendTaskDueNotificationAsync(CrmTask task, CancellationToken ct = default);
    
    Task<List<CrmTask>> GetTasksNeedingRemindersAsync(CancellationToken ct = default);
    Task ProcessPendingRemindersAsync(CancellationToken ct = default);
    
    Task CreateAlertAsync(string title, string message, string severity = "Info", CancellationToken ct = default);
    Task<List<CrmAlert>> GetActiveAlertsAsync(CancellationToken ct = default);
    Task DismissAlertAsync(int alertId, CancellationToken ct = default);
}