using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using OpCentrix.Models;
using OpCentrix.Services;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class SchedulerSettingsModel : PageModel
    {
        private readonly ISchedulerSettingsService _settingsService;
        private readonly ILogger<SchedulerSettingsModel> _logger;

        public SchedulerSettingsModel(ISchedulerSettingsService settingsService, ILogger<SchedulerSettingsModel> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        [BindProperty]
        public SchedulerSettings Settings { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> ValidationErrors { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                Settings = await _settingsService.GetSettingsAsync();
                _logger.LogInformation("Loaded scheduler settings for admin view");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scheduler settings");
                ErrorMessage = "Error loading scheduler settings. Please try again.";
                Settings = new SchedulerSettings(); // Use defaults
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            try
            {
                // Validate the model
                if (!ModelState.IsValid)
                {
                    ValidationErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Page();
                }

                // Validate business rules
                var businessErrors = Settings.ValidateSettings();
                if (businessErrors.Any())
                {
                    ValidationErrors = businessErrors;
                    return Page();
                }

                // Save settings
                var username = User.Identity?.Name ?? "Unknown";
                await _settingsService.UpdateSettingsAsync(Settings, username);

                SuccessMessage = "Scheduler settings saved successfully!";
                _logger.LogInformation("Scheduler settings updated by {Username}", username);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving scheduler settings");
                ErrorMessage = "Error saving settings. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostResetAsync()
        {
            try
            {
                var username = User.Identity?.Name ?? "Unknown";
                Settings = await _settingsService.ResetToDefaultsAsync(username);

                SuccessMessage = "Scheduler settings reset to defaults successfully!";
                _logger.LogInformation("Scheduler settings reset to defaults by {Username}", username);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting scheduler settings");
                ErrorMessage = "Error resetting settings. Please try again.";
                return Page();
            }
        }

        public async Task<IActionResult> OnGetTestChangeoverAsync(string fromMaterial, string toMaterial)
        {
            try
            {
                var changeoverTime = await _settingsService.GetChangeoverTimeAsync(fromMaterial, toMaterial);

                return new JsonResult(new
                {
                    success = true,
                    changeoverTime,
                    message = $"Changeover from {fromMaterial} to {toMaterial}: {changeoverTime} minutes"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing changeover time");
                return new JsonResult(new { success = false, message = "Error calculating changeover time" });
            }
        }

        public async Task<IActionResult> OnGetTestOperatorAvailabilityAsync(DateTime startTime, DateTime endTime)
        {
            try
            {
                var isAvailable = await _settingsService.IsOperatorAvailableAsync(startTime, endTime);

                return new JsonResult(new
                {
                    success = true,
                    isAvailable,
                    message = $"Operator {(isAvailable ? "is" : "is not")} available from {startTime:HH:mm} to {endTime:HH:mm}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing operator availability");
                return new JsonResult(new { success = false, message = "Error checking operator availability" });
            }
        }
    }
}