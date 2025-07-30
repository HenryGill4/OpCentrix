using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpCentrix.Pages.Admin.BT
{
    [Authorize(Policy = "AdminOnly")]
    public class PartClassificationsModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}