namespace OpCentrix.MultiTenancy;

/// <inheritdoc />
public class TenantContext : ITenantContext
{
    public string? TenantCode { get; private set; }

    public bool IsResolved => !string.IsNullOrWhiteSpace(TenantCode);

    public void SetTenant(string tenantCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantCode);

        if (IsResolved)
            throw new InvalidOperationException($"Tenant already set to '{TenantCode}'. Cannot change mid-request.");

        TenantCode = tenantCode;
    }
}
