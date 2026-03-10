using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models.MachineProviders
{
    /// <summary>
    /// The latest persisted machine state written by MachineSyncService.
    /// One row per machine; upserted on every successful poll.
    /// </summary>
    public class MachineStateRecord
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string MachineId { get; set; } = string.Empty;

        [StringLength(50)]
        public string ProviderType { get; set; } = string.Empty;

        public MachineState State { get; set; } = MachineState.Unknown;

        [Range(0, 100)]
        public double BuildProgressPercent { get; set; } = 0;

        [StringLength(200)]
        public string? ActiveJobId { get; set; }

        [StringLength(200)]
        public string? ActiveJobName { get; set; }

        public DateTime? EstimatedCompletion { get; set; }

        /// <summary>Serialised TelemetryReading — use the Telemetry property to access.</summary>
        public string TelemetryJson { get; set; } = "{}";

        /// <summary>Serialised List&lt;MachineAlert&gt; — use the Alerts property to access.</summary>
        public string AlertsJson { get; set; } = "[]";

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        #region Computed (not mapped)

        [NotMapped]
        public TelemetryReading? Telemetry
        {
            get
            {
                try { return JsonSerializer.Deserialize<TelemetryReading>(TelemetryJson); }
                catch { return null; }
            }
        }

        [NotMapped]
        public IReadOnlyList<MachineAlert> Alerts
        {
            get
            {
                try { return JsonSerializer.Deserialize<List<MachineAlert>>(AlertsJson) ?? []; }
                catch { return []; }
            }
        }

        [NotMapped]
        public bool HasActiveAlerts => Alerts.Any(a => a.Severity >= AlertSeverity.Warning);

        #endregion

        public static MachineStateRecord FromStatus(MachineStatus status)
        {
            return new MachineStateRecord
            {
                MachineId = status.MachineId,
                ProviderType = status.ProviderType,
                State = status.State,
                BuildProgressPercent = status.BuildProgressPercent,
                ActiveJobId = status.ActiveJobId,
                ActiveJobName = status.ActiveJobName,
                EstimatedCompletion = status.EstimatedCompletion,
                TelemetryJson = JsonSerializer.Serialize(status.Telemetry),
                RecordedAt = status.Timestamp
            };
        }
    }
}
