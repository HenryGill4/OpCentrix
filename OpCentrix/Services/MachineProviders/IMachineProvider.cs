using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders;

/// <summary>
/// Interface for machine communication providers.
/// Implemented by MockMachineProvider, EosMachineProvider, GenericOpcUaProvider, etc.
/// </summary>
public interface IMachineProvider
{
    /// <summary>
    /// The provider type identifier (e.g., "Mock", "Eos", "GenericOpcUa")
    /// </summary>
    string ProviderType { get; }

    /// <summary>
    /// Get the current status of a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Current machine status</returns>
    Task<MachineStatus> GetStatusAsync(int machineId, CancellationToken ct = default);

    /// <summary>
    /// Get full telemetry data from a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Full machine data including telemetry</returns>
    Task<MachineData> GetMachineDataAsync(int machineId, CancellationToken ct = default);

    /// <summary>
    /// Send a command to a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="command">Command to send</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Result of the command</returns>
    Task<MachineCommandResult> SendCommandAsync(int machineId, MachineCommand command, CancellationToken ct = default);

    /// <summary>
    /// Get active alarms from a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of active alarm messages</returns>
    Task<List<string>> GetAlarmsAsync(int machineId, CancellationToken ct = default);

    /// <summary>
    /// Test connection to a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>True if connection successful</returns>
    Task<bool> TestConnectionAsync(int machineId, CancellationToken ct = default);

    /// <summary>
    /// Initialize the provider with connection settings
    /// Called once when provider is first used for a machine
    /// </summary>
    /// <param name="settings">Connection settings for the machine</param>
    /// <param name="ct">Cancellation token</param>
    Task InitializeAsync(MachineConnectionSettings settings, CancellationToken ct = default);

    /// <summary>
    /// Dispose of any connections or resources for a machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    Task DisposeConnectionAsync(int machineId);
}

/// <summary>
/// Factory for creating and managing machine providers
/// </summary>
public interface IMachineProviderFactory
{
    /// <summary>
    /// Get a provider instance by type name
    /// </summary>
    /// <param name="providerType">Provider type: "Mock", "Eos", "GenericOpcUa"</param>
    /// <returns>Provider instance or null if type not found</returns>
    IMachineProvider? GetProvider(string providerType);

    /// <summary>
    /// Get the configured provider for a specific machine
    /// </summary>
    /// <param name="machineId">Machine.Id (int)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Configured provider or fallback to Mock</returns>
    Task<IMachineProvider> GetProviderForMachineAsync(int machineId, CancellationToken ct = default);

    /// <summary>
    /// Get all registered provider types
    /// </summary>
    /// <returns>List of available provider type names</returns>
    IReadOnlyList<string> GetAvailableProviderTypes();
}
