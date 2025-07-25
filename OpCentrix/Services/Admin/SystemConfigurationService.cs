using OpCentrix.Services.Admin;
using OpCentrix.Models;
using Microsoft.Extensions.Options;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for loading system settings into application configuration
/// Task 3: System Settings Panel - Configuration integration
/// </summary>
public interface ISystemConfigurationService
{
    Task LoadSettingsIntoConfigurationAsync();
    Task<T> GetConfigurationValueAsync<T>(string key, T defaultValue = default);
    Task RefreshConfigurationAsync();
    bool IsConfigurationLoaded { get; }
}

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SystemConfigurationService> _logger;
    private readonly Dictionary<string, object> _cachedSettings = new();
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public bool IsConfigurationLoaded { get; private set; }

    public SystemConfigurationService(
        ISystemSettingService systemSettingService,
        IConfiguration configuration,
        ILogger<SystemConfigurationService> logger)
    {
        _systemSettingService = systemSettingService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task LoadSettingsIntoConfigurationAsync()
    {
        try
        {
            _logger.LogInformation("Loading system settings into application configuration");

            var settings = await _systemSettingService.GetAllSettingsAsync();
            
            foreach (var setting in settings)
            {
                try
                {
                    var value = ConvertSettingValue(setting);
                    _cachedSettings[setting.SettingKey] = value;
                    
                    _logger.LogDebug("Loaded setting {SettingKey} = {Value}", setting.SettingKey, value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load setting {SettingKey}", setting.SettingKey);
                }
            }

            _lastRefresh = DateTime.UtcNow;
            IsConfigurationLoaded = true;

            _logger.LogInformation("Successfully loaded {SettingCount} system settings into configuration", 
                _cachedSettings.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading system settings into configuration");
            IsConfigurationLoaded = false;
        }
    }

    public async Task<T> GetConfigurationValueAsync<T>(string key, T defaultValue = default)
    {
        try
        {
            // Check if cache needs refresh
            if (DateTime.UtcNow - _lastRefresh > _cacheExpiry)
            {
                await RefreshConfigurationAsync();
            }

            // Try to get from cached settings first
            if (_cachedSettings.TryGetValue(key, out var cachedValue))
            {
                if (cachedValue is T directValue)
                    return directValue;

                // Attempt conversion
                if (TryConvertValue<T>(cachedValue, out var convertedValue))
                    return convertedValue;
            }

            // Fallback to live database query
            var liveValue = await _systemSettingService.GetSettingValueAsync<T>(key, defaultValue);
            
            // Cache the result
            _cachedSettings[key] = liveValue;
            
            return liveValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration value for {Key}", key);
            return defaultValue;
        }
    }

    public async Task RefreshConfigurationAsync()
    {
        try
        {
            _logger.LogInformation("Refreshing system configuration cache");
            
            _cachedSettings.Clear();
            await LoadSettingsIntoConfigurationAsync();
            
            _logger.LogInformation("System configuration cache refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing system configuration cache");
        }
    }

    private object ConvertSettingValue(SystemSetting setting)
    {
        var value = setting.SettingValue;
        
        return setting.DataType.ToLower() switch
        {
            "integer" => int.Parse(value),
            "decimal" => decimal.Parse(value),
            "boolean" => bool.Parse(value),
            "timespan" => TimeSpan.Parse(value),
            "datetime" => DateTime.Parse(value),
            "string" => value,
            _ => value
        };
    }

    private bool TryConvertValue<T>(object value, out T result)
    {
        result = default;
        
        try
        {
            if (value is T directValue)
            {
                result = directValue;
                return true;
            }

            // Handle string conversions to various types
            if (value is string stringValue)
            {
                var targetType = typeof(T);
                
                if (targetType == typeof(int) && int.TryParse(stringValue, out var intVal))
                {
                    result = (T)(object)intVal;
                    return true;
                }
                
                if (targetType == typeof(decimal) && decimal.TryParse(stringValue, out var decVal))
                {
                    result = (T)(object)decVal;
                    return true;
                }
                
                if (targetType == typeof(bool) && bool.TryParse(stringValue, out var boolVal))
                {
                    result = (T)(object)boolVal;
                    return true;
                }
                
                if (targetType == typeof(TimeSpan) && TimeSpan.TryParse(stringValue, out var timeVal))
                {
                    result = (T)(object)timeVal;
                    return true;
                }
                
                if (targetType == typeof(DateTime) && DateTime.TryParse(stringValue, out var dateVal))
                {
                    result = (T)(object)dateVal;
                    return true;
                }
            }

            // Use Convert.ChangeType as fallback
            result = (T)Convert.ChangeType(value, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Configuration helper class for easy access to system settings
/// </summary>
public static class SystemConfiguration
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public static async Task<T> GetValueAsync<T>(string key, T defaultValue = default)
    {
        if (_serviceProvider == null)
            return defaultValue;

        try
        {
            var configService = _serviceProvider.GetService<ISystemConfigurationService>();
            if (configService == null)
                return defaultValue;

            return await configService.GetConfigurationValueAsync(key, defaultValue);
        }
        catch
        {
            return defaultValue;
        }
    }

    // Convenience methods for common settings
    public static async Task<int> GetChangeoverDurationHoursAsync() =>
        await GetValueAsync(SystemSettingKeys.DefaultChangeoverDurationHours, 3);

    public static async Task<int> GetCooldownTimeHoursAsync() =>
        await GetValueAsync(SystemSettingKeys.DefaultCooldownTimeHours, 1);

    public static async Task<int> GetMaxJobsPerDayAsync() =>
        await GetValueAsync(SystemSettingKeys.MaxJobsPerDay, 3);

    public static async Task<bool> GetAutoSchedulingEnabledAsync() =>
        await GetValueAsync(SystemSettingKeys.AutoSchedulingEnabled, true);

    public static async Task<TimeSpan> GetDefaultShiftStartAsync() =>
        await GetValueAsync(SystemSettingKeys.DefaultShiftStart, TimeSpan.FromHours(8));

    public static async Task<TimeSpan> GetDefaultShiftEndAsync() =>
        await GetValueAsync(SystemSettingKeys.DefaultShiftEnd, TimeSpan.FromHours(17));

    public static async Task<int> GetSessionTimeoutMinutesAsync() =>
        await GetValueAsync(SystemSettingKeys.SessionTimeoutMinutes, 120);

    public static async Task<bool> GetDebugLoggingEnabledAsync() =>
        await GetValueAsync(SystemSettingKeys.EnableDebugLogging, false);
}