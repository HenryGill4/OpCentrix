namespace OpCentrix.Models.MachineProviders
{
    /// <summary>
    /// Operational state of any machine regardless of manufacturer.
    /// </summary>
    public enum MachineState
    {
        Unknown,
        Offline,
        Idle,
        Preheating,
        Building,
        Paused,
        Cooling,
        Maintenance,
        Error
    }

    public enum AlertSeverity { Info, Warning, Error, Critical }

    /// <summary>
    /// Real-time sensor readings normalised across all providers.
    /// Unused fields for a given machine type will be zero.
    /// </summary>
    public record TelemetryReading(
        double BuildTempC,
        double OxygenPpm,
        double ArgonFlowLpm,
        double LaserPowerW,
        double PowderLevelPercent,
        int LayersCompleted,
        int LayersTotal,
        int ActiveLaserCount);

    /// <summary>
    /// Snapshot of a machine's current state as returned by any provider.
    /// </summary>
    public record MachineStatus(
        string MachineId,
        string ProviderType,
        MachineState State,
        double BuildProgressPercent,
        DateTime? EstimatedCompletion,
        TelemetryReading Telemetry,
        string? ActiveJobId,
        string? ActiveJobName,
        DateTime Timestamp);

    public record MachineAlert(
        AlertSeverity Severity,
        string Code,
        string Message,
        DateTime Timestamp);

    /// <summary>
    /// Describes a job being sent to a machine.
    /// Provider implementations translate this into their native format.
    /// </summary>
    public record ProviderJobRequest(
        string JobId,
        string JobName,
        string Material,
        double EstimatedHours,
        double PartLengthMm,
        double PartWidthMm,
        double PartHeightMm,
        string? ParameterSetName = null);

    /// <summary>
    /// A completed job record retrieved from the machine's history.
    /// </summary>
    public record CompletedJobRecord(
        string ExternalJobId,
        string JobName,
        DateTime StartTime,
        DateTime EndTime,
        string Material,
        double ActualHours,
        bool Success,
        string? FailureReason = null);

    /// <summary>
    /// Process parameters recorded for a specific job.
    /// For EOS machines these are pulled from EOSCONNECT after job completion.
    /// </summary>
    public record ProcessParameters(
        string ExternalJobId,
        double LaserPowerW,
        double ScanSpeedMmPerSec,
        double LayerThicknessMicrons,
        string ParameterSetName,
        string Material);

    /// <summary>
    /// Capabilities advertised by a connected machine.
    /// Populated once on connect; re-queried after firmware updates.
    /// </summary>
    public record MachineCapabilities(
        string MachineId,
        string ModelName,
        string SerialNumber,
        string FirmwareVersion,
        double BuildLengthMm,
        double BuildWidthMm,
        double BuildHeightMm,
        int LaserCount,
        double MaxTotalLaserPowerW,
        IReadOnlyList<string> SupportedMaterials,
        bool SupportsJobControl);
}
