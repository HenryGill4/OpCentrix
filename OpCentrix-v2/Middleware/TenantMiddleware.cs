using OpCentrix.MultiTenancy;

namespace OpCentrix.Middleware;

/// <summary>
/// Resolves the current tenant from the route (<c>/{tenant}/…</c>) or the
/// <c>X-Tenant</c> header and populates <see cref="ITenantContext"/> for the request.
/// </summary>
public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private const string TenantHeader = "X-Tenant";

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantCode = ResolveTenantCode(context);

        if (!string.IsNullOrWhiteSpace(tenantCode))
        {
            tenantContext.SetTenant(tenantCode);
        }

        await _next(context);
    }

    private static string? ResolveTenantCode(HttpContext context)
    {
        // 1. Route value  e.g. /{tenant}/Dashboard
        if (context.Request.RouteValues.TryGetValue("tenant", out var routeValue)
            && routeValue is string rv
            && !string.IsNullOrWhiteSpace(rv))
        {
            return rv;
        }

        // 2. Header  e.g. X-Tenant: acme
        if (context.Request.Headers.TryGetValue(TenantHeader, out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        // 3. Query string fallback (useful for dev/testing)
        if (context.Request.Query.TryGetValue("tenant", out var queryValue)
            && !string.IsNullOrWhiteSpace(queryValue))
        {
            return queryValue.ToString();
        }

        return null;
    }
}
