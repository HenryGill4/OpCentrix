using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Data;

/// <summary>
/// Creates tenant-scoped <see cref="TenantDbContext"/> instances using
/// per-tenant SQLite databases stored under <c>App_Data/tenants/{code}.db</c>.
/// </summary>
public class TenantDbContextFactory
{
    private readonly IWebHostEnvironment _env;

    public TenantDbContextFactory(IWebHostEnvironment env)
    {
        _env = env;
    }

    /// <summary>
    /// Builds a <see cref="TenantDbContext"/> for the given tenant code.
    /// </summary>
    public TenantDbContext Create(string tenantCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantCode);

        var folder = Path.Combine(_env.ContentRootPath, "App_Data", "tenants");
        Directory.CreateDirectory(folder);

        var dbPath = Path.Combine(folder, $"{tenantCode}.db");
        var connectionString = $"Data Source={dbPath}";

        var options = new DbContextOptionsBuilder<TenantDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var context = new TenantDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
