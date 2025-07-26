using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Pages
{
    /// <summary>
    /// Page model for click-through testing navigation
    /// Provides easy access to all system pages for comprehensive error testing
    /// </summary>
    public class TestNavigationModel : PageModel
    {
        private readonly ILogger<TestNavigationModel> _logger;

        public TestNavigationModel(ILogger<TestNavigationModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var userName = User?.Identity?.Name ?? "Anonymous";
            
            _logger.LogInformation("?? [TEST-NAV-{OperationId}] Click-through testing page accessed by {User}",
                operationId, userName);
                
            _logger.LogInformation("?? [TEST-NAV-{OperationId}] Testing navigation page loaded - ready for comprehensive page testing",
                operationId);
        }
    }
}