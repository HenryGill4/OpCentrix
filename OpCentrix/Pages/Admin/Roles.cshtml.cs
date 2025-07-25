using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing role-based permissions
/// Task 4: Role-Based Permission Grid
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class RolesModel : PageModel
{
    private readonly IRolePermissionService _rolePermissionService;
    private readonly SchedulerContext _context;
    private readonly ILogger<RolesModel> _logger;

    public RolesModel(IRolePermissionService rolePermissionService, SchedulerContext context, ILogger<RolesModel> logger)
    {
        _rolePermissionService = rolePermissionService;
        _context = context;
        _logger = logger;
    }

    // Properties for the page
    public List<string> AvailableRoles { get; set; } = new();
    public List<string> AvailableFeatures { get; set; } = new();
    public Dictionary<string, Dictionary<string, RolePermission>> PermissionMatrix { get; set; } = new();
    public Dictionary<string, int> RoleUserCounts { get; set; } = new();

    [BindProperty]
    public List<PermissionUpdateModel> PermissionUpdates { get; set; } = new();

    [BindProperty]
    public string SourceRole { get; set; } = string.Empty;

    [BindProperty]
    public string TargetRole { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading role-based permissions page - User: {User}", User.Identity?.Name);

            await LoadPermissionDataAsync();

            _logger.LogInformation("Role permissions page loaded successfully - {RoleCount} roles, {FeatureCount} features",
                AvailableRoles.Count, AvailableFeatures.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading role permissions page");
            
            // Initialize with empty data on error
            AvailableRoles = new List<string>();
            AvailableFeatures = new List<string>();
            PermissionMatrix = new Dictionary<string, Dictionary<string, RolePermission>>();
            RoleUserCounts = new Dictionary<string, int>();
        }
    }

    public async Task<IActionResult> OnPostUpdatePermissionsAsync()
    {
        try
        {
            _logger.LogInformation("Admin {User} updating role permissions", User.Identity?.Name);

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var update in PermissionUpdates)
            {
                try
                {
                    var success = await _rolePermissionService.UpdatePermissionAsync(
                        update.Role, 
                        update.Feature, 
                        update.IsGranted, 
                        update.IsGranted ? "Write" : "Read");

                    if (success)
                    {
                        updatedCount++;
                        _logger.LogInformation("Permission updated: {Role} -> {Feature} = {IsGranted}", 
                            update.Role, update.Feature, update.IsGranted);
                    }
                    else
                    {
                        errors.Add($"Failed to update {update.Role} -> {update.Feature}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating permission for {Role} -> {Feature}", update.Role, update.Feature);
                    errors.Add($"Error updating {update.Role} -> {update.Feature}");
                }
            }

            if (errors.Any())
            {
                TempData["Error"] = $"Some permissions could not be updated: {string.Join(", ", errors)}";
            }

            if (updatedCount > 0)
            {
                TempData["Success"] = $"Successfully updated {updatedCount} permission(s).";
                _logger.LogInformation("Admin {User} successfully updated {Count} permissions", User.Identity?.Name, updatedCount);
            }

            if (updatedCount == 0 && !errors.Any())
            {
                TempData["Info"] = "No permission changes detected.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role permissions");
            TempData["Error"] = "An error occurred while updating permissions.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCopyPermissionsAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(SourceRole) || string.IsNullOrEmpty(TargetRole))
            {
                TempData["Error"] = "Please select both source and target roles.";
                return RedirectToPage();
            }

            if (SourceRole == TargetRole)
            {
                TempData["Error"] = "Source and target roles must be different.";
                return RedirectToPage();
            }

            _logger.LogInformation("Admin {User} copying permissions from {SourceRole} to {TargetRole}", 
                User.Identity?.Name, SourceRole, TargetRole);

            var success = await _rolePermissionService.CopyPermissionsAsync(
                SourceRole, 
                TargetRole, 
                User.Identity?.Name ?? "Unknown");

            if (success)
            {
                TempData["Success"] = $"Successfully copied permissions from {SourceRole} to {TargetRole}.";
                _logger.LogInformation("Permissions copied successfully from {SourceRole} to {TargetRole}", SourceRole, TargetRole);
            }
            else
            {
                TempData["Error"] = "Failed to copy permissions. Please check the role names and try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying permissions from {SourceRole} to {TargetRole}", SourceRole, TargetRole);
            TempData["Error"] = "An error occurred while copying permissions.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetRolePermissionsAsync(string roleName)
    {
        try
        {
            if (string.IsNullOrEmpty(roleName))
            {
                TempData["Error"] = "Role name is required.";
                return RedirectToPage();
            }

            _logger.LogWarning("Admin {User} resetting permissions for role: {Role}", User.Identity?.Name, roleName);

            // Reset to default permissions based on role
            var defaultPermissions = GetDefaultPermissionsForRole(roleName);
            var updatedCount = 0;

            foreach (var kvp in defaultPermissions)
            {
                var success = await _rolePermissionService.UpdatePermissionAsync(
                    roleName, 
                    kvp.Key, 
                    kvp.Value, 
                    kvp.Value ? "Write" : "Read");
                
                if (success) updatedCount++;
            }

            if (updatedCount > 0)
            {
                TempData["Success"] = $"Successfully reset {updatedCount} permissions for {roleName} to default values.";
                _logger.LogInformation("Role {Role} permissions reset to defaults", roleName);
            }
            else
            {
                TempData["Error"] = $"Failed to reset permissions for {roleName}.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting permissions for role {Role}", roleName);
            TempData["Error"] = "An error occurred while resetting permissions.";
        }

        return RedirectToPage();
    }

    // Helper methods
    private async Task LoadPermissionDataAsync()
    {
        // Get all available roles from users
        AvailableRoles = await _context.Users
            .Where(u => u.IsActive)
            .Select(u => u.Role)
            .Distinct()
            .OrderBy(r => r)
            .ToListAsync();

        // Get all available features from permission keys
        AvailableFeatures = GetAllPermissionKeys();

        // Get permission matrix
        var permissionsByRole = await _rolePermissionService.GetPermissionMatrixAsync();
        
        // Convert to the expected format
        PermissionMatrix = new Dictionary<string, Dictionary<string, RolePermission>>();
        foreach (var role in AvailableRoles)
        {
            PermissionMatrix[role] = new Dictionary<string, RolePermission>();
            
            if (permissionsByRole.ContainsKey(role))
            {
                foreach (var permission in permissionsByRole[role])
                {
                    PermissionMatrix[role][permission.PermissionKey] = permission;
                }
            }
        }

        // Get user counts for each role
        RoleUserCounts = await _context.Users
            .Where(u => u.IsActive)
            .GroupBy(u => u.Role)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    private List<string> GetAllPermissionKeys()
    {
        return new List<string>
        {
            PermissionKeys.AdminDashboard,
            PermissionKeys.AdminUsers,
            PermissionKeys.AdminRoles,
            PermissionKeys.AdminSettings,
            PermissionKeys.AdminMachines,
            PermissionKeys.AdminParts,
            PermissionKeys.AdminJobs,
            PermissionKeys.AdminShifts,
            PermissionKeys.AdminCheckpoints,
            PermissionKeys.AdminDefects,
            PermissionKeys.AdminArchive,
            PermissionKeys.AdminDatabase,
            PermissionKeys.AdminAlerts,
            PermissionKeys.AdminFeatures,
            PermissionKeys.AdminLogs,
            PermissionKeys.SchedulerView,
            PermissionKeys.SchedulerCreate,
            PermissionKeys.SchedulerEdit,
            PermissionKeys.SchedulerDelete,
            PermissionKeys.SchedulerReschedule,
            PermissionKeys.PrintingAccess,
            PermissionKeys.CoatingAccess,
            PermissionKeys.EDMAccess,
            PermissionKeys.MachiningAccess,
            PermissionKeys.QCAccess,
            PermissionKeys.ShippingAccess,
            PermissionKeys.MediaAccess,
            PermissionKeys.AnalyticsAccess,
            PermissionKeys.AdvancedReporting,
            PermissionKeys.OpcUaIntegration,
            PermissionKeys.BulkOperations,
            PermissionKeys.DataExport,
            PermissionKeys.MasterSchedule
        };
    }

    private Dictionary<string, bool> GetDefaultPermissionsForRole(string roleName)
    {
        return roleName switch
        {
            "Admin" => GetAllPermissionKeys().ToDictionary(k => k, k => true),
            "Manager" => new Dictionary<string, bool>
            {
                { PermissionKeys.AdminDashboard, true },
                { PermissionKeys.AdminUsers, false },
                { PermissionKeys.AdminRoles, false },
                { PermissionKeys.AdminSettings, false },
                { PermissionKeys.AdminMachines, true },
                { PermissionKeys.AdminParts, true },
                { PermissionKeys.AdminJobs, true },
                { PermissionKeys.AdminShifts, true },
                { PermissionKeys.AdminCheckpoints, true },
                { PermissionKeys.AdminDefects, true },
                { PermissionKeys.AdminArchive, false },
                { PermissionKeys.AdminDatabase, false },
                { PermissionKeys.AdminAlerts, true },
                { PermissionKeys.AdminFeatures, false },
                { PermissionKeys.AdminLogs, true },
                { PermissionKeys.SchedulerView, true },
                { PermissionKeys.SchedulerCreate, true },
                { PermissionKeys.SchedulerEdit, true },
                { PermissionKeys.SchedulerDelete, true },
                { PermissionKeys.SchedulerReschedule, true },
                { PermissionKeys.PrintingAccess, true },
                { PermissionKeys.CoatingAccess, true },
                { PermissionKeys.EDMAccess, true },
                { PermissionKeys.MachiningAccess, true },
                { PermissionKeys.QCAccess, true },
                { PermissionKeys.ShippingAccess, true },
                { PermissionKeys.MediaAccess, true },
                { PermissionKeys.AnalyticsAccess, true },
                { PermissionKeys.AdvancedReporting, true },
                { PermissionKeys.OpcUaIntegration, false },
                { PermissionKeys.BulkOperations, true },
                { PermissionKeys.DataExport, true },
                { PermissionKeys.MasterSchedule, true }
            },
            "Scheduler" => new Dictionary<string, bool>
            {
                { PermissionKeys.AdminDashboard, false },
                { PermissionKeys.AdminUsers, false },
                { PermissionKeys.AdminRoles, false },
                { PermissionKeys.AdminSettings, false },
                { PermissionKeys.AdminMachines, false },
                { PermissionKeys.AdminParts, false },
                { PermissionKeys.AdminJobs, false },
                { PermissionKeys.AdminShifts, false },
                { PermissionKeys.AdminCheckpoints, false },
                { PermissionKeys.AdminDefects, false },
                { PermissionKeys.AdminArchive, false },
                { PermissionKeys.AdminDatabase, false },
                { PermissionKeys.AdminAlerts, false },
                { PermissionKeys.AdminFeatures, false },
                { PermissionKeys.AdminLogs, false },
                { PermissionKeys.SchedulerView, true },
                { PermissionKeys.SchedulerCreate, true },
                { PermissionKeys.SchedulerEdit, true },
                { PermissionKeys.SchedulerDelete, false },
                { PermissionKeys.SchedulerReschedule, true },
                { PermissionKeys.PrintingAccess, false },
                { PermissionKeys.CoatingAccess, false },
                { PermissionKeys.EDMAccess, false },
                { PermissionKeys.MachiningAccess, false },
                { PermissionKeys.QCAccess, false },
                { PermissionKeys.ShippingAccess, false },
                { PermissionKeys.MediaAccess, false },
                { PermissionKeys.AnalyticsAccess, false },
                { PermissionKeys.AdvancedReporting, false },
                { PermissionKeys.OpcUaIntegration, false },
                { PermissionKeys.BulkOperations, false },
                { PermissionKeys.DataExport, false },
                { PermissionKeys.MasterSchedule, true }
            },
            "Operator" => new Dictionary<string, bool>
            {
                { PermissionKeys.SchedulerView, true },
                { PermissionKeys.SchedulerCreate, false },
                { PermissionKeys.SchedulerEdit, false },
                { PermissionKeys.SchedulerDelete, false },
                { PermissionKeys.SchedulerReschedule, false }
            },
            _ => GetAllPermissionKeys().ToDictionary(k => k, k => false) // Specialist roles - deny all by default
        };
    }

    public string GetFeatureDisplayName(string feature)
    {
        return feature switch
        {
            PermissionKeys.AdminDashboard => "?? Admin Dashboard",
            PermissionKeys.AdminUsers => "?? User Management",
            PermissionKeys.AdminRoles => "?? Role Management",
            PermissionKeys.AdminSettings => "?? System Settings",
            PermissionKeys.AdminMachines => "?? Machine Management",
            PermissionKeys.AdminParts => "?? Part Management",
            PermissionKeys.AdminJobs => "?? Job Management",
            PermissionKeys.AdminShifts => "?? Shift Management",
            PermissionKeys.AdminCheckpoints => "? Checkpoint Management",
            PermissionKeys.AdminDefects => "?? Defect Management",
            PermissionKeys.AdminArchive => "?? Archive Management",
            PermissionKeys.AdminDatabase => "??? Database Management",
            PermissionKeys.AdminAlerts => "?? Alert Management",
            PermissionKeys.AdminFeatures => "??? Feature Toggles",
            PermissionKeys.AdminLogs => "?? System Logs",
            PermissionKeys.SchedulerView => "??? View Schedule",
            PermissionKeys.SchedulerCreate => "? Create Jobs",
            PermissionKeys.SchedulerEdit => "?? Edit Jobs",
            PermissionKeys.SchedulerDelete => "??? Delete Jobs",
            PermissionKeys.SchedulerReschedule => "?? Reschedule Jobs",
            PermissionKeys.PrintingAccess => "??? 3D Printing",
            PermissionKeys.CoatingAccess => "?? Coating Operations",
            PermissionKeys.EDMAccess => "? EDM Operations",
            PermissionKeys.MachiningAccess => "?? Machining Operations",
            PermissionKeys.QCAccess => "? Quality Control",
            PermissionKeys.ShippingAccess => "?? Shipping Operations",
            PermissionKeys.MediaAccess => "?? Media Operations",
            PermissionKeys.AnalyticsAccess => "?? Analytics & Reports",
            PermissionKeys.AdvancedReporting => "?? Advanced Reporting",
            PermissionKeys.OpcUaIntegration => "?? OPC UA Integration",
            PermissionKeys.BulkOperations => "?? Bulk Operations",
            PermissionKeys.DataExport => "?? Data Export",
            PermissionKeys.MasterSchedule => "??? Master Schedule",
            _ => feature
        };
    }

    public string GetRoleDisplayName(string role)
    {
        return role switch
        {
            "Admin" => "??? Administrator",
            "Manager" => "????? Manager",
            "Scheduler" => "?? Scheduler",
            "Operator" => "?? Operator",
            "PrintingSpecialist" => "??? Printing Specialist",
            "CoatingSpecialist" => "?? Coating Specialist",
            "ShippingSpecialist" => "?? Shipping Specialist",
            "EDMSpecialist" => "? EDM Specialist",
            "MachiningSpecialist" => "?? Machining Specialist",
            "QCSpecialist" => "? QC Specialist",
            "MediaSpecialist" => "?? Media Specialist",
            "Analyst" => "?? Analyst",
            _ => role
        };
    }

    public string GetRoleDescription(string role)
    {
        return role switch
        {
            "Admin" => "Full system access with administrative privileges",
            "Manager" => "Management-level access to operations and scheduling",
            "Scheduler" => "Access to scheduling and job management functions",
            "Operator" => "Basic operational access with read-only permissions",
            "PrintingSpecialist" => "Specialized access to 3D printing operations",
            "CoatingSpecialist" => "Specialized access to coating operations",
            "ShippingSpecialist" => "Specialized access to shipping operations",
            "EDMSpecialist" => "Specialized access to EDM operations",
            "MachiningSpecialist" => "Specialized access to machining operations",
            "QCSpecialist" => "Specialized access to quality control functions",
            "MediaSpecialist" => "Specialized access to media and documentation",
            "Analyst" => "Access to analytics and reporting functions",
            _ => "Custom role with specific permissions"
        };
    }
}

/// <summary>
/// Model for permission updates from the UI
/// </summary>
public class PermissionUpdateModel
{
    public string Role { get; set; } = string.Empty;
    public string Feature { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
}