using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models.MachineProviders;

/// <summary>
/// Database entity storing machine provider connection settings.
/// Links to Machine via MachineId (int FK) and stores provider-specific config as JSON.
/// </summary>
public class MachineConnectionSettings
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Machine.Id (int PK, not the string MachineId)
    /// </summary>
    [Required]
    public int MachineId { get; set; }

    /// <summary>
    /// Provider type: "Mock", "Eos", "GenericOpcUa"
    /// Used by MachineProviderFactory to resolve the correct provider
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ProviderType { get; set; } = "Mock";

    /// <summary>
    /// JSON configuration specific to the provider type.
    /// Deserialized into MockConnectionConfig, EosConnectionConfig, etc.
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string ConnectionConfigJson { get; set; } = "{}";

    /// <summary>
    /// Whether this connection is enabled for sync operations
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Polling interval in seconds (overrides global setting)
    /// </summary>
    [Range(5, 3600)]
    public int PollIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Last successful connection timestamp
    /// </summary>
    public DateTime? LastConnectedAt { get; set; }

    /// <summary>
    /// Last error message (cleared on successful connection)
    /// </summary>
    [StringLength(500)]
    public string? LastError { get; set; }

    /// <summary>
    /// Number of consecutive connection failures
    /// </summary>
    public int ConsecutiveFailures { get; set; } = 0;

    /// <summary>
    /// Maximum consecutive failures before disabling (0 = never disable)
    /// </summary>
    public int MaxConsecutiveFailures { get; set; } = 10;

    #region Audit Fields

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [Required]
    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Navigation to the Machine entity
    /// </summary>
    public virtual Machine Machine { get; set; } = null!;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Get the typed connection config for this provider
    /// </summary>
    public T? GetConnectionConfig<T>() where T : MachineConnectionConfig
    {
        if (string.IsNullOrWhiteSpace(ConnectionConfigJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(ConnectionConfigJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Set the connection config from a typed object
    /// </summary>
    public void SetConnectionConfig<T>(T config) where T : MachineConnectionConfig
    {
        ConnectionConfigJson = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a successful connection
    /// </summary>
    public void RecordSuccess()
    {
        LastConnectedAt = DateTime.UtcNow;
        LastError = null;
        ConsecutiveFailures = 0;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Record a connection failure
    /// </summary>
    public void RecordFailure(string errorMessage)
    {
        LastError = errorMessage?.Length > 500 ? errorMessage[..500] : errorMessage;
        ConsecutiveFailures++;
        LastModifiedDate = DateTime.UtcNow;

        // Auto-disable if too many failures
        if (MaxConsecutiveFailures > 0 && ConsecutiveFailures >= MaxConsecutiveFailures)
        {
            IsEnabled = false;
        }
    }

    /// <summary>
    /// Check if this connection should be polled based on its state
    /// </summary>
    public bool ShouldPoll()
    {
        if (!IsEnabled) return false;

        // If we have consecutive failures, implement exponential backoff
        if (ConsecutiveFailures > 0)
        {
            // Wait longer between retries: 2^failures * poll interval, max 1 hour
            var backoffSeconds = Math.Min(3600, PollIntervalSeconds * Math.Pow(2, Math.Min(ConsecutiveFailures, 6)));
            var nextAttempt = LastModifiedDate.AddSeconds(backoffSeconds);
            return DateTime.UtcNow >= nextAttempt;
        }

        return true;
    }

    #endregion
}
