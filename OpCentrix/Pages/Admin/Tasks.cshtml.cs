using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;

namespace OpCentrix.Pages.Admin
{
    [Authorize(Policy = "AdminOnly")]
    public class TasksModel : PageModel
    {
        private readonly ILogger<TasksModel> _logger;

        public TasksModel(ILogger<TasksModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Redirect to the CRM task system which has replaced the legacy operational tasks
            _logger.LogInformation("Redirecting from legacy /Admin/Tasks to modern /CRM/Tasks/Admin");
            return RedirectToPage("/CRM/Tasks/AdminDashboard");
        }
    }
}