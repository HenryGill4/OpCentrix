using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders;

/// <summary>
/// Factory for creating and managing machine provider instances.
/// Handles provider resolution based on machine configuration.
/// </summary>
public class MachineProviderFactory : IMachineProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MachineProviderFactory> _logger;
    private readonly Dictionary<string, Type> _providerTypes;

    public MachineProviderFactory(
        IServiceProvider serviceProvider, 
        ILogger<MachineProviderFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Register available provider types
        _providerTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["Mock"] = typeof(MockMachineProvider),
            // Future providers:
            // ["Eos"] = typeof(EosMachineProvider),
            // ["GenericOpcUa"] = typeof(GenericOpcUaProvider),
        };
    }

    /// <inheritdoc />
    public IMachineProvider? GetProvider(string providerType)
    {
        if (string.IsNullOrWhiteSpace(providerType))
        {
            _logger.LogWarning("[FACTORY] Empty provider type requested, returning null");
            return null;
        }

        if (!_providerTypes.TryGetValue(providerType, out var providerTypeClass))
        {
            _logger.LogWarning("[FACTORY] Unknown provider type '{ProviderType}' requested", providerType);
            return null;
        }

        try
        {
            var provider = (IMachineProvider?)_serviceProvider.GetService(providerTypeClass);
            
            if (provider == null)
            {
                _logger.LogWarning("[FACTORY] Provider type '{ProviderType}' not registered in DI", providerType);
            }

            return provider;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FACTORY] Error creating provider type '{ProviderType}'", providerType);
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<IMachineProvider> GetProviderForMachineAsync(int machineId, CancellationToken ct = default)
    {
        try
        {
            // Get machine connection settings from database
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();

            var settings = await context.Set<MachineConnectionSettings>()
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.MachineId == machineId && s.IsEnabled, ct);

            if (settings != null)
            {
                var provider = GetProvider(settings.ProviderType);
                if (provider != null)
                {
                    await provider.InitializeAsync(settings, ct);
                    _logger.LogDebug("[FACTORY] Initialized {ProviderType} provider for machine {MachineId}", 
                        settings.ProviderType, machineId);
                    return provider;
                }
            }

            // Fallback to Mock provider
            _logger.LogDebug("[FACTORY] No configured provider for machine {MachineId}, using Mock", machineId);
            var mockProvider = GetProvider("Mock")!;
            await mockProvider.InitializeAsync(new MachineConnectionSettings
            {
                MachineId = machineId,
                ProviderType = "Mock",
                IsEnabled = true
            }, ct);

            return mockProvider;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[FACTORY] Error getting provider for machine {MachineId}, falling back to Mock", machineId);
            
            // Guaranteed fallback
            var mockProvider = GetProvider("Mock")!;
            await mockProvider.InitializeAsync(new MachineConnectionSettings
            {
                MachineId = machineId,
                ProviderType = "Mock",
                IsEnabled = true
            }, ct);

            return mockProvider;
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableProviderTypes()
    {
        return _providerTypes.Keys.ToList().AsReadOnly();
    }
}
