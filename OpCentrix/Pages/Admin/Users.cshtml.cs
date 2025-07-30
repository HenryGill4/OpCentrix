using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Models;
using OpCentrix.Services;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Admin;

/// <summary>
/// Admin page for managing user accounts
/// Task 5: User Management Panel
/// </summary>
[Authorize(Policy = "AdminOnly")]
public class UsersModel : PageModel
{
    private readonly IAuthenticationService _authenticationService;
    private readonly SchedulerContext _context;
    private readonly ILogger<UsersModel> _logger;

    public UsersModel(IAuthenticationService authenticationService, SchedulerContext context, ILogger<UsersModel> logger)
    {
        _authenticationService = authenticationService;
        _context = context;
        _logger = logger;
    }

    // Properties for the page
    public List<User> Users { get; set; } = new();
    public Dictionary<string, int> RoleStatistics { get; set; } = new();
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }

    // Filtering and search
    [BindProperty(SupportsGet = true)]
    public string SearchTerm { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string RoleFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string StatusFilter { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "Username";

    [BindProperty(SupportsGet = true)]
    public string SortDirection { get; set; } = "asc";

    // User creation/editing
    [BindProperty]
    public UserCreateEditModel UserInput { get; set; } = new();

    [BindProperty]
    public int? EditingUserId { get; set; }

    // Password reset
    [BindProperty]
    public PasswordResetModel PasswordReset { get; set; } = new();

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Loading user management page - Admin: {Admin}", User.Identity?.Name);

            await LoadUsersAsync();
            await LoadStatisticsAsync();

            _logger.LogInformation("User management page loaded - {UserCount} users found", Users.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user management page");
            TempData["Error"] = "Error loading user data. Please try again.";
            
            // Initialize with empty data on error
            Users = new List<User>();
            RoleStatistics = new Dictionary<string, int>();
        }
    }

    public async Task<IActionResult> OnPostCreateUserAsync()
    {
        try
        {
            // SEGMENT 5 FIX 5.1: Enhanced input validation enforcement
            if (!ValidateInputLimits())
            {
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Input validation failed. Please check field lengths and try again.";
                return Page();
            }
            
            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            // Check if username already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username.ToLower() == UserInput.Username.ToLower());

            if (existingUser != null)
            {
                ModelState.AddModelError("UserInput.Username", "Username already exists.");
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Username already exists. Please choose a different username.";
                return Page();
            }

            // Check if email already exists
            var existingEmail = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == UserInput.Email.ToLower());

            if (existingEmail != null)
            {
                ModelState.AddModelError("UserInput.Email", "Email address already exists.");
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Email address already exists. Please use a different email.";
                return Page();
            }

            var newUser = new User
            {
                Username = UserInput.Username,
                FullName = UserInput.FullName,
                Email = UserInput.Email,
                Role = UserInput.Role,
                Department = UserInput.Department,
                IsActive = UserInput.IsActive,
                CreatedBy = User.Identity?.Name ?? "Admin",
                LastModifiedBy = User.Identity?.Name ?? "Admin",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow
            };

            var createdUser = await _authenticationService.CreateUserAsync(newUser, UserInput.Password);

            if (createdUser != null)
            {
                TempData["Success"] = $"User '{UserInput.Username}' created successfully.";
                _logger.LogInformation("Admin {Admin} created new user: {Username} ({Role})", 
                    User.Identity?.Name, UserInput.Username, UserInput.Role);
                
                // Clear the form
                UserInput = new UserCreateEditModel();
            }
            else
            {
                TempData["Error"] = "Failed to create user. Please try again.";
                _logger.LogError("Failed to create user {Username}", UserInput.Username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", UserInput.Username);
            TempData["Error"] = "An error occurred while creating the user.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditUserAsync()
    {
        try
        {
            if (!EditingUserId.HasValue)
            {
                TempData["Error"] = "Invalid user ID.";
                return RedirectToPage();
            }

            // SEGMENT 5 FIX 5.1: Enhanced input validation enforcement for edit
            if (!ValidateInputLimits())
            {
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Input validation failed. Please check field lengths and try again.";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                await LoadUsersAsync();
                await LoadStatisticsAsync();
                TempData["Error"] = "Please correct the validation errors and try again.";
                return Page();
            }

            var user = await _context.Users.FindAsync(EditingUserId.Value);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage();
            }

            // Check if username is being changed and if it conflicts
            if (user.Username.ToLower() != UserInput.Username.ToLower())
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == UserInput.Username.ToLower() && u.Id != user.Id);

                if (existingUser != null)
                {
                    ModelState.AddModelError("UserInput.Username", "Username already exists.");
                    await LoadUsersAsync();
                    await LoadStatisticsAsync();
                    TempData["Error"] = "Username already exists. Please choose a different username.";
                    return Page();
                }
            }

            // Check if email is being changed and if it conflicts
            if (user.Email.ToLower() != UserInput.Email.ToLower())
            {
                var existingEmail = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == UserInput.Email.ToLower() && u.Id != user.Id);

                if (existingEmail != null)
                {
                    ModelState.AddModelError("UserInput.Email", "Email address already exists.");
                    await LoadUsersAsync();
                    await LoadStatisticsAsync();
                    TempData["Error"] = "Email address already exists. Please use a different email.";
                    return Page();
                }
            }

            // Update user properties
            user.Username = UserInput.Username;
            user.FullName = UserInput.FullName;
            user.Email = UserInput.Email;
            user.Role = UserInput.Role;
            user.Department = UserInput.Department;
            user.IsActive = UserInput.IsActive;
            user.LastModifiedBy = User.Identity?.Name ?? "Admin";
            user.LastModifiedDate = DateTime.UtcNow;

            var success = await _authenticationService.UpdateUserAsync(user);

            if (success)
            {
                TempData["Success"] = $"User '{user.Username}' updated successfully.";
                _logger.LogInformation("Admin {Admin} updated user: {Username} ({Role})", 
                    User.Identity?.Name, user.Username, user.Role);

                // Clear editing state
                EditingUserId = null;
                UserInput = new UserCreateEditModel();
            }
            else
            {
                TempData["Error"] = "Failed to update user. Please try again.";
                _logger.LogError("Failed to update user {Username}", user.Username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user ID {UserId}", EditingUserId);
            TempData["Error"] = "An error occurred while updating the user.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostResetPasswordAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(PasswordReset.NewPassword) || PasswordReset.UserId <= 0)
            {
                TempData["Error"] = "Invalid password reset data.";
                return RedirectToPage();
            }

            var user = await _context.Users.FindAsync(PasswordReset.UserId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage();
            }

            var success = await _authenticationService.ChangePasswordAsync(PasswordReset.UserId, PasswordReset.NewPassword);

            if (success)
            {
                TempData["Success"] = $"Password reset successfully for user '{user.Username}'.";
                _logger.LogInformation("Admin {Admin} reset password for user: {Username}", 
                    User.Identity?.Name, user.Username);

                // Clear the form
                PasswordReset = new PasswordResetModel();
            }
            else
            {
                TempData["Error"] = "Failed to reset password. Please try again.";
                _logger.LogError("Failed to reset password for user {Username}", user.Username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user ID {UserId}", PasswordReset.UserId);
            TempData["Error"] = "An error occurred while resetting the password.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleUserStatusAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage();
            }

            // Prevent disabling the last admin user
            if (user.Role == UserRoles.Admin && user.IsActive)
            {
                var adminCount = await _context.Users.CountAsync(u => u.Role == UserRoles.Admin && u.IsActive);
                if (adminCount <= 1)
                {
                    TempData["Error"] = "Cannot disable the last active admin user.";
                    return RedirectToPage();
                }
            }

            user.IsActive = !user.IsActive;
            user.LastModifiedBy = User.Identity?.Name ?? "Admin";
            user.LastModifiedDate = DateTime.UtcNow;

            var success = await _authenticationService.UpdateUserAsync(user);

            if (success)
            {
                var action = user.IsActive ? "enabled" : "disabled";
                TempData["Success"] = $"User '{user.Username}' {action} successfully.";
                _logger.LogInformation("Admin {Admin} {Action} user: {Username}", 
                    User.Identity?.Name, action, user.Username);
            }
            else
            {
                TempData["Error"] = "Failed to update user status. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for user ID {UserId}", userId);
            TempData["Error"] = "An error occurred while updating user status.";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToPage();
            }

            // Prevent deleting admin users
            if (user.Role == UserRoles.Admin)
            {
                TempData["Error"] = "Admin users cannot be deleted for security reasons.";
                return RedirectToPage();
            }

            // Prevent deleting the current user
            var currentUser = await _authenticationService.GetCurrentUserAsync(HttpContext);
            if (currentUser?.Id == userId)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToPage();
            }

            var success = await _authenticationService.DeleteUserAsync(userId);

            if (success)
            {
                TempData["Success"] = $"User '{user.Username}' deleted successfully.";
                _logger.LogWarning("Admin {Admin} deleted user: {Username} ({Role})", 
                    User.Identity?.Name, user.Username, user.Role);
            }
            else
            {
                TempData["Error"] = "Failed to delete user. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user ID {UserId}", userId);
            TempData["Error"] = "An error occurred while deleting the user.";
        }

        return RedirectToPage();
    }

    // Helper methods
    private async Task LoadUsersAsync()
    {
        var query = _context.Users.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(SearchTerm))
        {
            query = query.Where(u => 
                u.Username.Contains(SearchTerm) ||
                u.FullName.Contains(SearchTerm) ||
                u.Email.Contains(SearchTerm) ||
                u.Department.Contains(SearchTerm));
        }

        // Apply role filter
        if (!string.IsNullOrEmpty(RoleFilter))
        {
            query = query.Where(u => u.Role == RoleFilter);
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(StatusFilter))
        {
            if (StatusFilter == "Active")
                query = query.Where(u => u.IsActive);
            else if (StatusFilter == "Inactive")
                query = query.Where(u => !u.IsActive);
        }

        // Apply sorting
        query = SortDirection.ToLower() == "desc"
            ? SortBy switch
            {
                "FullName" => query.OrderByDescending(u => u.FullName),
                "Email" => query.OrderByDescending(u => u.Email),
                "Role" => query.OrderByDescending(u => u.Role),
                "Department" => query.OrderByDescending(u => u.Department),
                "CreatedDate" => query.OrderByDescending(u => u.CreatedDate),
                "LastLoginDate" => query.OrderByDescending(u => u.LastLoginDate),
                "IsActive" => query.OrderByDescending(u => u.IsActive),
                _ => query.OrderByDescending(u => u.Username)
            }
            : SortBy switch
            {
                "FullName" => query.OrderBy(u => u.FullName),
                "Email" => query.OrderBy(u => u.Email),
                "Role" => query.OrderBy(u => u.Role),
                "Department" => query.OrderBy(u => u.Department),
                "CreatedDate" => query.OrderBy(u => u.CreatedDate),
                "LastLoginDate" => query.OrderBy(u => u.LastLoginDate),
                "IsActive" => query.OrderBy(u => u.IsActive),
                _ => query.OrderBy(u => u.Username)
            };

        Users = await query.ToListAsync();
    }

    private async Task LoadStatisticsAsync()
    {
        TotalUsers = await _context.Users.CountAsync();
        ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
        InactiveUsers = TotalUsers - ActiveUsers;

        RoleStatistics = await _context.Users
            .GroupBy(u => u.Role)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    private bool ValidateInputLimits()
    {
        var isValid = true;
        
        // SEGMENT 5 FIX 5.1: Comprehensive input length validation
        // Check for overly long inputs that exceed reasonable limits
        if (!string.IsNullOrEmpty(UserInput.Username) && UserInput.Username.Length > 50)
        {
            ModelState.AddModelError("UserInput.Username", "Username exceeds maximum length of 50 characters.");
            isValid = false;
        }
        
        if (!string.IsNullOrEmpty(UserInput.FullName) && UserInput.FullName.Length > 100)
        {
            ModelState.AddModelError("UserInput.FullName", "Full name exceeds maximum length of 100 characters.");
            isValid = false;
        }
        
        if (!string.IsNullOrEmpty(UserInput.Email) && UserInput.Email.Length > 100)
        {
            ModelState.AddModelError("UserInput.Email", "Email exceeds maximum length of 100 characters.");
            isValid = false;
        }
        
        if (!string.IsNullOrEmpty(UserInput.Password) && UserInput.Password.Length > 50)
        {
            ModelState.AddModelError("UserInput.Password", "Password exceeds maximum length of 50 characters.");
            isValid = false;
        }
        
        if (!string.IsNullOrEmpty(UserInput.Department) && UserInput.Department.Length > 100)
        {
            ModelState.AddModelError("UserInput.Department", "Department exceeds maximum length of 100 characters.");
            isValid = false;
        }
        
        // SEGMENT 5 FIX 5.1: Check for extremely long inputs (security measure against malicious data)
        var allInputs = new[] { UserInput.Username, UserInput.FullName, UserInput.Email, UserInput.Password, UserInput.Department };
        foreach (var input in allInputs)
        {
            if (!string.IsNullOrEmpty(input) && input.Length > 1000)
            {
                ModelState.AddModelError("", "Input exceeds reasonable length limits. This may be a security concern.");
                isValid = false;
                _logger.LogWarning("Extremely long input detected in user creation: {Length} characters", input.Length);
                break;
            }
        }
        
        return isValid;
    }

    public void LoadUserForEditing(User user)
    {
        EditingUserId = user.Id;
        UserInput = new UserCreateEditModel
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Department = user.Department,
            IsActive = user.IsActive
        };
    }

    public string GetNextSortDirection(string column)
    {
        if (SortBy == column)
            return SortDirection == "asc" ? "desc" : "asc";
        return "asc";
    }

    public string GetSortIcon(string column)
    {
        if (SortBy != column) return "?";
        return SortDirection == "asc" ? "??" : "??";
    }

    /// <summary>
    /// Get CSS class for role badge
    /// </summary>
    public string GetRoleBadgeClass(string role)
    {
        return role switch
        {
            UserRoles.Admin => "bg-red-100 text-red-800",
            UserRoles.Manager => "bg-purple-100 text-purple-800",
            UserRoles.Scheduler => "bg-blue-100 text-blue-800",
            UserRoles.Operator => "bg-green-100 text-green-800",
            UserRoles.PrintingSpecialist => "bg-orange-100 text-orange-800",
            UserRoles.CoatingSpecialist => "bg-yellow-100 text-yellow-800",
            UserRoles.ShippingSpecialist => "bg-indigo-100 text-indigo-800",
            UserRoles.EDMSpecialist => "bg-pink-100 text-pink-800",
            UserRoles.MachiningSpecialist => "bg-gray-100 text-gray-800",
            UserRoles.QCSpecialist => "bg-teal-100 text-teal-800",
            UserRoles.Analyst => "bg-cyan-100 text-cyan-800",
            _ => "bg-gray-100 text-gray-800"
        };
    }

    /// <summary>
    /// Get display name for role
    /// </summary>
    public string GetRoleDisplayName(string role)
    {
        return role switch
        {
            UserRoles.Admin => "Administrator",
            UserRoles.Manager => "Manager",
            UserRoles.Scheduler => "Scheduler",
            UserRoles.Operator => "Operator",
            UserRoles.PrintingSpecialist => "Printing Specialist",
            UserRoles.CoatingSpecialist => "Coating Specialist",
            UserRoles.ShippingSpecialist => "Shipping Specialist",
            UserRoles.EDMSpecialist => "EDM Specialist",
            UserRoles.MachiningSpecialist => "Machining Specialist",
            UserRoles.QCSpecialist => "QC Specialist",
            UserRoles.Analyst => "Analyst",
            _ => role
        };
    }
}

/// <summary>
/// Model for creating and editing users
/// </summary>
public class UserCreateEditModel
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 50 characters")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Role")]
    public string Role { get; set; } = UserRoles.Operator;

    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    [Display(Name = "Department")]
    public string Department { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Model for password reset operations
/// </summary>
public class PasswordResetModel
{
    public int UserId { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}