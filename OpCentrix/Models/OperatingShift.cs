using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents operating shift definitions for production scheduling
/// Allows admins to define working hours for each day of the week
/// </summary>
public class OperatingShift
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Day of the week (0 = Sunday, 1 = Monday, etc.)
    /// </summary>
    [Range(0, 6)]
    public int DayOfWeek { get; set; }

    /// <summary>
    /// Shift start time
    /// </summary>
    [Required]
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Shift end time (can be next day if crosses midnight)
    /// </summary>
    [Required]
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Whether this is a holiday or non-working day
    /// </summary>
    public bool IsHoliday { get; set; } = false;

    /// <summary>
    /// Whether this shift is active/enabled
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional description for this shift (e.g., "First Shift", "Holiday - Christmas")
    /// </summary>
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Specific date for holiday overrides (optional)
    /// If set, this overrides the normal day-of-week schedule for this specific date
    /// </summary>
    public DateTime? SpecificDate { get; set; }

    /// <summary>
    /// Audit fields
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    [StringLength(100)]
    public string CreatedBy { get; set; } = "System";

    [StringLength(100)]
    public string LastModifiedBy { get; set; } = "System";

    /// <summary>
    /// Helper property to get day name
    /// </summary>
    [NotMapped]
    public string DayName => ((DayOfWeek)DayOfWeek).ToString();

    /// <summary>
    /// Helper property to calculate shift duration in hours
    /// </summary>
    [NotMapped]
    public double DurationHours
    {
        get
        {
            var duration = EndTime - StartTime;
            if (duration.TotalHours < 0) // Crosses midnight
                duration = duration.Add(TimeSpan.FromDays(1));
            return duration.TotalHours;
        }
    }

    /// <summary>
    /// Check if a given DateTime falls within this shift
    /// </summary>
    public bool IsTimeWithinShift(DateTime dateTime)
    {
        if (!IsActive || IsHoliday)
            return false;

        // Check if this is a specific date override
        if (SpecificDate.HasValue)
        {
            if (dateTime.Date != SpecificDate.Value.Date)
                return false;
        }
        else
        {
            // Check day of week
            if ((int)dateTime.DayOfWeek != DayOfWeek)
                return false;
        }

        var time = dateTime.TimeOfDay;

        // Handle shifts that cross midnight
        if (EndTime < StartTime)
        {
            return time >= StartTime || time <= EndTime;
        }
        else
        {
            return time >= StartTime && time <= EndTime;
        }
    }
}