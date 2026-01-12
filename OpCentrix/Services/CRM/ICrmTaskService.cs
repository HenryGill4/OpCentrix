using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public interface ICrmTaskService
{
    Task<CrmTask> CreateAsync(
        string title,
        string? description,
        int priority,
        DateTime? dueAt,
        int createdByUserId,
        int? assignedToUserId,
        int? accountId,
        int? contactId,
        bool hasReminder = false,
        int reminderMinutesBefore = 15,
        string reminderType = "Email",
        CancellationToken ct = default);

    Task UpdateAsync(
        int id,
        string title,
        string? description,
        int priority,
        DateTime? dueAt,
        string status,
        int? assignedToUserId,
        int? accountId,
        int? contactId,
        bool hasReminder = false,
        int reminderMinutesBefore = 15,
        string reminderType = "Email",
        bool notifyOnStatusChange = true,
        bool notifyAssignee = true,
        bool notifyCreator = true,
        CancellationToken ct = default);

    Task AssignAsync(int taskId, int? assignedToUserId, CancellationToken ct = default);
    Task CompleteAsync(int taskId, CancellationToken ct = default);
    Task<CrmTask?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<List<CrmTask>> ListAsync(
        string? status,
        int? assignedToUserId,
        int? accountId,
        CancellationToken ct = default);
        
    Task<List<CrmTask>> GetTasksDueForReminderAsync(CancellationToken ct = default);
    Task SetReminderAsync(int taskId, DateTime reminderDateTime, string reminderType = "Email", CancellationToken ct = default);
}
