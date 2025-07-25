using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services.Admin;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing system settings
/// Task 3: System Settings Panel
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class SettingsModel : PageModel
{
    private readonly ISystemSettingService _systemSettingService;
    private readonly ILogger<SettingsModel> _logger;

    public SettingsModel(ISystemSettingService systemSettingService, ILogger<SettingsModel> logger)
    {
        _systemSettingService = systemSettingService;
        _logger = logger;
    }

    // Properties for the page
    public Dictionary<string, List<SystemSetting>> SettingsByCategory { get; set; } = new();
    public List<SystemSetting> AllSettings { get; set; } = new();

    // Form binding for updates
    [BindProperty]
    public Dictionary<string, string> SettingValues { get; set; } = new();

    [BindProperty]
    public SystemSetting NewSetting { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading admin settings page - User: {User}", User.Identity?.Name);

            AllSettings = await _systemSettingService.GetAllSettingsAsync();
            
            // Group settings by category
            SettingsByCategory = AllSettings
                .GroupBy(s => s.Category)
                .ToDictionary(g => g.Key, g => g.OrderBy(s => s.DisplayOrder).ThenBy(s => s.SettingKey).ToList());

            // Populate current values for form binding
            SettingValues = AllSettings.ToDictionary(s => s.SettingKey, s => s.SettingValue);

            _logger.LogInformation("Admin settings page loaded successfully - {SettingCount} settings across {CategoryCount} categories",
                AllSettings.Count, SettingsByCategory.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin settings page");
            
            // Initialize with empty data on error
            SettingsByCategory = new Dictionary<string, List<SystemSetting>>();
            AllSettings = new List<SystemSetting>();
            SettingValues = new Dictionary<string, string>();
        }
    }

    public async Task<IActionResult> OnPostUpdateSettingsAsync()
    {
        try
        {
            _logger.LogInformation("Admin {User} updating system settings", User.Identity?.Name);

            var updatedCount = 0;
            var errors = new List<string>();

            foreach (var kvp in SettingValues)
            {
                var settingKey = kvp.Key;
                var newValue = kvp.Value;

                // Get the current setting to validate
                var currentSetting = await _systemSettingService.GetSettingAsync(settingKey);
                if (currentSetting == null)
                {
                    errors.Add($"Setting '{settingKey}' not found");
                    continue;
                }

                // Skip read-only settings
                if (currentSetting.IsReadOnly)
                {
                    _logger.LogWarning("Attempted to update read-only setting: {SettingKey}", settingKey);
                    continue;
                }

                // Validate the new value based on data type
                if (!ValidateSettingValue(currentSetting, newValue, out var validationError))
                {
                    errors.Add($"{currentSetting.SettingKey}: {validationError}");
                    continue;
                }

                // Update the setting
                var success = await _systemSettingService.UpdateSettingAsync(settingKey, newValue, User.Identity?.Name ?? "Unknown");
                if (success)
                {
                    updatedCount++;
                    _logger.LogInformation("Setting {SettingKey} updated from '{OldValue}' to '{NewValue}'", 
                        settingKey, currentSetting.SettingValue, newValue);
                }
                else
                {
                    errors.Add($"Failed to update setting '{settingKey}'");
                }
            }

            if (errors.Any())
            {
                TempData["Error"] = $"Some settings could not be updated: {string.Join(", ", errors)}";
            }

            if (updatedCount > 0)
            {
                TempData["Success"] = $"Successfully updated {updatedCount} setting(s).";
                _logger.LogInformation("Admin {User} successfully updated {Count} settings", User.Identity?.Name, updatedCount);
            }

            if (updatedCount == 0 && !errors.Any())
            {
                TempData["Info"] = "No changes detected.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system settings");
            TempData["Error"] = "An error occurred while updating settings.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateSettingAsync()
    {
        try
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return RedirectToPage();
            }

            _logger.LogInformation("Admin {User} creating new setting: {SettingKey}", User.Identity?.Name, NewSetting.SettingKey);

            NewSetting.CreatedBy = User.Identity?.Name ?? "Unknown";
            NewSetting.LastModifiedBy = User.Identity?.Name ?? "Unknown";

            var success = await _systemSettingService.CreateSettingAsync(NewSetting);
            
            if (success)
            {
                TempData["Success"] = $"Setting '{NewSetting.SettingKey}' created successfully.";
                _logger.LogInformation("New setting created: {SettingKey} = {SettingValue}", NewSetting.SettingKey, NewSetting.SettingValue);
            }
            else
            {
                TempData["Error"] = $"Setting '{NewSetting.SettingKey}' already exists or could not be created.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new setting");
            TempData["Error"] = "An error occurred while creating the setting.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteSettingAsync(string settingKey)
    {
        try
        {
            if (string.IsNullOrEmpty(settingKey))
                return BadRequest("Setting key is required");

            _logger.LogWarning("Admin {User} deleting setting: {SettingKey}", User.Identity?.Name, settingKey);

            var success = await _systemSettingService.DeleteSettingAsync(settingKey);
            
            if (success)
            {
                TempData["Success"] = $"Setting '{settingKey}' deleted successfully.";
                _logger.LogInformation("Setting deleted: {SettingKey}", settingKey);
            }
            else
            {
                TempData["Error"] = $"Setting '{settingKey}' could not be deleted (may be read-only or not found).";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting {SettingKey}", settingKey);
            TempData["Error"] = "An error occurred while deleting the setting.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetToDefaultAsync(string settingKey)
    {
        try
        {
            if (string.IsNullOrEmpty(settingKey))
                return BadRequest("Setting key is required");

            var setting = await _systemSettingService.GetSettingAsync(settingKey);
            if (setting == null)
            {
                TempData["Error"] = $"Setting '{settingKey}' not found.";
                return RedirectToPage();
            }

            _logger.LogInformation("Admin {User} resetting setting to default: {SettingKey}", User.Identity?.Name, settingKey);

            var success = await _systemSettingService.UpdateSettingAsync(settingKey, setting.DefaultValue, User.Identity?.Name ?? "Unknown");
            
            if (success)
            {
                TempData["Success"] = $"Setting '{settingKey}' reset to default value.";
                _logger.LogInformation("Setting reset to default: {SettingKey} = {DefaultValue}", settingKey, setting.DefaultValue);
            }
            else
            {
                TempData["Error"] = $"Could not reset setting '{settingKey}' to default.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting setting to default {SettingKey}", settingKey);
            TempData["Error"] = "An error occurred while resetting the setting.";
        }

        return RedirectToPage();
    }

    // Helper methods
    private bool ValidateSettingValue(SystemSetting setting, string newValue, out string error)
    {
        error = string.Empty;

        if (string.IsNullOrEmpty(newValue))
        {
            error = "Value cannot be empty";
            return false;
        }

        try
        {
            switch (setting.DataType.ToLower())
            {
                case "integer":
                    if (!int.TryParse(newValue, out var intValue))
                    {
                        error = "Must be a valid integer";
                        return false;
                    }
                    break;

                case "decimal":
                    if (!decimal.TryParse(newValue, out var decimalValue))
                    {
                        error = "Must be a valid decimal number";
                        return false;
                    }
                    break;

                case "boolean":
                    if (!bool.TryParse(newValue, out var boolValue))
                    {
                        error = "Must be 'true' or 'false'";
                        return false;
                    }
                    break;

                case "timespan":
                    if (!TimeSpan.TryParse(newValue, out var timeSpanValue))
                    {
                        error = "Must be a valid time span (e.g., '08:00' or '1.05:30:00')";
                        return false;
                    }
                    break;

                case "datetime":
                    if (!DateTime.TryParse(newValue, out var dateTimeValue))
                    {
                        error = "Must be a valid date/time";
                        return false;
                    }
                    break;

                case "string":
                default:
                    // String validation
                    if (newValue.Length > 2000) // Max length from SystemSetting model
                    {
                        error = "Value is too long (maximum 2000 characters)";
                        return false;
                    }
                    break;
            }

            // Additional validation rules if specified
            if (!string.IsNullOrEmpty(setting.ValidationRules))
            {
                if (!setting.IsValid())
                {
                    error = "Value does not meet validation requirements";
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating setting value for {SettingKey}", setting.SettingKey);
            error = "Validation error occurred";
            return false;
        }
    }

    public string GetCategoryDisplayName(string category)
    {
        return category switch
        {
            "Scheduler" => "?? Scheduler Settings",
            "Operations" => "?? Operations Settings",
            "Quality" => "? Quality Settings",
            "System" => "??? System Settings",
            "Notifications" => "?? Notification Settings",
            "Integration" => "?? Integration Settings",
            _ => $"?? {category} Settings"
        };
    }

    public string GetDataTypeDisplayName(string dataType)
    {
        return dataType.ToLower() switch
        {
            "integer" => "Number (Integer)",
            "decimal" => "Number (Decimal)",
            "boolean" => "True/False",
            "timespan" => "Time Duration",
            "datetime" => "Date/Time",
            "string" => "Text",
            _ => dataType
        };
    }

    public string GetSettingInputType(string dataType)
    {
        return dataType.ToLower() switch
        {
            "integer" => "number",
            "decimal" => "number",
            "boolean" => "checkbox",
            "timespan" => "time",
            "datetime" => "datetime-local",
            "string" => "text",
            _ => "text"
        };
    }
}