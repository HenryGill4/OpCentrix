using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.Platform;

namespace OpCentrix.Services.Platform;

/// <summary>
/// Manages tenant lifecycle against <see cref="PlatformDbContext"/> (platform.db)
/// and provisions per-tenant SQLite databases via <see cref="TenantDbContextFactory"/>.
/// </summary>
public class TenantService : ITenantService
{
    private readonly PlatformDbContext _platformDb;
    private readonly TenantDbContextFactory _tenantFactory;

    public TenantService(PlatformDbContext platformDb, TenantDbContextFactory tenantFactory)
    {
        _platformDb = platformDb;
        _tenantFactory = tenantFactory;
    }

    public async Task<List<Tenant>> GetAllTenantsAsync()
    {
        return await _platformDb.Tenants
            .OrderBy(t => t.CompanyName)
            .ToListAsync();
    }

    public async Task<List<Tenant>> GetActiveTenantsAsync()
    {
        return await _platformDb.Tenants
            .Where(t => t.IsActive)
            .OrderBy(t => t.CompanyName)
            .ToListAsync();
    }

    public async Task<Tenant?> GetByCodeAsync(string tenantCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantCode);

        return await _platformDb.Tenants
            .FirstOrDefaultAsync(t => t.Code == tenantCode);
    }

    public async Task<Tenant?> GetByIdAsync(int id)
    {
        return await _platformDb.Tenants.FindAsync(id);
    }

    public async Task<Tenant> CreateTenantAsync(string code, string companyName, string createdBy)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(companyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdBy);

        code = code.ToLowerInvariant().Trim();

        var existing = await _platformDb.Tenants.AnyAsync(t => t.Code == code);
        if (existing)
            throw new InvalidOperationException($"Tenant with code '{code}' already exists.");

        var tenant = new Tenant
        {
            Code = code,
            CompanyName = companyName,
            CreatedBy = createdBy,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _platformDb.Tenants.Add(tenant);
        await _platformDb.SaveChangesAsync();

        // Provision the tenant database (EnsureCreated is called inside Create)
        using var tenantDb = _tenantFactory.Create(code);
        SeedTenantDefaults(tenantDb, createdBy);
        await tenantDb.SaveChangesAsync();

        return tenant;
    }

    public async Task UpdateTenantAsync(Tenant tenant)
    {
        ArgumentNullException.ThrowIfNull(tenant);

        _platformDb.Tenants.Update(tenant);
        await _platformDb.SaveChangesAsync();
    }

    public async Task DeactivateTenantAsync(int id)
    {
        var tenant = await _platformDb.Tenants.FindAsync(id)
            ?? throw new InvalidOperationException($"Tenant with id {id} not found.");

        tenant.IsActive = false;
        await _platformDb.SaveChangesAsync();
    }

    public async Task ReactivateTenantAsync(int id)
    {
        var tenant = await _platformDb.Tenants.FindAsync(id)
            ?? throw new InvalidOperationException($"Tenant with id {id} not found.");

        tenant.IsActive = true;
        await _platformDb.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds a freshly created tenant database with default production stages,
    /// machines, materials, operating shifts, and system settings.
    /// This will be expanded when IDataSeedingService is created (B16).
    /// </summary>
    private static void SeedTenantDefaults(TenantDbContext db, string createdBy)
    {
        if (db.ProductionStages.Any())
            return;

        var now = DateTime.UtcNow;

        db.ProductionStages.AddRange(
            CreateStage(1, "SLS/LPBF Printing", "sls-printing", "SLS", 8.0, true, true, false, "#3B82F6", "fas fa-print", 30, createdBy, now),
            CreateStage(2, "Depowdering", "depowdering", "SLS", 1.0, true, true, false, "#8B5CF6", "fas fa-wind", 15, createdBy, now),
            CreateStage(3, "Heat Treatment", "heat-treatment", "Post-Process", 4.0, true, true, false, "#EF4444", "fas fa-fire", 20, createdBy, now),
            CreateStage(4, "Wire EDM", "wire-edm", "EDM", 2.0, false, true, false, "#F59E0B", "fas fa-bolt", 30, createdBy, now),
            CreateStage(5, "CNC Machining", "cnc-machining", "Machining", 3.0, false, true, false, "#10B981", "fas fa-cogs", 30, createdBy, now),
            CreateStage(6, "Laser Engraving", "laser-engraving", "Engraving", 0.5, false, true, true, "#6366F1", "fas fa-pen-nib", 10, createdBy, now),
            CreateStage(7, "Surface Finishing", "surface-finishing", "Finishing", 1.5, true, true, false, "#EC4899", "fas fa-brush", 15, createdBy, now),
            CreateStage(8, "Quality Control", "qc", "Quality", 0.5, false, true, false, "#14B8A6", "fas fa-clipboard-check", 10, createdBy, now),
            CreateStage(9, "Shipping", "shipping", "Shipping", 0.5, false, true, false, "#6B7280", "fas fa-truck", 10, createdBy, now)
        );

        db.Machines.AddRange(
            CreateMachine("TI1", "TruPrint 1000 #1", "SLS", createdBy, now),
            CreateMachine("TI2", "TruPrint 1000 #2", "SLS", createdBy, now),
            CreateMachine("INC1", "Incidental SLS", "SLS", createdBy, now),
            CreateMachine("EDM1", "Wire EDM #1", "EDM", createdBy, now),
            CreateMachine("CNC1", "CNC Mill #1", "CNC", createdBy, now)
        );

        db.Materials.AddRange(
            new Models.Material { Name = "Ti-6Al-4V Grade 5", Category = "Metal Powder", CostPerKg = 450.00m, IsActive = true, CreatedDate = now, LastModifiedDate = now, CreatedBy = createdBy, LastModifiedBy = createdBy },
            new Models.Material { Name = "316L Stainless Steel", Category = "Metal Powder", CostPerKg = 80.00m, IsActive = true, CreatedDate = now, LastModifiedDate = now, CreatedBy = createdBy, LastModifiedBy = createdBy },
            new Models.Material { Name = "Inconel 718", Category = "Metal Powder", CostPerKg = 350.00m, IsActive = true, CreatedDate = now, LastModifiedDate = now, CreatedBy = createdBy, LastModifiedBy = createdBy }
        );

        db.OperatingShifts.Add(new Models.OperatingShift
        {
            Name = "Day Shift",
            StartTime = new TimeSpan(6, 0, 0),
            EndTime = new TimeSpan(18, 0, 0),
            DaysOfWeek = "Mon,Tue,Wed,Thu,Fri",
            IsActive = true,
            CreatedDate = now
        });

        db.SystemSettings.AddRange(
            new Models.SystemSetting { Key = "CompanyName", Value = "OpCentrix Manufacturing", Category = "Branding", LastModifiedDate = now, LastModifiedBy = createdBy },
            new Models.SystemSetting { Key = "SerialNumberPrefix", Value = "SN", Category = "Serial", LastModifiedDate = now, LastModifiedBy = createdBy },
            new Models.SystemSetting { Key = "SerialNumberNextValue", Value = "1", Category = "Serial", LastModifiedDate = now, LastModifiedBy = createdBy },
            new Models.SystemSetting { Key = "ShowDebugBuildForm", Value = "true", Category = "Debug", LastModifiedDate = now, LastModifiedBy = createdBy }
        );
    }

    private static Models.ProductionStage CreateStage(
        int order, string name, string slug, string department, double durationHours,
        bool isBatch, bool hasBuiltInPage, bool requiresSerialNumber,
        string color, string icon, int setupMinutes, string createdBy, DateTime now)
    {
        return new Models.ProductionStage
        {
            Name = name,
            StageSlug = slug,
            DisplayOrder = order,
            Department = department,
            DefaultDurationHours = durationHours,
            IsBatchStage = isBatch,
            HasBuiltInPage = hasBuiltInPage,
            RequiresSerialNumber = requiresSerialNumber,
            StageColor = color,
            StageIcon = icon,
            DefaultSetupMinutes = setupMinutes,
            IsActive = true,
            CreatedDate = now,
            LastModifiedDate = now,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy
        };
    }

    private static Models.Machine CreateMachine(string machineId, string name, string type, string createdBy, DateTime now)
    {
        return new Models.Machine
        {
            MachineId = machineId,
            Name = name,
            MachineType = type,
            IsActive = true,
            CreatedDate = now,
            LastModifiedDate = now,
            CreatedBy = createdBy,
            LastModifiedBy = createdBy
        };
    }
}
