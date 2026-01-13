using OpCentrix.Models.CRM;

namespace OpCentrix.Services.CRM;

public interface ICrmContactService
{
    Task<CrmContact> CreateAsync(int accountId, string name, string? email, string? phone, string? title, CancellationToken ct = default);
    Task<CrmContact?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(int id, string name, string? email, string? phone, string? title, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<List<CrmContact>> GetByAccountIdAsync(int accountId, CancellationToken ct = default);
}
