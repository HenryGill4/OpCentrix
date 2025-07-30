using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace OpCentrix.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(SchedulerContext context, ILogger<AuthenticationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Authentication attempt for user: {Username}", operationId, username);

            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Authentication failed: Empty username", operationId);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Authentication failed: Empty password for user {Username}", operationId, username);
                    return null;
                }

                _logger.LogDebug("?? [AUTH-{OperationId}] Looking up user in database: {Username}", operationId, username);

                var user = await _context.Users
                    .Include(u => u.Settings)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Authentication failed: User not found or inactive: {Username}", operationId, username);
                    return null;
                }

                _logger.LogDebug("?? [AUTH-{OperationId}] User found: {Username} ({Role}), verifying password", operationId, username, user.Role);

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Authentication failed: Invalid password for user {Username}", operationId, username);
                    return null;
                }

                // Update last login date
                user.LastLoginDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("? [AUTH-{OperationId}] Authentication successful for user: {Username} ({Role})", 
                    operationId, username, user.Role);
                
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error during authentication for user {Username}: {ErrorMessage}", 
                    operationId, username, ex.Message);
                return null;
            }
        }

        public async Task<bool> LoginAsync(HttpContext context, User user)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Starting login process for user: {Username} ({Role})", 
                operationId, user.Username, user.Role);

            try
            {
                // Get user session timeout preference
                var sessionTimeoutMinutes = user.Settings?.SessionTimeoutMinutes ?? 120;
                _logger.LogDebug("?? [AUTH-{OperationId}] Using session timeout: {TimeoutMinutes} minutes", 
                    operationId, sessionTimeoutMinutes);

                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.GivenName, user.FullName),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Role, user.Role),
                    new("Department", user.Department ?? ""),
                    new("UserId", user.Id.ToString()),
                    new("SessionTimeout", sessionTimeoutMinutes.ToString())
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(sessionTimeoutMinutes),
                    AllowRefresh = true
                };

                _logger.LogDebug("?? [AUTH-{OperationId}] Creating authentication session with {ClaimCount} claims", 
                    operationId, claims.Count);

                await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                _logger.LogInformation("? [AUTH-{OperationId}] Login successful for user: {Username} ({Role}) - Session expires: {ExpiryTime}", 
                    operationId, user.Username, user.Role, authProperties.ExpiresUtc);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error during login for user {Username}: {ErrorMessage}", 
                    operationId, user.Username, ex.Message);
                return false;
            }
        }

        public async Task LogoutAsync(HttpContext context)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var username = context.User.Identity?.Name ?? "Unknown";
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value ?? "Unknown";

            _logger.LogInformation("?? [AUTH-{OperationId}] Starting logout process for user: {Username} ({Role})", 
                operationId, username, userRole);

            try
            {
                // Step 1: Clear authentication cookies
                await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Step 2: Clear all authentication-related cookies
                foreach (var cookie in context.Request.Cookies.Keys)
                {
                    if (cookie.StartsWith(".AspNetCore.") || 
                        cookie.Contains("Auth") || 
                        cookie.Contains("Session"))
                    {
                        context.Response.Cookies.Delete(cookie, new CookieOptions
                        {
                            Path = "/",
                            HttpOnly = true,
                            Secure = context.Request.IsHttps,
                            SameSite = SameSiteMode.Lax
                        });
                        _logger.LogDebug("?? [AUTH-{OperationId}] Deleted cookie: {CookieName}", operationId, cookie);
                    }
                }
                
                // Step 3: Ensure session is abandoned
                if (context.Session.IsAvailable)
                {
                    context.Session.Clear();
                }
                
                // Step 4: Add cache control headers to prevent caching of authenticated content
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";

                _logger.LogInformation("? [AUTH-{OperationId}] Logout successful for user: {Username} ({Role})", 
                    operationId, username, userRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error during logout for user {Username}: {ErrorMessage}", 
                    operationId, username, ex.Message);
                throw;
            }
        }

        public string HashPassword(string password)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [AUTH-{OperationId}] Hashing password", operationId);

            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Attempted to hash empty password", operationId);
                    throw new ArgumentException("Password cannot be empty");
                }

                using var sha256 = SHA256.Create();
                var saltedPassword = password + "OpCentrixSalt2024!";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                var hashedPassword = Convert.ToBase64String(hashedBytes);

                _logger.LogDebug("? [AUTH-{OperationId}] Password hashed successfully", operationId);
                return hashedPassword;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error hashing password: {ErrorMessage}", 
                    operationId, ex.Message);
                throw;
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [AUTH-{OperationId}] Verifying password", operationId);

            try
            {
                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Attempted to verify empty password", operationId);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(hash))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Attempted to verify against empty hash", operationId);
                    return false;
                }

                var hashedPassword = HashPassword(password);
                var isValid = hashedPassword == hash;

                _logger.LogDebug("?? [AUTH-{OperationId}] Password verification result: {IsValid}", operationId, isValid);
                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error verifying password: {ErrorMessage}", 
                    operationId, ex.Message);
                return false;
            }
        }

        public async Task<User?> GetCurrentUserAsync(HttpContext context)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var username = context.User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogDebug("?? [AUTH-{OperationId}] No authenticated user found", operationId);
                return null;
            }

            _logger.LogDebug("?? [AUTH-{OperationId}] Getting current user: {Username}", operationId, username);

            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Current user not found in database: {Username}", operationId, username);
                    return null;
                }

                _logger.LogDebug("? [AUTH-{OperationId}] Current user retrieved: {Username} ({Role})", 
                    operationId, username, user.Role);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error getting current user {Username}: {ErrorMessage}", 
                    operationId, username, ex.Message);
                return null;
            }
        }

        public async Task<User?> GetCurrentUserWithSettingsAsync(HttpContext context)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var username = context.User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                _logger.LogDebug("?? [AUTH-{OperationId}] No authenticated user found for settings retrieval", operationId);
                return null;
            }

            _logger.LogDebug("?? [AUTH-{OperationId}] Getting current user with settings: {Username}", operationId, username);

            try
            {
                var user = await _context.Users
                    .Include(u => u.Settings)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if (user == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Current user not found in database: {Username}", operationId, username);
                    return null;
                }

                _logger.LogDebug("? [AUTH-{OperationId}] Current user with settings retrieved: {Username} ({Role}) - Settings: {HasSettings}", 
                    operationId, username, user.Role, user.Settings != null);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error getting current user with settings {Username}: {ErrorMessage}", 
                    operationId, username, ex.Message);
                return null;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("?? [AUTH-{OperationId}] Getting all users", operationId);

            try
            {
                var users = await _context.Users
                    .Include(u => u.Settings)
                    .OrderBy(u => u.Username)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogDebug("? [AUTH-{OperationId}] Retrieved {UserCount} users", operationId, users.Count);
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error getting all users: {ErrorMessage}", 
                    operationId, ex.Message);
                return new List<User>();
            }
        }

        public async Task<User?> CreateUserAsync(User user, string password)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Creating new user: {Username} ({Role})", 
                operationId, user.Username, user.Role);

            try
            {
                // Validate user data
                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot create user: Empty username", operationId);
                    throw new ArgumentException("Username is required");
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot create user {Username}: Empty password", operationId, user.Username);
                    throw new ArgumentException("Password is required");
                }

                // Check for duplicate username
                var existingUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == user.Username.ToLower());

                if (existingUser != null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot create user: Username {Username} already exists", 
                        operationId, user.Username);
                    throw new ArgumentException($"Username '{user.Username}' already exists");
                }

                // Set up new user
                user.PasswordHash = HashPassword(password);
                user.CreatedDate = DateTime.UtcNow;
                user.LastModifiedDate = DateTime.UtcNow;
                user.IsActive = true;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("? [AUTH-{OperationId}] User created successfully: {Username} (ID: {UserId})", 
                    operationId, user.Username, user.Id);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error creating user {Username}: {ErrorMessage}", 
                    operationId, user.Username, ex.Message);
                throw;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Updating user: {Username} (ID: {UserId})", 
                operationId, user.Username, user.Id);

            try
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot update user: User ID {UserId} not found", 
                        operationId, user.Id);
                    return false;
                }

                // Track changes
                var changes = new List<string>();
                if (existingUser.FullName != user.FullName)
                    changes.Add($"FullName: {existingUser.FullName} -> {user.FullName}");
                if (existingUser.Email != user.Email)
                    changes.Add($"Email: {existingUser.Email} -> {user.Email}");
                if (existingUser.Role != user.Role)
                    changes.Add($"Role: {existingUser.Role} -> {user.Role}");
                if (existingUser.IsActive != user.IsActive)
                    changes.Add($"IsActive: {existingUser.IsActive} -> {user.IsActive}");

                if (changes.Any())
                {
                    _logger.LogDebug("?? [AUTH-{OperationId}] User changes detected: {Changes}", 
                        operationId, string.Join("; ", changes));
                }

                // Update user fields
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.Role = user.Role;
                existingUser.Department = user.Department;
                existingUser.IsActive = user.IsActive;
                existingUser.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("? [AUTH-{OperationId}] User updated successfully: {Username} (ID: {UserId})", 
                    operationId, user.Username, user.Id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error updating user {Username} (ID: {UserId}): {ErrorMessage}", 
                    operationId, user.Username, user.Id, ex.Message);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Deleting user with ID: {UserId}", operationId, userId);

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot delete user: User ID {UserId} not found", 
                        operationId, userId);
                    return false;
                }

                _logger.LogDebug("?? [AUTH-{OperationId}] Deleting user: {Username}", operationId, user.Username);

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("? [AUTH-{OperationId}] User deleted successfully: {Username} (ID: {UserId})", 
                    operationId, user.Username, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error deleting user ID {UserId}: {ErrorMessage}", 
                    operationId, userId, ex.Message);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string newPassword)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [AUTH-{OperationId}] Changing password for user ID: {UserId}", operationId, userId);

            try
            {
                if (string.IsNullOrWhiteSpace(newPassword))
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot change password: Empty password provided", operationId);
                    throw new ArgumentException("Password cannot be empty");
                }

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("?? [AUTH-{OperationId}] Cannot change password: User ID {UserId} not found", 
                        operationId, userId);
                    return false;
                }

                _logger.LogDebug("?? [AUTH-{OperationId}] Changing password for user: {Username}", operationId, user.Username);

                user.PasswordHash = HashPassword(newPassword);
                user.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("? [AUTH-{OperationId}] Password changed successfully for user: {Username} (ID: {UserId})", 
                    operationId, user.Username, userId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [AUTH-{OperationId}] Error changing password for user ID {UserId}: {ErrorMessage}", 
                    operationId, userId, ex.Message);
                return false;
            }
        }
    }
}