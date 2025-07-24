using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;

namespace OpCentrix.Pages.Account
{
    public class ExtendSessionModel : PageModel
    {
        private readonly IAuthenticationService _authService;

        public ExtendSessionModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _authService.GetCurrentUserAsync(HttpContext);
            if (user == null)
                return new UnauthorizedResult();

            // Re-authenticate to extend session
            await _authService.LoginAsync(HttpContext, user);
            
            return new JsonResult(new { success = true, message = "Session extended successfully" });
        }
    }
}