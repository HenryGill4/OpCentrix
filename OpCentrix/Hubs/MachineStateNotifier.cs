using Microsoft.AspNetCore.SignalR;

namespace OpCentrix.Hubs;

/// <summary>
/// Implementation of IMachineStateNotifier using SignalR hub context.
/// Broadcasts machine state changes to connected clients.
/// </summary>
public class MachineStateNotifier : IMachineStateNotifier
{
    private readonly IHubContext<MachineStateHub> _hubContext;
    private readonly ILogger<MachineStateNotifier> _logger;

    public MachineStateNotifier(
        IHubContext<MachineStateHub> hubContext,
        ILogger<MachineStateNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyMachineStateAsync(MachineStateUpdate update, CancellationToken ct = default)
    {
        var groupName = MachineStateHub.GetMachineGroupName(update.MachineId);

        try
        {
            // Send to specific machine subscribers
            await _hubContext.Clients.Group(groupName)
                .SendAsync("MachineStateChanged", update, ct);

            // Also send to "all machines" subscribers
            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineStateChanged", update, ct);

            if (update.IsStateChange)
            {
                _logger.LogInformation("[NOTIFIER] State change for {MachineCode}: {Previous} ? {Current}",
                    update.MachineCode, update.PreviousStatus, update.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to send state update for machine {MachineId}", update.MachineId);
        }
    }

    public async Task NotifyAllMachinesAsync(MachineStateUpdate update, CancellationToken ct = default)
    {
        try
        {
            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineStateChanged", update, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to broadcast to AllMachines");
        }
    }

    public async Task NotifyBatchAsync(IEnumerable<MachineStateUpdate> updates, CancellationToken ct = default)
    {
        try
        {
            var updateList = updates.ToList();
            
            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineStatesBatch", updateList, ct);

            _logger.LogDebug("[NOTIFIER] Sent batch update with {Count} machines", updateList.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to send batch update");
        }
    }

    public async Task NotifyAlarmAsync(int machineId, string machineCode, string alarm, string severity, CancellationToken ct = default)
    {
        var groupName = MachineStateHub.GetMachineGroupName(machineId);
        var alarmData = new
        {
            MachineId = machineId,
            MachineCode = machineCode,
            Alarm = alarm,
            Severity = severity,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync("MachineAlarm", alarmData, ct);

            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineAlarm", alarmData, ct);

            _logger.LogWarning("[NOTIFIER] Alarm on {MachineCode}: [{Severity}] {Alarm}",
                machineCode, severity, alarm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to send alarm for machine {MachineId}", machineId);
        }
    }

    public async Task NotifyMachineOfflineAsync(int machineId, string machineCode, string? error = null, CancellationToken ct = default)
    {
        var groupName = MachineStateHub.GetMachineGroupName(machineId);
        var offlineData = new
        {
            MachineId = machineId,
            MachineCode = machineCode,
            Error = error,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync("MachineOffline", offlineData, ct);

            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineOffline", offlineData, ct);

            _logger.LogWarning("[NOTIFIER] Machine {MachineCode} went offline: {Error}", 
                machineCode, error ?? "No error details");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to send offline notification for machine {MachineId}", machineId);
        }
    }

    public async Task NotifyMachineOnlineAsync(int machineId, string machineCode, CancellationToken ct = default)
    {
        var groupName = MachineStateHub.GetMachineGroupName(machineId);
        var onlineData = new
        {
            MachineId = machineId,
            MachineCode = machineCode,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            await _hubContext.Clients.Group(groupName)
                .SendAsync("MachineOnline", onlineData, ct);

            await _hubContext.Clients.Group("AllMachines")
                .SendAsync("MachineOnline", onlineData, ct);

            _logger.LogInformation("[NOTIFIER] Machine {MachineCode} came online", machineCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[NOTIFIER] Failed to send online notification for machine {MachineId}", machineId);
        }
    }
}
