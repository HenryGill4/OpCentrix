using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using OpCentrix.Authorization;

namespace OpCentrix.Pages
{
    [Authorize]
    [EDMAccess]
    public class EDMModel : PageModel
    {
        private readonly ILogger<EDMModel> _logger;

        public EDMModel(ILogger<EDMModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("?? EDM Operations page accessed by user: {User}", User.Identity?.Name ?? "Unknown");
        }

        public IActionResult OnPost()
        {
            // Handle any form submissions if needed in the future
            _logger.LogInformation("?? EDM log form submitted by user: {User}", User.Identity?.Name ?? "Unknown");

            // For now, just return the page - the form is handled client-side for printing
            return Page();
        }
    }
}
