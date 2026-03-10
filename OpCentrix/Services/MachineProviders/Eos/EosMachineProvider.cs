using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos
{
    /// <summary>
    /// IMachineProvider implementation for the EOS M4 Onyx.
    ///
    /// Data strategy:
    ///   1. Try OPC UA first (lowest latency, real-time sensor data).
    ///   2. Fall back to EOSCONNECT REST API if OPC UA is unavailable.
    ///   3. Return null / empty when both are unreachable — sync service
    ///      will record the failure and skip this poll cycle.
    ///
    /// Job control (start/pause/resume) is routed exclusively through
    /// OPC UA because the REST API is read-only.
    /// </summary>
    public sealed class EosMachineProvider : IMachineProvider
    {
        private readonly EosRestClient _rest;
        private readonly EosOpcUaClient _opcUa;
        private readonly ILogger<EosMachineProvider> _logger;

        private MachineConnectionSettings? _settings;

        public string ProviderType => "EOS";

        public bool SupportsJobControl =>
            _settings?.JobControlEnabled == true && _opcUa.IsConnected;

        private static readonly IReadOnlyList<string> _supportedMaterials = new[]
        {
            "Ti-6Al-4V Grade 5",
            "Ti-6Al-4V ELI Grade 23",
            "Inconel 718",
            "Inconel 625",
            "316L Stainless Steel"
        };

        public EosMachineProvider(
            EosRestClient rest,
            EosOpcUaClient opcUa,
            ILogger<EosMachineProvider> logger)
        {
            _rest = rest;
            _opcUa = opcUa;
            _logger = logger;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public async Task<bool> ConnectAsync(MachineConnectionSettings settings, CancellationToken ct = default)
        {
            _settings = settings;

            // Configure REST client if credentials are present
            if (!string.IsNullOrEmpty(settings.RestApiBaseUrl) &&
                !string.IsNullOrEmpty(settings.OAuthClientId))
            {
                _rest.Configure(
                    settings.RestApiBaseUrl,
                    settings.OAuthClientId,
                    settings.OAuthClientSecretEncrypted ?? string.Empty);

                var tokenOk = await _rest.EnsureTokenAsync(ct);
                _logger.LogInformation("[EOS] REST client configured for {MachineId}, token={Ok}",
                    settings.MachineId, tokenOk);
            }

            // Configure OPC UA client if endpoint is present
            if (!string.IsNullOrEmpty(settings.OpcUaEndpointUrl))
            {
                _opcUa.Configure(
                    settings.OpcUaEndpointUrl,
                    settings.OpcUaUsername ?? string.Empty,
                    settings.OpcUaPasswordHash ?? string.Empty,
                    settings.JobControlEnabled);

                var opcOk = await _opcUa.ConnectAsync(ct);
                _logger.LogInformation("[EOS] OPC UA configured for {MachineId}, connected={Ok}",
                    settings.MachineId, opcOk);
            }

            return true;
        }

        public async Task DisconnectAsync(CancellationToken ct = default)
        {
            await _opcUa.DisconnectAsync(ct);
            _logger.LogInformation("[EOS] Disconnected from {MachineId}", _settings?.MachineId);
        }

        public Task<bool> IsConnectedAsync(CancellationToken ct = default) =>
            Task.FromResult(true); // REST client has no persistent connection — always "up"

        // ── Capabilities ──────────────────────────────────────────────────────

        public Task<MachineCapabilities?> GetCapabilitiesAsync(CancellationToken ct = default)
        {
            // EOS M4 Onyx fixed specifications.
            // TODO: Source serial number and firmware version from REST /system endpoint.
            var caps = new MachineCapabilities(
                MachineId: _settings?.MachineId ?? "EOS",
                ModelName: "EOS M4 Onyx",
                SerialNumber: "UNKNOWN",
                FirmwareVersion: "UNKNOWN",
                BuildLengthMm: 450,
                BuildWidthMm: 450,
                BuildHeightMm: 400,
                LaserCount: 6,
                MaxTotalLaserPowerW: 2400,
                SupportedMaterials: _supportedMaterials,
                SupportsJobControl: _settings?.JobControlEnabled ?? false);

            return Task.FromResult<MachineCapabilities?>(caps);
        }

        // ── Real-time state ───────────────────────────────────────────────────

        public async Task<MachineStatus> GetStatusAsync(CancellationToken ct = default)
        {
            var machineId = _settings?.MachineId ?? "EOS";
            var now = DateTime.UtcNow;

            // 1. Try OPC UA (real-time)
            if (_opcUa.IsConnected)
            {
                var telemetryData = await _opcUa.ReadTelemetryAsync(ct);
                if (telemetryData != null)
                    return MapOpcUaToStatus(machineId, telemetryData, now);
            }

            // 2. Fall back to REST
            var restStatus = await _rest.GetMachineStatusAsync(ct);
            var restSensors = await _rest.GetSensorDataAsync(ct);

            if (restStatus != null)
                return MapRestToStatus(machineId, restStatus, restSensors, now);

            // 3. Both unavailable — return offline status
            _logger.LogWarning("[EOS] No data source available for {MachineId}", machineId);
            return new MachineStatus(
                MachineId: machineId,
                ProviderType: ProviderType,
                State: MachineState.Offline,
                BuildProgressPercent: 0,
                EstimatedCompletion: null,
                Telemetry: EmptyTelemetry(),
                ActiveJobId: null,
                ActiveJobName: null,
                Timestamp: now);
        }

        public async Task<IReadOnlyList<MachineAlert>> GetAlertsAsync(CancellationToken ct = default)
        {
            // TODO: Map system messages from REST /status/messages endpoint to MachineAlert list.
            // For now return empty until the REST client is wired.
            await Task.CompletedTask;
            return [];
        }

        // ── Job control ───────────────────────────────────────────────────────

        public async Task<bool> SendJobAsync(ProviderJobRequest job, CancellationToken ct = default)
        {
            // TODO: Upload the job file via REST or network share, then queue via OPC UA.
            _logger.LogInformation("[EOS] SendJob '{JobName}' to {MachineId} — stubbed",
                job.JobName, _settings?.MachineId);
            await Task.CompletedTask;
            return false;
        }

        public Task<bool> StartJobAsync(string externalJobId, CancellationToken ct = default) =>
            _opcUa.StartJobAsync(externalJobId, ct);

        public Task<bool> PauseJobAsync(string externalJobId, CancellationToken ct = default) =>
            _opcUa.PauseJobAsync(externalJobId, ct);

        public Task<bool> ResumeJobAsync(string externalJobId, CancellationToken ct = default) =>
            _opcUa.ResumeJobAsync(externalJobId, ct);

        // ── Historical data ───────────────────────────────────────────────────

        public async Task<IReadOnlyList<CompletedJobRecord>> GetCompletedJobsAsync(
            DateTime from, DateTime to, CancellationToken ct = default)
        {
            var eosJobs = await _rest.GetJobsAsync(ct);

            return eosJobs
                .Where(j => j.EndTime >= from && j.EndTime <= to && j.EndTime != null)
                .Select(j => new CompletedJobRecord(
                    ExternalJobId: j.JobId ?? string.Empty,
                    JobName: j.JobName ?? string.Empty,
                    StartTime: j.StartTime ?? DateTime.MinValue,
                    EndTime: j.EndTime!.Value,
                    Material: j.Material ?? string.Empty,
                    ActualHours: j.StartTime.HasValue
                        ? (j.EndTime.Value - j.StartTime.Value).TotalHours
                        : j.EstimatedDurationHours ?? 0,
                    Success: j.Success ?? false,
                    FailureReason: j.FailureReason))
                .ToList();
        }

        public async Task<ProcessParameters?> GetJobProcessParametersAsync(
            string externalJobId, CancellationToken ct = default)
        {
            var dto = await _rest.GetJobProcessParametersAsync(externalJobId, ct);
            if (dto == null) return null;

            return new ProcessParameters(
                ExternalJobId: externalJobId,
                LaserPowerW: dto.LaserPowerW ?? 0,
                ScanSpeedMmPerSec: dto.ScanSpeedMmPerSec ?? 0,
                LayerThicknessMicrons: dto.LayerThicknessMicrons ?? 0,
                ParameterSetName: dto.ParameterSetName ?? string.Empty,
                Material: dto.Material ?? string.Empty);
        }

        public async ValueTask DisposeAsync()
        {
            await _opcUa.DisposeAsync();
            await _rest.DisposeAsync();
        }

        // ── Mapping helpers ───────────────────────────────────────────────────

        private MachineStatus MapOpcUaToStatus(
            string machineId, EosOpcUaTelemetry data, DateTime now)
        {
            var state = ParseMachineState(data.MachineState);
            return new MachineStatus(
                MachineId: machineId,
                ProviderType: ProviderType,
                State: state,
                BuildProgressPercent: data.BuildProgressPercent,
                EstimatedCompletion: null, // TODO: Calculate from layer rate
                Telemetry: new TelemetryReading(
                    BuildTempC: data.BuildChamberTempC,
                    OxygenPpm: data.OxygenContentPpm,
                    ArgonFlowLpm: data.ArgonFlowRateLpm,
                    LaserPowerW: data.LaserPowerW,
                    PowderLevelPercent: data.PowderLevelPercent,
                    LayersCompleted: data.CurrentLayer,
                    LayersTotal: data.TotalLayers,
                    ActiveLaserCount: state == MachineState.Building ? 6 : 0),
                ActiveJobId: data.ActiveJobId,
                ActiveJobName: null, // OPC UA doesn't return a human-readable name
                Timestamp: now);
        }

        private MachineStatus MapRestToStatus(
            string machineId,
            EosMachineStatusDto status,
            EosSensorDataDto? sensors,
            DateTime now)
        {
            var state = ParseMachineState(status.MachineState);
            return new MachineStatus(
                MachineId: machineId,
                ProviderType: ProviderType,
                State: state,
                BuildProgressPercent: status.BuildProgressPercent ?? 0,
                EstimatedCompletion: status.EstimatedJobEnd,
                Telemetry: new TelemetryReading(
                    BuildTempC: sensors?.BuildChamberTempC ?? 0,
                    OxygenPpm: sensors?.OxygenContentPpm ?? 0,
                    ArgonFlowLpm: sensors?.ArgonFlowRateLpm ?? 0,
                    LaserPowerW: sensors?.LaserPowerW ?? 0,
                    PowderLevelPercent: sensors?.PowderLevelPercent ?? 0,
                    LayersCompleted: sensors?.CurrentLayer ?? 0,
                    LayersTotal: sensors?.TotalLayers ?? 0,
                    ActiveLaserCount: state == MachineState.Building ? 6 : 0),
                ActiveJobId: status.ActiveJobId,
                ActiveJobName: null,
                Timestamp: now);
        }

        private static MachineState ParseMachineState(string? raw) => raw?.ToLower() switch
        {
            "idle"        => MachineState.Idle,
            "building"    => MachineState.Building,
            "preheating"  => MachineState.Preheating,
            "cooling"     => MachineState.Cooling,
            "paused"      => MachineState.Paused,
            "maintenance" => MachineState.Maintenance,
            "error"       => MachineState.Error,
            "offline"     => MachineState.Offline,
            _             => MachineState.Unknown
        };

        private static TelemetryReading EmptyTelemetry() =>
            new(0, 0, 0, 0, 0, 0, 0, 0);
    }
}
