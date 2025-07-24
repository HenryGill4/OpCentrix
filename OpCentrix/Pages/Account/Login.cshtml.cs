using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpCentrix.Services;
using System.ComponentModel.DataAnnotations;

namespace OpCentrix.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAuthenticationService _authService;

        public LoginModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public LoginInput Input { get; set; } = new();

        public string? ReturnUrl { get; set; }
        public string? ErrorMessage { get; set; }

        public class LoginInput
        {
            [Required]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Remember me")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return Page();

            var user = await _authService.AuthenticateAsync(Input.Username, Input.Password);
            if (user == null)
            {
                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            await _authService.LoginAsync(HttpContext, user);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect based on user role with Print Tracking support
            return user.Role switch
            {
                "Admin" => RedirectToPage("/Admin/Index"),
                "Manager" => RedirectToPage("/Scheduler/Index"),
                "Scheduler" => RedirectToPage("/Scheduler/Index"),
                "Operator" => RedirectToPage("/PrintTracking/Index"), // Operators go to Print Tracking
                "PrintingSpecialist" => RedirectToPage("/PrintTracking/Index"), // Printer users go to Print Tracking
                "CoatingSpecialist" => RedirectToPage("/Coating/Index"),
                "ShippingSpecialist" => RedirectToPage("/Shipping/Index"),
                "EDMSpecialist" => RedirectToPage("/EDM/Index"),
                "MachiningSpecialist" => RedirectToPage("/Machining/Index"),
                "QCSpecialist" => RedirectToPage("/QC/Index"),
                "MediaSpecialist" => RedirectToPage("/Media/Index"),
                "Analyst" => RedirectToPage("/Analytics/Index"),
                _ => RedirectToPage("/Scheduler/Index")
            };
        }
    }
}