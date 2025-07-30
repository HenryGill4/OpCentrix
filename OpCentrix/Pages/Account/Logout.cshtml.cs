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
                
                // SEGMENT 5 FIX 5.2: Enhanced session clearing for test environments
                // Clear any test-specific session state
                if (HttpContext.Session.IsAvailable)
                {
                    HttpContext.Session.Clear();
                }
                
                // Force cache headers to prevent caching of authenticated content
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, private";
                Response.Headers["Pragma"] = "no-cache";
                Response.Headers["Expires"] = "0";
                
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
                
                // SEGMENT 5 FIX 5.2: Enhanced session invalidation response
                // For AJAX/HTMX requests, return JSON with redirect instruction
                if (Request.Headers.ContainsKey("HX-Request") || Request.Headers.ContainsKey("X-Requested-With"))
                {
                    Response.Headers.Add("HX-Redirect", "/Account/Login");
                    return new JsonResult(new { success = true, message = "Logged out successfully", redirect = "/Account/Login" });
                }
                
                // For regular POST requests, ensure no caching and clear any remaining session state
                Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                Response.Headers["Pragma"] = "no-cache";
                
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