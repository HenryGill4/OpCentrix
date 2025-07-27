using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for managing system settings
/// Task 2: Admin Control System - SystemSetting management
/// </summary>
public interface ISystemSettingService
{
    Task<List<SystemSetting>> GetAllSettingsAsync();
    Task<SystemSetting?> GetSettingAsync(string key);
    Task<T> GetSettingValueAsync<T>(string key, T defaultValue = default);
    Task<bool> UpdateSettingAsync(string key, object value, string updatedBy);
    Task<bool> CreateSettingAsync(SystemSetting setting);
    Task<bool> DeleteSettingAsync(string key);
    Task<Dictionary<string, SystemSetting>> GetSettingsByCategoryAsync(string category);
}

public class SystemSettingService : ISystemSettingService
{
    private readonly SchedulerContext _context;
    private readonly ILogger<SystemSettingService> _logger;

    public SystemSettingService(SchedulerContext context, ILogger<SystemSettingService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync()
    {
        try
        {
            return await _context.SystemSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.DisplayOrder)
                .ThenBy(s => s.SettingKey)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all system settings");
            return new List<SystemSetting>();
        }
    }

    public async Task<SystemSetting?> GetSettingAsync(string key)
    {
        try
        {
            return await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key && s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving setting {SettingKey}", key);
            return null;
        }
    }

    public async Task<T> GetSettingValueAsync<T>(string key, T defaultValue = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return defaultValue ?? default(T)!;
            
        try
        {
            var setting = await GetSettingAsync(key);
            if (setting == null)
                return defaultValue ?? default(T)!;

            return setting.GetValue<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting value for {SettingKey}", key);
            return defaultValue ?? default(T)!;
        }
    }

    public async Task<bool> UpdateSettingAsync(string key, object value, string updatedBy)
    {
        try
        {
            var setting = await GetSettingAsync(key);
            if (setting == null)
                return false;

            setting.SetValue(value);
            setting.LastModifiedBy = updatedBy;
            setting.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Setting {SettingKey} updated to {Value} by {User}", 
                key, value, updatedBy);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating setting {SettingKey}", key);
            return false;
        }
    }

    public async Task<bool> CreateSettingAsync(SystemSetting setting)
    {
        try
        {
            var existing = await GetSettingAsync(setting.SettingKey);
            if (existing != null)
                return false;

            _context.SystemSettings.Add(setting);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Setting {SettingKey} created by {User}", 
                setting.SettingKey, setting.CreatedBy);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating setting {SettingKey}", setting.SettingKey);
            return false;
        }
    }

    public async Task<bool> DeleteSettingAsync(string key)
    {
        try
        {
            var setting = await GetSettingAsync(key);
            if (setting == null || setting.IsReadOnly)
                return false;

            _context.SystemSettings.Remove(setting);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Setting {SettingKey} deleted", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting {SettingKey}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, SystemSetting>> GetSettingsByCategoryAsync(string category)
    {
        try
        {
            var settings = await _context.SystemSettings
                .Where(s => s.Category == category && s.IsActive)
                .ToListAsync();

            return settings.ToDictionary(s => s.SettingKey, s => s);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings for category {Category}", category);
            return new Dictionary<string, SystemSetting>();
        }
    }
}

/// <summary>
/// Common system setting keys used throughout the application
/// </summary>
public static class SystemSettingKeys
{
    // Scheduler settings
    public const string DefaultChangeoverDurationHours = "scheduler.default_changeover_duration_hours";
    public const string DefaultCooldownTimeHours = "scheduler.default_cooldown_time_hours";
    public const string MaxJobsPerDay = "scheduler.max_jobs_per_day";
    public const string AutoSchedulingEnabled = "scheduler.auto_scheduling_enabled";

    // Operational settings
    public const string DefaultShiftStart = "operations.default_shift_start";
    public const string DefaultShiftEnd = "operations.default_shift_end";
    public const string MaintenanceIntervalHours = "operations.maintenance_interval_hours";

    // Quality settings
    public const string DefaultQualityThreshold = "quality.default_threshold_percent";
    public const string RequireInspectionSign0ff = "quality.require_inspection_signoff";

    // System settings
    public const string SessionTimeoutMinutes = "system.session_timeout_minutes";
    public const string EnableDebugLogging = "system.enable_debug_logging";
    public const string AutoBackupEnabled = "system.auto_backup_enabled";
    public const string BackupRetentionDays = "system.backup_retention_days";

    // Notification settings
    public const string EmailNotificationsEnabled = "notifications.email_enabled";
    public const string SmsNotificationsEnabled = "notifications.sms_enabled";
    public const string SlackWebhookUrl = "notifications.slack_webhook_url";

    // Integration settings
    public const string OpcUaEnabled = "integration.opcua_enabled";
    public const string EtlSyncIntervalMinutes = "integration.etl_sync_interval_minutes";
}
