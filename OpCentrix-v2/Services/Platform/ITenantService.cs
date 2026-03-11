using OpCentrix.Models.Platform;

namespace OpCentrix.Services.Platform;

/// <summary>
/// Manages tenant lifecycle: CRUD, DB provisioning, and seed data.
/// </summary>
public interface ITenantService
{
    /// <summary>Returns all tenants (active and inactive).</summary>
    Task<List<Tenant>> GetAllTenantsAsync();

    /// <summary>Returns only active tenants.</summary>
    Task<List<Tenant>> GetActiveTenantsAsync();

    /// <summary>Gets a tenant by its unique code (e.g. "acme").</summary>
    Task<Tenant?> GetByCodeAsync(string tenantCode);

    /// <summary>Gets a tenant by primary key.</summary>
    Task<Tenant?> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new tenant record in platform.db, provisions the tenant SQLite database,
    /// and seeds default data (stages, machines, materials, shifts, settings).
    /// </summary>
    Task<Tenant> CreateTenantAsync(string code, string companyName, string createdBy);

    /// <summary>Updates tenant metadata (company name, logo, color, tier).</summary>
    Task UpdateTenantAsync(Tenant tenant);

    /// <summary>Deactivates a tenant (soft delete — DB file is preserved).</summary>
    Task DeactivateTenantAsync(int id);

    /// <summary>Reactivates a previously deactivated tenant.</summary>
    Task ReactivateTenantAsync(int id);
}
