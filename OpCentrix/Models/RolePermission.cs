using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents role-based permissions for features and functionalities
/// Allows fine-grained control over what each user role can access
/// </summary>
public class RolePermission
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// User role name (Admin, Manager, Scheduler, Operator, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// Feature or permission identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string PermissionKey { get; set; } = string.Empty;

    /// <summary>
    /// Whether this role has access to this feature
    /// </summary>
    public bool HasPermission { get; set; } = false;

    /// <summary>
    /// Permission level (Read, Write, Delete, Admin)
    /// </summary>
    [StringLength(20)]
    public string PermissionLevel { get; set; } = "Read";

    /// <summary>
    /// Category for grouping related permissions
    /// </summary>
    [StringLength(50)]
    public string Category { get; set; } = "General";

    /// <summary>
    /// Human-readable description of this permission
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this permission is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority for permission checking (lower numbers = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Additional constraints or conditions for this permission (JSON format)
    /// </summary>
    [StringLength(1000)]
    public string Constraints { get; set; } = "{}";

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
    public string DisplayName => $"{RoleName} - {PermissionKey}";

    /// <summary>
    /// Helper property to check if permission allows write operations
    /// </summary>
    [NotMapped]
    public bool CanWrite => HasPermission && (PermissionLevel == "Write" || PermissionLevel == "Admin" || PermissionLevel == "Full");

    /// <summary>
    /// Helper property to check if permission allows delete operations
    /// </summary>
    [NotMapped]
    public bool CanDelete => HasPermission && (PermissionLevel == "Delete" || PermissionLevel == "Admin" || PermissionLevel == "Full");

    /// <summary>
    /// Helper property to check if permission allows admin operations
    /// </summary>
    [NotMapped]
    public bool CanAdmin => HasPermission && (PermissionLevel == "Admin" || PermissionLevel == "Full");

    /// <summary>
    /// Check if this permission is effective (active and has permission)
    /// </summary>
    public bool IsEffective => IsActive && HasPermission;

    /// <summary>
    /// Get permission constraints as a dictionary
    /// </summary>
    public Dictionary<string, object> GetConstraints()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Constraints) ?? new();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// Set permission constraints from a dictionary
    /// </summary>
    public void SetConstraints(Dictionary<string, object> constraints)
    {
        try
        {
            Constraints = System.Text.Json.JsonSerializer.Serialize(constraints);
        }
        catch
        {
            Constraints = "{}";
        }
    }

    /// <summary>
    /// Check if a specific constraint is met
    /// </summary>
    public bool CheckConstraint(string key, object value)
    {
        try
        {
            var constraints = GetConstraints();
            if (!constraints.ContainsKey(key))
                return true; // No constraint means allowed

            var constraintValue = constraints[key];
            return Equals(constraintValue, value);
        }
        catch
        {
            return true; // Default to allowed if constraint check fails
        }
    }
}

/// <summary>
/// Common permission keys used throughout the application
/// </summary>
public static class PermissionKeys
{
    // Admin permissions
    public const string AdminDashboard = "admin.dashboard";
    public const string AdminUsers = "admin.users";
    public const string AdminRoles = "admin.roles";
    public const string AdminSettings = "admin.settings";
    public const string AdminMachines = "admin.machines";
    public const string AdminParts = "admin.parts";
    public const string AdminJobs = "admin.jobs";
    public const string AdminShifts = "admin.shifts";
    public const string AdminCheckpoints = "admin.checkpoints";
    public const string AdminDefects = "admin.defects";
    public const string AdminArchive = "admin.archive";
    public const string AdminDatabase = "admin.database";
    public const string AdminAlerts = "admin.alerts";
    public const string AdminFeatures = "admin.features";
    public const string AdminLogs = "admin.logs";

    // Scheduler permissions
    public const string SchedulerView = "scheduler.view";
    public const string SchedulerCreate = "scheduler.create";
    public const string SchedulerEdit = "scheduler.edit";
    public const string SchedulerDelete = "scheduler.delete";
    public const string SchedulerReschedule = "scheduler.reschedule";

    // Department permissions
    public const string PrintingAccess = "department.printing";
    public const string CoatingAccess = "department.coating";
    public const string EDMAccess = "department.edm";
    public const string MachiningAccess = "department.machining";
    public const string QCAccess = "department.qc";
    public const string ShippingAccess = "department.shipping";
    public const string MediaAccess = "department.media";
    public const string AnalyticsAccess = "department.analytics";

    // Feature permissions
    public const string AdvancedReporting = "feature.advanced_reporting";
    public const string OpcUaIntegration = "feature.opcua_integration";
    public const string BulkOperations = "feature.bulk_operations";
    public const string DataExport = "feature.data_export";
    public const string MasterSchedule = "feature.master_schedule";
}

/// <summary>
/// Permission levels used throughout the application
/// </summary>
public static class PermissionLevels
{
    public const string Read = "Read";
    public const string Write = "Write";
    public const string Delete = "Delete";
    public const string Admin = "Admin";
    public const string Full = "Full";
}