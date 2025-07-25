using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents feature toggle configurations for runtime feature management
/// Allows enabling/disabling experimental or optional features without code deployment
/// </summary>
public class FeatureToggle
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique feature identifier/key
    /// </summary>
    [Required]
    [StringLength(100)]
    public string FeatureName { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable display name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the feature
    /// </summary>
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this feature is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// Feature category for organization (UI, API, Process, Integration, etc.)
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "General";

    /// <summary>
    /// Environment where this feature is available (Development, Staging, Production, All)
    /// </summary>
    [StringLength(20)]
    public string Environment { get; set; } = "All";

    /// <summary>
    /// Minimum required role to access this feature
    /// </summary>
    [StringLength(50)]
    public string RequiredRole { get; set; } = "User";

    /// <summary>
    /// Feature rollout percentage (0-100) for gradual feature rollout
    /// </summary>
    [Range(0, 100)]
    public int RolloutPercentage { get; set; } = 100;

    /// <summary>
    /// Start date for when this feature becomes available
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for when this feature expires/gets removed
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Whether changes to this feature require application restart
    /// </summary>
    public bool RequiresRestart { get; set; } = false;

    /// <summary>
    /// Feature dependencies (comma-separated list of other feature names)
    /// </summary>
    [StringLength(500)]
    public string Dependencies { get; set; } = string.Empty;

    /// <summary>
    /// Features that conflict with this one (comma-separated list)
    /// </summary>
    [StringLength(500)]
    public string Conflicts { get; set; } = string.Empty;

    /// <summary>
    /// Configuration parameters for the feature (JSON format)
    /// </summary>
    [StringLength(2000)]
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// Usage tracking - how many times this feature has been accessed
    /// </summary>
    public long UsageCount { get; set; } = 0;

    /// <summary>
    /// Last time this feature was accessed
    /// </summary>
    public DateTime? LastUsed { get; set; }

    /// <summary>
    /// Performance impact notes or warnings
    /// </summary>
    [StringLength(500)]
    public string PerformanceNotes { get; set; } = string.Empty;

    /// <summary>
    /// Security considerations or warnings
    /// </summary>
    [StringLength(500)]
    public string SecurityNotes { get; set; } = string.Empty;

    /// <summary>
    /// Version when this feature was introduced
    /// </summary>
    [StringLength(20)]
    public string IntroducedInVersion { get; set; } = string.Empty;

    /// <summary>
    /// Planned version for feature removal or graduation
    /// </summary>
    [StringLength(20)]
    public string PlannedRemovalVersion { get; set; } = string.Empty;

    /// <summary>
    /// Feature status (Experimental, Beta, Stable, Deprecated)
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Experimental";

    /// <summary>
    /// Sort order for admin interface display
    /// </summary>
    public int SortOrder { get; set; } = 100;

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
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayText => $"{Category} - {DisplayName}";

    /// <summary>
    /// Helper property to check if feature is currently available
    /// </summary>
    [NotMapped]
    public bool IsAvailable
    {
        get
        {
            if (!IsEnabled)
                return false;

            var now = DateTime.UtcNow;

            if (StartDate.HasValue && now < StartDate.Value)
                return false;

            if (EndDate.HasValue && now > EndDate.Value)
                return false;

            return true;
        }
    }

    /// <summary>
    /// Helper property to get status color for UI
    /// </summary>
    [NotMapped]
    public string StatusColor => Status switch
    {
        "Experimental" => "#F59E0B", // orange
        "Beta" => "#3B82F6", // blue
        "Stable" => "#10B981", // green
        "Deprecated" => "#6B7280", // gray
        _ => "#6B7280"
    };

    /// <summary>
    /// Check if this feature is available for a specific user role
    /// </summary>
    public bool IsAvailableForRole(string userRole)
    {
        if (!IsAvailable)
            return false;

        // Admin can access all features
        if (userRole == "Admin")
            return true;

        // Check role hierarchy
        var roleHierarchy = new Dictionary<string, int>
        {
            ["Admin"] = 100,
            ["Manager"] = 80,
            ["Scheduler"] = 60,
            ["Operator"] = 40,
            ["User"] = 20,
            ["Guest"] = 0
        };

        var requiredLevel = roleHierarchy.GetValueOrDefault(RequiredRole, 0);
        var userLevel = roleHierarchy.GetValueOrDefault(userRole, 0);

        return userLevel >= requiredLevel;
    }

    /// <summary>
    /// Check if this feature is available in the current environment
    /// </summary>
    public bool IsAvailableInEnvironment(string currentEnvironment)
    {
        if (Environment == "All")
            return true;

        return string.Equals(Environment, currentEnvironment, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Check if all dependencies are satisfied
    /// </summary>
    public bool AreDependenciesSatisfied(IEnumerable<FeatureToggle> allFeatures)
    {
        if (string.IsNullOrEmpty(Dependencies))
            return true;

        var dependencyList = Dependencies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(d => d.Trim())
                                        .ToList();

        foreach (var dependency in dependencyList)
        {
            var dependentFeature = allFeatures.FirstOrDefault(f => f.FeatureName == dependency);
            if (dependentFeature == null || !dependentFeature.IsAvailable)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Check if there are any conflicting features enabled
    /// </summary>
    public bool HasConflicts(IEnumerable<FeatureToggle> allFeatures)
    {
        if (string.IsNullOrEmpty(Conflicts))
            return false;

        var conflictList = Conflicts.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                   .Select(c => c.Trim())
                                   .ToList();

        foreach (var conflict in conflictList)
        {
            var conflictingFeature = allFeatures.FirstOrDefault(f => f.FeatureName == conflict);
            if (conflictingFeature != null && conflictingFeature.IsAvailable)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Check if user should see this feature based on rollout percentage
    /// </summary>
    public bool ShouldShowForUser(string userId)
    {
        if (RolloutPercentage >= 100)
            return true;

        if (RolloutPercentage <= 0)
            return false;

        // Use hash of user ID to determine if they're in the rollout percentage
        var hash = userId.GetHashCode();
        var userPercentile = Math.Abs(hash) % 100;
        return userPercentile < RolloutPercentage;
    }

    /// <summary>
    /// Get feature configuration as a dictionary
    /// </summary>
    public Dictionary<string, object> GetConfiguration()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Configuration) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Set feature configuration from a dictionary
    /// </summary>
    public void SetConfiguration(Dictionary<string, object> config)
    {
        try
        {
            Configuration = System.Text.Json.JsonSerializer.Serialize(config);
        }
        catch
        {
            Configuration = "{}";
        }
    }

    /// <summary>
    /// Record feature usage
    /// </summary>
    public void RecordUsage()
    {
        UsageCount++;
        LastUsed = DateTime.UtcNow;
    }

    /// <summary>
    /// Get dependencies as a list
    /// </summary>
    public List<string> GetDependenciesList()
    {
        if (string.IsNullOrEmpty(Dependencies))
            return new List<string>();

        return Dependencies.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(d => d.Trim())
                          .ToList();
    }

    /// <summary>
    /// Get conflicts as a list
    /// </summary>
    public List<string> GetConflictsList()
    {
        if (string.IsNullOrEmpty(Conflicts))
            return new List<string>();

        return Conflicts.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(c => c.Trim())
                       .ToList();
    }
}

/// <summary>
/// Common feature categories
/// </summary>
public static class FeatureCategories
{
    public const string UI = "UI";
    public const string API = "API";
    public const string Process = "Process";
    public const string Integration = "Integration";
    public const string Analytics = "Analytics";
    public const string Performance = "Performance";
    public const string Security = "Security";
    public const string Experimental = "Experimental";
}

/// <summary>
/// Feature statuses
/// </summary>
public static class FeatureStatuses
{
    public const string Experimental = "Experimental";
    public const string Beta = "Beta";
    public const string Stable = "Stable";
    public const string Deprecated = "Deprecated";
}

/// <summary>
/// Common feature names used throughout the application
/// </summary>
public static class FeatureNames
{
    public const string AdvancedScheduling = "advanced_scheduling";
    public const string OpcUaIntegration = "opcua_integration";
    public const string MasterScheduleView = "master_schedule_view";
    public const string BulkJobOperations = "bulk_job_operations";
    public const string AdvancedAnalytics = "advanced_analytics";
    public const string RealTimeNotifications = "realtime_notifications";
    public const string MultiSiteSupport = "multisite_support";
    public const string CustomReporting = "custom_reporting";
    public const string MobileAccess = "mobile_access";
    public const string DataExportImport = "data_export_import";
    public const string AutoBackup = "auto_backup";
    public const string PredictiveMaintenance = "predictive_maintenance";
    public const string QualityPrediction = "quality_prediction";
    public const string CostOptimization = "cost_optimization";
    public const string ScheduleOptimization = "schedule_optimization";
}