using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Pages
{
    [MediaAccess]
    public class MediaModel : PageModel
    {
        private readonly ILogger<MediaModel> _logger;

        public MediaModel(ILogger<MediaModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("Media page accessed");
        }
    }
}
