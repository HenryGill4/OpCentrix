using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders
{
    /// <summary>
    /// Universal contract for communicating with any metal AM machine regardless of manufacturer.
    ///
    /// Implementations:
    ///   MockMachineProvider  — deterministic test data, no hardware required
    ///   EosMachineProvider   — EOS M4 Onyx via EOSCONNECT REST + OPC UA
    ///
    /// All scheduling, sync, and queue services depend only on this interface,
    /// never on a concrete implementation.
    /// </summary>
    public interface IMachineProvider : IAsyncDisposable
    {
        /// <summary>Identifies the provider type, e.g. "EOS", "Mock".</summary>
        string ProviderType { get; }

        // ── Lifecycle ────────────────────────────────────────────────────────

        /// <summary>
        /// Establish a connection to the machine using the provided settings.
        /// Must be called before any other method.
        /// </summary>
        Task<bool> ConnectAsync(MachineConnectionSettings settings, CancellationToken ct = default);

        Task DisconnectAsync(CancellationToken ct = default);

        Task<bool> IsConnectedAsync(CancellationToken ct = default);

        // ── Capabilities ─────────────────────────────────────────────────────

        /// <summary>
        /// Returns static machine capabilities (build volume, laser count, etc.).
        /// Retrieved once on connect; may be refreshed on demand.
        /// </summary>
        Task<MachineCapabilities?> GetCapabilitiesAsync(CancellationToken ct = default);

        // ── Real-time state ──────────────────────────────────────────────────

        /// <summary>Returns the current operational state and live telemetry.</summary>
        Task<MachineStatus> GetStatusAsync(CancellationToken ct = default);

        /// <summary>Returns active alerts. Empty list if none.</summary>
        Task<IReadOnlyList<MachineAlert>> GetAlertsAsync(CancellationToken ct = default);

        // ── Job control (requires OPC UA Control license for real machines) ──

        /// <summary>
        /// Whether this provider and its current connection support job control commands.
        /// Always false for read-only connections or when JobControlEnabled = false.
        /// </summary>
        bool SupportsJobControl { get; }

        /// <summary>Transfer a job file to the machine's print queue.</summary>
        Task<bool> SendJobAsync(ProviderJobRequest job, CancellationToken ct = default);

        /// <summary>Start or resume the specified job.</summary>
        Task<bool> StartJobAsync(string externalJobId, CancellationToken ct = default);

        /// <summary>Pause the currently running job.</summary>
        Task<bool> PauseJobAsync(string externalJobId, CancellationToken ct = default);

        /// <summary>Resume a paused job.</summary>
        Task<bool> ResumeJobAsync(string externalJobId, CancellationToken ct = default);

        // ── Historical data ───────────────────────────────────────────────────

        /// <summary>Retrieve completed job records within a time window.</summary>
        Task<IReadOnlyList<CompletedJobRecord>> GetCompletedJobsAsync(
            DateTime from, DateTime to, CancellationToken ct = default);

        /// <summary>
        /// Retrieve the process parameters that were actually used for a job.
        /// Returns null if the machine does not retain parameter history.
        /// </summary>
        Task<ProcessParameters?> GetJobProcessParametersAsync(
            string externalJobId, CancellationToken ct = default);
    }
}
