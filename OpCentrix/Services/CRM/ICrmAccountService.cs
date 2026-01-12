using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public interface ICrmAccountService
{
    Task<CrmAccount> CreateAsync(string name, string? status, string? notes, CancellationToken ct = default);
    Task UpdateAsync(int id, string name, string? status, string? notes, CancellationToken ct = default);
    Task<CrmAccount?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<CrmAccount>> SearchAsync(string? query, CancellationToken ct = default);
}
