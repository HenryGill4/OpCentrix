using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpCentrix.Models;

/// <summary>
/// Represents system-wide configuration settings
/// Allows admins to manage global application settings via the admin interface
/// </summary>
public class SystemSetting
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Unique setting key identifier
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SettingKey { get; set; } = string.Empty;

    /// <summary>
    /// Setting value as string (can be parsed to different types)
    /// </summary>
    [StringLength(2000)]
    public string SettingValue { get; set; } = string.Empty;

    /// <summary>
    /// Data type of the setting value (String, Integer, Decimal, Boolean, DateTime, TimeSpan)
    /// </summary>
    [Required]
    [StringLength(20)]
    public string DataType { get; set; } = "String";

    /// <summary>
    /// Category for grouping related settings
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = "General";

    /// <summary>
    /// Human-readable description of the setting
    /// </summary>
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Default value for the setting
    /// </summary>
    [StringLength(500)]
    public string DefaultValue { get; set; } = string.Empty;

    /// <summary>
    /// Whether this setting is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this setting is read-only (cannot be modified via UI)
    /// </summary>
    public bool IsReadOnly { get; set; } = false;

    /// <summary>
    /// Whether the application needs to be restarted for this setting to take effect
    /// </summary>
    public bool RequiresRestart { get; set; } = false;

    /// <summary>
    /// Validation rules for the setting value (regex pattern, min/max values, etc.)
    /// </summary>
    [StringLength(200)]
    public string ValidationRules { get; set; } = string.Empty;

    /// <summary>
    /// Display order for admin interface
    /// </summary>
    public int DisplayOrder { get; set; } = 100;

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
    /// Get the setting value as a specific type
    /// </summary>
    public T GetValue<T>()
    {
        if (string.IsNullOrEmpty(SettingValue))
        {
            if (!string.IsNullOrEmpty(DefaultValue))
                return (T)Convert.ChangeType(DefaultValue, typeof(T));
            return default(T)!;
        }

        try
        {
            return (T)Convert.ChangeType(SettingValue, typeof(T));
        }
        catch
        {
            if (!string.IsNullOrEmpty(DefaultValue))
                return (T)Convert.ChangeType(DefaultValue, typeof(T));
            return default(T)!;
        }
    }

    /// <summary>
    /// Get the setting value as a TimeSpan (for duration settings)
    /// </summary>
    public TimeSpan GetTimeSpanValue()
    {
        if (TimeSpan.TryParse(SettingValue, out var timeSpan))
            return timeSpan;

        if (TimeSpan.TryParse(DefaultValue, out var defaultTimeSpan))
            return defaultTimeSpan;

        return TimeSpan.Zero;
    }

    /// <summary>
    /// Get the setting value as an integer
    /// </summary>
    public int GetIntValue()
    {
        return GetValue<int>();
    }

    /// <summary>
    /// Get the setting value as a decimal
    /// </summary>
    public decimal GetDecimalValue()
    {
        return GetValue<decimal>();
    }

    /// <summary>
    /// Get the setting value as a boolean
    /// </summary>
    public bool GetBoolValue()
    {
        return GetValue<bool>();
    }

    /// <summary>
    /// Set the setting value from any object
    /// </summary>
    public void SetValue(object value)
    {
        SettingValue = value?.ToString() ?? string.Empty;
        LastModifiedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Helper property for display
    /// </summary>
    [NotMapped]
    public string DisplayName => $"{Category} - {SettingKey}";

    /// <summary>
    /// Validate the current setting value against validation rules
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(ValidationRules))
            return true;

        try
        {
            // Basic validation rules implementation
            var rules = ValidationRules.Split('|');
            foreach (var rule in rules)
            {
                if (rule.StartsWith("required") && string.IsNullOrEmpty(SettingValue))
                    return false;

                if (rule.StartsWith("min:") && int.TryParse(rule.Substring(4), out var min))
                {
                    if (GetIntValue() < min)
                        return false;
                }

                if (rule.StartsWith("max:") && int.TryParse(rule.Substring(4), out var max))
                {
                    if (GetIntValue() > max)
                        return false;
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}