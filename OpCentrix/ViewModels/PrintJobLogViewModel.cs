using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OpCentrix.Models;

namespace OpCentrix.ViewModels
{
    /// <summary>
    /// View model for the Print Job Log page showing comprehensive Parts ? Jobs ? BuildJobs tracking
    /// </summary>
    public class PrintJobLogViewModel
    {
        public List<PrintJobLogEntry> LogEntries { get; set; } = new List<PrintJobLogEntry>();
        public PrintJobStatistics Statistics { get; set; } = new PrintJobStatistics();
        
        // Filter properties
        [Display(Name = "Search")]
        public string? SearchTerm { get; set; }
        
        [Display(Name = "Status")]
        public string? StatusFilter { get; set; }
        
        [Display(Name = "Machine")]
        public string? MachineFilter { get; set; }
        
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime? StartDate { get; set; }
        
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime? EndDate { get; set; }
        
        // Pagination
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        
        // Filter options for dropdowns
        public List<string> AvailableStatuses { get; set; } = new List<string>();
        public List<string> AvailableMachines { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents a complete print job log entry showing the full lifecycle
    /// </summary>
    public class PrintJobLogEntry
    {
        // Part Information
        public int? PartId { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public string PartName { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string SlsMaterial { get; set; } = string.Empty;
        
        // Job Information
        public int? ScheduledJobId { get; set; }
        public DateTime? ScheduledStart { get; set; }
        public DateTime? ScheduledEnd { get; set; }
        public double EstimatedHours { get; set; }
        public int Priority { get; set; }
        public string JobStatus { get; set; } = string.Empty;
        public string JobCreatedBy { get; set; } = string.Empty;
        public DateTime? JobCreatedDate { get; set; }
        
        // BuildJob Information
        public int? BuildJobId { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? ActualEnd { get; set; }
        public string BuildJobStatus { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        
        // Calculated Properties
        public TimeSpan? ActualDuration => ActualEnd.HasValue && ActualStart.HasValue 
            ? ActualEnd.Value - ActualStart.Value 
            : null;
            
        public double ActualHours => ActualDuration?.TotalHours ?? 0;
        
        public string OverallStatus
        {
            get
            {
                if (!string.IsNullOrEmpty(BuildJobStatus))
                {
                    return BuildJobStatus switch
                    {
                        "In Progress" => "?? Printing",
                        "Completed" => "?? Complete",
                        "Aborted" => "?? Aborted",
                        "Error" => "?? Error",
                        _ => $"?? {BuildJobStatus}"
                    };
                }
                
                if (!string.IsNullOrEmpty(JobStatus))
                {
                    return JobStatus switch
                    {
                        "Scheduled" => "?? Scheduled",
                        "Ready" => "? Ready",
                        "Cancelled" => "? Cancelled",
                        _ => $"?? {JobStatus}"
                    };
                }
                
                return "? Unknown";
            }
        }
        
        public string LifecycleStage
        {
            get
            {
                if (ActualEnd.HasValue) return "Complete";
                if (ActualStart.HasValue) return "In Progress";
                if (ScheduledStart.HasValue) return "Scheduled";
                return "Planning";
            }
        }
        
        public double EfficiencyPercentage
        {
            get
            {
                if (EstimatedHours > 0 && ActualHours > 0)
                {
                    return Math.Round((EstimatedHours / ActualHours) * 100, 1);
                }
                return 0;
            }
        }
        
        public string PerformanceIndicator
        {
            get
            {
                var efficiency = EfficiencyPercentage;
                if (efficiency == 0) return "? N/A";
                if (efficiency >= 95) return "?? Excellent";
                if (efficiency >= 85) return "?? Good";
                if (efficiency >= 75) return "?? Fair";
                return "?? Poor";
            }
        }
    }

    /// <summary>
    /// Statistics for the print job log dashboard
    /// </summary>
    public class PrintJobStatistics
    {
        // Counts
        public int TotalJobs { get; set; }
        public int CompletedJobs { get; set; }
        public int InProgressJobs { get; set; }
        public int ScheduledJobs { get; set; }
        public int CancelledJobs { get; set; }
        
        // Time Analytics
        public double TotalEstimatedHours { get; set; }
        public double TotalActualHours { get; set; }
        public double AverageJobDuration { get; set; }
        public double OverallEfficiency { get; set; }
        
        // Production Analytics
        public Dictionary<string, int> JobsByMachine { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> JobsByMaterial { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> EfficiencyByMachine { get; set; } = new Dictionary<string, double>();
        
        // Recent Activity
        public List<RecentActivity> RecentActivities { get; set; } = new List<RecentActivity>();
        
        // Performance Metrics
        public double OnTimePerformance { get; set; }
        public double QualityRate { get; set; }
        public int PartsProduced { get; set; }
        
        // Calculated Properties
        public double CompletionRate => TotalJobs > 0 ? (double)CompletedJobs / TotalJobs * 100 : 0;
        public double ScheduleAdherence => OverallEfficiency;
        public string PerformanceTrend => OverallEfficiency >= 95 ? "?? Excellent" : 
                                         OverallEfficiency >= 85 ? "?? Good" : "?? Needs Improvement";
    }

    /// <summary>
    /// Recent activity item for the dashboard
    /// </summary>
    public class RecentActivity
    {
        public DateTime Timestamp { get; set; }
        public string Activity { get; set; } = string.Empty;
        public string PartNumber { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public string Operator { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty; // "Started", "Completed", "Cancelled", etc.
        
        public string ActivityIcon => ActivityType switch
        {
            "Started" => "??",
            "Completed" => "?",
            "Cancelled" => "?",
            "Error" => "??",
            "Scheduled" => "??",
            _ => "??"
        };
        
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.UtcNow - Timestamp;
                if (timeSpan.TotalMinutes < 1) return "Just now";
                if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays}d ago";
                return Timestamp.ToString("MMM dd");
            }
        }
    }
}