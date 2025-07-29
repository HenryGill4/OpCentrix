using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.PrintTracking;
using OpCentrix.ViewModels.Shared;
using OpCentrix.Services;
using OpCentrix.Authorization;
using System.Security.Claims;

namespace OpCentrix.Pages.PrintTracking
{
    [PrintTrackingAccess] // Use specific Print Tracking access attribute
    public class IndexModel : PageModel
    {
        private readonly IPrintTrackingService _printTrackingService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public PrintTrackingDashboardViewModel Dashboard { get; set; } = new();

        public IndexModel(IPrintTrackingService printTrackingService, SchedulerContext context, ILogger<IndexModel> logger)
        {
            _printTrackingService = printTrackingService;
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);
                
                _logger.LogInformation("Print tracking dashboard loaded for user {UserId} with role {Role}", 
                    userId, User.FindFirst("Role")?.Value ?? "Unknown");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading print tracking dashboard for user {UserId}", GetCurrentUserId());
                Dashboard = new PrintTrackingDashboardViewModel 
                { 
                    UserId = GetCurrentUserId(),
                    OperatorName = User.Identity?.Name ?? "Unknown"
                };
            }
        }

        public async Task<IActionResult> OnGetStartPrintModalAsync(string? printerName = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                // Check if printer already has active build
                if (!string.IsNullOrEmpty(printerName))
                {
                    var hasActiveBuild = await _printTrackingService.HasActiveBuildAsync(printerName);
                    if (hasActiveBuild)
                    {
                        var activeBuild = await _printTrackingService.GetActiveBuildJobAsync(printerName);
                        return Partial("_ActiveBuildWarning", new { 
                            PrinterName = printerName, 
                            ActiveBuild = activeBuild 
                        });
                    }
                }

                var model = new PrintStartViewModel
                {
                    PrinterName = printerName ?? string.Empty,
                    ActualStartTime = DateTime.Now,
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    AvailableScheduledJobs = !string.IsNullOrEmpty(printerName) 
                        ? await _printTrackingService.GetAvailableScheduledJobsAsync(printerName)
                        : new List<Job>()
                };

                return Partial("_StartPrintModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading start print modal");
                return Partial("_ErrorModal", new { Message = "Error loading form. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostStartPrintAsync([FromForm] PrintStartViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _context.Users.FindAsync(GetCurrentUserId());
                    model.OperatorName = user?.FullName ?? "Unknown";
                    model.UserId = GetCurrentUserId();
                    model.AvailableScheduledJobs = await _printTrackingService.GetAvailableScheduledJobsAsync(model.PrinterName);
                    return Partial("_StartPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var buildId = await _printTrackingService.StartPrintJobAsync(model, userId);

                _logger.LogInformation("Print started: BuildId {BuildId}, Printer {PrinterName}, User {UserId}", 
                    buildId, model.PrinterName, userId);

                // Return success response that will refresh the dashboard
                return new JsonResult(new { success = true, buildId, message = "Print started successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job");
                return new JsonResult(new { success = false, message = "Error starting print. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetPostPrintModalAsync(int? buildId = null, string? printerName = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                var model = new PostPrintViewModel
                {
                    UserId = userId,
                    OperatorName = user?.FullName ?? "Unknown",
                    BuildId = buildId,
                    PrinterName = printerName ?? string.Empty,
                    ActualStartTime = DateTime.Now.AddHours(-4), // Default to 4 hours ago
                    ActualEndTime = DateTime.Now,
                    Parts = new List<PostPrintPartEntry> { new PostPrintPartEntry { IsPrimary = true } },
                    AvailableParts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync()
                };

                // If buildId provided, load existing build data
                if (buildId.HasValue)
                {
                    var existingBuild = await _context.BuildJobs.FindAsync(buildId.Value);
                    if (existingBuild != null)
                    {
                        model.PrinterName = existingBuild.PrinterName;
                        model.ActualStartTime = existingBuild.ActualStartTime;
                        model.ScheduledStartTime = existingBuild.ScheduledStartTime;
                        // Remove SetupNotes reference since it doesn't exist in PostPrintViewModel
                    }
                }
                // If printerName provided, check for active build
                else if (!string.IsNullOrEmpty(printerName))
                {
                    var activeBuild = await _printTrackingService.GetActiveBuildJobAsync(printerName);
                    if (activeBuild != null)
                    {
                        model.BuildId = activeBuild.BuildId;
                        model.ActualStartTime = activeBuild.ActualStartTime;
                        model.ScheduledStartTime = activeBuild.ScheduledStartTime;
                    }
                }

                // Check for delays and populate delay info if needed
                CheckForDelays(model);

                return Partial("_PostPrintModal", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading post-print modal");
                return Partial("_ErrorModal", new { Message = "Error loading form. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostCompletePrintAsync([FromForm] PostPrintViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _context.Users.FindAsync(GetCurrentUserId());
                    model.OperatorName = user?.FullName ?? "Unknown";
                    model.UserId = GetCurrentUserId();
                    model.AvailableParts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                    CheckForDelays(model);
                    return Partial("_PostPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var success = await _printTrackingService.CompletePrintJobAsync(model, userId);

                if (success)
                {
                    _logger.LogInformation("Print completed: BuildId {BuildId}, Printer {PrinterName}, User {UserId}", 
                        model.BuildId, model.PrinterName, userId);

                    return new JsonResult(new { success = true, message = "Print completed successfully!" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Error completing print." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing print job");
                return new JsonResult(new { success = false, message = "Error completing print. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetRefreshDashboardAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);
                return Partial("_PrintTrackingDashboard", Dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard");
                return Content("Error refreshing dashboard");
            }
        }

        public async Task<IActionResult> OnGetEmbeddedViewAsync()
        {
            try
            {
                // Load current jobs for today and next 2 days (mini view)
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(3);
                
                var jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.ScheduledStart < endDate && j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ThenBy(j => j.MachineId)
                    .AsNoTracking()
                    .Take(20) // Limit for mini view
                    .ToListAsync();

                // Create a proper view model for the mini scheduler
                var miniViewModel = new EmbeddedSchedulerViewModel
                {
                    Jobs = jobs,
                    Machines = jobs.Select(j => j.MachineId).Distinct().OrderBy(m => m).ToList(),
                    Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList(),
                    StartDate = startDate
                };

                return Partial("_EmbeddedScheduler", miniViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading embedded scheduler view");
                
                // Return a fallback view with helpful message
                return Content(@"
                    <div class='p-3 text-center text-gray-500'>
                        <svg class='w-8 h-8 mx-auto mb-2' fill='none' stroke='currentColor' viewBox='0 0 24 24'>
                            <path stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z'></path>
                        </svg>
                        <p>Unable to load schedule</p>
                        <p class='text-xs'>Please refresh the page or contact IT support</p>
                    </div>
                ");
            }
        }

        private int GetCurrentUserId()
        {
            // Try multiple claim types for user ID
            var userIdClaim = User.FindFirst("UserId")?.Value ?? 
                             User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                             User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            // Fallback - try to find user by username
            var username = User.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                var user = _context.Users.FirstOrDefault(u => u.Username == username);
                return user?.Id ?? 1; // Default to ID 1 if not found
            }

            return 1; // Default fallback
        }

        private void CheckForDelays(PostPrintViewModel model)
        {
            if (model.ScheduledStartTime.HasValue)
            {
                var delayMinutes = DelayLog.CalculateMaxDelay(
                    model.ScheduledStartTime, 
                    model.ActualStartTime, 
                    DateTime.UtcNow
                );

                if (delayMinutes > 0)
                {
                    model.HasDelay = true;
                    model.DelayInfo = new DelayInfo
                    {
                        DelayDuration = delayMinutes,
                        AvailableReasons = DelayLog.DelayReasons.ToList()
                    };
                }
            }
        }
    }
}