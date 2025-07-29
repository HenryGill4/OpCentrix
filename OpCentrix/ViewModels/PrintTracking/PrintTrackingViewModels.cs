using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpCentrix.Models;

namespace OpCentrix.ViewModels.PrintTracking
{
    /// <summary>
    /// View model for the Print Start Form - required when operators begin a new print
    /// </summary>
    public class PrintStartViewModel
    {
        [Required(ErrorMessage = "Printer selection is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; } = DateTime.Now;

        [Display(Name = "Associated Scheduled Job")]
        public int? AssociatedScheduledJobId { get; set; }

        [Display(Name = "Setup Notes")]
        [StringLength(1000, ErrorMessage = "Setup notes cannot exceed 1000 characters")]
        public string? SetupNotes { get; set; }

        // Available options for dropdowns
        public List<string> AvailablePrinters { get; set; } = new() { "TI1", "TI2", "INC" };
        public List<Job> AvailableScheduledJobs { get; set; } = new();

        // For displaying current operator info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    /// <summary>
    /// View model for the Post-Print Log Form - required when operators finish a print
    /// </summary>
    public class PostPrintViewModel
    {
        [Required(ErrorMessage = "Printer selection is required")]
        [Display(Name = "Printer")]
        public string PrinterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Actual start time is required")]
        [Display(Name = "Actual Start Time")]
        public DateTime ActualStartTime { get; set; }

        [Required(ErrorMessage = "Actual end time is required")]
        [Display(Name = "Actual End Time")]
        public DateTime ActualEndTime { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Reason for end is required")]
        [Display(Name = "Reason for End")]
        public string ReasonForEnd { get; set; } = string.Empty;

        // Part number list (repeatable group)
        [Required(ErrorMessage = "At least one part must be specified")]
        public List<PostPrintPartEntry> Parts { get; set; } = new();

        // Optional fields (collapsed by default)
        [Display(Name = "Laser Run Time")]
        public string? LaserRunTime { get; set; }

        [Display(Name = "Gas Used (L)")]
        [Range(0, 10000, ErrorMessage = "Gas usage must be between 0 and 10000 liters")]
        public float? GasUsed_L { get; set; }

        [Display(Name = "Powder Used (L)")]
        [Range(0, 100, ErrorMessage = "Powder usage must be between 0 and 100 liters")]
        public float? PowderUsed_L { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        // Delay tracking (auto-triggered)
        public bool HasDelay { get; set; }
        public DelayInfo? DelayInfo { get; set; }

        // Available options
        public List<string> AvailablePrinters { get; set; } = new() { "TI1", "TI2", "INC" };
        public List<string> EndReasons { get; set; } = new() 
        { 
            "Completed", "Aborted", "Error", "Material Issue", "Power Failure", "Emergency Stop" 
        };
        public List<Part> AvailableParts { get; set; } = new();

        // For operator info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }

        // Build job info
        public int? BuildId { get; set; }
        public DateTime? ScheduledStartTime { get; set; }
    }

    /// <summary>
    /// Individual part entry in post-print form
    /// </summary>
    public class PostPrintPartEntry
    {
        [Required(ErrorMessage = "Part number is required")]
        [Display(Name = "Part Number")]
        public string PartNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, 1000, ErrorMessage = "Quantity must be between 1 and 1000")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Is Primary Part")]
        public bool IsPrimary { get; set; }

        // For "Add New" functionality
        public bool IsNewPart { get; set; }
        public string? Description { get; set; }
        public string? Material { get; set; }
    }

    /// <summary>
    /// Delay information when delays are detected
    /// </summary>
    public class DelayInfo
    {
        [Required(ErrorMessage = "Delay reason is required")]
        [Display(Name = "Delay Reason")]
        public string DelayReason { get; set; } = string.Empty;

        [Display(Name = "Delay Notes")]
        [StringLength(1000, ErrorMessage = "Delay notes cannot exceed 1000 characters")]
        public string? DelayNotes { get; set; }

        [Display(Name = "Calculated Delay (minutes)")]
        public int DelayDuration { get; set; }

        public List<string> AvailableReasons { get; set; } = DelayLog.DelayReasons.ToList();
    }

    /// <summary>
    /// Combined view model for the print tracking dashboard
    /// </summary>
    public class PrintTrackingDashboardViewModel
    {
        public List<BuildJob> ActiveBuilds { get; set; } = new();
        public List<BuildJob> RecentCompletedBuilds { get; set; } = new();
        public List<DelayLog> RecentDelays { get; set; } = new();
        
        // Quick stats
        public Dictionary<string, int> ActiveJobsByPrinter { get; set; } = new();
        public Dictionary<string, double> HoursToday { get; set; } = new();
        public int TotalDelaysToday { get; set; }
        public double AverageDelayMinutes { get; set; }

        // Current user info
        public string OperatorName { get; set; } = string.Empty;
        public int UserId { get; set; }
    }
}