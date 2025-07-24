using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;

namespace OpCentrix.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthenticationService _authService;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(IAuthenticationService authService, ILogger<LogoutModel> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
            
            _logger.LogInformation("?? Logout requested via GET by user: {UserName} ({Role})", userName, userRole);
            
            try 
            {
                await _authService.LogoutAsync(HttpContext);
                _logger.LogInformation("? User {UserName} logged out successfully", userName);
                
                // Add success message for next request
                TempData["LogoutMessage"] = "You have been successfully logged out.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error during logout for user: {UserName}", userName);
                TempData["LogoutError"] = "An error occurred during logout. Please close your browser if you continue to have issues.";
            }
            
            return RedirectToPage("/Account/Login");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userName = User.Identity?.Name ?? "Unknown";
            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";
            
            _logger.LogInformation("?? Logout requested via POST by user: {UserName} ({Role})", userName, userRole);
            
            try 
            {
                await _authService.LogoutAsync(HttpContext);
                _logger.LogInformation("? User {UserName} logged out successfully", userName);
                
                // For AJAX/HTMX requests, return success status
                if (Request.Headers.ContainsKey("HX-Request") || Request.Headers.ContainsKey("X-Requested-With"))
                {
                    return new JsonResult(new { success = true, message = "Logged out successfully" });
                }
                
                // Add success message for regular requests
                TempData["LogoutMessage"] = "You have been successfully logged out.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error during logout for user: {UserName}", userName);
                
                // For AJAX/HTMX requests, return error status
                if (Request.Headers.ContainsKey("HX-Request") || Request.Headers.ContainsKey("X-Requested-With"))
                {
                    return new JsonResult(new { success = false, message = "Logout failed" }) { StatusCode = 500 };
                }
                
                TempData["LogoutError"] = "An error occurred during logout. Please close your browser if you continue to have issues.";
            }
            
            return RedirectToPage("/Account/Login");
        }
    }
}