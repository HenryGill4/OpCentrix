using System.Text.Json;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos;

/// <summary>
/// Configuration for EOS M4 ONIX machine connections.
/// Supports both REST API (job control) and OPC UA (telemetry) endpoints.
/// </summary>
public record EosConnectionConfig : MachineConnectionConfig
{
    #region REST API Configuration

    /// <summary>
    /// Base URL for EOS REST API (e.g., "https://eos-m4-onix.local/api/v1")
    /// </summary>
    public string RestApiBaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// OAuth2 token endpoint for authentication
    /// </summary>
    public string OAuth2TokenEndpoint { get; init; } = string.Empty;

    /// <summary>
    /// OAuth2 client ID for API access
    /// </summary>
    public string OAuth2ClientId { get; init; } = string.Empty;

    /// <summary>
    /// OAuth2 client secret for API access (should be stored securely)
    /// </summary>
    public string OAuth2ClientSecret { get; init; } = string.Empty;

    /// <summary>
    /// OAuth2 scopes required for API access
    /// </summary>
    public string OAuth2Scopes { get; init; } = "machine.read machine.write";

    #endregion

    #region OPC UA Configuration

    /// <summary>
    /// OPC UA endpoint URL (e.g., "opc.tcp://eos-m4-onix.local:4840")
    /// </summary>
    public string OpcUaEndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// OPC UA security mode: None, Sign, SignAndEncrypt
    /// </summary>
    public string OpcUaSecurityMode { get; init; } = "None";

    /// <summary>
    /// OPC UA security policy URI
    /// </summary>
    public string OpcUaSecurityPolicy { get; init; } = "http://opcfoundation.org/UA/SecurityPolicy#None";

    /// <summary>
    /// Path to client certificate for OPC UA authentication (PFX or PEM)
    /// </summary>
    public string? OpcUaClientCertificatePath { get; init; }

    /// <summary>
    /// Password for client certificate
    /// </summary>
    public string? OpcUaClientCertificatePassword { get; init; }

    /// <summary>
    /// Whether to trust all server certificates (development only!)
    /// </summary>
    public bool OpcUaTrustAllCertificates { get; init; } = false;

    /// <summary>
    /// OPC UA application name for session
    /// </summary>
    public string OpcUaApplicationName { get; init; } = "OpCentrix MES";

    /// <summary>
    /// OPC UA session timeout in milliseconds
    /// </summary>
    public int OpcUaSessionTimeoutMs { get; init; } = 60000;

    /// <summary>
    /// Whether to enable OPC UA subscriptions for real-time updates
    /// </summary>
    public bool EnableOpcUaSubscriptions { get; init; } = true;

    /// <summary>
    /// OPC UA subscription publishing interval in milliseconds
    /// </summary>
    public int OpcUaPublishingIntervalMs { get; init; } = 1000;

    #endregion

    #region Connection Behavior

    /// <summary>
    /// Prefer REST API over OPC UA for status queries
    /// </summary>
    public bool PreferRestApi { get; init; } = false;

    /// <summary>
    /// Use OPC UA for real-time telemetry even if REST is preferred
    /// </summary>
    public bool UseOpcUaForTelemetry { get; init; } = true;

    /// <summary>
    /// Fall back to test data if connection fails
    /// </summary>
    public bool FallbackToTestData { get; init; } = true;

    /// <summary>
    /// Number of retry attempts for failed API calls
    /// </summary>
    public int RetryAttempts { get; init; } = 3;

    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    public int RetryDelayMs { get; init; } = 1000;

    #endregion

    #region EOS-Specific Settings

    /// <summary>
    /// EOS machine model (e.g., "M4 ONIX", "M 290", "M 400")
    /// </summary>
    public string MachineModel { get; init; } = "M4 ONIX";

    /// <summary>
    /// EOS software version for API compatibility
    /// </summary>
    public string SoftwareVersion { get; init; } = string.Empty;

    /// <summary>
    /// Namespace index for EOS-specific OPC UA nodes
    /// </summary>
    public int EosNamespaceIndex { get; init; } = 2;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Validate the configuration
    /// </summary>
    public (bool IsValid, List<string> Errors) Validate()
    {
        var errors = new List<string>();

        // At least one connection method must be configured
        var hasRest = !string.IsNullOrWhiteSpace(RestApiBaseUrl);
        var hasOpcUa = !string.IsNullOrWhiteSpace(OpcUaEndpointUrl);

        if (!hasRest && !hasOpcUa)
        {
            errors.Add("At least one connection method (REST API or OPC UA) must be configured");
        }

        // If REST is configured, OAuth2 credentials are required
        if (hasRest)
        {
            if (string.IsNullOrWhiteSpace(OAuth2TokenEndpoint))
                errors.Add("OAuth2TokenEndpoint is required when REST API is configured");
            if (string.IsNullOrWhiteSpace(OAuth2ClientId))
                errors.Add("OAuth2ClientId is required when REST API is configured");
            if (string.IsNullOrWhiteSpace(OAuth2ClientSecret))
                errors.Add("OAuth2ClientSecret is required when REST API is configured");
        }

        // If OPC UA security is enabled, certificates may be required
        if (hasOpcUa && OpcUaSecurityMode != "None" && string.IsNullOrWhiteSpace(OpcUaClientCertificatePath))
        {
            errors.Add("Client certificate is required when OPC UA security mode is not 'None'");
        }

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Create a config for development/testing with test data fallback
    /// </summary>
    public static EosConnectionConfig CreateTestConfig(string machineModel = "M4 ONIX")
    {
        return new EosConnectionConfig
        {
            MachineModel = machineModel,
            FallbackToTestData = true,
            PreferRestApi = false,
            UseOpcUaForTelemetry = false,
            ConnectionTimeoutSeconds = 5,
            OperationTimeoutSeconds = 10
        };
    }

    /// <summary>
    /// Serialize to JSON for storage
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserialize from JSON
    /// </summary>
    public static EosConnectionConfig? FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<EosConnectionConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    #endregion
}

/// <summary>
/// EOS-specific OPC UA node IDs for M4 ONIX
/// These are placeholder values - real values come from EOS documentation
/// </summary>
public static class EosOpcUaNodeIds
{
    // Machine State
    public const string MachineState = "ns=2;s=Machine.State";
    public const string MachineStatus = "ns=2;s=Machine.Status";
    public const string MachineMode = "ns=2;s=Machine.Mode";
    public const string IsRunning = "ns=2;s=Machine.IsRunning";
    public const string IsPaused = "ns=2;s=Machine.IsPaused";
    public const string HasError = "ns=2;s=Machine.HasError";

