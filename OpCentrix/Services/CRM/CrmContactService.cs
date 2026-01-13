using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public class CrmContactService : ICrmContactService
{
    private readonly SchedulerContext _context;

    public CrmContactService(SchedulerContext context)
    {
        _context = context;
    }

    public async Task<CrmContact> CreateAsync(int accountId, string name, string? email, string? phone, string? title, CancellationToken ct = default)
    {
        var accountExists = await _context.CrmAccounts.AnyAsync(a => a.Id == accountId, ct);
        if (!accountExists) throw new InvalidOperationException("CRM account not found.");

        var entity = new CrmContact
        {
            AccountId = accountId,
            Name = name.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim()
        };

        _context.CrmContacts.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<CrmContact?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.CrmContacts
            .Include(c => c.Account)
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task UpdateAsync(int id, string name, string? email, string? phone, string? title, CancellationToken ct = default)
    {
        var entity = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity == null) throw new InvalidOperationException("CRM contact not found.");

        entity.Name = name.Trim();
        entity.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        entity.Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        entity.Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.CrmContacts.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (entity == null) return false;

        _context.CrmContacts.Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public Task<List<CrmContact>> GetByAccountIdAsync(int accountId, CancellationToken ct = default)
        => _context.CrmContacts
            .Where(c => c.AccountId == accountId)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}
