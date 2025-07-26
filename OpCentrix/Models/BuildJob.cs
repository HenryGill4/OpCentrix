using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models
{
    /// <summary>
    /// Represents a single build job for 3D printer tracking (TI1, TI2, INC)
    /// Focused on actual print time tracking, delays, and operator accountability
    /// </summary>
    public class BuildJob
    {
        [Key] // FIXED: Add explicit Key attribute to resolve primary key issue
        public int BuildId { get; set; }

        #region Core Print Tracking

        [Required]
        [StringLength(10)]
        public string PrinterName { get; set; } = string.Empty; // TI1, TI2, INC

        [Required]
        public DateTime ActualStartTime { get; set; }

        public DateTime? ActualEndTime { get; set; }

        // Optional - for comparison with scheduled times
        public DateTime? ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "In Progress"; // Scheduled, In Progress, Completed, Aborted, Error

        [Required]
        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        #endregion

        #region Print Details (from printer summary screen)

        [StringLength(50)]
        public string? LaserRunTime { get; set; }

        public float? GasUsed_L { get; set; }

        public float? PowderUsed_L { get; set; }

        [StringLength(50)]
        public string? ReasonForEnd { get; set; } // Completed, Aborted, Error, etc.

        #endregion

        #region Optional Setup Notes

        [StringLength(1000)]
        public string? SetupNotes { get; set; }

        // Link to existing scheduled job if applicable
        public int? AssociatedScheduledJobId { get; set; }

        #endregion

        #region Audit Trail

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        #endregion

        #region Computed Properties

        [NotMapped]
        public TimeSpan? PrintDuration => ActualEndTime.HasValue 
            ? ActualEndTime.Value - ActualStartTime 
            : null;

        [NotMapped]
        public double PrintHours => PrintDuration?.TotalHours ?? 0;

        [NotMapped]
        public bool IsCompleted => Status == "Completed" && ActualEndTime.HasValue;

        [NotMapped]
        public bool IsActive => Status == "In Progress";

        [NotMapped]
        public string StatusColor => Status?.ToLower() switch
        {
            "completed" => "#10B981", // green
            "in progress" => "#F59E0B", // amber
            "aborted" => "#EF4444", // red
            "error" => "#DC2626", // dark red
            _ => "#6B7280" // gray (scheduled)
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate if this build started late compared to scheduled time
        /// </summary>
        public bool IsStartedLate()
        {
            return ScheduledStartTime.HasValue && ActualStartTime > ScheduledStartTime.Value;
        }

        /// <summary>
        /// Get delay in minutes if started late
        /// </summary>
        public int GetDelayMinutes()
        {
            if (!IsStartedLate()) return 0;
            return (int)(ActualStartTime - ScheduledStartTime!.Value).TotalMinutes;
        }

        /// <summary>
        /// Complete the build job
        /// </summary>
        public void Complete()
        {
            Status = "Completed";
            ActualEndTime = DateTime.UtcNow;
            CompletedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Abort the build job
        /// </summary>
        public void Abort(string reason = "Aborted")
        {
            Status = "Aborted";
            ReasonForEnd = reason;
            ActualEndTime = DateTime.UtcNow;
            CompletedAt = DateTime.UtcNow;
        }

        #endregion
    }
}