    // Build Progress
    public const string BuildProgress = "ns=2;s=Build.Progress";
    public const string CurrentLayer = "ns=2;s=Build.CurrentLayer";
    public const string TotalLayers = "ns=2;s=Build.TotalLayers";
    public const string EstimatedTimeRemaining = "ns=2;s=Build.EstimatedTimeRemaining";
    public const string ElapsedTime = "ns=2;s=Build.ElapsedTime";
    public const string JobName = "ns=2;s=Build.JobName";

    // Laser Parameters
    public const string LaserPower = "ns=2;s=Laser.Power";
    public const string LaserPowerSetpoint = "ns=2;s=Laser.PowerSetpoint";
    public const string ScanSpeed = "ns=2;s=Laser.ScanSpeed";
    public const string HatchSpacing = "ns=2;s=Laser.HatchSpacing";

    // Build Chamber
    public const string BuildPlatformTemperature = "ns=2;s=Chamber.BuildPlatformTemp";
    public const string BuildPlatformTempSetpoint = "ns=2;s=Chamber.BuildPlatformTempSetpoint";
    public const string ChamberTemperature = "ns=2;s=Chamber.Temperature";
    public const string ChamberPressure = "ns=2;s=Chamber.Pressure";
    public const string OxygenContent = "ns=2;s=Chamber.OxygenContent";

    // Gas System
    public const string ArgonFlowRate = "ns=2;s=Gas.ArgonFlowRate";
    public const string ArgonPressure = "ns=2;s=Gas.ArgonPressure";
    public const string ArgonPurity = "ns=2;s=Gas.ArgonPurity";

    // Powder System
    public const string PowderLevel = "ns=2;s=Powder.Level";
    public const string RecoaterPosition = "ns=2;s=Powder.RecoaterPosition";
    public const string LayerThickness = "ns=2;s=Powder.LayerThickness";

    // Alarms
    public const string ActiveAlarms = "ns=2;s=Alarms.Active";
    public const string AlarmCount = "ns=2;s=Alarms.Count";
    public const string HighestAlarmSeverity = "ns=2;s=Alarms.HighestSeverity";

    // Maintenance
    public const string FilterLifeRemaining = "ns=2;s=Maintenance.FilterLifePercent";
    public const string LaserHoursTotal = "ns=2;s=Maintenance.LaserHoursTotal";
    public const string NextServiceDue = "ns=2;s=Maintenance.NextServiceDue";
}

/// <summary>
/// EOS machine states
/// </summary>
public enum EosMachineState
{
    Unknown = 0,
    Idle = 1,
    Warming = 2,
    Ready = 3,
    Printing = 4,
    Paused = 5,
    Cooling = 6,
    Completed = 7,
    Error = 8,
    Maintenance = 9,
    Offline = 10
}

/// <summary>
/// Maps EOS state codes to standard status strings
/// </summary>
public static class EosStateMapper
{
    public static string ToStatusString(EosMachineState state) => state switch
    {
        EosMachineState.Idle => "Idle",
        EosMachineState.Warming => "Preheating",
        EosMachineState.Ready => "Ready",
        EosMachineState.Printing => "Printing",
        EosMachineState.Paused => "Paused",
        EosMachineState.Cooling => "Cooling",
        EosMachineState.Completed => "Completed",
        EosMachineState.Error => "Error",
        EosMachineState.Maintenance => "Maintenance",
        EosMachineState.Offline => "Offline",
        _ => "Unknown"
    };

    public static EosMachineState FromStatusString(string status) => status?.ToLowerInvariant() switch
    {
        "idle" => EosMachineState.Idle,
        "warming" or "preheating" => EosMachineState.Warming,
        "ready" => EosMachineState.Ready,
        "printing" or "running" => EosMachineState.Printing,
        "paused" => EosMachineState.Paused,
        "cooling" => EosMachineState.Cooling,
        "completed" or "done" => EosMachineState.Completed,
        "error" or "fault" => EosMachineState.Error,
        "maintenance" => EosMachineState.Maintenance,
        "offline" or "disconnected" => EosMachineState.Offline,
        _ => EosMachineState.Unknown
    };
}
