using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models.CRM;
using System.Security.Claims;

namespace OpCentrix.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ILogger<DashboardModel> _logger;
        private readonly SchedulerContext _context;

        public DashboardModel(ILogger<DashboardModel> logger, SchedulerContext context)
        {
            _logger = logger;
            _context = context;
        }

        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DashboardData Dashboard { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                UserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
                UserName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.Identity?.Name ?? "";
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    await LoadDashboardDataAsync(userId);
                }

                _logger.LogInformation("Dashboard accessed by user {UserName} with role {UserRole}", UserName, UserRole);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard for user {UserName}", UserName);
                return Page(); // Still show page with default data
            }
        }

        private async Task LoadDashboardDataAsync(int userId)
        {
            var now = DateTime.UtcNow;
            var startOfDay = now.Date;
            var endOfDay = startOfDay.AddDays(1);

            // Load user-specific task data
            var userTasks = await _context.CrmTasks
                .Where(t => t.AssignedToUserId == userId)
                .ToListAsync();

            Dashboard.TotalTasks = userTasks.Count;
            Dashboard.ActiveTasks = userTasks.Count(t => t.Status != "Completed");
            Dashboard.CompletedTasks = userTasks.Count(t => t.Status == "Completed");
            Dashboard.OverdueTasks = userTasks.Count(t => t.DueAt.HasValue && t.DueAt < now && t.Status != "Completed");
            Dashboard.TasksDueToday = userTasks.Count(t => t.DueAt.HasValue && 
                t.DueAt >= startOfDay && t.DueAt < endOfDay && t.Status != "Completed");

            // Recent task activity
            Dashboard.RecentTasks = userTasks
                .Where(t => t.Status != "Completed")
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueAt ?? DateTime.MaxValue)
                .Take(5)
                .ToList();

            // Load recent progress entries
            Dashboard.RecentProgress = await _context.CrmTaskProgress
                .Include(p => p.Task)
                .Where(p => p.CreatedByUserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .Take(3)
                .ToListAsync();

            // Quick stats for management roles
            if (UserRole is "Admin" or "Manager" or "Supervisor")
            {
                var allTasks = await _context.CrmTasks.ToListAsync();
                Dashboard.TeamTotalTasks = allTasks.Count;
                Dashboard.TeamActiveTasks = allTasks.Count(t => t.Status != "Completed");
                Dashboard.TeamOverdueTasks = allTasks.Count(t => t.DueAt.HasValue && t.DueAt < now && t.Status != "Completed");
            }
        }

        public class DashboardData
        {
            // Personal task stats
            public int TotalTasks { get; set; }
            public int ActiveTasks { get; set; }
            public int CompletedTasks { get; set; }
            public int OverdueTasks { get; set; }
            public int TasksDueToday { get; set; }
            
            // Team stats (for managers)
            public int TeamTotalTasks { get; set; }
            public int TeamActiveTasks { get; set; }
            public int TeamOverdueTasks { get; set; }
            
            // Recent activity
            public List<CrmTask> RecentTasks { get; set; } = new();
            public List<CrmTaskProgress> RecentProgress { get; set; } = new();
        }
    }
}
