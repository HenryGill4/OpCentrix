using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public class CrmAccountService : ICrmAccountService
{
    private readonly SchedulerContext _context;

    public CrmAccountService(SchedulerContext context)
    {
        _context = context;
    }

    public async Task<CrmAccount> CreateAsync(string name, string? status, string? notes, CancellationToken ct = default)
    {
        var entity = new CrmAccount
        {
            Name = name.Trim(),
            Status = string.IsNullOrWhiteSpace(status) ? "Active" : status.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
        };

        _context.CrmAccounts.Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateAsync(int id, string name, string? status, string? notes, CancellationToken ct = default)
    {
        var entity = await _context.CrmAccounts.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (entity == null) throw new InvalidOperationException("CRM account not found.");

        entity.Name = name.Trim();
        entity.Status = string.IsNullOrWhiteSpace(status) ? entity.Status : status.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        await _context.SaveChangesAsync(ct);
    }

    public Task<CrmAccount?> GetByIdAsync(int id, CancellationToken ct = default)
        => _context.CrmAccounts
            .Include(a => a.Contacts)
            .Include(a => a.Tasks)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<List<CrmAccount>> SearchAsync(string? query, CancellationToken ct = default)
    {
        var q = _context.CrmAccounts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(a => a.Name.Contains(term));
        }

        return await q
            .OrderBy(a => a.Name)
            .Take(200)
            .ToListAsync(ct);
    }
}
