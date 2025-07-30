using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing role-based permissions
/// Task 2: Admin Control System - RolePermission management
/// </summary>
public interface IRolePermissionService
{
    Task<List<RolePermission>> GetAllPermissionsAsync();
    Task<List<RolePermission>> GetPermissionsForRoleAsync(string roleName);
    Task<bool> HasPermissionAsync(string roleName, string permissionKey);
    Task<bool> UpdatePermissionAsync(string roleName, string permissionKey, bool hasPermission, string permissionLevel = "Read");
    Task<bool> CreatePermissionAsync(RolePermission permission);
    Task<bool> DeletePermissionAsync(int permissionId);
    Task<Dictionary<string, List<RolePermission>>> GetPermissionMatrixAsync();
    Task<bool> CopyPermissionsAsync(string fromRole, string toRole, string copiedBy);
}

public class RolePermissionService : IRolePermissionService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<RolePermissionService> _logger;

    public RolePermissionService(SchedulerContext context, ILogger<RolePermissionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<RolePermission>> GetAllPermissionsAsync()
    {
        try
        {
            return await _context.RolePermissions
                .Where(p => p.IsActive)
                .OrderBy(p => p.RoleName)
                .ThenBy(p => p.Category)
                .ThenBy(p => p.Priority)
                .ThenBy(p => p.PermissionKey)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all role permissions");
            return new List<RolePermission>();
        }
    }

    public async Task<List<RolePermission>> GetPermissionsForRoleAsync(string roleName)
    {
        try
        {
            return await _context.RolePermissions
                .Where(p => p.RoleName == roleName && p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Priority)
                .ThenBy(p => p.PermissionKey)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for role {RoleName}", roleName);
            return new List<RolePermission>();
        }
    }

    public async Task<bool> HasPermissionAsync(string roleName, string permissionKey)
    {
        try
        {
            var permission = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RoleName == roleName && 
                                         p.PermissionKey == permissionKey && 
                                         p.IsActive);

            return permission?.IsEffective ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {PermissionKey} for role {RoleName}", 
                permissionKey, roleName);
            return false;
        }
    }

    public async Task<bool> UpdatePermissionAsync(string roleName, string permissionKey, bool hasPermission, string permissionLevel = "Read")
    {
        try
        {
            var permission = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RoleName == roleName && p.PermissionKey == permissionKey);

            if (permission == null)
            {
                // Create new permission
                permission = new RolePermission
                {
                    RoleName = roleName,
                    PermissionKey = permissionKey,
                    HasPermission = hasPermission,
                    PermissionLevel = permissionLevel,
                    Category = GetPermissionCategory(permissionKey),
                    Description = GetPermissionDescription(permissionKey),
                    CreatedBy = "System",
                    LastModifiedBy = "System"
                };

                _context.RolePermissions.Add(permission);
            }
            else
            {
                permission.HasPermission = hasPermission;
                permission.PermissionLevel = permissionLevel;
                permission.LastModifiedDate = DateTime.UtcNow;
                permission.LastModifiedBy = "System";
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionKey} for role {RoleName} set to {HasPermission} ({Level})", 
                permissionKey, roleName, hasPermission, permissionLevel);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permission {PermissionKey} for role {RoleName}", 
                permissionKey, roleName);
            return false;
        }
    }

    public async Task<bool> CreatePermissionAsync(RolePermission permission)
    {
        try
        {
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(p => p.RoleName == permission.RoleName && 
                                         p.PermissionKey == permission.PermissionKey);

            if (existing != null)
                return false;

            _context.RolePermissions.Add(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionKey} created for role {RoleName}", 
                permission.PermissionKey, permission.RoleName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating permission {PermissionKey} for role {RoleName}", 
                permission.PermissionKey, permission.RoleName);
            return false;
        }
    }

    public async Task<bool> DeletePermissionAsync(int permissionId)
    {
        try
        {
            var permission = await _context.RolePermissions.FindAsync(permissionId);
            if (permission == null)
                return false;

            _context.RolePermissions.Remove(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Permission {PermissionId} deleted", permissionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting permission {PermissionId}", permissionId);
            return false;
        }
    }

    public async Task<Dictionary<string, List<RolePermission>>> GetPermissionMatrixAsync()
    {
        try
        {
            var permissions = await GetAllPermissionsAsync();
            return permissions.GroupBy(p => p.RoleName)
                             .ToDictionary(g => g.Key, g => g.ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permission matrix");
            return new Dictionary<string, List<RolePermission>>();
        }
    }

    public async Task<bool> CopyPermissionsAsync(string fromRole, string toRole, string copiedBy)
    {
        try
        {
            var sourcePermissions = await GetPermissionsForRoleAsync(fromRole);
            
            foreach (var sourcePermission in sourcePermissions)
            {
                var newPermission = new RolePermission
                {
                    RoleName = toRole,
                    PermissionKey = sourcePermission.PermissionKey,
                    HasPermission = sourcePermission.HasPermission,
                    PermissionLevel = sourcePermission.PermissionLevel,
                    Category = sourcePermission.Category,
                    Description = sourcePermission.Description,
                    Priority = sourcePermission.Priority,
                    Constraints = sourcePermission.Constraints,
                    CreatedBy = copiedBy,
                    LastModifiedBy = copiedBy
                };

                await UpdatePermissionAsync(toRole, newPermission.PermissionKey, 
                    newPermission.HasPermission, newPermission.PermissionLevel);
            }

            _logger.LogInformation("Permissions copied from {FromRole} to {ToRole} by {User}", 
                fromRole, toRole, copiedBy);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying permissions from {FromRole} to {ToRole}", fromRole, toRole);
            return false;
        }
    }

    private string GetPermissionCategory(string permissionKey)
    {
        if (permissionKey.StartsWith("Admin."))
            return "Admin";
        if (permissionKey.StartsWith("Scheduler."))
            return "Scheduler";
        if (permissionKey.StartsWith("Department."))
            return "Department";
        if (permissionKey.StartsWith("Feature."))
            return "Feature";
        
        // Legacy support for lowercase format
        if (permissionKey.StartsWith("admin."))
            return "Admin";
        if (permissionKey.StartsWith("scheduler."))
            return "Scheduler";
        if (permissionKey.StartsWith("department."))
            return "Department";
        if (permissionKey.StartsWith("feature."))
            return "Feature";
        
        return "General";
    }

    private string GetPermissionDescription(string permissionKey)
    {
        return permissionKey switch
        {
            PermissionKeys.AdminDashboard => "Access to admin dashboard",
            PermissionKeys.AdminUsers => "Manage user accounts",
            PermissionKeys.AdminViewUsers => "View user accounts",
            PermissionKeys.AdminRoles => "Manage user roles and permissions",
            PermissionKeys.AdminSettings => "Manage system settings",
            PermissionKeys.AdminMachines => "Manage machine configurations",
            PermissionKeys.AdminParts => "Manage part definitions",
            PermissionKeys.SchedulerView => "View scheduling interface",
            PermissionKeys.SchedulerCreate => "Create new jobs",
            PermissionKeys.SchedulerEdit => "Edit existing jobs",
            PermissionKeys.SchedulerDelete => "Delete jobs",
            
            // Legacy support for lowercase format
            "admin.dashboard" => "Access to admin dashboard",
            "admin.users" => "Manage user accounts",
            "admin.roles" => "Manage user roles and permissions",
            "admin.settings" => "Manage system settings",
            "admin.machines" => "Manage machine configurations", 
            "admin.parts" => "Manage part definitions",
            "scheduler.view" => "View scheduling interface",
            "scheduler.create" => "Create new jobs",
            "scheduler.edit" => "Edit existing jobs",
            "scheduler.delete" => "Delete jobs",
            
            _ => $"Permission for {permissionKey}"
        };
    }
}