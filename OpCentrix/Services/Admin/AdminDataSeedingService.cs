using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services.Admin;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for seeding default admin control system data
/// Task 2: Admin Control System - Default data seeding
/// </summary>
public interface IAdminDataSeedingService
{
    Task SeedDefaultSystemSettingsAsync();
    Task SeedDefaultRolePermissionsAsync();
    Task SeedDefaultOperatingShiftsAsync();
    Task SeedDefaultDefectCategoriesAsync();
    Task SeedAllDefaultDataAsync();
}

public class AdminDataSeedingService : IAdminDataSeedingService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<AdminDataSeedingService> _logger;

    public AdminDataSeedingService(SchedulerContext context, ILogger<AdminDataSeedingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedDefaultSystemSettingsAsync()
    {
        try
        {
            var defaultSettings = new List<SystemSetting>
            {
                // Scheduler settings
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.DefaultChangeoverDurationHours,
                    SettingValue = "3",
                    DataType = "Integer",
                    Category = "Scheduler",
                    Description = "Default changeover duration between jobs in hours",
                    DefaultValue = "3",
                    DisplayOrder = 1,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.DefaultCooldownTimeHours,
                    SettingValue = "1",
                    DataType = "Integer",
                    Category = "Scheduler",
                    Description = "Default cooldown time after job completion in hours",
                    DefaultValue = "1",
                    DisplayOrder = 2,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.MaxJobsPerDay,
                    SettingValue = "3",
                    DataType = "Integer",
                    Category = "Scheduler",
                    Description = "Maximum number of jobs per machine per day",
                    DefaultValue = "3",
                    DisplayOrder = 3,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.AutoSchedulingEnabled,
                    SettingValue = "true",
                    DataType = "Boolean",
                    Category = "Scheduler",
                    Description = "Enable automatic job scheduling optimization",
                    DefaultValue = "true",
                    DisplayOrder = 4,
                    CreatedBy = "System"
                },

                // Operational settings
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.DefaultShiftStart,
                    SettingValue = "08:00",
                    DataType = "TimeSpan",
                    Category = "Operations",
                    Description = "Default shift start time",
                    DefaultValue = "08:00",
                    DisplayOrder = 1,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.DefaultShiftEnd,
                    SettingValue = "17:00",
                    DataType = "TimeSpan",
                    Category = "Operations",
                    Description = "Default shift end time",
                    DefaultValue = "17:00",
                    DisplayOrder = 2,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.MaintenanceIntervalHours,
                    SettingValue = "500",
                    DataType = "Integer",
                    Category = "Operations",
                    Description = "Maintenance interval in operating hours",
                    DefaultValue = "500",
                    DisplayOrder = 3,
                    CreatedBy = "System"
                },

                // Quality settings
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.DefaultQualityThreshold,
                    SettingValue = "95",
                    DataType = "Decimal",
                    Category = "Quality",
                    Description = "Default quality threshold percentage",
                    DefaultValue = "95",
                    DisplayOrder = 1,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.RequireInspectionSign0ff,
                    SettingValue = "true",
                    DataType = "Boolean",
                    Category = "Quality",
                    Description = "Require quality inspector sign-off for job completion",
                    DefaultValue = "true",
                    DisplayOrder = 2,
                    CreatedBy = "System"
                },

                // System settings
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.SessionTimeoutMinutes,
                    SettingValue = "120",
                    DataType = "Integer",
                    Category = "System",
                    Description = "User session timeout in minutes",
                    DefaultValue = "120",
                    DisplayOrder = 1,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.EnableDebugLogging,
                    SettingValue = "false",
                    DataType = "Boolean",
                    Category = "System",
                    Description = "Enable debug level logging",
                    DefaultValue = "false",
                    DisplayOrder = 2,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.AutoBackupEnabled,
                    SettingValue = "true",
                    DataType = "Boolean",
                    Category = "System",
                    Description = "Enable automatic database backups",
                    DefaultValue = "true",
                    DisplayOrder = 3,
                    CreatedBy = "System"
                },
                new SystemSetting
                {
                    SettingKey = SystemSettingKeys.BackupRetentionDays,
                    SettingValue = "30",
                    DataType = "Integer",
                    Category = "System",
                    Description = "Number of days to retain backup files",
                    DefaultValue = "30",
                    DisplayOrder = 4,
                    CreatedBy = "System"
                }
            };

            foreach (var setting in defaultSettings)
            {
                var exists = await _context.SystemSettings
                    .AnyAsync(s => s.SettingKey == setting.SettingKey);

                if (!exists)
                {
                    _context.SystemSettings.Add(setting);
                    _logger.LogDebug("Added default system setting: {SettingKey}", setting.SettingKey);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default system settings seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default system settings");
        }
    }

    public async Task SeedDefaultRolePermissionsAsync()
    {
        try
        {
            var rolePermissions = new List<RolePermission>();

            // Admin role - full access
            var adminPermissions = new[]
            {
                PermissionKeys.AdminDashboard, PermissionKeys.AdminUsers, PermissionKeys.AdminRoles,
                PermissionKeys.AdminSettings, PermissionKeys.AdminMachines, PermissionKeys.AdminParts,
                PermissionKeys.AdminJobs, PermissionKeys.AdminShifts, PermissionKeys.AdminCheckpoints,
                PermissionKeys.AdminDefects, PermissionKeys.AdminArchive, PermissionKeys.AdminDatabase,
                PermissionKeys.AdminAlerts, PermissionKeys.AdminFeatures, PermissionKeys.AdminLogs,
                PermissionKeys.SchedulerView, PermissionKeys.SchedulerCreate, PermissionKeys.SchedulerEdit,
                PermissionKeys.SchedulerDelete, PermissionKeys.SchedulerReschedule
            };

            foreach (var permission in adminPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleName = "Admin",
                    PermissionKey = permission,
                    HasPermission = true,
                    PermissionLevel = PermissionLevels.Admin,
                    Category = permission.Split('.')[0],
                    Description = $"Admin access to {permission}",
                    IsActive = true,
                    Priority = 1,
                    CreatedBy = "System"
                });
            }

            // Manager role - limited admin access
            var managerPermissions = new[]
            {
                PermissionKeys.AdminDashboard, PermissionKeys.AdminParts, PermissionKeys.AdminJobs,
                PermissionKeys.SchedulerView, PermissionKeys.SchedulerCreate, PermissionKeys.SchedulerEdit,
                PermissionKeys.SchedulerReschedule
            };

            foreach (var permission in managerPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleName = "Manager",
                    PermissionKey = permission,
                    HasPermission = true,
                    PermissionLevel = PermissionLevels.Write,
                    Category = permission.Split('.')[0],
                    Description = $"Manager access to {permission}",
                    IsActive = true,
                    Priority = 2,
                    CreatedBy = "System"
                });
            }

            // Scheduler role - scheduling access
            var schedulerPermissions = new[]
            {
                PermissionKeys.SchedulerView, PermissionKeys.SchedulerCreate, PermissionKeys.SchedulerEdit,
                PermissionKeys.SchedulerReschedule
            };

            foreach (var permission in schedulerPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleName = "Scheduler",
                    PermissionKey = permission,
                    HasPermission = true,
                    PermissionLevel = PermissionLevels.Write,
                    Category = permission.Split('.')[0],
                    Description = $"Scheduler access to {permission}",
                    IsActive = true,
                    Priority = 3,
                    CreatedBy = "System"
                });
            }

            // Operator role - read-only access
            var operatorPermissions = new[]
            {
                PermissionKeys.SchedulerView
            };

            foreach (var permission in operatorPermissions)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleName = "Operator",
                    PermissionKey = permission,
                    HasPermission = true,
                    PermissionLevel = PermissionLevels.Read,
                    Category = permission.Split('.')[0],
                    Description = $"Operator access to {permission}",
                    IsActive = true,
                    Priority = 4,
                    CreatedBy = "System"
                });
            }

            foreach (var permission in rolePermissions)
            {
                var exists = await _context.RolePermissions
                    .AnyAsync(p => p.RoleName == permission.RoleName && p.PermissionKey == permission.PermissionKey);

                if (!exists)
                {
                    _context.RolePermissions.Add(permission);
                    _logger.LogDebug("Added default role permission: {Role} - {Permission}", 
                        permission.RoleName, permission.PermissionKey);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default role permissions seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default role permissions");
        }
    }

    public async Task SeedDefaultOperatingShiftsAsync()
    {
        try
        {
            var existingShifts = await _context.OperatingShifts.AnyAsync();
            if (existingShifts)
            {
                _logger.LogDebug("Operating shifts already exist, skipping seeding");
                return;
            }

            var defaultShifts = DefaultOperatingShifts.GetStandardBusinessHours();

            foreach (var shift in defaultShifts)
            {
                _context.OperatingShifts.Add(shift);
                _logger.LogDebug("Added default operating shift: {DayName} {StartTime}-{EndTime}", 
                    shift.DayName, shift.StartTime, shift.EndTime);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default operating shifts seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default operating shifts");
        }
    }

    public async Task SeedDefaultDefectCategoriesAsync()
    {
        try
        {
            var defaultCategories = new List<DefectCategory>
            {
                new DefectCategory
                {
                    Name = "Surface Defect",
                    Code = "SURF",
                    Description = "Surface finish issues, roughness, or texture problems",
                    SeverityLevel = 3,
                    CategoryGroup = DefectCategoryGroups.Surface,
                    ApplicableProcesses = "SLS Metal Printing",
                    StandardCorrectiveActions = "Review laser parameters, check powder quality",
                    PreventionMethods = "Optimize scan strategy, maintain proper powder conditions",
                    CostImpact = CostImpactLevels.Medium,
                    ColorCode = "#F59E0B",
                    CreatedBy = "System"
                },
                new DefectCategory
                {
                    Name = "Dimensional Deviation",
                    Code = "DIM",
                    Description = "Part dimensions outside specified tolerances",
                    SeverityLevel = 2,
                    CategoryGroup = DefectCategoryGroups.Dimensional,
                    ApplicableProcesses = "SLS Metal Printing",
                    StandardCorrectiveActions = "Calibrate machine, review part orientation",
                    PreventionMethods = "Regular machine calibration, proper support design",
                    CostImpact = CostImpactLevels.High,
                    ColorCode = "#DC2626",
                    CreatedBy = "System"
                },
                new DefectCategory
                {
                    Name = "Porosity",
                    Code = "POR",
                    Description = "Internal voids or porosity in printed parts",
                    SeverityLevel = 1,
                    CategoryGroup = DefectCategoryGroups.Material,
                    ApplicableProcesses = "SLS Metal Printing",
                    StandardCorrectiveActions = "Adjust laser power and speed, check powder quality",
                    PreventionMethods = "Optimize process parameters, use fresh powder",
                    CostImpact = CostImpactLevels.Critical,
                    RequiresImmediateNotification = true,
                    ColorCode = "#991B1B",
                    CreatedBy = "System"
                },
                new DefectCategory
                {
                    Name = "Support Marks",
                    Code = "SUP",
                    Description = "Marks or damage from support removal",
                    SeverityLevel = 4,
                    CategoryGroup = DefectCategoryGroups.Process,
                    ApplicableProcesses = "SLS Metal Printing",
                    StandardCorrectiveActions = "Improve support design, review removal process",
                    PreventionMethods = "Optimize support placement, use soluble supports",
                    CostImpact = CostImpactLevels.Low,
                    ColorCode = "#10B981",
                    CreatedBy = "System"
                },
                new DefectCategory
                {
                    Name = "Coating Defect",
                    Code = "COAT",
                    Description = "Issues with surface coating application",
                    SeverityLevel = 3,
                    CategoryGroup = DefectCategoryGroups.Surface,
                    ApplicableProcesses = "Cerakoting",
                    StandardCorrectiveActions = "Review coating parameters, check surface prep",
                    PreventionMethods = "Proper surface preparation, controlled environment",
                    CostImpact = CostImpactLevels.Medium,
                    ColorCode = "#8B5CF6",
                    CreatedBy = "System"
                }
            };

            foreach (var category in defaultCategories)
            {
                var exists = await _context.DefectCategories
                    .AnyAsync(d => d.Code == category.Code);

                if (!exists)
                {
                    _context.DefectCategories.Add(category);
                    _logger.LogDebug("Added default defect category: {Code} - {Name}", 
                        category.Code, category.Name);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Default defect categories seeded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding default defect categories");
        }
    }

    public async Task SeedAllDefaultDataAsync()
    {
        try
        {
            _logger.LogInformation("Starting admin control system data seeding");

            await SeedDefaultSystemSettingsAsync();
            await SeedDefaultRolePermissionsAsync();
            await SeedDefaultOperatingShiftsAsync();
            await SeedDefaultDefectCategoriesAsync();

            _logger.LogInformation("Admin control system data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during admin control system data seeding");
        }
    }
}