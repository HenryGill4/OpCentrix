using System;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Models
{
    /// <summary>
    /// Tracks delays in build jobs with reasons and duration
    /// Auto-triggered when actual start time exceeds scheduled time
    /// </summary>
    public class DelayLog
    {
        [Key] // FIXED: Add explicit Key attribute to resolve primary key issue
        public int DelayId { get; set; }

        [Required]
        public int BuildId { get; set; }
        public virtual BuildJob? BuildJob { get; set; }

        [Required]
        [StringLength(100)]
        public string DelayReason { get; set; } = string.Empty; // Dropdown values

        /// <summary>
        /// Auto-calculated delay duration in minutes
        /// Based on difference between actual and scheduled start, or form submission time
        /// </summary>
        [Required]
        public int DelayDuration { get; set; } = 0;

        [StringLength(1000)]
        public string? Description { get; set; }

        #region Audit Trail

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        #endregion

        #region Static Delay Reasons

        public static readonly string[] DelayReasons = new[]
        {
            "Argon Refill",
            "Plate Not Ready", 
            "Engineering Hold",
            "Material Issue",
            "Machine Maintenance",
            "Operator Unavailable",
            "Power Outage",
            "Quality Issue - Previous Job",
            "Tooling Issue",
            "Software Issue",
            "Emergency Stop",
            "Other"
        };

        #endregion

        #region Computed Properties

        public string DelayDurationDisplay => DelayDuration switch
        {
            < 60 => $"{DelayDuration}m",
            < 1440 => $"{DelayDuration / 60}h {DelayDuration % 60}m",
            _ => $"{DelayDuration / 1440}d {(DelayDuration % 1440) / 60}h"
        };

        public string SeverityClass => DelayDuration switch
        {
            < 30 => "text-yellow-600", // minor delay
            < 120 => "text-orange-600", // moderate delay  
            _ => "text-red-600" // major delay
        };

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculate delay from scheduled vs actual start time
        /// </summary>
        public static int CalculateDelayFromScheduled(DateTime scheduledStart, DateTime actualStart)
        {
            var delay = actualStart - scheduledStart;
            return delay.TotalMinutes > 0 ? (int)delay.TotalMinutes : 0;
        }

        /// <summary>
        /// Calculate delay from form submission time vs actual start time
        /// (for cases where delay is due to late form submission)
        /// </summary>
        public static int CalculateDelayFromSubmission(DateTime submissionTime, DateTime actualStart)
        {
            var delay = submissionTime - actualStart;
            return delay.TotalMinutes > 0 ? (int)delay.TotalMinutes : 0;
        }

        /// <summary>
        /// Get the greater of the two delay calculations
        /// </summary>
        public static int CalculateMaxDelay(DateTime? scheduledStart, DateTime actualStart, DateTime submissionTime)
        {
            var scheduleDelay = scheduledStart.HasValue 
                ? CalculateDelayFromScheduled(scheduledStart.Value, actualStart)
                : 0;
            
            var submissionDelay = CalculateDelayFromSubmission(submissionTime, actualStart);
            
            return Math.Max(scheduleDelay, submissionDelay);
        }

        #endregion
    }
}