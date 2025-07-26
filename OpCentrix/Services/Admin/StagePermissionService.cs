using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing stage-specific permissions
/// Task 11: Multi-Stage Scheduling
/// </summary>
public interface IStagePermissionService
{
    // Permission checking
    Task<bool> CanUserAccessStageAsync(string userId, string department, string action = "View");
    Task<bool> CanUserStartStageAsync(string userId, int stageId);
    Task<bool> CanUserCompleteStageAsync(string userId, int stageId);
    Task<bool> CanUserEditStageAsync(string userId, int stageId);
    Task<bool> CanUserDeleteStageAsync(string userId, int stageId);
    
    // Department permissions
    Task<List<string>> GetUserDepartmentsAsync(string userId);
    Task<List<string>> GetAllDepartmentsAsync();
    Task<bool> HasDepartmentPermissionAsync(string userId, string department, string permission);
    
    // Stage type permissions
    Task<bool> CanUserCreateStageTypeAsync(string userId, string stageType);
    Task<List<string>> GetAllowedStageTypesAsync(string userId);
    
    // Machine permissions for stages
    Task<bool> CanUserAssignMachineAsync(string userId, string machineId, string department);
    Task<List<string>> GetAssignableMachinesAsync(string userId, string department);
    
    // Operator permissions
    Task<bool> CanUserAssignOperatorAsync(string userId, string operatorId, int stageId);
    Task<List<User>> GetAssignableOperatorsAsync(string userId, string department);
    
    // Administrative permissions
    Task<bool> CanUserManageStagePermissionsAsync(string userId);
    Task<bool> CanUserViewAllStagesAsync(string userId);
    Task<bool> CanUserRescheduleStagesAsync(string userId);
}

public class StagePermissionService : IStagePermissionService
{
    private readonly SchedulerContext _context;
    private readonly IRolePermissionService _rolePermissionService;
    private readonly ILogger<StagePermissionService> _logger;

    // Stage permission keys
    public static class StagePermissions
    {
        public const string ViewStages = "Stages.View";
        public const string CreateStages = "Stages.Create";
        public const string EditStages = "Stages.Edit";
        public const string DeleteStages = "Stages.Delete";
        public const string StartStages = "Stages.Start";
        public const string CompleteStages = "Stages.Complete";
        public const string RescheduleStages = "Stages.Reschedule";
        public const string ManagePermissions = "Stages.ManagePermissions";
        public const string ViewAllDepartments = "Stages.ViewAllDepartments";
        public const string AssignOperators = "Stages.AssignOperators";
        public const string AssignMachines = "Stages.AssignMachines";
    }

    // Department-specific permissions
    public static class DepartmentPermissions
    {
        public const string PrintingView = "Department.Printing.View";
        public const string PrintingEdit = "Department.Printing.Edit";
        public const string PrintingStart = "Department.Printing.Start";
        public const string PrintingComplete = "Department.Printing.Complete";
        
        public const string EdmView = "Department.EDM.View";
        public const string EdmEdit = "Department.EDM.Edit";
        public const string EdmStart = "Department.EDM.Start";
        public const string EdmComplete = "Department.EDM.Complete";
        
        public const string CerakotingView = "Department.Cerakoting.View";
        public const string CerakotingEdit = "Department.Cerakoting.Edit";
        public const string CerakotingStart = "Department.Cerakoting.Start";
        public const string CerakotingComplete = "Department.Cerakoting.Complete";
        
        public const string AssemblyView = "Department.Assembly.View";
        public const string AssemblyEdit = "Department.Assembly.Edit";
        public const string AssemblyStart = "Department.Assembly.Start";
        public const string AssemblyComplete = "Department.Assembly.Complete";
        
        public const string InspectionView = "Department.Inspection.View";
        public const string InspectionEdit = "Department.Inspection.Edit";
        public const string InspectionStart = "Department.Inspection.Start";
        public const string InspectionComplete = "Department.Inspection.Complete";
        
        public const string ShippingView = "Department.Shipping.View";
        public const string ShippingEdit = "Department.Shipping.Edit";
        public const string ShippingStart = "Department.Shipping.Start";
        public const string ShippingComplete = "Department.Shipping.Complete";
    }

