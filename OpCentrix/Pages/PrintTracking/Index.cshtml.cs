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
    /// <summary>
    /// Print Tracking dashboard with SLS machine filtering and role-based views
    /// UPDATED: Only shows SLS machines and provides different views for admin vs operators
    /// </summary>
    [PrintTrackingAccess]
    public class IndexModel : PageModel
    {
        private readonly IPrintTrackingService _printTrackingService;
        private readonly IMachineManagementService _machineManagementService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public PrintTrackingDashboardViewModel Dashboard { get; set; } = new();

        // New: Role-based view properties
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;
        public string UserRole { get; set; } = string.Empty;

        public IndexModel(
            IPrintTrackingService printTrackingService,
            IMachineManagementService machineManagementService,
            SchedulerContext context,
            ILogger<IndexModel> logger)
        {
            _printTrackingService = printTrackingService ?? throw new ArgumentNullException(nameof(printTrackingService));
            _machineManagementService = machineManagementService ?? throw new ArgumentNullException(nameof(machineManagementService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync(int? jobId = null, string? machineId = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                // Determine user role and view type
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";

                // Load dashboard data with enhanced machine information (SLS only)
                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);

                // CRITICAL: Filter to show only SLS machines
                await PopulateSlsMachinesOnlyAsync();

                // Handle scheduler integration parameters
                await HandleSchedulerIntegrationAsync(jobId, machineId);

                _logger.LogInformation("Print tracking dashboard loaded for user {UserId} ({UserRole}) with {MachineCount} SLS machines. Admin view: {IsAdminView}",
                    userId, UserRole, Dashboard.AvailableMachines?.Count ?? 0, IsAdminView);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading print tracking dashboard for user {UserId}", GetCurrentUserId());

                // Return fallback dashboard
                Dashboard = CreateFallbackDashboard();
                TempData["Error"] = "Error loading dashboard data. Please refresh the page.";

                return Page();
            }
        }

        public async Task<IActionResult> OnGetRefreshDashboardAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";
                
                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);
                await PopulateSlsMachinesOnlyAsync();

                return Partial("_PrintTrackingDashboard", Dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing dashboard for user {UserId}", GetCurrentUserId());
                return StatusCode(500, "Error refreshing dashboard");
            }
        }

        public async Task<IActionResult> OnGetEmbeddedViewAsync()
        {
            try
            {
                var embeddedViewModel = await CreateEmbeddedSchedulerViewAsync();
                return Partial("_EmbeddedScheduler", embeddedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading embedded scheduler view");
                return StatusCode(500, "Error loading schedule view");
            }
        }

        public async Task<IActionResult> OnGetStartPrintModalAsync(string? printerName = null, int? jobId = null)
        {
            try
            {
                var viewModel = await CreateStartPrintViewModelAsync(printerName, jobId);
                return Partial("_StartPrintModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading start print modal for printer {PrinterName}, job {JobId}", printerName, jobId);
                return StatusCode(500, "Error loading start print form");
            }
        }

        public async Task<IActionResult> OnGetPostPrintModalAsync(int? buildId = null, string? printerName = null, int? jobId = null)
        {
            try
            {
                var viewModel = await CreatePostPrintViewModelAsync(buildId, printerName, jobId);
                return Partial("_PostPrintModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading post print modal for buildId {BuildId}, printer {PrinterName}", buildId, printerName);
                return StatusCode(500, "Error loading complete print form");
            }
        }

        public async Task<IActionResult> OnPostStartPrintAsync(PrintStartViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    model.Errors = errors.ToList();
                    return Partial("_StartPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var buildId = await _printTrackingService.StartPrintJobAsync(model, userId);

                _logger.LogInformation("Print started successfully: BuildId {BuildId}, Printer {PrinterName}, User {UserId}",
                    buildId, model.PrinterName, userId);

                // Return success response
                return new JsonResult(new { success = true, buildId, message = "Print started successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting print job for printer {PrinterName}", model.PrinterName);

                model.Errors = new List<string> { "Error starting print job. Please try again." };
                return Partial("_StartPrintModal", model);
            }
        }

        public async Task<IActionResult> OnPostCompletePrintAsync(PostPrintViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    model.Errors = errors.ToList();
                    await PopulatePostPrintViewModelAsync(model);
                    return Partial("_PostPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var success = await _printTrackingService.CompletePrintJobAsync(model, userId);

                if (success)
                {
                    _logger.LogInformation("Print completed successfully: BuildId {BuildId}, User {UserId}",
                        model.BuildId, userId);

                    return new JsonResult(new { success = true, message = "Print completed successfully" });
                }
                else
                {
                    model.Errors = new List<string> { "Error completing print job. Build job not found." };
                    await PopulatePostPrintViewModelAsync(model);
                    return Partial("_PostPrintModal", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing print job for buildId {BuildId}", model.BuildId);

                model.Errors = new List<string> { "Error completing print job. Please try again." };
                await PopulatePostPrintViewModelAsync(model);
                return Partial("_PostPrintModal", model);
            }
        }

        public async Task<IActionResult> OnGetJobDetailsAsync(int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return NotFound(new { success = false, error = "Job not found" });
                }

                return new JsonResult(new
                {
                    success = true,
                    jobId = job.Id,
                    partId = job.PartId,
                    partNumber = job.PartNumber,
                    partDescription = job.Part?.Description ?? "",
                    machineId = job.MachineId,
                    material = job.Part?.SlsMaterial ?? "",
                    quantity = job.Quantity,
                    actualStart = job.ActualStart?.ToString("yyyy-MM-ddTHH:mm"),
                    buildId = job.Id // Using job ID as build reference
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job details for job {JobId}", jobId);
                return StatusCode(500, new { success = false, error = "Error loading job details" });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Populate dashboard with ONLY SLS machines from database
        /// CRITICAL: This method now filters to show only SLS machines for print tracking
        /// </summary>
        private async Task PopulateSlsMachinesOnlyAsync()
        {
            try
            {
                // CRITICAL FILTER: Only get SLS machines for print tracking
                var slsMachines = await _context.Machines
                    .Where(m => m.IsActive && 
                               m.IsAvailableForScheduling && 
                               m.MachineType.ToUpper() == "SLS") // SLS machines only
                    .OrderBy(m => m.Priority)
                    .ThenBy(m => m.MachineId)
                    .ToListAsync();

                if (!slsMachines.Any())
                {
                    _logger.LogWarning("No active SLS machines found in database. Creating fallback SLS machines.");
                    slsMachines = CreateFallbackSlsMachines();
                }

                // Populate available machines for forms (SLS only)
                Dashboard.AvailableMachines = slsMachines.Select(m => new OpCentrix.ViewModels.PrintTracking.MachineInfo
                {
                    MachineId = m.MachineId,
                    MachineName = m.MachineName,
                    MachineType = m.MachineType,
                    Status = m.Status,
                    IsActive = m.IsActive,
                    IsAvailableForScheduling = m.IsAvailableForScheduling,
                    Priority = m.Priority,
                    SupportedMaterials = m.SupportedMaterials.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    CurrentMaterial = m.CurrentMaterial,
                    Location = m.Location,
                    Department = m.Department,
                    BuildVolumeInfo = GetBuildVolumeInfo(m),
                    MaintenanceStatus = GetMaintenanceStatus(m),
                    UtilizationPercent = Dashboard.UtilizationByMachine.GetValueOrDefault(m.MachineId, 0),
                    ActiveJobs = Dashboard.ActiveJobsByPrinter.GetValueOrDefault(m.MachineId, 0),
                    QueuedJobs = Dashboard.QueueDepth.GetValueOrDefault(m.MachineId, 0)
                }).ToList();

                // Update existing machine-based data with SLS machines only
                await UpdateMachineBasedStatsAsync(slsMachines);

                _logger.LogInformation("Successfully populated {MachineCount} SLS machines from database for {UserRole} user", 
                    slsMachines.Count, UserRole);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating SLS machine data");

                // Fallback to basic SLS machine list
                Dashboard.AvailableMachines = CreateFallbackSlsMachineInfo();
            }
        }

        /// <summary>
        /// Update machine-based statistics with actual database machines
        /// </summary>
        private async Task UpdateMachineBasedStatsAsync(List<Machine> machines)
        {
            try
            {
                var today = DateTime.Today;
                var machineIds = machines.Select(m => m.MachineId).ToList();

                // Update active jobs by printer
                var activeJobsByMachine = await _context.BuildJobs
                    .Where(b => b.Status == "In Progress" && machineIds.Contains(b.PrinterName))
                    .GroupBy(b => b.PrinterName)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                // Update hours today by machine
                var hoursToday = await _context.BuildJobs
                    .Where(b => b.ActualStartTime >= today &&
                               b.Status == "Completed" &&
                               b.ActualEndTime.HasValue &&
                               machineIds.Contains(b.PrinterName))
                    .GroupBy(b => b.PrinterName)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Sum(b => (b.ActualEndTime!.Value - b.ActualStartTime).TotalHours)
                    );

                // Update queue depth by machine
                var queueDepth = await _context.Jobs
                    .Where(j => j.Status == "Scheduled" && machineIds.Contains(j.MachineId))
                    .GroupBy(j => j.MachineId)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                // Merge with existing data
                foreach (var machine in machines)
                {
                    Dashboard.ActiveJobsByPrinter[machine.MachineId] = activeJobsByMachine.GetValueOrDefault(machine.MachineId, 0);
                    Dashboard.HoursToday[machine.MachineId] = hoursToday.GetValueOrDefault(machine.MachineId, 0);
                    Dashboard.QueueDepth[machine.MachineId] = queueDepth.GetValueOrDefault(machine.MachineId, 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating machine-based statistics");
            }
        }

        /// <summary>
        /// Handle scheduler integration parameters
        /// </summary>
        private async Task HandleSchedulerIntegrationAsync(int? jobId, string? machineId)
        {
            try
            {
                if (jobId.HasValue)
                {
                    ViewData["HighlightJobId"] = jobId.Value;
                    ViewData["ScrollToJob"] = true;

                    var job = await _context.Jobs
                        .Include(j => j.Part)
                        .FirstOrDefaultAsync(j => j.Id == jobId.Value);

                    if (job != null)
                    {
                        ViewData["JobContext"] = job;
                        TempData["Info"] = $"Scheduler job loaded: {job.PartNumber} on {job.MachineId}";

                        _logger.LogInformation("Scheduler integration: Loaded job {JobId} ({PartNumber}) on machine {MachineId}",
                            jobId.Value, job.PartNumber, job.MachineId);
                    }
                }

                if (!string.IsNullOrEmpty(machineId))
                {
                    ViewData["HighlightMachineId"] = machineId;
                    _logger.LogInformation("Scheduler integration: Highlighting machine {MachineId}", machineId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling scheduler integration for jobId {JobId}, machineId {MachineId}", jobId, machineId);
            }
        }

        /// <summary>
        /// Create embedded scheduler view model (SLS machines only)
        /// </summary>
        private async Task<OpCentrix.ViewModels.PrintTracking.EmbeddedSchedulerViewModel> CreateEmbeddedSchedulerViewAsync()
        {
            try
            {
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(3);

                var jobs = await _context.Jobs
                    .Where(j => j.ScheduledStart >= startDate && j.ScheduledStart < endDate)
                    .OrderBy(j => j.ScheduledStart)
                    .Take(50)
                    .ToListAsync();

                // FILTER: Only SLS machines for embedded scheduler
                var slsMachines = await _context.Machines
                    .Where(m => m.IsActive && 
                               m.IsAvailableForScheduling && 
                               m.MachineType.ToUpper() == "SLS")
                    .OrderBy(m => m.Priority)
                    .Select(m => m.MachineId)
                    .ToListAsync();

                return new OpCentrix.ViewModels.PrintTracking.EmbeddedSchedulerViewModel
                {
                    Jobs = jobs,
                    Machines = slsMachines,
                    StartDate = startDate,
                    Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating embedded scheduler view");
                throw;
            }
        }

        /// <summary>
        /// Create start print view model (SLS machines only)
        /// </summary>
        private async Task<PrintStartViewModel> CreateStartPrintViewModelAsync(string? printerName, int? jobId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                var viewModel = new PrintStartViewModel
                {
                    PrinterName = printerName ?? "",
                    ActualStartTime = DateTime.Now,
                    EstimatedEndTime = DateTime.Now.AddHours(4),
                    AssociatedScheduledJobId = jobId,
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    Errors = new List<string>()
                };

                // Populate available options (SLS machines only)
                await PopulateStartPrintViewModelAsync(viewModel);

                // Pre-populate from job if specified
                if (jobId.HasValue)
                {
                    await PrePopulateFromJobAsync(viewModel, jobId.Value);
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating start print view model");
                throw;
            }
        }

        /// <summary>
        /// Create post print view model (SLS machines only)
        /// </summary>
        private async Task<PostPrintViewModel> CreatePostPrintViewModelAsync(int? buildId, string? printerName, int? jobId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(userId);

                var viewModel = new PostPrintViewModel
                {
                    BuildId = buildId ?? 0,
                    PrinterName = printerName ?? "",
                    ActualStartTime = DateTime.Now.AddHours(-4),
                    ActualEndTime = DateTime.Now,
                    OperatorActualHours = 4.0m,
                    OperatorName = user?.FullName ?? "Unknown",
                    UserId = userId,
                    Parts = new List<PostPrintPartEntry>
                    {
                        new PostPrintPartEntry
                        {
                            PartNumber = "",
                            Quantity = 1,
                            GoodParts = 1,
                            IsPrimary = true
                        }
                    },
                    Errors = new List<string>()
                };

                await PopulatePostPrintViewModelAsync(viewModel);

                // Pre-populate from build if specified
                if (buildId.HasValue)
                {
                    await PrePopulateFromBuildAsync(viewModel, buildId.Value);
                }

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post print view model");
                throw;
            }
        }

        /// <summary>
        /// Populate start print view model with available options (SLS machines only)
        /// </summary>
        private async Task PopulateStartPrintViewModelAsync(PrintStartViewModel viewModel)
        {
            try
            {
                // Get available SLS printers only
                var slsMachines = await _context.Machines
                    .Where(m => m.IsActive && 
                               m.IsAvailableForScheduling && 
                               m.MachineType.ToUpper() == "SLS")
                    .OrderBy(m => m.Priority)
                    .ToListAsync();

                viewModel.AvailablePrinters = slsMachines.Select(m => m.MachineId).ToList();

                // Get available scheduled jobs
                if (!string.IsNullOrEmpty(viewModel.PrinterName))
                {
                    viewModel.AvailableScheduledJobs = await _printTrackingService.GetAvailableScheduledJobsAsync(viewModel.PrinterName);
                }

                // Get available parts
                viewModel.AvailableParts = await _printTrackingService.GetAvailablePartsAsync();

                // Get available job stages and prototype jobs
                if (!string.IsNullOrEmpty(viewModel.PrinterName))
                {
                    viewModel.AvailableJobStages = await _printTrackingService.GetAvailableJobStagesAsync(viewModel.PrinterName);
                }
                viewModel.AvailablePrototypeJobs = await _printTrackingService.GetAvailablePrototypeJobsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating start print view model");
                throw;
            }
        }

        /// <summary>
        /// Populate post print view model with available options (SLS machines only)
        /// </summary>
        private async Task PopulatePostPrintViewModelAsync(PostPrintViewModel viewModel)
        {
            try
            {
                // Get available SLS printers only
                var slsMachines = await _context.Machines
                    .Where(m => m.IsActive && 
                               m.IsAvailableForScheduling && 
                               m.MachineType.ToUpper() == "SLS")
                    .OrderBy(m => m.Priority)
                    .ToListAsync();

                viewModel.AvailablePrinters = slsMachines.Select(m => m.MachineId).ToList();

                // Get available parts
                viewModel.AvailableParts = await _printTrackingService.GetAvailablePartsAsync();

                // Get running jobs
                viewModel.AvailableRunningJobs = await _context.Jobs
                    .Where(j => j.Status == "In Progress" || j.Status == "Building")
                    .OrderBy(j => j.ActualStart)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating post print view model");
                throw;
            }
        }

        /// <summary>
        /// Pre-populate start print model from job
        /// </summary>
        private async Task PrePopulateFromJobAsync(PrintStartViewModel viewModel, int jobId)
        {
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job != null)
                {
                    viewModel.PrinterName = job.MachineId;
                    viewModel.PartId = job.PartId;
                    viewModel.PartNumber = job.PartNumber;
                    viewModel.PartDescription = job.Part?.Description ?? "";
                    viewModel.Material = job.Part?.SlsMaterial ?? "";
                    viewModel.Quantity = job.Quantity;
                    viewModel.EstimatedHours = job.EstimatedHours;
                    viewModel.ScheduledStartTime = job.ScheduledStart;
                    viewModel.ScheduledEndTime = job.ScheduledEnd;

                    if (job.ScheduledStart < DateTime.Now)
                    {
                        var delayMinutes = (int)(DateTime.Now - job.ScheduledStart).TotalMinutes;
                        if (delayMinutes > 5) // Only consider significant delays
                        {
                            // Add delay information to a separate field since IsDelayed and DelayMinutes are computed
                            viewModel.ScheduledStartTime = job.ScheduledStart;
                            viewModel.ScheduledEndTime = job.ScheduledEnd;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pre-populating from job {JobId}", jobId);
            }
        }

        /// <summary>
        /// Pre-populate post print model from build
        /// </summary>
        private async Task PrePopulateFromBuildAsync(PostPrintViewModel viewModel, int buildId)
        {
            try
            {
                var buildJob = await _context.BuildJobs
                    .Include(b => b.Part)
                    .FirstOrDefaultAsync(b => b.BuildId == buildId);

                if (buildJob != null)
                {
                    viewModel.PrinterName = buildJob.PrinterName;
                    viewModel.ActualStartTime = buildJob.ActualStartTime;
                    viewModel.PartId = buildJob.PartId;
                    viewModel.PartNumber = buildJob.Part?.PartNumber ?? "";
                    viewModel.PartDescription = buildJob.Part?.Description ?? "";

                    if (buildJob.OperatorEstimatedHours.HasValue)
                    {
                        viewModel.OperatorEstimatedHours = buildJob.OperatorEstimatedHours.Value;
                    }

                    // Pre-populate parts list if available
                    if (!string.IsNullOrEmpty(viewModel.PartNumber))
                    {
                        viewModel.Parts = new List<PostPrintPartEntry>
                        {
                            new PostPrintPartEntry
                            {
                                PartNumber = viewModel.PartNumber,
                                Quantity = buildJob.TotalPartsInBuild > 0 ? buildJob.TotalPartsInBuild : 1,
                                GoodParts = buildJob.TotalPartsInBuild > 0 ? buildJob.TotalPartsInBuild : 1,
                                IsPrimary = true,
                                Description = viewModel.PartDescription
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pre-populating from build {BuildId}", buildId);
            }
        }

        /// <summary>
        /// Create fallback dashboard when errors occur
        /// </summary>
        private PrintTrackingDashboardViewModel CreateFallbackDashboard()
        {
            var userId = GetCurrentUserId();
            var user = User.Identity?.Name ?? "Unknown";

            return new PrintTrackingDashboardViewModel
            {
                UserId = userId,
                OperatorName = user,
                UserRole = GetCurrentUserRole(),
                ActiveBuilds = new List<BuildJob>(),
                RecentCompletedBuilds = new List<BuildJob>(),
                RecentDelays = new List<DelayLog>(),
                ActiveJobsByPrinter = new Dictionary<string, int>(),
                HoursToday = new Dictionary<string, double>(),
                UtilizationByMachine = new Dictionary<string, double>(),
                QueueDepth = new Dictionary<string, int>(),
                MaintenanceAlerts = new List<MaintenanceAlert>(),
                AvailableMachines = CreateFallbackSlsMachineInfo(),
                RefreshTime = DateTime.Now
            };
        }

        /// <summary>
        /// Create fallback SLS machines when database is unavailable
        /// CRITICAL: Only returns SLS machines for print tracking
        /// </summary>
        private List<Machine> CreateFallbackSlsMachines()
        {
            return new List<Machine>
            {
                new Machine { 
                    MachineId = "TI1", 
                    MachineName = "TruPrint 3000 #1", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 1,
                    Location = "Print Floor",
                    Department = "Printing",
                    CurrentMaterial = "SS316L"
                },
                new Machine { 
                    MachineId = "TI2", 
                    MachineName = "TruPrint 3000 #2", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 2,
                    Location = "Print Floor",
                    Department = "Printing",
                    CurrentMaterial = "SS316L"
                },
                new Machine { 
                    MachineId = "INC", 
                    MachineName = "Inconel Printer", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 3,
                    Location = "Print Floor",
                    Department = "Printing",
                    CurrentMaterial = "Inconel 625"
                }
            };
        }

        /// <summary>
        /// Create fallback SLS machine info
        /// </summary>
        private List<OpCentrix.ViewModels.PrintTracking.MachineInfo> CreateFallbackSlsMachineInfo()
        {
            return new List<OpCentrix.ViewModels.PrintTracking.MachineInfo>
            {
                new OpCentrix.ViewModels.PrintTracking.MachineInfo { 
                    MachineId = "TI1", 
                    MachineName = "TruPrint 3000 #1", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 1,
                    Location = "Print Floor",
                    Department = "Printing"
                },
                new OpCentrix.ViewModels.PrintTracking.MachineInfo { 
                    MachineId = "TI2", 
                    MachineName = "TruPrint 3000 #2", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 2,
                    Location = "Print Floor",
                    Department = "Printing"
                },
                new OpCentrix.ViewModels.PrintTracking.MachineInfo { 
                    MachineId = "INC", 
                    MachineName = "Inconel Printer", 
                    MachineType = "SLS", 
                    Status = "Idle", 
                    IsActive = true, 
                    IsAvailableForScheduling = true, 
                    Priority = 3,
                    Location = "Print Floor",
                    Department = "Printing"
                }
            };
        }

        /// <summary>
        /// Get build volume information for a machine
        /// </summary>
        private string GetBuildVolumeInfo(Machine machine)
        {
            if (machine.MachineType == "SLS")
            {
                return $"{machine.BuildLengthMm} × {machine.BuildWidthMm} × {machine.BuildHeightMm} mm";
            }
            return "N/A";
        }

        /// <summary>
        /// Get maintenance status for a machine
        /// </summary>
        private string GetMaintenanceStatus(Machine machine)
        {
            if (machine.RequiresMaintenance)
            {
                return "Due";
            }
            else if (machine.HoursSinceLastMaintenance > machine.MaintenanceIntervalHours * 0.8)
            {
                return "Soon";
            }
            return "OK";
        }

        /// <summary>
        /// Get current user ID safely
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            // Fallback: try to get from other claims or create default
            _logger.LogWarning("Unable to get user ID from claims for user {UserName}", User.Identity?.Name ?? "Unknown");
            return 1; // Default admin user ID
        }

        /// <summary>
        /// Get current user role safely
        /// </summary>
        private string GetCurrentUserRole()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ??
                          User.FindFirst("Role")?.Value ?? "Operator";
            return userRole;
        }

        #endregion
    }
}