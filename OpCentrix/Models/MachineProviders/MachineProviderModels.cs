namespace OpCentrix.Models.MachineProviders;

/// <summary>
/// Represents the current status of a machine from its provider.
/// Immutable record for thread-safe status passing.
/// </summary>
public record MachineStatus
{
    /// <summary>
    /// Machine ID (matches Machine.Id in database)
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// Current machine state: Idle, Printing, Preheating, Cooling, Error, Offline, Maintenance
    /// </summary>
    public string Status { get; init; } = "Unknown";

    /// <summary>
    /// Build progress percentage (0-100), null if not building
    /// </summary>
    public double? BuildProgressPercent { get; init; }

    /// <summary>
    /// Current layer number being printed (for SLS machines)
    /// </summary>
    public int? CurrentLayer { get; init; }

    /// <summary>
    /// Total layers in current build (for SLS machines)
    /// </summary>
    public int? TotalLayers { get; init; }

    /// <summary>
    /// Estimated time remaining in current operation (minutes)
    /// </summary>
    public double? EstimatedMinutesRemaining { get; init; }

    /// <summary>
    /// Current job/build identifier from the machine
    /// </summary>
    public string? CurrentJobReference { get; init; }

    /// <summary>
    /// Active alarms on the machine
    /// </summary>
    public List<string> Alarms { get; init; } = new();

    /// <summary>
    /// Whether the machine is connected and responding
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// Timestamp when this status was captured
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Short status display string
    /// </summary>
    public string DisplayStatus => IsConnected 
        ? (BuildProgressPercent.HasValue ? $"{Status} ({BuildProgressPercent:F1}%)" : Status)
        : "Disconnected";
}

/// <summary>
/// Full machine telemetry data from a provider.
/// Contains all available sensor readings and parameters.
/// </summary>
public record MachineData
{
    /// <summary>
    /// Machine ID (matches Machine.Id in database)
    /// </summary>
    public int MachineId { get; init; }

    /// <summary>
    /// The basic status information
    /// </summary>
    public MachineStatus Status { get; init; } = new();

    /// <summary>
    /// Key-value pairs of telemetry data
    /// Keys are provider-specific (e.g., "LaserPowerWatts", "BuildTemperatureCelsius")
    /// </summary>
    public Dictionary<string, object> Telemetry { get; init; } = new();

    /// <summary>
    /// Raw JSON data from the machine (for debugging/logging)
    /// </summary>
    public string? RawDataJson { get; init; }

    /// <summary>
    /// Provider type that generated this data
    /// </summary>
    public string ProviderType { get; init; } = "Unknown";

