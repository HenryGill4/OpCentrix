using Microsoft.Extensions.Logging;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos;

/// <summary>
/// Machine provider for EOS M4 ONIX and compatible EOS machines.
/// Combines REST API and OPC UA for full machine integration.
/// </summary>
public class EosMachineProvider : IMachineProvider
{
    private readonly ILogger<EosMachineProvider> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<int, EosConnectionConfig> _configs = new();
    private readonly Dictionary<int, EosRestClient> _restClients = new();
    private readonly Dictionary<int, EosOpcUaClient> _opcUaClients = new();
    private readonly EosTestDataGenerator _testDataGenerator;

    public string ProviderType => "Eos";

    public EosMachineProvider(ILogger<EosMachineProvider> logger, ILoggerFactory loggerFactory)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _testDataGenerator = new EosTestDataGenerator();
    }

    public Task InitializeAsync(MachineConnectionSettings settings, CancellationToken ct = default)
    {
        var config = settings.GetConnectionConfig<EosConnectionConfig>()
                     ?? EosConnectionConfig.CreateTestConfig();

        _configs[settings.MachineId] = config;

        _logger.LogInformation("[EOS] Initialized provider for machine {MachineId}, model: {Model}",
            settings.MachineId, config.MachineModel);

        return Task.CompletedTask;
    }

    public async Task<MachineStatus> GetStatusAsync(int machineId, CancellationToken ct = default)
    {
        var config = GetConfig(machineId);
        var data = await GetMachineDataAsync(machineId, ct);

        return data.Status;
    }

    public async Task<MachineData> GetMachineDataAsync(int machineId, CancellationToken ct = default)
    {
        var config = GetConfig(machineId);

        // Try REST API first if configured
        if (!string.IsNullOrWhiteSpace(config.RestApiBaseUrl) && config.PreferRestApi)
        {
            try
            {
                return await GetDataFromRestAsync(machineId, config, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EOS] REST API failed for machine {MachineId}, trying OPC UA", machineId);
            }
        }

        // Try OPC UA
        if (!string.IsNullOrWhiteSpace(config.OpcUaEndpointUrl))
        {
            try
            {
                return await GetDataFromOpcUaAsync(machineId, config, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[EOS] OPC UA failed for machine {MachineId}", machineId);
            }
        }

        // Fallback to test data if configured
        if (config.FallbackToTestData)
        {
            _logger.LogDebug("[EOS] Using test data for machine {MachineId}", machineId);
            return _testDataGenerator.GenerateMachineData(machineId);
        }

        // Return offline status
        return new MachineData
        {
            MachineId = machineId,
            Status = new MachineStatus
            {
                MachineId = machineId,
                Status = "Offline",
                IsConnected = false,
                Timestamp = DateTime.UtcNow
            },
            ProviderType = ProviderType,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<MachineData> GetDataFromRestAsync(int machineId, EosConnectionConfig config, CancellationToken ct)
    {
        var client = GetOrCreateRestClient(machineId, config);

        var statusTask = client.GetMachineStatusAsync(ct);
        var buildTask = client.GetBuildStatusAsync(ct);
        var alarmsTask = client.GetActiveAlarmsAsync(ct);

        await Task.WhenAll(statusTask, buildTask, alarmsTask);

        var status = await statusTask;
        var build = await buildTask;
        var alarms = await alarmsTask;

        return new MachineData
        {
            MachineId = machineId,
            Status = new MachineStatus
            {
                MachineId = machineId,
                Status = EosStateMapper.ToStatusString(EosStateMapper.FromStatusString(status?.State ?? "Unknown")),
                IsConnected = true,
                BuildProgressPercent = build?.Progress,
                CurrentLayer = build?.CurrentLayer,
                TotalLayers = build?.TotalLayers,
                CurrentJobReference = build?.JobName,
                EstimatedMinutesRemaining = build?.EstimatedRemainingSeconds / 60.0,
                Alarms = alarms.Select(a => a.Message).ToList(),
                Timestamp = DateTime.UtcNow
            },
            Telemetry = new Dictionary<string, object>
            {
                ["isRunning"] = status?.IsRunning ?? false,
                ["hasError"] = status?.HasError ?? false,
                ["currentLayer"] = build?.CurrentLayer ?? 0,
                ["totalLayers"] = build?.TotalLayers ?? 0,
                ["alarmCount"] = alarms.Count
            },
            ProviderType = ProviderType,
            Timestamp = DateTime.UtcNow
        };
    }

    private async Task<MachineData> GetDataFromOpcUaAsync(int machineId, EosConnectionConfig config, CancellationToken ct)
    {
        var client = GetOrCreateOpcUaClient(machineId, config);

        if (!client.IsConnected)
        {
            await client.ConnectAsync(ct);
        }

        var telemetry = await client.GetTelemetrySnapshotAsync(ct);

        return new MachineData
        {
            MachineId = machineId,
            Status = new MachineStatus
            {
                MachineId = machineId,
                Status = telemetry.GetStatusString(),
                IsConnected = telemetry.IsConnected,
                BuildProgressPercent = telemetry.BuildProgress,
                CurrentLayer = telemetry.CurrentLayer,
                TotalLayers = telemetry.TotalLayers,
                Timestamp = telemetry.Timestamp
            },
            Telemetry = new Dictionary<string, object>
            {
                ["isRunning"] = telemetry.IsRunning,
                ["laserPower"] = telemetry.LaserPower,
                ["buildPlatformTemp"] = telemetry.BuildPlatformTemp,
                ["oxygenContent"] = telemetry.OxygenContent,
                ["argonFlowRate"] = telemetry.ArgonFlowRate
            },
            ProviderType = ProviderType,
            Timestamp = telemetry.Timestamp
        };
    }

    public async Task<MachineCommandResult> SendCommandAsync(int machineId, MachineCommand command, CancellationToken ct = default)
    {
        _logger.LogInformation("[EOS] Command {Type} for machine {MachineId}", command.CommandType, machineId);

        // Commands would go through REST API
        var config = GetConfig(machineId);

        if (string.IsNullOrWhiteSpace(config.RestApiBaseUrl))
        {
            return MachineCommandResult.Fail("REST API not configured for commands");
        }

        // TODO: Implement actual command sending via EosRestClient
        return MachineCommandResult.Ok();
    }

    public async Task<List<string>> GetAlarmsAsync(int machineId, CancellationToken ct = default)
    {
        var config = GetConfig(machineId);

        if (!string.IsNullOrWhiteSpace(config.RestApiBaseUrl))
        {
            var client = GetOrCreateRestClient(machineId, config);
            var alarms = await client.GetActiveAlarmsAsync(ct);
            return alarms.Select(a => a.Message).ToList();
        }

        return new List<string>();
    }

    public async Task<bool> TestConnectionAsync(int machineId, CancellationToken ct = default)
    {
        var config = GetConfig(machineId);

        // Test REST
        if (!string.IsNullOrWhiteSpace(config.RestApiBaseUrl))
        {
            var client = GetOrCreateRestClient(machineId, config);
            if (await client.TestConnectionAsync(ct))
                return true;
        }

        // Test OPC UA
        if (!string.IsNullOrWhiteSpace(config.OpcUaEndpointUrl))
        {
            var client = GetOrCreateOpcUaClient(machineId, config);
            if (await client.ConnectAsync(ct))
                return true;
        }

        return config.FallbackToTestData;
    }

    public async Task DisposeConnectionAsync(int machineId)
    {
        if (_restClients.TryGetValue(machineId, out var restClient))
        {
            restClient.Dispose();
            _restClients.Remove(machineId);
        }

        if (_opcUaClients.TryGetValue(machineId, out var opcUaClient))
        {
            await opcUaClient.DisconnectAsync();
            opcUaClient.Dispose();
            _opcUaClients.Remove(machineId);
        }

        _configs.Remove(machineId);
        _logger.LogInformation("[EOS] Disposed connections for machine {MachineId}", machineId);
    }

    private EosConnectionConfig GetConfig(int machineId)
    {
        if (!_configs.TryGetValue(machineId, out var config))
        {
            config = EosConnectionConfig.CreateTestConfig();
            _configs[machineId] = config;
        }
        return config;
    }

    private EosRestClient GetOrCreateRestClient(int machineId, EosConnectionConfig config)
    {
        if (!_restClients.TryGetValue(machineId, out var client))
        {
            client = new EosRestClient(config, _loggerFactory.CreateLogger<EosRestClient>());
            _restClients[machineId] = client;
        }
        return client;
    }

    private EosOpcUaClient GetOrCreateOpcUaClient(int machineId, EosConnectionConfig config)
    {
        if (!_opcUaClients.TryGetValue(machineId, out var client))
        {
            client = new EosOpcUaClient(config, _loggerFactory.CreateLogger<EosOpcUaClient>());
            _opcUaClients[machineId] = client;
        }
        return client;
    }
}
