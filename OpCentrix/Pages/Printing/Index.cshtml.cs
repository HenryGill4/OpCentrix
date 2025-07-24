using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Authorization;

namespace OpCentrix.Pages.Printing
{
    [PrintingAccess]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}