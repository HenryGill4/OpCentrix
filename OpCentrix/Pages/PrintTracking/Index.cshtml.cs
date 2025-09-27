using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;
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
    /// ENHANCED: Comprehensive error handling to prevent redirect loops
    /// </summary>
    [PrintTrackingAccess]
    public class IndexModel : PageModel
    {
        private readonly IPrintTrackingService _printTrackingService;
        private readonly IMachineManagementService _machineManagementService;
        private readonly IMaterialService _materialService;
        private readonly SchedulerContext _context;
        private readonly ILogger<IndexModel> _logger;

        public PrintTrackingDashboardViewModel Dashboard { get; set; } = new();

        // New: Role-based view properties
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;
        public string UserRole { get; set; } = string.Empty;

        // ENHANCED: Error tracking properties
        public List<string> PageErrors { get; set; } = new();
        public bool HasCriticalError { get; set; } = false;
        public string ErrorContext { get; set; } = string.Empty;

        public IndexModel(
            IPrintTrackingService printTrackingService,
            IMachineManagementService machineManagementService,
            IMaterialService materialService,
            SchedulerContext context,
            ILogger<IndexModel> logger)
        {
            _printTrackingService = printTrackingService ?? throw new ArgumentNullException(nameof(printTrackingService));
            _machineManagementService = machineManagementService ?? throw new ArgumentNullException(nameof(machineManagementService));
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> OnGetAsync(int? jobId = null, string? machineId = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [PRINT-TRACKING-{OperationId}] PrintTracking page load initiated", operationId);

            try
            {
                // CRITICAL: Enhanced user identification with comprehensive logging
                var userId = GetCurrentUserId();
                var userName = User.Identity?.Name ?? "Unknown";
                
                _logger.LogInformation("🔧 [PRINT-TRACKING-{OperationId}] User identification - ID: {UserId}, Name: {UserName}", 
                    operationId, userId, userName);

                // ENHANCED: Determine user role with error handling
                UserRole = GetCurrentUserRole();
                IsAdminView = DetermineAdminView();

                _logger.LogInformation("🔧 [PRINT-TRACKING-{OperationId}] Role determination - Role: {UserRole}, IsAdminView: {IsAdminView}", 
                    operationId, UserRole, IsAdminView);

                // ENHANCED: Load dashboard data with comprehensive error handling
                try
                {
                    Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);
                    _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] Dashboard service data loaded successfully", operationId);
                }
                catch (Exception dashEx)
                {
                    _logger.LogError(dashEx, "❌ [PRINT-TRACKING-{OperationId}] Error loading dashboard service data", operationId);
                    Dashboard = CreateFallbackDashboard();
                    PageErrors.Add("Dashboard service unavailable - using fallback data");
                }

                // CRITICAL: Filter to show only SLS machines with enhanced error handling
                try
                {
                    await PopulateSlsMachinesOnlyAsync();
                    _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] SLS machines populated successfully - Count: {MachineCount}", 
                        operationId, Dashboard.AvailableMachines?.Count ?? 0);
                }
                catch (Exception machineEx)
                {
                    _logger.LogError(machineEx, "❌ [PRINT-TRACKING-{OperationId}] Error populating SLS machines", operationId);
                    Dashboard.AvailableMachines = CreateFallbackSlsMachineInfo();
                    PageErrors.Add("Machine data service unavailable - using fallback data");
                }

                // ENHANCED: Handle scheduler integration parameters with error handling
                try
                {
                    await HandleSchedulerIntegrationAsync(jobId, machineId);
                    if (jobId.HasValue || !string.IsNullOrEmpty(machineId))
                    {
                        _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] Scheduler integration handled - JobId: {JobId}, MachineId: {MachineId}", 
                            operationId, jobId, machineId);
                    }
                }
                catch (Exception intEx)
                {
                    _logger.LogError(intEx, "❌ [PRINT-TRACKING-{OperationId}] Error handling scheduler integration", operationId);
                    PageErrors.Add("Scheduler integration partially unavailable");
                }

                // FINAL: Log successful completion
                _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] PrintTracking dashboard loaded successfully for user {UserName} ({UserRole}) with {MachineCount} SLS machines. Admin view: {IsAdminView}",
                    operationId, userName, UserRole, Dashboard.AvailableMachines?.Count ?? 0, IsAdminView);

                // Set success message if there were minor errors but page still works
                if (PageErrors.Any() && !HasCriticalError)
                {
                    TempData["Warning"] = $"Page loaded with minor issues: {string.Join(", ", PageErrors)}";
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PRINT-TRACKING-{OperationId}] Critical error loading PrintTracking dashboard for user {UserName}", 
                    operationId, User.Identity?.Name ?? "Unknown");

                // CRITICAL: Don't redirect on error - show error page instead
                HasCriticalError = true;
                ErrorContext = $"Critical error loading dashboard (Operation: {operationId})";
                PageErrors.Add($"Critical system error: {ex.Message}");

                // Return fallback dashboard to prevent redirect
                Dashboard = CreateFallbackDashboard();
                TempData["Error"] = "Critical error loading dashboard. Please refresh the page or contact support.";

                return Page(); // Don't redirect - stay on page with error message
            }
        }

        public async Task<IActionResult> OnGetRefreshDashboardAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                var userId = GetCurrentUserId();
                UserRole = GetCurrentUserRole();
                IsAdminView = DetermineAdminView();

                Dashboard = await _printTrackingService.GetDashboardDataAsync(userId);
                await PopulateSlsMachinesOnlyAsync();

                _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] Dashboard refreshed successfully", operationId);
                return Partial("_PrintTrackingDashboard", Dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PRINT-TRACKING-{OperationId}] Error refreshing dashboard for user {UserId}", 
                    operationId, GetCurrentUserId());
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
                _logger.LogInformation("CompletePrint request received - BuildId: {BuildId}, OperatorActualHours: {ActualHours}",
                    model.BuildId, model.OperatorActualHours);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    model.Errors = errors.ToList();

                    // CRITICAL: Log validation errors for debugging
                    _logger.LogWarning("CompletePrint validation failed. Errors: {Errors}", string.Join(", ", errors));

                    // CRITICAL: Log specific field validation errors
                    foreach (var kvp in ModelState)
                    {
                        if (kvp.Value.Errors.Any())
                        {
                            _logger.LogWarning("Field '{FieldName}' has errors: {FieldErrors}",
                                kvp.Key, string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }

                    await PopulatePostPrintViewModelAsync(model);
                    return Partial("_PostPrintModal", model);
                }

                var userId = GetCurrentUserId();
                var success = await _printTrackingService.CompletePrintJobAsync(model, userId);

                if (success)
                {
                    _logger.LogInformation("Print completed successfully: BuildId {BuildId}, User {UserId}, ActualHours {ActualHours}",
                        model.BuildId, userId, model.OperatorActualHours);

                    return new JsonResult(new { success = true, message = "Print completed successfully" });
                }
                else
                {
                    _logger.LogWarning("CompletePrint failed - Build job not found for BuildId {BuildId}", model.BuildId);
                    model.Errors = new List<string> { "Error completing print job. Build job not found." };
                    await PopulatePostPrintViewModelAsync(model);
                    return Partial("_PostPrintModal", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing print job for buildId {BuildId}. Model: {@Model}", model.BuildId, model);

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
        /// ENHANCED: Determine admin view with comprehensive error handling
        /// </summary>
        private bool DetermineAdminView()
        {
            try
            {
                var result = UserRole == "Admin" || UserRole == "Manager";
                _logger.LogDebug("Admin view determination: Role='{UserRole}', Result={Result}", UserRole, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error determining admin view - defaulting to operator view");
                return false; // Default to operator view on error
            }
        }

        /// <summary>
        /// Populate dashboard with ONLY SLS machines from database
        /// CRITICAL: This method now filters to show only SLS machines for print tracking
        /// ENHANCED: Now includes scheduled jobs for each machine
        /// </summary>
        private async Task PopulateSlsMachinesOnlyAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔧 [PRINT-TRACKING-{OperationId}] Loading SLS machines with scheduled jobs", operationId);

            try
            {
                // Get all machines from machine service (with proper SLS filtering)
                var allMachines = await _machineManagementService.GetActiveMachinesAsync();
                
                // Filter to only SLS machines using the same logic as scheduler
                var slsMachines = allMachines
                    .Where(m => GetUnifiedMachineType(m) == "SLS")
                    .OrderBy(m => m.Priority)
                    .ToList();

                // Convert Machine models to MachineInfo view models
                Dashboard.AvailableMachines = slsMachines.Select(m => new MachineInfo
                {
                    MachineId = m.MachineId,
                    MachineName = m.Name,
                    MachineType = m.MachineType ?? "SLS",
                    Status = m.Status ?? "Unknown",
                    IsActive = m.IsActive,
                    IsAvailableForScheduling = m.IsAvailableForScheduling,
                    Priority = m.Priority,
                    CurrentMaterial = m.CurrentMaterial ?? "",
                    Location = m.Location ?? "",
                    MaintenanceStatus = m.RequiresMaintenance ? "Due" : "OK",
                    LastMaintenanceDate = m.LastMaintenanceDate,
                    NextMaintenanceDate = m.NextMaintenanceDate
                }).ToList();

                _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] Loaded {Count} SLS machines from database", 
                    operationId, Dashboard.AvailableMachines.Count);

                // ENHANCED: Load scheduled jobs for each SLS machine
                await LoadScheduledJobsForMachinesAsync(operationId);

                // Update machine-based statistics
                await UpdateMachineBasedStatsAsync(Dashboard.AvailableMachines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PRINT-TRACKING-{OperationId}] Critical error loading SLS machines", operationId);
                
                // Fallback to prevent crashes
                Dashboard.AvailableMachines = new List<MachineInfo>();
                Dashboard.ScheduledJobsByMachine = new Dictionary<string, List<Job>>();
                Dashboard.NextJobByMachine = new Dictionary<string, Job?>();
                Dashboard.NextJobTimeByMachine = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Load scheduled jobs for each SLS machine (next 3 days)
        /// ENHANCED: Provides job schedule data directly in machine cards
        /// </summary>
        private async Task LoadScheduledJobsForMachinesAsync(string operationId)
        {
            try
            {
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(3); // Next 3 days
                var machineIds = Dashboard.AvailableMachines.Select(m => m.MachineId).ToList();

                if (!machineIds.Any())
                {
                    _logger.LogWarning("⚠️ [PRINT-TRACKING-{OperationId}] No machines available for job loading", operationId);
                    return;
                }

                // Load scheduled jobs for all SLS machines
                var scheduledJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => machineIds.Contains(j.MachineId) && 
                               j.ScheduledStart >= startDate && 
                               j.ScheduledStart < endDate &&
                               (j.Status == "Scheduled" || j.Status == "Building" || j.Status == "In Progress"))
                    .OrderBy(j => j.ScheduledStart)
                    .ThenBy(j => j.Priority)
                    .AsNoTracking()
                    .ToListAsync();

                // Group jobs by machine
                Dashboard.ScheduledJobsByMachine = scheduledJobs
                    .GroupBy(j => j.MachineId)
                    .ToDictionary(g => g.Key, g => g.Take(3).ToList()); // Limit to next 3 jobs per machine

                // Get next job for each machine
                Dashboard.NextJobByMachine = Dashboard.AvailableMachines
                    .ToDictionary(m => m.MachineId, m => 
                        Dashboard.ScheduledJobsByMachine.GetValueOrDefault(m.MachineId, new List<Job>())
                            .FirstOrDefault(j => j.ScheduledStart > DateTime.Now));

                // Generate friendly time display for next jobs
                Dashboard.NextJobTimeByMachine = Dashboard.NextJobByMachine
                    .Where(kvp => kvp.Value != null)
                    .ToDictionary(kvp => kvp.Key, kvp => GetFriendlyTimeDisplay(kvp.Value!.ScheduledStart));

                _logger.LogInformation("✅ [PRINT-TRACKING-{OperationId}] Loaded {JobCount} scheduled jobs across {MachineCount} machines", 
                    operationId, scheduledJobs.Count, Dashboard.ScheduledJobsByMachine.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [PRINT-TRACKING-{OperationId}] Error loading scheduled jobs for machines", operationId);
                
                // Initialize empty collections to prevent UI errors
                Dashboard.ScheduledJobsByMachine = new Dictionary<string, List<Job>>();
                Dashboard.NextJobByMachine = new Dictionary<string, Job?>();
                Dashboard.NextJobTimeByMachine = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Helper: Get unified machine type (same logic as scheduler)
        /// </summary>
        private static string GetUnifiedMachineType(Machine m)
        {
            if (m == null) return "Unknown";
            string raw = (m.MachineType ?? "").Trim();
            string name = (m.MachineName ?? m.Name ?? "").Trim();
            string model = (m.MachineModel ?? "").Trim();
            string all = string.Join(" ", raw, name, model).ToUpperInvariant();

            // SLS group (TruPrint + Custom SLS + generic SLS keywords)
            if (all.Contains("TRUPRINT") || all.Contains("TRU PRINT") || all.Contains("SLS") || all.Contains("SELECTIVE LASER"))
                return "SLS";

            // CNC group (Haas, Doosan, Mazak, generic CNC)
            if (all.Contains("CNC") || all.Contains("HAAS") || all.Contains("MAZAK") || all.Contains("DOOSAN"))
                return "CNC";

            // EDM group
            if (all.Contains("EDM") || all.Contains("WIRE EDM"))
                return "EDM";

            return string.IsNullOrWhiteSpace(raw) ? "Other" : raw;
        }

        /// <summary>
        /// Helper: Get friendly time display for scheduled jobs
        /// </summary>
        private string GetFriendlyTimeDisplay(DateTime scheduledTime)
        {
            var now = DateTime.Now;
            var diff = scheduledTime - now;

            if (scheduledTime.Date == now.Date)
                return $"Today {scheduledTime:HH:mm}";
            else if (scheduledTime.Date == now.Date.AddDays(1))
                return $"Tomorrow {scheduledTime:HH:mm}";
            else if (diff.TotalDays <= 7)
                return $"{scheduledTime:ddd HH:mm}";
            else
                return scheduledTime.ToString("MM/dd HH:mm");
        }

        /// <summary>
        /// ENHANCED: Get current user ID with comprehensive error handling
        /// </summary>
        private int GetCurrentUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    return userId;
                }

                // Fallback: try to get from other claims
                var nameIdentifier = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
                if (!string.IsNullOrEmpty(nameIdentifier) && int.TryParse(nameIdentifier, out var fallbackUserId))
                {
                    _logger.LogWarning("Used fallback method to get user ID for user {UserName}", User.Identity?.Name ?? "Unknown");
                    return fallbackUserId;
                }

                // Final fallback: use default admin user
                _logger.LogWarning("Unable to get user ID from claims for user {UserName} - using default", User.Identity?.Name ?? "Unknown");
                return 1; // Default admin user ID
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return 1; // Default admin user ID
            }
        }

        /// <summary>
        /// ENHANCED: Get current user role with comprehensive error handling
        /// </summary>
        private string GetCurrentUserRole()
        {
            try
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value ??
                              User.FindFirst("Role")?.Value ?? 
                              User.FindFirst("role")?.Value ?? 
                              "Operator"; // Default to operator

                _logger.LogDebug("Current user role determined: {UserRole} for user {UserName}", userRole, User.Identity?.Name ?? "Unknown");
                return userRole;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user role - defaulting to Operator");
                return "Operator";
            }
        }

        private PrintTrackingDashboardViewModel CreateFallbackDashboard()
        {
            return new PrintTrackingDashboardViewModel
            {
                ActiveBuilds = new List<BuildJob>(),
                RecentCompletedBuilds = new List<BuildJob>(),
                RecentDelays = new List<DelayLog>(),
                AvailableMachines = new List<MachineInfo>(),
                OperatorName = User.Identity?.Name ?? "Unknown",
                UserId = GetCurrentUserId(),
                UserRole = "Operator",
                Errors = new List<string> { "Unable to load dashboard data" }
            };
        }

        private List<MachineInfo> CreateFallbackSlsMachineInfo()
        {
            // Return a basic set of SLS machines for fallback
            return new List<MachineInfo>
            {
                new MachineInfo
                {
                    MachineId = "TI1",
                    MachineName = "TruPrint 3000 #1",
                    MachineType = "SLS",
                    Status = "Unknown",
                    IsActive = true,
                    IsAvailableForScheduling = false
                },
                new MachineInfo
                {
                    MachineId = "TI2", 
                    MachineName = "TruPrint 3000 #2",
                    MachineType = "SLS",
                    Status = "Unknown",
                    IsActive = true,
                    IsAvailableForScheduling = false
                }
            };
        }

        private async Task HandleSchedulerIntegrationAsync(int? jobId, string? machineId)
        {
            // Handle any scheduler integration parameters
            if (jobId.HasValue)
            {
                ViewData["HighlightJobId"] = jobId.Value;
            }
            
            if (!string.IsNullOrEmpty(machineId))
            {
                ViewData["HighlightMachineId"] = machineId;
            }
            
            await Task.CompletedTask;
        }

        private async Task<OpCentrix.ViewModels.Shared.EmbeddedSchedulerViewModel> CreateEmbeddedSchedulerViewAsync()
        {
            // Create a basic embedded scheduler view
            return new OpCentrix.ViewModels.Shared.EmbeddedSchedulerViewModel
            {
                Jobs = new List<Job>(),
                Machines = new List<string>(),
                Dates = new List<DateTime>(),
                StartDate = DateTime.Today
            };
        }

        private async Task<PrintStartViewModel> CreateStartPrintViewModelAsync(string? printerName, int? jobId)
        {
            var viewModel = new PrintStartViewModel
            {
                PrinterName = printerName ?? "",
                ActualStartTime = DateTime.Now,
                OperatorName = User.Identity?.Name ?? "Unknown",
                UserId = GetCurrentUserId(),
                AvailablePrinters = new List<string> { "TI1", "TI2", "INC" }
            };

            if (jobId.HasValue)
            {
                // Load job details if provided
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
                if (job != null)
                {
                    viewModel.AssociatedScheduledJobId = job.Id;
                    viewModel.PartId = job.PartId;
                    viewModel.PartNumber = job.PartNumber;
                    viewModel.Quantity = job.Quantity;
                    viewModel.EstimatedHours = job.EstimatedHours;
                }
            }

            return viewModel;
        }

        private async Task<PostPrintViewModel> CreatePostPrintViewModelAsync(int? buildId, string? printerName, int? jobId)
        {
            var viewModel = new PostPrintViewModel
            {
                PrinterName = printerName ?? "",
                ActualStartTime = DateTime.Now.AddHours(-4), // Default to 4 hours ago
                ActualEndTime = DateTime.Now,
                OperatorName = User.Identity?.Name ?? "Unknown",
                UserId = GetCurrentUserId(),
                AvailablePrinters = new List<string> { "TI1", "TI2", "INC" },
                Parts = new List<PostPrintPartEntry>()
            };

            if (buildId.HasValue)
            {
                viewModel.BuildId = buildId.Value;
                // Load build job details
                var buildJob = await _context.BuildJobs.Include(b => b.Part).FirstOrDefaultAsync(b => b.BuildId == buildId.Value);
                if (buildJob != null)
                {
                    viewModel.ActualStartTime = buildJob.ActualStartTime;
                    viewModel.PrinterName = buildJob.PrinterName;
                    viewModel.OperatorEstimatedHours = buildJob.OperatorEstimatedHours;
                }
            }

            return viewModel;
        }

        private async Task PopulatePostPrintViewModelAsync(PostPrintViewModel model)
        {
            // Populate dropdown options
            model.AvailablePrinters = new List<string> { "TI1", "TI2", "INC" };
            model.AvailableParts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
            
            // Ensure at least one part entry exists
            if (!model.Parts.Any())
            {
                model.Parts.Add(new PostPrintPartEntry
                {
                    PartNumber = "",
                    Quantity = 1,
                    GoodParts = 1,
                    IsPrimary = true
                });
            }
        }

        private async Task UpdateMachineBasedStatsAsync(List<MachineInfo> machines)
        {
            // Update machine-based statistics
            foreach (var machine in machines)
            {
                // Calculate active jobs
                machine.ActiveJobs = await _context.BuildJobs
                    .CountAsync(b => b.PrinterName == machine.MachineId && b.Status == "In Progress");

                // Calculate queued jobs
                machine.QueuedJobs = await _context.Jobs
                    .CountAsync(j => j.MachineId == machine.MachineId && j.Status == "Scheduled");

                // Calculate hours today
                var today = DateTime.Today;
                var todayBuilds = await _context.BuildJobs
                    .Where(b => b.PrinterName == machine.MachineId && 
                               b.ActualStartTime >= today && 
                               b.Status == "Completed" && 
                               b.ActualEndTime.HasValue)
                    .ToListAsync();

                machine.HoursToday = todayBuilds
                    .Sum(b => (b.ActualEndTime!.Value - b.ActualStartTime).TotalHours);

                // Set utilization (simplified calculation)
                machine.UtilizationPercent = machine.HoursToday / 24.0 * 100;
            }
        }

        #endregion
    }
}