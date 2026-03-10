using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Hubs;

/// <summary>
/// Interface for notifying connected clients of machine state changes.
/// Injected into MachineSyncService to broadcast updates via SignalR.
/// </summary>
public interface IMachineStateNotifier
{
    /// <summary>
    /// Notify clients subscribed to a specific machine
    /// </summary>
    Task NotifyMachineStateAsync(MachineStateUpdate update, CancellationToken ct = default);

    /// <summary>
    /// Notify all clients subscribed to any machine updates
    /// </summary>
    Task NotifyAllMachinesAsync(MachineStateUpdate update, CancellationToken ct = default);

    /// <summary>
    /// Broadcast a batch of machine updates
    /// </summary>
    Task NotifyBatchAsync(IEnumerable<MachineStateUpdate> updates, CancellationToken ct = default);

    /// <summary>
    /// Notify clients of a machine alarm
    /// </summary>
    Task NotifyAlarmAsync(int machineId, string machineCode, string alarm, string severity, CancellationToken ct = default);

    /// <summary>
    /// Notify clients that a machine went offline
    /// </summary>
    Task NotifyMachineOfflineAsync(int machineId, string machineCode, string? error = null, CancellationToken ct = default);

    /// <summary>
    /// Notify clients that a machine came online
    /// </summary>
    Task NotifyMachineOnlineAsync(int machineId, string machineCode, CancellationToken ct = default);
}
