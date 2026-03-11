namespace OpCentrix.MultiTenancy;

/// <summary>
/// Provides the resolved tenant code for the current request scope.
/// Registered as <c>Scoped</c> so each request gets its own instance.
/// </summary>
public interface ITenantContext
{
    /// <summary>Current tenant code (e.g. "acme"). Null when not yet resolved.</summary>
    string? TenantCode { get; }

    /// <summary>True after <see cref="SetTenant"/> has been called with a non-empty value.</summary>
    bool IsResolved { get; }

    /// <summary>Called by <see cref="Middleware.TenantMiddleware"/> once per request.</summary>
    void SetTenant(string tenantCode);
}
