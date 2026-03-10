using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders
{
    /// <summary>
    /// Deterministic mock provider simulating an EOS M4 Onyx machine.
    /// Returns realistic data based on elapsed time — no hardware required.
    ///
    /// Build cycle simulated:
    ///   0–5 min   → Preheating
    ///   5–245 min → Building  (progress 0–100%)
    ///   245–285 min → Cooling
    ///   285+ min  → Idle (cycle restarts after 300 min)
    /// </summary>
    public class MockMachineProvider : IMachineProvider
    {
        private readonly ILogger<MockMachineProvider> _logger;

        // Simulated build cycle duration in minutes
        private const int CycleDurationMinutes = 300;
        private const int PreheatMinutes = 5;
        private const int BuildMinutes = 240;
        private const int CoolMinutes = 40;

        private bool _connected = false;
        private MachineConnectionSettings? _settings;

        // Deterministic base time so all machines don't appear in sync
        private static readonly DateTime _epoch = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public string ProviderType => "Mock";
        public bool SupportsJobControl => true;

        private static readonly IReadOnlyList<string> _supportedMaterials = new[]
        {
            "Ti-6Al-4V Grade 5",
            "Ti-6Al-4V ELI Grade 23",
            "Inconel 718",
            "Inconel 625",
            "316L Stainless Steel"
        };

        private static readonly string[] _mockJobNames =
        [
            "Turbine Bracket Assembly",
            "Hip Implant Prototype",
            "Aerospace Duct Section",
            "Heat Exchanger Core",
            "Surgical Instrument Set"
        ];

        public MockMachineProvider(ILogger<MockMachineProvider> logger)
        {
            _logger = logger;
        }

        public Task<bool> ConnectAsync(MachineConnectionSettings settings, CancellationToken ct = default)
        {
            _settings = settings;
            _connected = true;
            _logger.LogInformation("[Mock] Connected to machine {MachineId}", settings.MachineId);
            return Task.FromResult(true);
        }

        public Task DisconnectAsync(CancellationToken ct = default)
        {
            _connected = false;
            _logger.LogInformation("[Mock] Disconnected from machine {MachineId}", _settings?.MachineId);
            return Task.CompletedTask;
        }

        public Task<bool> IsConnectedAsync(CancellationToken ct = default) =>
            Task.FromResult(_connected);

        public Task<MachineCapabilities?> GetCapabilitiesAsync(CancellationToken ct = default)
        {
            var caps = new MachineCapabilities(
                MachineId: _settings?.MachineId ?? "MOCK",
                ModelName: "EOS M4 Onyx",
                SerialNumber: "M4OX-MOCK-0001",
                FirmwareVersion: "4.1.0-mock",
                BuildLengthMm: 450,
                BuildWidthMm: 450,
                BuildHeightMm: 400,
                LaserCount: 6,
                MaxTotalLaserPowerW: 2400,   // 6 × 400 W
                SupportedMaterials: _supportedMaterials,
                SupportsJobControl: true);

            return Task.FromResult<MachineCapabilities?>(caps);
        }

        public Task<MachineStatus> GetStatusAsync(CancellationToken ct = default)
        {
            var machineId = _settings?.MachineId ?? "MOCK";

            // Use a machine-specific offset so multiple mock machines have varied states
            var offset = machineId.GetHashCode() % CycleDurationMinutes;
            var minutesInCycle = (int)(DateTime.UtcNow - _epoch).TotalMinutes % CycleDurationMinutes;
            minutesInCycle = (minutesInCycle + Math.Abs(offset)) % CycleDurationMinutes;

            var (state, progress, eta) = GetCycleState(minutesInCycle, DateTime.UtcNow);
            var jobIndex = (machineId.GetHashCode() & 0x7FFFFFFF) % _mockJobNames.Length;
            var activeJob = state is MachineState.Building or MachineState.Preheating or MachineState.Cooling;

            var status = new MachineStatus(
                MachineId: machineId,
                ProviderType: ProviderType,
                State: state,
                BuildProgressPercent: progress,
                EstimatedCompletion: eta,
                Telemetry: GetTelemetry(state, progress),
                ActiveJobId: activeJob ? $"MOCK-JOB-{jobIndex:D4}" : null,
                ActiveJobName: activeJob ? _mockJobNames[jobIndex] : null,
                Timestamp: DateTime.UtcNow);

            return Task.FromResult(status);
        }

        public Task<IReadOnlyList<MachineAlert>> GetAlertsAsync(CancellationToken ct = default)
        {
            // Deterministically return a low-powder warning for one machine
            var machineId = _settings?.MachineId ?? "MOCK";
            IReadOnlyList<MachineAlert> alerts = [];

            if (machineId.EndsWith("1"))
            {
                alerts = new[]
                {
                    new MachineAlert(
                        AlertSeverity.Warning,
                        "POWDER_LOW",
                        "Powder level below 20% — schedule refill before next build.",
                        DateTime.UtcNow.AddHours(-1))
                };
            }

            return Task.FromResult(alerts);
        }

        public Task<bool> SendJobAsync(ProviderJobRequest job, CancellationToken ct = default)
        {
            _logger.LogInformation("[Mock] Job '{JobName}' queued on {MachineId}", job.JobName, _settings?.MachineId);
            return Task.FromResult(true);
        }

        public Task<bool> StartJobAsync(string externalJobId, CancellationToken ct = default)
        {
            _logger.LogInformation("[Mock] StartJob {JobId} on {MachineId}", externalJobId, _settings?.MachineId);
            return Task.FromResult(true);
        }

        public Task<bool> PauseJobAsync(string externalJobId, CancellationToken ct = default)
        {
            _logger.LogInformation("[Mock] PauseJob {JobId} on {MachineId}", externalJobId, _settings?.MachineId);
            return Task.FromResult(true);
        }

        public Task<bool> ResumeJobAsync(string externalJobId, CancellationToken ct = default)
        {
            _logger.LogInformation("[Mock] ResumeJob {JobId} on {MachineId}", externalJobId, _settings?.MachineId);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<CompletedJobRecord>> GetCompletedJobsAsync(
            DateTime from, DateTime to, CancellationToken ct = default)
        {
            // Generate realistic history seeded from the date range
            var records = new List<CompletedJobRecord>();
            var cursor = from;
            var rng = new Random((int)from.Ticks);

            while (cursor < to)
            {
                var duration = TimeSpan.FromHours(rng.Next(4, 10));
                var end = cursor + duration;
                if (end > to) break;

                var jobIndex = rng.Next(_mockJobNames.Length);
                records.Add(new CompletedJobRecord(
                    ExternalJobId: $"MOCK-{cursor:yyyyMMddHHmm}",
                    JobName: _mockJobNames[jobIndex],
                    StartTime: cursor,
                    EndTime: end,
                    Material: _supportedMaterials[rng.Next(_supportedMaterials.Count)],
                    ActualHours: duration.TotalHours,
                    Success: rng.Next(10) > 0));

                cursor = end + TimeSpan.FromMinutes(rng.Next(30, 120));
            }

            return Task.FromResult<IReadOnlyList<CompletedJobRecord>>(records);
        }

        public Task<ProcessParameters?> GetJobProcessParametersAsync(
            string externalJobId, CancellationToken ct = default)
        {
            var p = new ProcessParameters(
                ExternalJobId: externalJobId,
                LaserPowerW: 340,
                ScanSpeedMmPerSec: 1200,
                LayerThicknessMicrons: 60,
                ParameterSetName: "Ti64-60um-v2.1",
                Material: "Ti-6Al-4V Grade 5");

            return Task.FromResult<ProcessParameters?>(p);
        }

        public ValueTask DisposeAsync()
        {
            _connected = false;
            return ValueTask.CompletedTask;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static (MachineState state, double progress, DateTime? eta) GetCycleState(
            int minutesInCycle, DateTime now)
        {
            if (minutesInCycle < PreheatMinutes)
                return (MachineState.Preheating, 0, now.AddMinutes(PreheatMinutes - minutesInCycle + BuildMinutes + CoolMinutes));

            var buildMinute = minutesInCycle - PreheatMinutes;
            if (buildMinute < BuildMinutes)
            {
                var progress = Math.Round((double)buildMinute / BuildMinutes * 100, 1);
                var eta = now.AddMinutes(BuildMinutes - buildMinute + CoolMinutes);
                return (MachineState.Building, progress, eta);
            }

            var coolMinute = minutesInCycle - PreheatMinutes - BuildMinutes;
            if (coolMinute < CoolMinutes)
                return (MachineState.Cooling, 100, now.AddMinutes(CoolMinutes - coolMinute));

            return (MachineState.Idle, 0, null);
        }

        private static TelemetryReading GetTelemetry(MachineState state, double progress)
        {
            return state switch
            {
                MachineState.Preheating => new TelemetryReading(
                    BuildTempC: 80 + progress * 1.2,
                    OxygenPpm: 800,
                    ArgonFlowLpm: 20,
                    LaserPowerW: 0,
                    PowderLevelPercent: 72,
                    LayersCompleted: 0,
                    LayersTotal: 2400,
                    ActiveLaserCount: 0),

                MachineState.Building => new TelemetryReading(
                    BuildTempC: 175 + (progress / 100) * 5,
                    OxygenPpm: 45,
                    ArgonFlowLpm: 22,
                    LaserPowerW: 6 * 340,        // 6 lasers at 340 W each
                    PowderLevelPercent: 72 - (progress * 0.4),
                    LayersCompleted: (int)(progress / 100 * 2400),
                    LayersTotal: 2400,
                    ActiveLaserCount: 6),

                MachineState.Cooling => new TelemetryReading(
                    BuildTempC: 180 - (progress * 0.8),
                    OxygenPpm: 50,
                    ArgonFlowLpm: 10,
                    LaserPowerW: 0,
                    PowderLevelPercent: 40,
                    LayersCompleted: 2400,
                    LayersTotal: 2400,
                    ActiveLaserCount: 0),

                _ => new TelemetryReading(
                    BuildTempC: 22,
                    OxygenPpm: 20900,
                    ArgonFlowLpm: 0,
                    LaserPowerW: 0,
                    PowderLevelPercent: 72,
                    LayersCompleted: 0,
                    LayersTotal: 0,
                    ActiveLaserCount: 0)
            };
        }
    }
}