    public StagePermissionService(
        SchedulerContext context, 
        IRolePermissionService rolePermissionService,
        ILogger<StagePermissionService> logger)
    {
        _context = context;
        _rolePermissionService = rolePermissionService;
        _logger = logger;
    }

    #region Permission Checking

    public async Task<bool> CanUserAccessStageAsync(string userId, string department, string action = "View")
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            // Admin can access everything
            if (user.Role == "Admin") return true;

            // Check general stage permission
            var generalPermission = action switch
            {
                "View" => StagePermissions.ViewStages,
                "Create" => StagePermissions.CreateStages,
                "Edit" => StagePermissions.EditStages,
                "Delete" => StagePermissions.DeleteStages,
                "Start" => StagePermissions.StartStages,
                "Complete" => StagePermissions.CompleteStages,
                _ => StagePermissions.ViewStages
            };

            var hasGeneralPermission = await _rolePermissionService.HasPermissionAsync(user.Role, generalPermission);
            if (!hasGeneralPermission) return false;

            // Check department-specific permission
            return await HasDepartmentPermissionAsync(userId, department, action);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stage access for user {UserId}, department {Department}, action {Action}", 
                userId, department, action);
            return false;
        }
    }

    public async Task<bool> CanUserStartStageAsync(string userId, int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return false;

            return await CanUserAccessStageAsync(userId, stage.Department, "Start");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking start permission for user {UserId}, stage {StageId}", userId, stageId);
            return false;
        }
    }

    public async Task<bool> CanUserCompleteStageAsync(string userId, int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return false;

            return await CanUserAccessStageAsync(userId, stage.Department, "Complete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking complete permission for user {UserId}, stage {StageId}", userId, stageId);
            return false;
        }
    }

    public async Task<bool> CanUserEditStageAsync(string userId, int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return false;

            return await CanUserAccessStageAsync(userId, stage.Department, "Edit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking edit permission for user {UserId}, stage {StageId}", userId, stageId);
            return false;
        }
    }

    public async Task<bool> CanUserDeleteStageAsync(string userId, int stageId)
    {
        try
        {
            var stage = await _context.JobStages.FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return false;

            return await CanUserAccessStageAsync(userId, stage.Department, "Delete");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking delete permission for user {UserId}, stage {StageId}", userId, stageId);
            return false;
        }
    }

    #endregion

    #region Department Permissions

    public async Task<List<string>> GetUserDepartmentsAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return new List<string>();

            // Admin can access all departments
            if (user.Role == "Admin")
                return await GetAllDepartmentsAsync();

            var departments = new List<string>();
            var allDepartments = await GetAllDepartmentsAsync();

            foreach (var department in allDepartments)
            {
                if (await HasDepartmentPermissionAsync(userId, department, "View"))
                {
                    departments.Add(department);
                }
            }

            return departments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departments for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetAllDepartmentsAsync()
    {
        try
        {
            return await _context.JobStages
                .Select(s => s.Department)
                .Distinct()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all departments");
            return new List<string> { "Printing", "EDM", "Cerakoting", "Assembly", "Inspection", "Shipping" };
        }
    }

    public async Task<bool> HasDepartmentPermissionAsync(string userId, string department, string permission)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            // Admin has all permissions
            if (user.Role == "Admin") return true;

            // Build department permission key
            var permissionKey = $"Department.{department}.{permission}";
            
            return await _rolePermissionService.HasPermissionAsync(user.Role, permissionKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking department permission for user {UserId}, department {Department}, permission {Permission}", 
                userId, department, permission);
            return false;
        }
    }

    #endregion

    #region Stage Type Permissions

    public async Task<bool> CanUserCreateStageTypeAsync(string userId, string stageType)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            // Check if user has general create permission
            var hasCreatePermission = await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.CreateStages);
            if (!hasCreatePermission) return false;

            // Map stage type to department
            var department = MapStageTypeToDepartment(stageType);
            return await HasDepartmentPermissionAsync(userId, department, "Create");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stage type creation permission for user {UserId}, stage type {StageType}", 
                userId, stageType);
            return false;
        }
    }

    public async Task<List<string>> GetAllowedStageTypesAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return new List<string>();

            if (user.Role == "Admin")
                return new List<string> { "Printing", "EDM", "Cerakoting", "Assembly", "Inspection", "Shipping", "Custom" };

            var allowedTypes = new List<string>();
            var stageTypes = new[] { "Printing", "EDM", "Cerakoting", "Assembly", "Inspection", "Shipping" };

            foreach (var stageType in stageTypes)
            {
                if (await CanUserCreateStageTypeAsync(userId, stageType))
                {
                    allowedTypes.Add(stageType);
                }
            }

            return allowedTypes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting allowed stage types for user {UserId}", userId);
            return new List<string>();
        }
    }

    private string MapStageTypeToDepartment(string stageType)
    {
        return stageType.ToLower() switch
        {
            "printing" => "Printing",
            "edm" => "EDM",
            "cerakoting" => "Cerakoting",
            "assembly" => "Assembly",
            "inspection" => "Inspection",
            "shipping" => "Shipping",
            _ => "General"
        };
    }

    #endregion

    #region Machine Permissions

    public async Task<bool> CanUserAssignMachineAsync(string userId, string machineId, string department)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            // Check if user has machine assignment permission
            var hasMachinePermission = await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.AssignMachines);
            if (!hasMachinePermission) return false;

            // Check department permission
            return await HasDepartmentPermissionAsync(userId, department, "Edit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking machine assignment permission for user {UserId}, machine {MachineId}, department {Department}", 
                userId, machineId, department);
            return false;
        }
    }

    public async Task<List<string>> GetAssignableMachinesAsync(string userId, string department)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return new List<string>();

            // Get machines available for the department
            var machines = await _context.Machines
                .Where(m => m.IsActive && m.IsAvailableForScheduling)
                .ToListAsync();

            var assignableMachines = new List<string>();

            foreach (var machine in machines)
            {
                if (await CanUserAssignMachineAsync(userId, machine.MachineId, department))
                {
                    assignableMachines.Add(machine.MachineId);
                }
            }

            return assignableMachines;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable machines for user {UserId}, department {Department}", userId, department);
            return new List<string>();
        }
    }

    #endregion

    #region Operator Permissions

    public async Task<bool> CanUserAssignOperatorAsync(string userId, string operatorId, int stageId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            var stage = await _context.JobStages.FirstOrDefaultAsync(s => s.Id == stageId);
            if (stage == null) return false;

            // Check if user has operator assignment permission
            var hasOperatorPermission = await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.AssignOperators);
            if (!hasOperatorPermission) return false;

            // Check department permission
            return await HasDepartmentPermissionAsync(userId, stage.Department, "Edit");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking operator assignment permission for user {UserId}, operator {OperatorId}, stage {StageId}", 
                userId, operatorId, stageId);
            return false;
        }
    }

    public async Task<List<User>> GetAssignableOperatorsAsync(string userId, string department)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return new List<User>();

            if (user.Role == "Admin")
            {
                return await _context.Users
                    .Where(u => u.IsActive)
                    .ToListAsync();
            }

            // For non-admin users, return operators they can assign based on department permissions
            var operators = await _context.Users
                .Where(u => u.IsActive && (u.Role == "Operator" || u.Role == "Supervisor"))
                .ToListAsync();

            var assignableOperators = new List<User>();

            foreach (var op in operators)
            {
                // Check if user can assign this operator to this department
                if (await HasDepartmentPermissionAsync(userId, department, "Edit"))
                {
                    assignableOperators.Add(op);
                }
            }

            return assignableOperators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignable operators for user {UserId}, department {Department}", userId, department);
            return new List<User>();
        }
    }

    #endregion

    #region Administrative Permissions

    public async Task<bool> CanUserManageStagePermissionsAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            return await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.ManagePermissions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking stage permission management for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CanUserViewAllStagesAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            return await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.ViewAllDepartments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking view all stages permission for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> CanUserRescheduleStagesAsync(string userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userId);
            if (user == null) return false;

            if (user.Role == "Admin") return true;

            return await _rolePermissionService.HasPermissionAsync(user.Role, StagePermissions.RescheduleStages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking reschedule permission for user {UserId}", userId);
            return false;
        }
    }

    #endregion
}