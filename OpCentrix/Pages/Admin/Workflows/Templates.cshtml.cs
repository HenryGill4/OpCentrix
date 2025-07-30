using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpCentrix.Pages.Admin.Workflows
{
    [Authorize(Policy = "AdminOnly")]
    public class TemplatesModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}