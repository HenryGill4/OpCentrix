using Microsoft.Extensions.Logging;
using OpCentrix.Models.MachineProviders;
using System.Collections.Concurrent;

namespace OpCentrix.Services.MachineProviders;

/// <summary>
/// Mock machine provider for development and testing.
/// Simulates machine behavior with configurable responses.
/// </summary>
public class MockMachineProvider : IMachineProvider
{
    private readonly ILogger<MockMachineProvider> _logger;
    private readonly ConcurrentDictionary<int, MockMachineState> _machineStates = new();
    private readonly ConcurrentDictionary<int, MachineConnectionSettings> _settings = new();
    private readonly Random _random = new();

    public string ProviderType => "Mock";

    public MockMachineProvider(ILogger<MockMachineProvider> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync(MachineConnectionSettings settings, CancellationToken ct = default)
    {
        _settings[settings.MachineId] = settings;
        
        // Initialize machine state if not exists
        _machineStates.GetOrAdd(settings.MachineId, _ => new MockMachineState
        {
            Status = "Idle",
            BuildProgress = 0,
            IsConnected = true,
            LastUpdate = DateTime.UtcNow
        });

        _logger.LogDebug("[MOCK] Initialized mock provider for machine {MachineId}", settings.MachineId);
        return Task.CompletedTask;
    }

    public Task<MachineStatus> GetStatusAsync(int machineId, CancellationToken ct = default)
    {
        var state = GetOrCreateState(machineId);
        UpdateStateSimulation(machineId, state);

        return Task.FromResult(new MachineStatus
        {
            MachineId = machineId,
            Status = state.Status,
            BuildProgressPercent = state.Status == "Printing" ? state.BuildProgress : null,
            CurrentLayer = state.Status == "Printing" ? (int)(state.BuildProgress * 10) : null,
            TotalLayers = state.Status == "Printing" ? 1000 : null,
            EstimatedMinutesRemaining = state.Status == "Printing" 
                ? (100 - state.BuildProgress) * 6 // ~10 hours total build
                : null,
            CurrentJobReference = state.CurrentJobReference,
            Alarms = state.Alarms.ToList(),
            IsConnected = state.IsConnected,
            Timestamp = DateTime.UtcNow
        });
    }

    public Task<MachineData> GetMachineDataAsync(int machineId, CancellationToken ct = default)
    {
        var state = GetOrCreateState(machineId);
        UpdateStateSimulation(machineId, state);

        var status = new MachineStatus
        {
            MachineId = machineId,
            Status = state.Status,
            BuildProgressPercent = state.Status == "Printing" ? state.BuildProgress : null,
            CurrentLayer = state.Status == "Printing" ? (int)(state.BuildProgress * 10) : null,
            TotalLayers = state.Status == "Printing" ? 1000 : null,
            CurrentJobReference = state.CurrentJobReference,
            Alarms = state.Alarms.ToList(),
            IsConnected = state.IsConnected,
            Timestamp = DateTime.UtcNow
        };

        var telemetry = GenerateMockTelemetry(state);

        return Task.FromResult(new MachineData
        {
            MachineId = machineId,
            Status = status,
            Telemetry = telemetry,
            ProviderType = ProviderType,
            Timestamp = DateTime.UtcNow
        });
    }

    public Task<MachineCommandResult> SendCommandAsync(int machineId, MachineCommand command, CancellationToken ct = default)
    {
        var state = GetOrCreateState(machineId);

        _logger.LogInformation("[MOCK] Received command {CommandType} for machine {MachineId}", 
            command.CommandType, machineId);

        switch (command.CommandType)
        {
            case MachineCommandType.Start:
                state.Status = "Printing";
                state.BuildProgress = 0;
                state.CurrentJobReference = command.JobReference ?? $"MOCK-{DateTime.UtcNow:yyyyMMddHHmmss}";
                break;

            case MachineCommandType.Stop:
                state.Status = "Idle";
                state.BuildProgress = 0;
                state.CurrentJobReference = null;
                break;

            case MachineCommandType.Pause:
                if (state.Status == "Printing")
                    state.Status = "Paused";
                break;

            case MachineCommandType.Resume:
                if (state.Status == "Paused")
                    state.Status = "Printing";
                break;

            case MachineCommandType.Reset:
                state.Status = "Idle";
                state.Alarms.Clear();
                break;

            case MachineCommandType.AcknowledgeAlarm:
                state.Alarms.Clear();
                break;
        }

        state.LastUpdate = DateTime.UtcNow;

        return Task.FromResult(MachineCommandResult.Ok(new Dictionary<string, object>
        {
            ["newStatus"] = state.Status,
            ["timestamp"] = DateTime.UtcNow
        }));
    }

    public Task<List<string>> GetAlarmsAsync(int machineId, CancellationToken ct = default)
    {
        var state = GetOrCreateState(machineId);
        return Task.FromResult(state.Alarms.ToList());
    }

    public Task<bool> TestConnectionAsync(int machineId, CancellationToken ct = default)
    {
        var state = GetOrCreateState(machineId);
        
        // Simulate occasional connection failures (5% chance)
        if (_random.NextDouble() < 0.05)
        {
            _logger.LogDebug("[MOCK] Simulated connection failure for machine {MachineId}", machineId);
            return Task.FromResult(false);
        }

        return Task.FromResult(state.IsConnected);
    }

    public Task DisposeConnectionAsync(int machineId)
    {
        _machineStates.TryRemove(machineId, out _);
        _settings.TryRemove(machineId, out _);
        _logger.LogDebug("[MOCK] Disposed mock connection for machine {MachineId}", machineId);
        return Task.CompletedTask;
    }

    #region Private Methods

    private MockMachineState GetOrCreateState(int machineId)
    {
        return _machineStates.GetOrAdd(machineId, _ => new MockMachineState
        {
            Status = "Idle",
            BuildProgress = 0,
            IsConnected = true,
            LastUpdate = DateTime.UtcNow
        });
    }

    private void UpdateStateSimulation(int machineId, MockMachineState state)
    {
        var now = DateTime.UtcNow;
        var elapsed = (now - state.LastUpdate).TotalSeconds;

        // Get config if available
        var config = _settings.TryGetValue(machineId, out var settings)
            ? settings.GetConnectionConfig<MockConnectionConfig>()
            : null;

        // Progress build if printing
        if (state.Status == "Printing")
        {
            var increment = config?.BuildProgressIncrementPercent ?? 0.5;
            state.BuildProgress += increment * (elapsed / 30.0); // Increment per 30 seconds

            // Complete build at 100%
            if (state.BuildProgress >= 100)
            {
                state.Status = "Cooling";
                state.BuildProgress = 100;
                _logger.LogInformation("[MOCK] Machine {MachineId} build completed, entering cooling", machineId);
            }
        }
        // Transition from Cooling to Idle after ~5 minutes
        else if (state.Status == "Cooling" && elapsed > 300)
        {
            state.Status = "Idle";
            state.BuildProgress = 0;
            state.CurrentJobReference = null;
            _logger.LogInformation("[MOCK] Machine {MachineId} cooling complete, now idle", machineId);
        }

        // Random status changes (for idle machines)
        var randomChangeProbability = config?.RandomStatusChangeProbability ?? 0.05;
        if (state.Status == "Idle" && _random.NextDouble() < randomChangeProbability * (elapsed / 30.0))
        {
            var newStatus = _random.NextDouble() switch
            {
                < 0.3 => "Preheating",
                < 0.5 => "Maintenance",
                < 0.7 => "Error",
                _ => "Idle"
            };

            if (newStatus != state.Status)
            {
                state.Status = newStatus;
                _logger.LogDebug("[MOCK] Machine {MachineId} random status change to {Status}", 
                    machineId, newStatus);

                if (newStatus == "Error")
                {
                    state.Alarms.Add($"Simulated error at {DateTime.UtcNow:HH:mm:ss}");
                }
            }
        }

        // Preheating transitions to Printing after ~2 minutes
        if (state.Status == "Preheating" && elapsed > 120)
        {
            state.Status = "Printing";
            state.BuildProgress = 0;
            state.CurrentJobReference = $"MOCK-AUTO-{DateTime.UtcNow:yyyyMMddHHmmss}";
            _logger.LogInformation("[MOCK] Machine {MachineId} preheat complete, starting print", machineId);
        }

        state.LastUpdate = now;
    }

    private Dictionary<string, object> GenerateMockTelemetry(MockMachineState state)
    {
        var telemetry = new Dictionary<string, object>
        {
            ["status"] = state.Status,
            ["isConnected"] = state.IsConnected,
            ["timestamp"] = DateTime.UtcNow
        };

        // Add status-specific telemetry
        switch (state.Status)
        {
            case "Printing":
                telemetry["buildProgressPercent"] = state.BuildProgress;
                telemetry["currentLayer"] = (int)(state.BuildProgress * 10);
                telemetry["totalLayers"] = 1000;
                telemetry["laserPowerWatts"] = 280 + _random.NextDouble() * 20;
                telemetry["scanSpeedMmPerSec"] = 1200 + _random.NextDouble() * 100;
                telemetry["buildTemperatureCelsius"] = 180 + _random.NextDouble() * 10;
                telemetry["oxygenContentPpm"] = 50 + _random.NextDouble() * 20;
                telemetry["argonFlowRateLpm"] = 12 + _random.NextDouble() * 2;
                break;

            case "Preheating":
                telemetry["buildTemperatureCelsius"] = 50 + _random.NextDouble() * 130;
                telemetry["targetTemperatureCelsius"] = 180;
                telemetry["preheatProgress"] = _random.NextDouble() * 100;
                break;

            case "Cooling":
                telemetry["buildTemperatureCelsius"] = 50 + _random.NextDouble() * 100;
                telemetry["targetTemperatureCelsius"] = 40;
                break;

            case "Idle":
                telemetry["buildTemperatureCelsius"] = 20 + _random.NextDouble() * 10;
                telemetry["chamberPressureBar"] = 1.0 + _random.NextDouble() * 0.1;
                break;

            case "Error":
                telemetry["errorCode"] = $"E{_random.Next(1000, 9999)}";
                telemetry["errorMessage"] = "Simulated error condition";
                break;
        }

        return telemetry;
    }

    #endregion

    /// <summary>
    /// Internal state tracking for mock machines
    /// </summary>
    private class MockMachineState
    {
        public string Status { get; set; } = "Idle";
        public double BuildProgress { get; set; }
        public string? CurrentJobReference { get; set; }
        public bool IsConnected { get; set; } = true;
        public List<string> Alarms { get; set; } = new();
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
    }
}
