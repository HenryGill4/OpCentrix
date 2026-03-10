using Microsoft.AspNetCore.SignalR;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Hubs;

/// <summary>
/// SignalR hub for real-time machine state updates.
/// Clients can subscribe to specific machines or all machines.
/// </summary>
public class MachineStateHub : Hub
{
    private readonly ILogger<MachineStateHub> _logger;

    public MachineStateHub(ILogger<MachineStateHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to updates for a specific machine
    /// </summary>
    public async Task JoinMachineGroup(int machineId)
    {
        var groupName = GetMachineGroupName(machineId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("[HUB] Client {ConnectionId} joined machine group {MachineId}", 
            Context.ConnectionId, machineId);
    }

    /// <summary>
    /// Unsubscribe from a specific machine
    /// </summary>
    public async Task LeaveMachineGroup(int machineId)
    {
        var groupName = GetMachineGroupName(machineId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        _logger.LogDebug("[HUB] Client {ConnectionId} left machine group {MachineId}", 
            Context.ConnectionId, machineId);
    }

    /// <summary>
    /// Subscribe to all machine updates
    /// </summary>
    public async Task JoinAllMachines()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "AllMachines");
        _logger.LogDebug("[HUB] Client {ConnectionId} joined AllMachines group", Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from all machine updates
    /// </summary>
    public async Task LeaveAllMachines()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AllMachines");
        _logger.LogDebug("[HUB] Client {ConnectionId} left AllMachines group", Context.ConnectionId);
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("[HUB] Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "[HUB] Client disconnected with error: {ConnectionId}", 
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("[HUB] Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }

    internal static string GetMachineGroupName(int machineId) => $"Machine_{machineId}";
}

/// <summary>
/// DTO for machine state updates sent to clients
/// </summary>
public class MachineStateUpdate
{
    public int MachineId { get; init; }
    public string MachineCode { get; init; } = "";
    public string Status { get; init; } = "Unknown";
    public double? BuildProgressPercent { get; init; }
    public int? CurrentLayer { get; init; }
    public int? TotalLayers { get; init; }
    public double? EstimatedMinutesRemaining { get; init; }
    public string? CurrentJobReference { get; init; }
    public List<string> Alarms { get; init; } = new();
    public bool IsConnected { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public bool IsStateChange { get; init; }
    public string? PreviousStatus { get; init; }

    public static MachineStateUpdate FromMachineStatus(MachineStatus status, string machineCode, bool isStateChange = false, string? previousStatus = null)
    {
        return new MachineStateUpdate
        {
            MachineId = status.MachineId,
            MachineCode = machineCode,
            Status = status.Status,
            BuildProgressPercent = status.BuildProgressPercent,
            CurrentLayer = status.CurrentLayer,
            TotalLayers = status.TotalLayers,
            EstimatedMinutesRemaining = status.EstimatedMinutesRemaining,
            CurrentJobReference = status.CurrentJobReference,
            Alarms = status.Alarms,
            IsConnected = status.IsConnected,
            Timestamp = status.Timestamp,
            IsStateChange = isStateChange,
            PreviousStatus = previousStatus
        };
    }
}
