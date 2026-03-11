using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;

namespace OpCentrix.Services.Auth;

/// <summary>
/// Multi-tenant authentication service.
/// Login flow per spec §7A: check platform.db ? scan tenant DBs ? return claims.
/// </summary>
public class AuthService : IAuthService
{
    private readonly PlatformDbContext _platformDb;
    private readonly TenantDbContextFactory _tenantFactory;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        PlatformDbContext platformDb,
        TenantDbContextFactory tenantFactory,
        ILogger<AuthService> logger)
    {
        _platformDb = platformDb;
        _tenantFactory = tenantFactory;
        _logger = logger;
    }

    public async Task<AuthResult?> LoginAsync(string username, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        username = username.Trim();

        // 1. Check platform.db ? PlatformUsers (super admin)
        var platformUser = await _platformDb.PlatformUsers
            .FirstOrDefaultAsync(u => u.Username == username);

        if (platformUser is not null && VerifyPassword(password, platformUser.PasswordHash))
        {
            _logger.LogInformation("Platform user '{Username}' authenticated", username);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, platformUser.Username),
                new(ClaimTypes.Role, platformUser.Role),
                new("IsPlatform", "true"),
                new("UserId", platformUser.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            return new AuthResult(principal, "/Platform/Tenants", true, null);
        }

        // 2. Scan active tenant DBs for matching username
        var activeTenants = await _platformDb.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();

        foreach (var tenant in activeTenants)
        {
            try
            {
                using var tenantDb = _tenantFactory.Create(tenant.Code);

                var tenantUser = await tenantDb.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (tenantUser is null || !VerifyPassword(password, tenantUser.PasswordHash))
                    continue;

                // Update last login
                tenantUser.LastLoginDate = DateTime.UtcNow;
                await tenantDb.SaveChangesAsync();

                _logger.LogInformation(
                    "Tenant user '{Username}' authenticated for tenant '{TenantCode}'",
                    username, tenant.Code);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, tenantUser.Username),
                    new(ClaimTypes.Role, tenantUser.Role),
                    new("TenantCode", tenant.Code),
                    new("CompanyName", tenant.CompanyName),
                    new("UserId", tenantUser.Id.ToString()),
                    new("FullName", tenantUser.FullName)
                };

                if (!string.IsNullOrWhiteSpace(tenantUser.Department))
                    claims.Add(new Claim("Department", tenantUser.Department));

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                return new AuthResult(principal, "/", false, tenant.Code);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking tenant '{TenantCode}' during login", tenant.Code);
            }
        }

        _logger.LogWarning("Login failed for username '{Username}'", username);
        return null;
    }

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task EnsureDefaultSuperAdminAsync()
    {
        var hasAdmin = await _platformDb.PlatformUsers.AnyAsync();
        if (hasAdmin)
            return;

        var admin = new Models.Platform.PlatformUser
        {
            Username = "admin",
            PasswordHash = HashPassword("admin"),
            Role = "SuperAdmin"
        };

        _platformDb.PlatformUsers.Add(admin);
        await _platformDb.SaveChangesAsync();

        _logger.LogInformation("Default super admin account created (username: admin)");
    }
}
