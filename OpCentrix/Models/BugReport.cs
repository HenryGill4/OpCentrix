using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace OpCentrix.Models
{
    /// <summary>
    /// Bug Report model for tracking issues across all OpCentrix pages
    /// Auto-fills with page information and logs to master bug log with separate page-specific files
    /// </summary>
    public class BugReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string BugId { get; set; } = Guid.NewGuid().ToString("N")[..12];

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium";

        [Required]
        [StringLength(20)]
        public string Priority { get; set; } = "Medium";

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "New";

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = "General";

        // Page Context Information
        [Required]
        [StringLength(200)]
        public string PageUrl { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string PageName { get; set; } = "";

        [StringLength(50)]
        public string PageArea { get; set; } = "";

        [StringLength(50)]
        public string PageController { get; set; } = "";

        [StringLength(50)]
        public string PageAction { get; set; } = "";

        // User Context Information
        [Required]
        [StringLength(100)]
        public string ReportedBy { get; set; } = "";

        [StringLength(20)]
        public string UserRole { get; set; } = "";

        [StringLength(100)]
        public string UserEmail { get; set; } = "";

        [Required]
        public DateTime ReportedDate { get; set; } = DateTime.UtcNow;

        // Browser/Environment Information
        [StringLength(500)]
        public string UserAgent { get; set; } = "";

        [StringLength(50)]
        public string BrowserName { get; set; } = "";

        [StringLength(20)]
        public string BrowserVersion { get; set; } = "";

        [StringLength(50)]
        public string OperatingSystem { get; set; } = "";

        [StringLength(20)]
        public string ScreenResolution { get; set; } = "";

        [StringLength(50)]
        public string IpAddress { get; set; } = "";

        // Error Details
        [StringLength(100)]
        public string ErrorType { get; set; } = "";

        public string ErrorMessage { get; set; } = "";

        public string StackTrace { get; set; } = "";

        [StringLength(50)]
        public string OperationId { get; set; } = "";

        // Reproduction Information
        public string StepsToReproduce { get; set; } = "";

        public string ExpectedBehavior { get; set; } = "";

        public string ActualBehavior { get; set; } = "";

        // Additional Context
        public string AdditionalNotes { get; set; } = "";

        [StringLength(500)]
        public string AttachedFiles { get; set; } = "";

        public string FormData { get; set; } = "";

        public string NetworkRequests { get; set; } = "";

        public string ConsoleErrors { get; set; } = "";

        // Assignment and Resolution
        [StringLength(100)]
        public string AssignedTo { get; set; } = "";

        public DateTime? AssignedDate { get; set; }

        [StringLength(100)]
        public string ResolvedBy { get; set; } = "";

        public DateTime? ResolvedDate { get; set; }

        public string ResolutionNotes { get; set; } = "";

        [StringLength(50)]
        public string ResolutionType { get; set; } = "";

        // Tracking and Analytics
        public int ViewCount { get; set; } = 0;

        public int VoteCount { get; set; } = 0;

        public DateTime? LastViewedDate { get; set; }

        [StringLength(100)]
        public string LastViewedBy { get; set; } = "";

        public bool IsReproduced { get; set; } = false;

        [StringLength(100)]
        public string ReproducedBy { get; set; } = "";

        public DateTime? ReproducedDate { get; set; }

        // System Fields
        public bool IsActive { get; set; } = true;

        public bool IsPublic { get; set; } = false;

        public bool NotifyReporter { get; set; } = true;

        [StringLength(100)]
        public string CreatedBy { get; set; } = "";

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string LastModifiedBy { get; set; } = "";

        public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

        // Related Bug Reports
        [StringLength(200)]
        public string RelatedBugIds { get; set; } = "";

        [StringLength(200)]
        public string DuplicateOf { get; set; } = "";

        // Performance Impact
        [StringLength(20)]
        public string PerformanceImpact { get; set; } = "None";

        public decimal? PageLoadTime { get; set; }

        public decimal? MemoryUsage { get; set; }

        public decimal? CpuUsage { get; set; }

        // Custom Tags and Metadata
        [StringLength(500)]
        public string Tags { get; set; } = "";

        public string CustomMetadata { get; set; } = "{}";

        // Helper properties for JSON serialization
        [NotMapped]
        public List<string> AttachedFilesList
        {
            get => string.IsNullOrEmpty(AttachedFiles) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(AttachedFiles) ?? new List<string>();
            set => AttachedFiles = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> RelatedBugIdsList
        {
            get => string.IsNullOrEmpty(RelatedBugIds) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(RelatedBugIds) ?? new List<string>();
            set => RelatedBugIds = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<string> TagsList
        {
            get => string.IsNullOrEmpty(Tags) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
            set => Tags = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dictionary<string, object> CustomMetadataDict
        {
            get => string.IsNullOrEmpty(CustomMetadata) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(CustomMetadata) ?? new Dictionary<string, object>();
            set => CustomMetadata = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public Dictionary<string, object> FormDataDict
        {
            get => string.IsNullOrEmpty(FormData) ? new Dictionary<string, object>() : JsonSerializer.Deserialize<Dictionary<string, object>>(FormData) ?? new Dictionary<string, object>();
            set => FormData = JsonSerializer.Serialize(value);
        }

        // Helper methods
        public string GetSeverityIcon()
        {
            return Severity.ToLower() switch
            {
                "critical" => "??",
                "high" => "??",
                "medium" => "??",
                "low" => "??",
                _ => "?"
            };
        }

        public string GetStatusIcon()
        {
            return Status.ToLower() switch
            {
                "new" => "??",
                "inprogress" => "??",
                "resolved" => "?",
                "closed" => "??",
                "reopened" => "??",
                _ => "?"
            };
        }

        public string GetCategoryIcon()
        {
            return Category.ToLower() switch
            {
                "ui" => "??",
                "backend" => "??",
                "database" => "???",
                "performance" => "?",
                "security" => "??",
                "api" => "??",
                "forms" => "??",
                "navigation" => "??",
                "authentication" => "??",
                "reporting" => "??",
                _ => "??"
            };
        }

        public bool IsHighPriority()
        {
            return Priority.ToLower() is "critical" or "high" || Severity.ToLower() is "critical" or "high";
        }

        public bool IsRecent()
        {
            return ReportedDate > DateTime.UtcNow.AddDays(-7);
        }

        public int GetAgeInDays()
        {
            return (DateTime.UtcNow - ReportedDate).Days;
        }

        public string GetFormattedAge()
        {
            var age = GetAgeInDays();
            return age switch
            {
                0 => "Today",
                1 => "Yesterday",
                < 7 => $"{age} days ago",
                < 30 => $"{age / 7} weeks ago",
                < 365 => $"{age / 30} months ago",
                _ => $"{age / 365} years ago"
            };
        }
    }

    /// <summary>
    /// Bug Report Statistics for dashboard display
    /// </summary>
    public class BugReportStatistics
    {
        public int TotalBugs { get; set; }
        public int NewBugs { get; set; }
        public int InProgressBugs { get; set; }
        public int ResolvedBugs { get; set; }
        public int CriticalBugs { get; set; }
        public int HighPriorityBugs { get; set; }
        public int BugsThisWeek { get; set; }
        public int BugsThisMonth { get; set; }
        public decimal AverageResolutionDays { get; set; }
        public Dictionary<string, int> BugsByPage { get; set; } = new();
        public Dictionary<string, int> BugsByCategory { get; set; } = new();
        public Dictionary<string, int> BugsBySeverity { get; set; } = new();
        public List<BugReport> RecentBugs { get; set; } = new();
        public List<BugReport> HighPriorityBugsList { get; set; } = new();
    }

    /// <summary>
    /// Bug Report Filter for searching and filtering
    /// </summary>
    public class BugReportFilter
    {
        public string? SearchTerm { get; set; }
        public string? Status { get; set; }
        public string? Severity { get; set; }
        public string? Priority { get; set; }
        public string? Category { get; set; }
        public string? PageArea { get; set; }
        public string? AssignedTo { get; set; }
        public string? ReportedBy { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsHighPriority { get; set; }
        public bool? IsRecent { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortBy { get; set; } = "ReportedDate";
        public string SortDirection { get; set; } = "desc";

        /// <summary>
        /// Get route data for pagination links
        /// </summary>
        public Dictionary<string, string?> GetRouteData()
        {
            var routeData = new Dictionary<string, string?>();
            
            if (!string.IsNullOrEmpty(SearchTerm)) routeData["Filter.SearchTerm"] = SearchTerm;
            if (!string.IsNullOrEmpty(Status)) routeData["Filter.Status"] = Status;
            if (!string.IsNullOrEmpty(Severity)) routeData["Filter.Severity"] = Severity;
            if (!string.IsNullOrEmpty(Priority)) routeData["Filter.Priority"] = Priority;
            if (!string.IsNullOrEmpty(Category)) routeData["Filter.Category"] = Category;
            if (!string.IsNullOrEmpty(PageArea)) routeData["Filter.PageArea"] = PageArea;
            if (!string.IsNullOrEmpty(AssignedTo)) routeData["Filter.AssignedTo"] = AssignedTo;
            if (!string.IsNullOrEmpty(ReportedBy)) routeData["Filter.ReportedBy"] = ReportedBy;
            if (FromDate.HasValue) routeData["Filter.FromDate"] = FromDate.Value.ToString("yyyy-MM-dd");
            if (ToDate.HasValue) routeData["Filter.ToDate"] = ToDate.Value.ToString("yyyy-MM-dd");
            if (IsHighPriority.HasValue) routeData["Filter.IsHighPriority"] = IsHighPriority.Value.ToString();
            if (IsRecent.HasValue) routeData["Filter.IsRecent"] = IsRecent.Value.ToString();
            routeData["Filter.PageSize"] = PageSize.ToString();
            routeData["Filter.SortBy"] = SortBy;
            routeData["Filter.SortDirection"] = SortDirection;
            
            return routeData;
        }
    }
}