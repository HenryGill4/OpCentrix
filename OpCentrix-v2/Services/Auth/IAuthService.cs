using System.Security.Claims;

namespace OpCentrix.Services.Auth;

/// <summary>
/// Handles authentication for both platform (super admin) and tenant users.
/// Multi-tenant login flow: platform.db first, then scan tenant DBs.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user by checking platform.db first (super admin),
    /// then scanning active tenant databases for a matching username.
    /// Returns a <see cref="ClaimsPrincipal"/> on success, or null on failure.
    /// </summary>
    Task<AuthResult?> LoginAsync(string username, string password);

    /// <summary>Hashes a plaintext password using BCrypt.</summary>
    string HashPassword(string password);

    /// <summary>Verifies a plaintext password against a BCrypt hash.</summary>
    bool VerifyPassword(string password, string hash);

    /// <summary>
    /// Creates a default super admin account in platform.db if none exists.
    /// Called during app startup.
    /// </summary>
    Task EnsureDefaultSuperAdminAsync();
}

/// <summary>
/// Result of a successful login attempt.
/// </summary>
public record AuthResult(
    ClaimsPrincipal Principal,
    string RedirectUrl,
    bool IsPlatformUser,
    string? TenantCode
);
