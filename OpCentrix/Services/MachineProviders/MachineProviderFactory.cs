using OpCentrix.Models.MachineProviders;
using OpCentrix.Services.MachineProviders.Eos;

namespace OpCentrix.Services.MachineProviders
{
    /// <summary>
    /// Creates and connects an IMachineProvider for a given MachineConnectionSettings record.
    ///
    /// Adding a new machine brand:
    ///   1. Implement IMachineProvider in a new class (e.g. HaasMachineProvider).
    ///   2. Register it in Program.cs.
    ///   3. Add a case here.
    ///   4. Set ProviderType = "Haas" on the MachineConnectionSettings row.
    ///   Nothing else in the codebase needs to change.
    /// </summary>
    public sealed class MachineProviderFactory
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<MachineProviderFactory> _logger;

        public MachineProviderFactory(
            IServiceProvider sp,
            ILogger<MachineProviderFactory> logger)
        {
            _sp = sp;
            _logger = logger;
        }

        /// <summary>
        /// Instantiate and connect a provider for the given settings.
        /// The caller is responsible for disposing the returned provider.
        /// Returns null if the provider type is unrecognised.
        /// </summary>
        public async Task<IMachineProvider?> CreateAndConnectAsync(
            MachineConnectionSettings settings,
            CancellationToken ct = default)
        {
            IMachineProvider provider = settings.ProviderType switch
            {
                "EOS"  => _sp.GetRequiredService<EosMachineProvider>(),
                "Mock" => _sp.GetRequiredService<MockMachineProvider>(),
                _ => throw new NotSupportedException(
                    $"Provider type '{settings.ProviderType}' is not registered. " +
                    $"Supported types: EOS, Mock")
            };

            var connected = await provider.ConnectAsync(settings, ct);
            if (!connected)
            {
                _logger.LogWarning(
                    "Provider {Type} failed to connect for machine {MachineId}",
                    settings.ProviderType, settings.MachineId);
            }

            return provider;
        }
    }
}
