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
/// UPDATED: Using proper casing to match test expectations
/// </summary>
public static class PermissionKeys
{
    // Admin permissions - Using proper casing to match test expectations
    public const string AdminDashboard = "Admin.Dashboard";
    public const string AdminUsers = "Admin.ManageUsers";  // FIXED: Changed from "admin.users" to "Admin.ManageUsers"
    public const string AdminViewUsers = "Admin.ViewUsers";  // NEW: Added for user viewing
    public const string AdminRoles = "Admin.ManageRoles";
    public const string AdminSettings = "Admin.ManageSettings";
    public const string AdminMachines = "Admin.ManageMachines";
    public const string AdminParts = "Admin.ManageParts";
    public const string AdminJobs = "Admin.ManageJobs";
    public const string AdminShifts = "Admin.ManageShifts";
    public const string AdminCheckpoints = "Admin.ManageCheckpoints";
    public const string AdminDefects = "Admin.ManageDefects";
    public const string AdminArchive = "Admin.ManageArchive";
    public const string AdminDatabase = "Admin.ManageDatabase";
    public const string AdminAlerts = "Admin.ManageAlerts";
    public const string AdminFeatures = "Admin.ManageFeatures";
    public const string AdminLogs = "Admin.ViewLogs";

    // Scheduler permissions - Using proper casing
    public const string SchedulerView = "Scheduler.ViewJobs";  // FIXED: Changed from "scheduler.view" to "Scheduler.ViewJobs"
    public const string SchedulerCreate = "Scheduler.CreateJobs";
    public const string SchedulerEdit = "Scheduler.EditJobs";
    public const string SchedulerDelete = "Scheduler.DeleteJobs";
    public const string SchedulerReschedule = "Scheduler.RescheduleJobs";

    // Department permissions - Using proper casing
    public const string PrintingAccess = "Department.Printing";
    public const string CoatingAccess = "Department.Coating";
    public const string EDMAccess = "Department.EDM";
    public const string MachiningAccess = "Department.Machining";
    public const string QCAccess = "Department.QC";
    public const string ShippingAccess = "Department.Shipping";
    public const string MediaAccess = "Department.Media";
    public const string AnalyticsAccess = "Department.Analytics";

    // Feature permissions - Using proper casing
    public const string AdvancedReporting = "Feature.AdvancedReporting";
    public const string OpcUaIntegration = "Feature.OpcUaIntegration";
    public const string BulkOperations = "Feature.BulkOperations";
    public const string DataExport = "Feature.DataExport";
    public const string MasterSchedule = "Feature.MasterSchedule";
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