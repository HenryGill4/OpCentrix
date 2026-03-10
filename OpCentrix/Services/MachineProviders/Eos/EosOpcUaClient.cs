using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos
{
    /// <summary>
    /// OPC UA client for the EOS M4 Onyx machine.
    ///
    /// Uses the OPCFoundation.NetStandard.Opc.Ua package already in the project.
    /// Provides real-time sensor data and — when JobControlEnabled = true —
    /// remote job start/pause/resume via the EOS OPC UA Control license.
    ///
    /// OPC UA endpoint: opc.tcp://{printer-ip}:4840
    /// Documentation: accessible via web browser on the printer PC.
    ///
    /// Stub status: Session creation and node reads are wired up with correct
    /// OPC UA patterns. Replace the TODO markers once the machine is reachable.
    ///
    /// Node ID namespace reference (EOS standard):
    ///   ns=2;s=Machine.Status.State
    ///   ns=2;s=Machine.Status.BuildProgress
    ///   ns=2;s=Machine.Sensors.BuildChamberTemp
    ///   ns=2;s=Machine.Sensors.OxygenContent
    ///   ns=2;s=Machine.Sensors.ArgonFlowRate
    ///   ns=2;s=Machine.Sensors.LaserPower
    ///   ns=2;s=Machine.Sensors.PowderLevel
    ///   ns=2;s=Machine.Build.CurrentLayer
    ///   ns=2;s=Machine.Build.TotalLayers
    ///   ns=2;s=Machine.Job.ActiveJobId
    ///   ns=2;s=Machine.Control.StartJob      (requires Control license)
    ///   ns=2;s=Machine.Control.PauseJob      (requires Control license)
    ///   ns=2;s=Machine.Control.ResumeJob     (requires Control license)
    /// </summary>
    internal sealed class EosOpcUaClient : IAsyncDisposable
    {
        private readonly ILogger<EosOpcUaClient> _logger;

        private string? _endpointUrl;
        private string? _username;
        private string? _password;
        private bool _jobControlEnabled;
        private bool _connected = false;

        // TODO: Replace object with Opc.Ua.Client.Session once wiring up the real client:
        //   private Opc.Ua.Client.Session? _session;

        public EosOpcUaClient(ILogger<EosOpcUaClient> logger)
        {
            _logger = logger;
        }

        public void Configure(
            string endpointUrl,
            string username,
            string password,
            bool jobControlEnabled)
        {
            _endpointUrl = endpointUrl;
            _username = username;
            _password = password;
            _jobControlEnabled = jobControlEnabled;
        }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public async Task<bool> ConnectAsync(CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(_endpointUrl))
            {
                _logger.LogWarning("[EOS OPC UA] No endpoint configured — skipping connection");
                return false;
            }

            // TODO: Replace with real session creation:
            //
            //   var config = new ApplicationConfiguration
            //   {
            //       ApplicationName = "OpCentrix",
            //       ApplicationType  = ApplicationType.Client,
            //       SecurityConfiguration = new SecurityConfiguration
            //       {
            //           ApplicationCertificate = new CertificateIdentifier(),
            //           AutoAcceptUntrustedCertificates = true  // tighten for production
            //       }
            //   };
            //   await config.Validate(ApplicationType.Client);
            //
            //   var selectedEndpoint = CoreClientUtils.SelectEndpoint(_endpointUrl, useSecurity: false);
            //   var session = await Session.Create(
            //       config,
            //       new ConfiguredEndpoint(null, selectedEndpoint),
            //       updateBeforeConnect: false,
            //       sessionName: "OpCentrix",
            //       sessionTimeout: 60_000,
            //       new UserIdentity(_username, _password),
            //       preferredLocales: null);
            //   _session = session;

            _logger.LogInformation("[EOS OPC UA] Connection stubbed for {Endpoint}", _endpointUrl);
            _connected = false; // Will become true once real session is established
            return false;
        }

        public Task DisconnectAsync(CancellationToken ct = default)
        {
            // TODO: await _session?.CloseAsync(ct);
            //       _session?.Dispose();
            _connected = false;
            return Task.CompletedTask;
        }

        public bool IsConnected => _connected;

        // ── Sensor reads ──────────────────────────────────────────────────────

        /// <summary>
        /// Read a set of sensor nodes in a single OPC UA service call.
        /// Returns null if not connected.
        /// </summary>
        public Task<EosOpcUaTelemetry?> ReadTelemetryAsync(CancellationToken ct = default)
        {
            if (!_connected) return Task.FromResult<EosOpcUaTelemetry?>(null);

            // TODO: Build a ReadValueIdCollection from the node IDs above,
            //       call _session.Read(...), and map results to EosOpcUaTelemetry.
            //
            //   var nodeIds = new[] {
            //       "ns=2;s=Machine.Sensors.BuildChamberTemp",
            //       "ns=2;s=Machine.Sensors.OxygenContent",
            //       ...
            //   };
            //   var readResults = _session.ReadValues(nodeIds.Select(n => new NodeId(n)).ToList());
            //   return new EosOpcUaTelemetry { BuildChamberTempC = (double)readResults[0].Value, ... };

            return Task.FromResult<EosOpcUaTelemetry?>(null);
        }

        // ── Job control (Control license required) ────────────────────────────

        public Task<bool> StartJobAsync(string jobId, CancellationToken ct = default)
        {
            if (!_connected || !_jobControlEnabled)
            {
                _logger.LogWarning("[EOS OPC UA] StartJob skipped — connected={C} controlEnabled={E}",
                    _connected, _jobControlEnabled);
                return Task.FromResult(false);
            }

            // TODO: Write to the control node:
            //   _session.Write(null, new WriteValueCollection
            //   {
            //       new WriteValue {
            //           NodeId      = new NodeId("ns=2;s=Machine.Control.StartJob"),
            //           AttributeId = Attributes.Value,
            //           Value       = new DataValue(new Variant(jobId))
            //       }
            //   }, out _, out _);

            _logger.LogInformation("[EOS OPC UA] StartJob {JobId} stubbed", jobId);
            return Task.FromResult(false);
        }

        public Task<bool> PauseJobAsync(string jobId, CancellationToken ct = default)
        {
            if (!_connected || !_jobControlEnabled) return Task.FromResult(false);

            // TODO: Write to ns=2;s=Machine.Control.PauseJob

            _logger.LogInformation("[EOS OPC UA] PauseJob {JobId} stubbed", jobId);
            return Task.FromResult(false);
        }

        public Task<bool> ResumeJobAsync(string jobId, CancellationToken ct = default)
        {
            if (!_connected || !_jobControlEnabled) return Task.FromResult(false);

            // TODO: Write to ns=2;s=Machine.Control.ResumeJob

            _logger.LogInformation("[EOS OPC UA] ResumeJob {JobId} stubbed", jobId);
            return Task.FromResult(false);
        }

        public ValueTask DisposeAsync()
        {
            // TODO: _session?.Dispose();
            _connected = false;
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>Telemetry values read from OPC UA sensor nodes.</summary>
    internal sealed class EosOpcUaTelemetry
    {
        public double BuildChamberTempC { get; set; }
        public double OxygenContentPpm { get; set; }
        public double ArgonFlowRateLpm { get; set; }
        public double LaserPowerW { get; set; }
        public double PowderLevelPercent { get; set; }
        public int CurrentLayer { get; set; }
        public int TotalLayers { get; set; }
        public string? MachineState { get; set; }
        public string? ActiveJobId { get; set; }
        public double BuildProgressPercent { get; set; }
    }
}
