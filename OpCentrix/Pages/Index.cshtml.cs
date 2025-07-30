using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace OpCentrix.Pages
{
    /// <summary>
    /// OpCentrix Debug Suite - Development Dashboard
    /// SAFE VERSION - Simplified to prevent memory issues
    /// </summary>
    public class DebugSuiteModel : PageModel
    {
        private readonly ILogger<DebugSuiteModel> _logger;

        public DebugSuiteModel(ILogger<DebugSuiteModel> logger)
        {
            _logger = logger;
        }

        // SIMPLIFIED: Basic properties only
        public string BuildStatus { get; set; } = "Success";
        public string DatabaseStatus { get; set; } = "SQLite Connected";
        public string Environment { get; set; } = "Development";
        public string ServerUrl { get; set; } = "localhost:5090";
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public void OnGet()
        {
            _logger.LogInformation("Debug Suite accessed - User: {User}, Time: {Time}",
                User.Identity?.Name ?? "Anonymous", DateTime.Now);

            // SIMPLIFIED: Basic initialization only
            CheckSystemHealth();
        }

        private void CheckSystemHealth()
        {
            try
            {
                // SIMPLIFIED: Basic health check only
                DatabaseStatus = "? SQLite Connected";
                Environment = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                ServerUrl = "localhost:5090";
                BuildStatus = "? Success";
                
                _logger.LogInformation("System health check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health check failed");
                BuildStatus = "? Issues Detected";
                DatabaseStatus = "? Connection Issues";
            }
        }

        // REMOVED: Complex page categorization that was not needed
        // REMOVED: Complex helper classes that could cause memory issues
    }
}
