using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Account
{
    public class SettingsModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly IAuthenticationService _authService;

        public SettingsModel(SchedulerContext context, IAuthenticationService authService)
        {
            _context = context;
            _authService = authService;
        }

        [BindProperty]
        public UserSettingsInput Settings { get; set; } = new();

        [BindProperty]
        public PasswordChangeInput PasswordChange { get; set; } = new();

        public string? Message { get; set; }
        public string? ErrorMessage { get; set; }

        public class UserSettingsInput
        {
            [Display(Name = "Full Name")]
            [Required]
            [StringLength(100)]
            public string FullName { get; set; } = string.Empty;

            [Display(Name = "Email")]
            [Required]
            [EmailAddress]
            [StringLength(100)]
            public string Email { get; set; } = string.Empty;

            [Display(Name = "Session Timeout (minutes)")]
            [Range(30, 180, ErrorMessage = "Session timeout must be between 30 minutes and 3 hours")]
            public int SessionTimeoutMinutes { get; set; } = 120;

            [Display(Name = "Theme")]
            public string Theme { get; set; } = "Light";

            [Display(Name = "Email Notifications")]
            public bool EmailNotifications { get; set; } = true;

            [Display(Name = "Browser Notifications")]
            public bool BrowserNotifications { get; set; } = true;

            [Display(Name = "Default Page")]
            public string DefaultPage { get; set; } = "/Scheduler";

            [Display(Name = "Items Per Page")]
            [Range(10, 100)]
            public int ItemsPerPage { get; set; } = 20;

            [Display(Name = "Time Zone")]
            public string TimeZone { get; set; } = "UTC";
        }

        public class PasswordChangeInput
        {
            [Display(Name = "Current Password")]
            [Required]
            [DataType(DataType.Password)]
            public string CurrentPassword { get; set; } = string.Empty;

            [Display(Name = "New Password")]
            [Required]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
            public string NewPassword { get; set; } = string.Empty;

            [Display(Name = "Confirm New Password")]
            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
                return RedirectToPage("/Account/Login");

            // Load user with settings
            user = await _context.Users
                .Include(u => u.Settings)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (user == null)
                return RedirectToPage("/Account/Login");

            // Populate form
            Settings.FullName = user.FullName;
            Settings.Email = user.Email;

            if (user.Settings != null)
            {
                Settings.SessionTimeoutMinutes = user.Settings.SessionTimeoutMinutes;
                Settings.Theme = user.Settings.Theme;
                Settings.EmailNotifications = user.Settings.EmailNotifications;
                Settings.BrowserNotifications = user.Settings.BrowserNotifications;
                Settings.DefaultPage = user.Settings.DefaultPage;
                Settings.ItemsPerPage = user.Settings.ItemsPerPage;
                Settings.TimeZone = user.Settings.TimeZone;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateSettingsAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
                return RedirectToPage("/Account/Login");

            try
            {
                // Load user with settings
                user = await _context.Users
                    .Include(u => u.Settings)
                    .FirstOrDefaultAsync(u => u.Id == user.Id);

                if (user == null)
                    return RedirectToPage("/Account/Login");

                // Update user info
                user.FullName = Settings.FullName;
                user.Email = Settings.Email;
                user.LastModifiedDate = DateTime.UtcNow;

                // Update or create settings
                if (user.Settings == null)
                {
                    user.Settings = new UserSettings
                    {
                        UserId = user.Id
                    };
                    _context.UserSettings.Add(user.Settings);
                }

                user.Settings.SessionTimeoutMinutes = Settings.SessionTimeoutMinutes;
                user.Settings.Theme = Settings.Theme;
                user.Settings.EmailNotifications = Settings.EmailNotifications;
                user.Settings.BrowserNotifications = Settings.BrowserNotifications;
                user.Settings.DefaultPage = Settings.DefaultPage;
                user.Settings.ItemsPerPage = Settings.ItemsPerPage;
                user.Settings.TimeZone = Settings.TimeZone;
                user.Settings.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                Message = "Settings updated successfully!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error updating settings: {ex.Message}";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
                return RedirectToPage("/Account/Login");

            try
            {
                // Verify current password
                if (!_authService.VerifyPassword(PasswordChange.CurrentPassword, user.PasswordHash))
                {
                    ErrorMessage = "Current password is incorrect.";
                    return Page();
                }

                // Update password
                await _authService.ChangePasswordAsync(user.Id, PasswordChange.NewPassword);

                Message = "Password changed successfully!";
                PasswordChange = new PasswordChangeInput(); // Clear form
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error changing password: {ex.Message}";
            }

            return Page();
        }
    }
}