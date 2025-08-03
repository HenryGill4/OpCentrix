using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.ViewModels.PrintTracking;
using OpCentrix.ViewModels.Shared;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using OpCentrix.Authorization;
using System.Security.Claims;

namespace OpCentrix.Pages.PrintTracking
{
    [PrintTrackingAccess] // Use specific Print Tracking access attribute
    public class IndexModel : PageModel
    {
        private readonly IPrintTrackingService _printTrackingService;
        private readonly IMachineManagementService _machineManagementService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public PrintTrackingDashboardViewModel Dashboard { get; set; } = new();

        public IndexModel(IPrintTrackingService printTrackingService,
                         IMachineManagementService machineManagementService,
                         SchedulerContext context,
                         ILogger<IndexModel> logger)
        {
            _printTrackingService = printTrackingService;
            _machineManagementService = machineManagementService;
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync(int? jobId = null, string? machineId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);

                // ENHANCED: Handle scheduler integration parameters
                if (jobId.HasValue)
                {
                    ViewData["HighlightJobId"] = jobId.Value;
                    ViewData["ScrollToJob"] = true;

                    // Pre-load job details for context
                    var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
                    if (job != null)
                    {
                        ViewData["JobContext"] = job;
                        TempData["Info"] = $"Scheduler job loaded: {job.PartNumber} on {job.MachineId}";
                    }
                }

                if (!string.IsNullOrEmpty(machineId))
                {
                    ViewData["HighlightMachineId"] = machineId;
                }

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

        public async Task<IActionResult> OnGetStartPrintModalAsync(string? printerName = null, int? jobId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                // Get available printers from machine management service
                var availableMachines = await _machineManagementService.GetActiveMachinesAsync();
                var printerMachines = availableMachines
                    .Where(m => m.MachineType == "SLS" || m.MachineId.Contains("TI") || m.MachineId.Contains("INC"))
                    .OrderBy(m => m.Priority)
                    .ThenBy(m => m.MachineId)
                    .ToList();

                // Check if printer already has active build
                if (!string.IsNullOrEmpty(printerName))
                {
                    var hasActiveBuild = await _printTrackingService.HasActiveBuildAsync(printerName);
                    if (hasActiveBuild)
                    {
                        var activeBuild = await _printTrackingService.GetActiveBuildJobAsync(printerName);
                        return Partial("_ActiveBuildWarning", new
                        {
                            PrinterName = printerName,
                            ActiveBuild = activeBuild
                        });
                    }
                }

                var model = new PrintStartViewModel
                {
                    PrinterName = printerName ?? string.Empty,
                    ActualStartTime = DateTime.Now,
                    EstimatedEndTime = DateTime.Now.AddHours(4), // Default 4 hours from now
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    AvailablePrinters = printerMachines.Select(m => m.MachineId).ToList(),
                    AvailableScheduledJobs = !string.IsNullOrEmpty(printerName)
                        ? await _printTrackingService.GetAvailableScheduledJobsAsync(printerName)
                        : new List<Job>()
                };

                // Add fallback printers if no machines found in database
                if (!model.AvailablePrinters.Any())
                {
                    model.AvailablePrinters = new List<string> { "TI1", "TI2", "INC", "INC1", "INC2" };
                    _logger.LogWarning("No SLS machines found in database, using fallback printer list");
                }

                // ENHANCED: Pre-populate from scheduler job if provided
                if (jobId.HasValue)
                {
                    var scheduledJob = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
                    if (scheduledJob != null)
                    {
                        model.AssociatedScheduledJobId = scheduledJob.Id;
                        model.PartId = scheduledJob.PartId;
                        model.PartNumber = scheduledJob.PartNumber;
                        model.PartDescription = scheduledJob.Part?.Description ?? "";
                        model.Material = scheduledJob.SlsMaterial;
                        model.Quantity = scheduledJob.Quantity;
                        model.EstimatedHours = scheduledJob.EstimatedHours;
                        model.ScheduledStartTime = scheduledJob.ScheduledStart;
                        model.ScheduledEndTime = scheduledJob.ScheduledEnd;
                        model.EstimatedEndTime = scheduledJob.ScheduledStart.AddHours(scheduledJob.EstimatedHours);

                        _logger.LogInformation("Pre-populated start print form from scheduler job {JobId}", jobId.Value);
                    }
                }

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
                // ENHANCED: Handle prototype additions validation properly
                if (!model.AddPrototypes)
                {
                    // Clear prototype additions if not adding prototypes to avoid validation errors
                    model.PrototypeAdditions = new List<PrototypeAddition>();
                }
                else
                {
                    // Filter out empty prototype additions
                    model.PrototypeAdditions = model.PrototypeAdditions?
                        .Where(p => !string.IsNullOrWhiteSpace(p.PartNumber))
                        .ToList() ?? new List<PrototypeAddition>();
                }

                // Remove validation errors for prototype additions if not adding prototypes
                if (!model.AddPrototypes)
                {
                    var keysToRemove = ModelState.Keys
                        .Where(key => key.StartsWith("PrototypeAdditions"))
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        ModelState.Remove(key);
                    }
                }

                // Add validation debugging
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {string.Join(", ", x.Value.Errors.Select(y => y.ErrorMessage))}")
                        .ToList();

                    _logger.LogWarning("Start print validation failed for printer {PrinterName}. Errors: {Errors}",
                        model.PrinterName, string.Join(", ", errors));

                    // Reload required data for the form
                    var user = await _context.Users.FindAsync(GetCurrentUserId());
                    model.OperatorName = user?.FullName ?? "Unknown";
                    model.UserId = GetCurrentUserId();

                    // Reload available printers
                    var availableMachines = await _machineManagementService.GetActiveMachinesAsync();
                    var printerMachines = availableMachines
                        .Where(m => m.MachineType == "SLS" || m.MachineId.Contains("TI") || m.MachineId.Contains("INC"))
                        .Select(m => m.MachineId)
                        .ToList();

                    if (!printerMachines.Any())
                    {
                        printerMachines.AddRange(new[] { "TI1", "TI2", "INC", "INC1", "INC2" });
                    }

                    model.AvailablePrinters = printerMachines;
                    model.AvailableScheduledJobs = await _printTrackingService.GetAvailableScheduledJobsAsync(model.PrinterName);

                    return Partial("_StartPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var buildId = await _printTrackingService.StartPrintJobAsync(model, userId);

                _logger.LogInformation("Print started: BuildId {BuildId}, Printer {PrinterName}, User {UserId}",
                    buildId, model.PrinterName, userId);

                // ENHANCED: Return success with scheduler integration info
                var responseData = new
                {
                    success = true,
                    buildId,
                    message = "Print started successfully!",
                    schedulerJobId = model.AssociatedScheduledJobId,
                    partNumber = model.PartNumber,
                    machineId = model.PrinterName,
                    operatorEstimateHours = model.OperatorEstimatedHours,
                    scheduleUpdated = model.AssociatedScheduledJobId.HasValue,
                    refreshSchedulerNeeded = true
                };

                return new JsonResult(responseData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job for printer {PrinterName}", model?.PrinterName ?? "Unknown");
                return new JsonResult(new { success = false, message = "Error starting print. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetPostPrintModalAsync(int? buildId = null, string? printerName = null, int? jobId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                var model = new PostPrintViewModel
                {
                    UserId = userId,
                    OperatorName = user?.FullName ?? "Unknown",
                    BuildId = buildId ?? 0,
                    PrinterName = printerName ?? string.Empty,
                    ActualStartTime = DateTime.Now.AddHours(-4), // Default to 4 hours ago
                    ActualEndTime = DateTime.Now,
                    Parts = new List<PostPrintPartEntry> { new PostPrintPartEntry { IsPrimary = true } },
                    AvailableParts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync(),
                    ReasonForEnd = "Completed Successfully", // Default to successful completion
                    OperatorActualHours = 4.0m // Default 4 hours
                };

                // Get available printers from machine management service
                var availableMachines = await _machineManagementService.GetActiveMachinesAsync();
                var printerMachines = availableMachines
                    .Where(m => m.MachineType == "SLS" || m.MachineId.Contains("TI") || m.MachineId.Contains("INC"))
                    .Select(m => m.MachineId)
                    .ToList();

                if (!printerMachines.Any())
                {
                    printerMachines.AddRange(new[] { "TI1", "TI2", "INC", "INC1", "INC2" });
                }

                model.AvailablePrinters = printerMachines;

                // ENHANCED: Get all running jobs for dropdown selection
                var runningJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.Status == "In Progress" || j.Status == "Building")
                    .OrderBy(j => j.MachineId)
                    .ThenBy(j => j.ScheduledStart)
                    .ToListAsync();

                model.AvailableRunningJobs = runningJobs;

                // ENHANCED: Handle scheduler job integration - auto-populate from selected job
                if (jobId.HasValue)
                {
                    var scheduledJob = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
                    if (scheduledJob != null)
                    {
                        await PopulateFromScheduledJob(model, scheduledJob);
                        _logger.LogInformation("Pre-populated completion form from scheduler job {JobId}", jobId.Value);
                    }
                }
                // If buildId provided, load existing build data
                else if (buildId.HasValue)
                {
                    var existingBuild = await _context.BuildJobs
                        .Include(b => b.BuildJobParts)
                        .FirstOrDefaultAsync(b => b.BuildId == buildId.Value);
                    if (existingBuild != null)
                    {
                        await PopulateFromBuildJob(model, existingBuild);
                        _logger.LogInformation("Pre-populated completion form from build job {BuildId}", buildId.Value);
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
                        
                        // Try to find associated job for this build
                        if (activeBuild.AssociatedScheduledJobId.HasValue)
                        {
                            var associatedJob = await _context.Jobs
                                .Include(j => j.Part)
                                .FirstOrDefaultAsync(j => j.Id == activeBuild.AssociatedScheduledJobId.Value);
                            if (associatedJob != null)
                            {
                                await PopulateFromScheduledJob(model, associatedJob);
                            }
                        }
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

        // NEW: Get job details for auto-population when job is selected from dropdown
        public async Task<IActionResult> OnGetJobDetailsAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }

                // Find associated build job if exists
                var buildJob = await _context.BuildJobs
                    .FirstOrDefaultAsync(b => b.AssociatedScheduledJobId == jobId);

                var jobDetails = new
                {
                    success = true,
                    jobId = job.Id,
                    partNumber = job.PartNumber,
                    partDescription = job.Part?.Description ?? "",
                    partId = job.PartId,
                    quantity = job.Quantity,
                    material = job.SlsMaterial,
                    machineId = job.MachineId,
                    scheduledStart = job.ScheduledStart.ToString("yyyy-MM-ddTHH:mm"),
                    scheduledEnd = job.ScheduledEnd.ToString("yyyy-MM-ddTHH:mm"),
                    actualStart = job.ActualStart?.ToString("yyyy-MM-ddTHH:mm"),
                    estimatedHours = job.EstimatedHours,
                    buildId = buildJob?.BuildId,
                    operatorEstimatedHours = buildJob?.OperatorEstimatedHours
                };

                return new JsonResult(jobDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job details for job {JobId}", jobId);
                return new JsonResult(new { success = false, error = "Error loading job details" });
            }
        }

        // Helper method to populate model from scheduled job
        private async Task PopulateFromScheduledJob(PostPrintViewModel model, Job scheduledJob)
        {
            model.JobId = scheduledJob.Id;
            model.PartId = scheduledJob.PartId;
            model.PartNumber = scheduledJob.PartNumber;
            model.PartDescription = scheduledJob.Part?.Description ?? "";
            model.PrinterName = scheduledJob.MachineId;

            // Pre-populate parts list from scheduler job
            model.Parts = new List<PostPrintPartEntry>
            {
                new PostPrintPartEntry
                {
                    PartNumber = scheduledJob.PartNumber,
                    Quantity = scheduledJob.Quantity,
                    GoodParts = scheduledJob.Quantity, // Default to all good
                    Description = scheduledJob.Part?.Description ?? "",
                    Material = scheduledJob.SlsMaterial ?? "",
                    IsPrimary = true
                }
            };

            // Use actual times if job was started
            if (scheduledJob.ActualStart.HasValue)
            {
                model.ActualStartTime = scheduledJob.ActualStart.Value;
            }
            else
            {
                model.ActualStartTime = scheduledJob.ScheduledStart;
            }

            model.ScheduledStartTime = scheduledJob.ScheduledStart;
            model.ScheduledEndTime = scheduledJob.ScheduledEnd;
            model.AssociatedJobNumber = $"Job #{scheduledJob.Id}";
            model.EstimatedHours = scheduledJob.EstimatedHours;

            // Set estimated actual hours from build job if available
            var buildJob = await _context.BuildJobs
                .FirstOrDefaultAsync(b => b.AssociatedScheduledJobId == scheduledJob.Id);
            if (buildJob != null)
            {
                model.BuildId = buildJob.BuildId;
                model.OperatorEstimatedHours = buildJob.OperatorEstimatedHours;
                if (buildJob.ActualStartTime != DateTime.MinValue)
                {
                    model.ActualStartTime = buildJob.ActualStartTime;
                }
            }
        }

        // Helper method to populate model from build job
        private async Task PopulateFromBuildJob(PostPrintViewModel model, BuildJob buildJob)
        {
            model.BuildId = buildJob.BuildId;
            model.PrinterName = buildJob.PrinterName;
            model.ActualStartTime = buildJob.ActualStartTime;
            model.ScheduledStartTime = buildJob.ScheduledStartTime;
            model.ScheduledEndTime = buildJob.ScheduledEndTime;
            model.OperatorEstimatedHours = buildJob.OperatorEstimatedHours;

            // Populate from associated scheduled job if available
            if (buildJob.AssociatedScheduledJobId.HasValue)
            {
                var associatedJob = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == buildJob.AssociatedScheduledJobId.Value);
                if (associatedJob != null)
                {
                    model.JobId = associatedJob.Id;
                    model.PartId = associatedJob.PartId;
                    model.PartNumber = associatedJob.PartNumber;
                    model.PartDescription = associatedJob.Part?.Description ?? "";
                    model.AssociatedJobNumber = $"Job #{associatedJob.Id}";
                    model.EstimatedHours = associatedJob.EstimatedHours;

                    // Pre-populate parts from job
                    model.Parts = new List<PostPrintPartEntry>
                    {
                        new PostPrintPartEntry
                        {
                            PartNumber = associatedJob.PartNumber,
                            Quantity = associatedJob.Quantity,
                            GoodParts = associatedJob.Quantity, // Default to all good
                            Description = associatedJob.Part?.Description ?? "",
                            Material = associatedJob.SlsMaterial ?? "",
                            IsPrimary = true
                        }
                    };
                }
            }
            // If no associated job, populate from build job parts
            else if (buildJob.BuildJobParts?.Any() == true)
            {
                model.Parts = buildJob.BuildJobParts.Select(bjp => new PostPrintPartEntry
                {
                    PartNumber = bjp.PartNumber,
                    Quantity = bjp.Quantity,
                    GoodParts = bjp.Quantity, // Default to all good  
                    Description = bjp.Description ?? "",
                    Material = bjp.Material ?? "",
                    IsPrimary = bjp.IsPrimary
                }).ToList();
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

        // ENHANCED: New method to get scheduled jobs for a machine (for scheduler integration)
        public async Task<IActionResult> OnGetScheduledJobsAsync(string machineId)
        {
            try
            {
                var jobs = await _printTrackingService.GetAvailableScheduledJobsAsync(machineId);

                var jobData = jobs.Select(j => new
                {
                    id = j.Id,
                    partNumber = j.PartNumber,
                    partDescription = j.Part?.Description ?? "",
                    quantity = j.Quantity,
                    scheduledStart = j.ScheduledStart.ToString("yyyy-MM-ddTHH:mm"),
                    scheduledEnd = j.ScheduledEnd.ToString("yyyy-MM-ddTHH:mm"),
                    estimatedHours = j.EstimatedHours,
                    material = j.SlsMaterial,
                    status = j.Status,
                    isDelayed = j.ScheduledStart < DateTime.Now
                }).ToList();

                return new JsonResult(new { success = true, jobs = jobData });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scheduled jobs for machine {MachineId}", machineId);
                return new JsonResult(new { success = false, error = "Error loading scheduled jobs" });
            }
        }

        public async Task<IActionResult> OnGetEmbeddedViewAsync()
        {
            try
            {
                // ENHANCED: Load current jobs with scheduler integration
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

                // Add status indicators for print tracking
                var jobsWithStatus = jobs.Select(j => new
                {
                    Job = j,
                    IsScheduled = j.Status == "Scheduled",
                    IsBuilding = j.Status == "Building" || j.Status == "In Progress",
                    IsComplete = j.Status == "Completed" || j.Status == "Complete",
                    IsDelayed = j.ActualStart.HasValue && j.ActualStart > j.ScheduledStart.AddMinutes(15) ||
                               (!j.ActualStart.HasValue && j.ScheduledStart < DateTime.Now.AddMinutes(-15)),
                    CanStart = j.Status == "Scheduled" && Math.Abs((j.ScheduledStart - DateTime.Now).TotalMinutes) <= 60,
                    CanComplete = j.Status == "Building" || j.Status == "In Progress"
                }).ToList();

                // Create a proper view model for the mini scheduler - FIXED: Use fully qualified namespace
                var miniViewModel = new OpCentrix.ViewModels.PrintTracking.EmbeddedSchedulerViewModel
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
                        AvailableReasons = DelayLog.DelayReasons.ToList(),
                        DelayReason = "Late Start", // Default reason
                        DelayNotes = $"Print started {delayMinutes} minutes after scheduled time"
                    };
                }
            }
        }
    }
}