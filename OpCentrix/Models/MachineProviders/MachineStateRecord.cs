using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models.MachineProviders;

/// <summary>
/// Database entity storing historical machine state snapshots.
/// Used for analytics, troubleshooting, and state change detection.
/// </summary>
public class MachineStateRecord
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Machine.Id (int PK)
    /// </summary>
    [Required]
    public int MachineId { get; set; }

    /// <summary>
    /// Machine status at time of snapshot: Idle, Printing, Preheating, Cooling, Error, Offline, Maintenance
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Unknown";

    /// <summary>
    /// Build progress percentage (0-100), null if not building
    /// </summary>
    public double? BuildProgressPercent { get; set; }

    /// <summary>
    /// Current job/build reference from the machine
    /// </summary>
    [StringLength(100)]
    public string? CurrentJobReference { get; set; }

    /// <summary>
    /// Whether the machine was connected when this snapshot was taken
    /// </summary>
    public bool IsConnected { get; set; } = true;

    /// <summary>
    /// Active alarms at time of snapshot (comma-separated)
    /// </summary>
    [StringLength(1000)]
    public string? AlarmsSnapshot { get; set; }

    /// <summary>
    /// Full telemetry data as JSON
    /// </summary>
    [Column(TypeName = "TEXT")]
    public string? StateDataJson { get; set; }

    /// <summary>
    /// Provider type that generated this data
    /// </summary>
    [StringLength(50)]
    public string ProviderType { get; set; } = "Unknown";

    /// <summary>
    /// Whether this record represents a state change from previous
    /// </summary>
    public bool IsStateChange { get; set; } = false;

    /// <summary>
    /// Previous status if this is a state change
    /// </summary>
    [StringLength(50)]
    public string? PreviousStatus { get; set; }

    /// <summary>
    /// Timestamp when this state was captured
    /// </summary>
    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    #region Navigation Properties

    /// <summary>
    /// Navigation to the Machine entity
    /// </summary>
    public virtual Machine Machine { get; set; } = null!;

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create a state record from MachineData
    /// </summary>
    public static MachineStateRecord FromMachineData(MachineData data, MachineStateRecord? previous = null)
    {
        var record = new MachineStateRecord
        {
            MachineId = data.MachineId,
            Status = data.Status.Status,
            BuildProgressPercent = data.Status.BuildProgressPercent,
            CurrentJobReference = data.Status.CurrentJobReference,
            IsConnected = data.Status.IsConnected,
            AlarmsSnapshot = data.Status.Alarms.Any() ? string.Join(",", data.Status.Alarms) : null,
            StateDataJson = JsonSerializer.Serialize(data.Telemetry),
            ProviderType = data.ProviderType,
            Timestamp = data.Timestamp
        };

        // Detect state change
        if (previous != null && previous.Status != record.Status)
        {
            record.IsStateChange = true;
            record.PreviousStatus = previous.Status;
        }

        return record;
    }

    /// <summary>
    /// Get the telemetry data as a dictionary
    /// </summary>
    public Dictionary<string, object> GetTelemetry()
    {
        if (string.IsNullOrWhiteSpace(StateDataJson))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(StateDataJson) 
                   ?? new Dictionary<string, object>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Get the alarms as a list
    /// </summary>
    public List<string> GetAlarms()
    {
        if (string.IsNullOrWhiteSpace(AlarmsSnapshot))
            return new List<string>();

        return AlarmsSnapshot.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(a => a.Trim())
            .ToList();
    }

    /// <summary>
    /// Convert to MachineStatus for API responses
    /// </summary>
    public MachineStatus ToMachineStatus()
    {
        return new MachineStatus
        {
            MachineId = MachineId,
            Status = Status,
            BuildProgressPercent = BuildProgressPercent,
            CurrentJobReference = CurrentJobReference,
            IsConnected = IsConnected,
            Alarms = GetAlarms(),
            Timestamp = Timestamp
        };
    }

    #endregion
}
