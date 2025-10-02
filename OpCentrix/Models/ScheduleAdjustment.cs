using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models;

/// <summary>
/// Records automatic or manual schedule shifts applied after a build start delay or duration change.
/// Downstream jobs whose start/end times were moved get one record each for traceability.
/// </summary>
public class ScheduleAdjustment
{
    [Key]
    public int Id { get; set; }

    /// <summary>Primary job (or build) that triggered the cascade.</summary>
    public int TriggerJobId { get; set; }

    /// <summary>The downstream job whose schedule was modified.</summary>
    public int AffectedJobId { get; set; }

    /// <summary>Original scheduled start (UTC) before adjustment.</summary>
    public DateTime OriginalStart { get; set; }

    /// <summary>Original scheduled end (UTC) before adjustment.</summary>
    public DateTime OriginalEnd { get; set; }

    /// <summary>New scheduled start (UTC) after adjustment.</summary>
    public DateTime NewStart { get; set; }

    /// <summary>New scheduled end (UTC) after adjustment.</summary>
    public DateTime NewEnd { get; set; }

    /// <summary>Total shift applied (positive = delayed, negative = pulled ahead).</summary>
    public double ShiftMinutes { get; set; }

    /// <summary>Reason category (DelayCascade, DurationIncrease, ManualOverride).</summary>
    [StringLength(50)]
    public string Reason { get; set; } = "DelayCascade";

    /// <summary>Optional free?text explanation.</summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [StringLength(100)] public string CreatedBy { get; set; } = "System";
}