    /// <summary>
    /// Timestamp when this data was captured
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Get a typed telemetry value, or default if not present
    /// </summary>
    public T? GetTelemetryValue<T>(string key, T? defaultValue = default)
    {
        if (Telemetry.TryGetValue(key, out var value))
        {
            try
            {
                return (T?)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }
}

/// <summary>
/// Represents a command to send to a machine.
/// </summary>
public record MachineCommand
{
    /// <summary>
    /// Type of command: Start, Stop, Pause, Resume, Reset, Custom
    /// </summary>
    public MachineCommandType CommandType { get; init; }

    /// <summary>
    /// Optional parameters for the command (command-specific)
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>
    /// For Custom commands, the command identifier
    /// </summary>
    public string? CustomCommandId { get; init; }

    /// <summary>
    /// Job reference for Start commands
    /// </summary>
    public string? JobReference { get; init; }

    /// <summary>
    /// User who initiated this command
    /// </summary>
    public string InitiatedBy { get; init; } = "System";

    /// <summary>
    /// Timestamp when command was created
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Types of commands that can be sent to machines
/// </summary>
public enum MachineCommandType
{
    /// <summary>
    /// Start a new job/build
    /// </summary>
    Start,

    /// <summary>
    /// Stop the current operation (cannot resume)
    /// </summary>
    Stop,

    /// <summary>
    /// Pause the current operation (can resume)
    /// </summary>
    Pause,

    /// <summary>
    /// Resume a paused operation
    /// </summary>
    Resume,

    /// <summary>
    /// Reset machine state/clear errors
    /// </summary>
    Reset,

    /// <summary>
    /// Acknowledge an alarm
    /// </summary>
    AcknowledgeAlarm,

    /// <summary>
    /// Custom/provider-specific command
    /// </summary>
    Custom
}

/// <summary>
/// Result of sending a command to a machine
/// </summary>
public record MachineCommandResult
{
    /// <summary>
    /// Whether the command was successfully sent
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if command failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Response data from the machine (if any)
    /// </summary>
    public Dictionary<string, object>? ResponseData { get; init; }

    /// <summary>
    /// Timestamp when response was received
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful result
    /// </summary>
    public static MachineCommandResult Ok(Dictionary<string, object>? responseData = null) 
        => new() { Success = true, ResponseData = responseData };

    /// <summary>
    /// Create a failed result
    /// </summary>
    public static MachineCommandResult Fail(string errorMessage) 
        => new() { Success = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Connection configuration for different provider types.
/// Serialized as JSON in MachineConnectionSettings.ConnectionConfigJson
/// </summary>
public abstract record MachineConnectionConfig
{
    /// <summary>
    /// Timeout for connection attempts in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; init; } = 30;

    /// <summary>
    /// Timeout for individual operations in seconds
    /// </summary>
    public int OperationTimeoutSeconds { get; init; } = 60;
}

/// <summary>
/// Configuration for Mock provider (testing/development)
/// </summary>
public record MockConnectionConfig : MachineConnectionConfig
{
    /// <summary>
    /// Simulated status to return
    /// </summary>
    public string SimulatedStatus { get; init; } = "Idle";

    /// <summary>
    /// Whether to simulate a build in progress
    /// </summary>
    public bool SimulateBuild { get; init; } = false;

    /// <summary>
    /// Build progress increment per poll (percentage points)
    /// </summary>
    public double BuildProgressIncrementPercent { get; init; } = 0.5;

    /// <summary>
    /// Probability of random status changes (0-1)
    /// </summary>
    public double RandomStatusChangeProbability { get; init; } = 0.05;
}

/// <summary>
/// Configuration for EOS provider
/// </summary>
public record EosConnectionConfig : MachineConnectionConfig
{
    /// <summary>
    /// REST API base URL (e.g., "https://eos-machine.local/api")
    /// </summary>
    public string RestApiBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// OPC UA endpoint URL (e.g., "opc.tcp://eos-machine.local:4840")
    /// </summary>
    public string OpcUaEndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// OAuth2 client ID for REST API
    /// </summary>
    public string? OAuth2ClientId { get; init; }

    /// <summary>
    /// OAuth2 client secret for REST API (should be stored securely)
    /// </summary>
    public string? OAuth2ClientSecret { get; init; }

    /// <summary>
    /// Path to client certificate for OPC UA (if required)
    /// </summary>
    public string? ClientCertificatePath { get; init; }

    /// <summary>
    /// Whether to use REST API for status (vs OPC UA)
    /// </summary>
    public bool PreferRestApi { get; init; } = false;

    /// <summary>
    /// Whether to enable OPC UA subscriptions for real-time updates
    /// </summary>
    public bool EnableOpcUaSubscriptions { get; init; } = true;
}

/// <summary>
/// Configuration for generic OPC UA provider
/// </summary>
public record GenericOpcUaConnectionConfig : MachineConnectionConfig
{
    /// <summary>
    /// OPC UA endpoint URL
    /// </summary>
    public string EndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// Security mode: None, Sign, SignAndEncrypt
    /// </summary>
    public string SecurityMode { get; init; } = "None";

    /// <summary>
    /// Username for authentication (if required)
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Password for authentication (if required)
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Node ID mappings for standard telemetry
    /// Key = telemetry name, Value = OPC UA Node ID
    /// </summary>
    public Dictionary<string, string> NodeIdMappings { get; init; } = new();
}
