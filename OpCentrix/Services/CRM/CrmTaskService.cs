using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public class CrmTaskService : ICrmTaskService
{
    private readonly SchedulerContext _context;

    public CrmTaskService(SchedulerContext context)
    {
        _context = context;
    }

    public async Task<CrmTask> CreateAsync(
        string title,
        string? description,
        int priority,
        DateTime? dueAt,
        int createdByUserId,
        int? assignedToUserId,
        int? accountId,
        int? contactId,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(title)) throw new InvalidOperationException("Title is required.");
        if (priority is < 1 or > 5) priority = 3;

        var createdByExists = await _context.Users.AnyAsync(u => u.Id == createdByUserId, ct);
        if (!createdByExists) throw new InvalidOperationException("Creating user not found.");

        if (assignedToUserId.HasValue)
        {
            var assigneeExists = await _context.Users.AnyAsync(u => u.Id == assignedToUserId.Value, ct);
            if (!assigneeExists) assignedToUserId = null;
        }

        if (accountId.HasValue)
        {
            var accountExists = await _context.CrmAccounts.AnyAsync(a => a.Id == accountId.Value, ct);
            if (!accountExists) accountId = null;
        }

        if (contactId.HasValue)
        {
            var contactExists = await _context.CrmContacts.AnyAsync(c => c.Id == contactId.Value, ct);
            if (!contactExists) contactId = null;
        }

        var entity = new CrmTask
        {
            Title = title.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            Priority = priority,
            DueAt = dueAt,
            Status = "Open",
            CreatedByUserId = createdByUserId,
            AssignedToUserId = assignedToUserId,
            AccountId = accountId,
            ContactId = contactId
        };

        _context.CrmTasks.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(
        int id,
        string title,
        string? description,
        int priority,
        DateTime? dueAt,
        string status,
        int? assignedToUserId,
        int? accountId,
        int? contactId,
        CancellationToken ct = default)
    {
        var entity = await _context.CrmTasks.FirstOrDefaultAsync(t => t.Id == id, ct);
        if (entity == null) throw new InvalidOperationException("CRM task not found.");

        if (string.IsNullOrWhiteSpace(title)) throw new InvalidOperationException("Title is required.");
        if (priority is < 1 or > 5) priority = 3;

        entity.Title = title.Trim();
        entity.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        entity.Priority = priority;
        entity.DueAt = dueAt;

        entity.Status = NormalizeStatus(status);

        if (entity.Status == "Completed")
        {
            entity.CompletedAt ??= DateTime.UtcNow;
        }
        else
        {
            entity.CompletedAt = null;
        }

        if (assignedToUserId.HasValue)
        {
            var assigneeExists = await _context.Users.AnyAsync(u => u.Id == assignedToUserId.Value, ct);
            entity.AssignedToUserId = assigneeExists ? assignedToUserId : null;
        }
        else
        {
            entity.AssignedToUserId = null;
        }

        if (accountId.HasValue)
        {
            var accountExists = await _context.CrmAccounts.AnyAsync(a => a.Id == accountId.Value, ct);
            entity.AccountId = accountExists ? accountId : null;
        }
        else
        {
            entity.AccountId = null;
        }

        if (contactId.HasValue)
        {
            var contactExists = await _context.CrmContacts.AnyAsync(c => c.Id == contactId.Value, ct);
            entity.ContactId = contactExists ? contactId : null;
        }
        else
        {
            entity.ContactId = null;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task AssignAsync(int taskId, int? assignedToUserId, CancellationToken ct = default)
    {
        var entity = await _context.CrmTasks.FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (entity == null) throw new InvalidOperationException("CRM task not found.");

        if (assignedToUserId.HasValue)
        {
            var exists = await _context.Users.AnyAsync(u => u.Id == assignedToUserId.Value, ct);
            entity.AssignedToUserId = exists ? assignedToUserId : null;
        }
        else
        {
            entity.AssignedToUserId = null;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task CompleteAsync(int taskId, CancellationToken ct = default)
    {
        var entity = await _context.CrmTasks.FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (entity == null) throw new InvalidOperationException("CRM task not found.");

        entity.Status = "Completed";
        entity.CompletedAt ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }

    public Task<CrmTask?> GetByIdAsync(int id, CancellationToken ct = default)
        => _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<List<CrmTask>> ListAsync(string? status, int? assignedToUserId, int? accountId, CancellationToken ct = default)
    {
        var q = _context.CrmTasks
            .Include(t => t.Account)
            .Include(t => t.Contact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = NormalizeStatus(status);
            q = q.Where(t => t.Status == s);
        }

        if (assignedToUserId.HasValue)
        {
            q = q.Where(t => t.AssignedToUserId == assignedToUserId);
        }

        if (accountId.HasValue)
        {
            q = q.Where(t => t.AccountId == accountId);
        }

        return await q
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.DueAt != null)
            .ThenBy(t => t.DueAt)
            .ThenByDescending(t => t.Id)
            .Take(500)
            .ToListAsync(ct);
    }

    private static string NormalizeStatus(string? status)
    {
        var s = (status ?? string.Empty).Trim();
        return s switch
        {
            "Open" => "Open",
            "InProgress" => "InProgress",
            "Completed" => "Completed",
            _ => "Open"
        };
    }
}
