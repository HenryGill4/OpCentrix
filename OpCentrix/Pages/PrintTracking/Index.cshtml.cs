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
using OpCentrix.Services.Maintenance; // added
using OpCentrix.Models.MaintenanceV2; // NEW for OperationalTask
using System.Text.Json; // added for interval summary parsing

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
        private readonly IMaintenanceService _maintenanceService; // added
        private readonly IOperationalTaskService _taskService; // NEW

        public PrintTrackingDashboardViewModel Dashboard { get; set; } = new();

        // NEW: Operational Tasks data (loaded via HTMX handlers)
        public List<OperationalTask> OpenTasks { get; set; } = new();
        public int OpenTaskCount => OpenTasks?.Count(t => t.Status == "Open" || t.Status == "InProgress") ?? 0;
        public int HighPriorityTaskCount => OpenTasks?.Count(t => t.Priority <= 2 && (t.Status == "Open" || t.Status == "InProgress")) ?? 0;

        // New: Role-based view properties
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;
        public string UserRole { get; set; } = string.Empty;

        // ENHANCED: Error tracking properties
        public List<string> PageErrors { get; set; } = new();
        public bool HasCriticalError { get; set; } = false;
        public string ErrorContext { get; set; } = string.Empty;

        // NEW: Task creation input binding model
        [BindProperty]
        public TaskInput NewTask { get; set; } = new();
        public class TaskInput
        {
            [BindProperty]
            public string Title { get; set; } = string.Empty;
            [BindProperty]
            public string? Description { get; set; }
            [BindProperty]
            public int Priority { get; set; } = 3; // 1-5
            [BindProperty]
            public string? MachineId { get; set; }
            [BindProperty]
            public DateTime? DueAt { get; set; }

            // ---- NEW ADVANCED FIELDS (not yet persisted structurally) ----
            // Controlling Intervals
            [BindProperty] public bool TrackIncBuilds { get; set; }
            [BindProperty] public bool TrackTiBuilds { get; set; }
            [BindProperty] public bool TrackTotalBuilds { get; set; }
            [BindProperty] public bool TrackMachineHours { get; set; }
            [BindProperty] public int? IncBuildThreshold { get; set; }
            [BindProperty] public int? TiBuildThreshold { get; set; }
            [BindProperty] public int? TotalBuildThreshold { get; set; }
            [BindProperty] public double? MachineHoursThreshold { get; set; }

            // Repeat Frequency
            [BindProperty] public bool EnableRepeat { get; set; }
            [BindProperty] public string? RepeatType { get; set; } // DayOfWeek|IncBuilds|TiBuilds|TotalBuilds|MachineHours|None
            [BindProperty] public DayOfWeek? RepeatDayOfWeek { get; set; }
            [BindProperty] public int? RepeatEveryNIncBuilds { get; set; }
            [BindProperty] public int? RepeatEveryNTiBuilds { get; set; }
            [BindProperty] public int? RepeatEveryNTotalBuilds { get; set; }
            [BindProperty] public double? RepeatEveryMachineHours { get; set; }
        }

        // FIXED constructor
        public IndexModel(
            IPrintTrackingService printTrackingService,
            IMachineManagementService machineManagementService,
            IMaterialService materialService,
            SchedulerContext context,
            ILogger<IndexModel> logger,
            IMaintenanceService maintenanceService,
            IOperationalTaskService taskService) // NEW
        {
            _printTrackingService = printTrackingService ?? throw new ArgumentNullException(nameof(printTrackingService));
            _machineManagementService = machineManagementService ?? throw new ArgumentNullException(nameof(machineManagementService));
            _materialService = materialService ?? throw new ArgumentNullException(nameof(materialService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maintenanceService = maintenanceService ?? throw new ArgumentNullException(nameof(maintenanceService));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
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
                    Dashboard.AvailableMachines = CreateFallbackSslMachineInfo();
                    PageErrors.Add("Machine data service unavailable - using fallback data");
                }

                // Load open tasks summary for initial page (list itself via HTMX later)
                if (IsAdminView)
                {
                    try
                    {
                        OpenTasks = await _taskService.GetOpenAsync(15);
                    }
                    catch (Exception tex)
                    {
                        _logger.LogWarning(tex, "Failed loading open tasks (non-blocking)");
                        OpenTasks = new List<OperationalTask>();
                    }
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

        // ==================== OPERATIONAL TASK HTMX HANDLERS ====================
        public async Task<IActionResult> OnGetTaskListAsync()
        {
            UserRole = GetCurrentUserRole();
            if (!IsAdmin()) return Unauthorized();
            OpenTasks = await SafeGetOpenTasksAsync();
            return Partial("PrintTracking/Partials/_TaskList", OpenTasks);
        }

        public IActionResult OnGetCreateTaskModal()
        {
            UserRole = GetCurrentUserRole();
            if (!IsAdmin()) return Unauthorized();
            NewTask = new TaskInput();
            return Partial("PrintTracking/Partials/_CreateTaskModal", this);
        }

        public async Task<IActionResult> OnPostCreateTaskAsync()
        {
            UserRole = GetCurrentUserRole();
            if (!IsAdmin()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(NewTask.Title) || NewTask.Title.Trim().Length < 3)
                ModelState.AddModelError("NewTask.Title", "Title must be at least 3 characters");
            if (NewTask.Title?.Length > 160)
                ModelState.AddModelError("NewTask.Title", "Title must be 160 characters or less");
            if (NewTask.Priority < 1 || NewTask.Priority > 5)
                ModelState.AddModelError("NewTask.Priority", "Priority must be between 1 and 5");

            var form = Request.Form;
            string intervalsJson = form["IntervalsJson"].FirstOrDefault() ?? "[]";
            bool suppressIfOpen = form["Options.SuppressIfOpen"].FirstOrDefault() == "on";
            bool matchAll = form["Options.MatchAll"].FirstOrDefault() == "on";

            // Basic lightweight validation of intervals JSON
            List<Dictionary<string, object?>> intervalList = new();
            try
            {
                intervalList = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object?>>>(intervalsJson) ?? new();
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Invalid intervals definition");
            }
            if (!intervalList.Any() && !NewTask.DueAt.HasValue)
            {
                ModelState.AddModelError(string.Empty, "At least one interval or a manual Due date is required");
            }

            if (!ModelState.IsValid)
                return Partial("PrintTracking/Partials/_CreateTaskModal", this);

            OperationalTask? entity = null;
            try
            {
                var config = new
                {
                    version = 2,
                    created = DateTime.UtcNow,
                    intervals = intervalList,
                    logic = matchAll ? "ALL" : "ANY",
                    options = new { suppressIfOpen }
                };
                string configJson = System.Text.Json.JsonSerializer.Serialize(config);

                entity = new OperationalTask
                {
                    Title = NewTask.Title.Trim(),
                    Description = string.IsNullOrWhiteSpace(NewTask.Description) ? null : NewTask.Description.Trim(),
                    Priority = NewTask.Priority,
                    MachineId = string.IsNullOrWhiteSpace(NewTask.MachineId) ? null : NewTask.MachineId.Trim(),
                    CreatedByUserId = GetCurrentUserId(),
                    DueAt = NewTask.DueAt.HasValue ? DateTime.SpecifyKind(NewTask.DueAt.Value, DateTimeKind.Local).ToUniversalTime() : null,
                    ConfigJson = configJson
                };
                await _taskService.CreateAsync(entity);

                // Attempt full refresh
                OpenTasks = await SafeGetOpenTasksAsync();

                // Fallback: ensure created task present even if query failed (schema patch race)
                if (entity != null && (OpenTasks == null || !OpenTasks.Any(o => o.Id == entity.Id)))
                {
                    OpenTasks ??= new List<OperationalTask>();
                    OpenTasks.Insert(0, entity); // prepend for visibility
                }

                if (entity != null && entity.Id > 0)
                    ViewData["NewTaskId"] = entity.Id; // expose to partial for scroll/highlight

                return Partial("PrintTracking/Partials/_TaskList", OpenTasks);
            }
            catch (DuplicateOperationalTaskException dex)
            {
                ModelState.AddModelError(string.Empty, dex.Message);
                return Partial("PrintTracking/Partials/_CreateTaskModal", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MAINT][ERR] Error creating operational task (interval wizard) - fallback insert attempt");
                if (entity != null && entity.Id == 0)
                {
                    // Present user-friendly message
                    ModelState.AddModelError(string.Empty, "Failed to create task (DB). Please retry.");
                    return Partial("PrintTracking/Partials/_CreateTaskModal", this);
                }
                ModelState.AddModelError(string.Empty, "Failed to create task");
                return Partial("PrintTracking/Partials/_CreateTaskModal", this);
            }
        }

        public async Task<IActionResult> OnPostCompleteTaskAsync(int id, bool early = false)
        {
            UserRole = GetCurrentUserRole();
            if (!IsAdmin()) return Unauthorized();
            try
            {
                await _taskService.CompleteAsync(id, GetCurrentUserId(), early: early);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed completing task {TaskId}", id);
            }
            OpenTasks = await SafeGetOpenTasksAsync();
            return Partial("PrintTracking/Partials/_TaskList", OpenTasks);
        }

        private bool IsAdmin() => UserRole == "Admin" || UserRole == "Manager" || UserRole == "Administrator";
        private async Task<List<OperationalTask>> SafeGetOpenTasksAsync()
        {
            try { return await _taskService.GetOpenAsync(50); } catch { return new List<OperationalTask>(); }
        }
        // =======================================================================

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

                // Refresh task counts for the stats card (list loaded lazily)
                if (IsAdminView)
                {
                    OpenTasks = await SafeGetOpenTasksAsync();
                }

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

                // NEW: If the build wasn't linked to a scheduled job (log shows JobId=null), try to attach earliest scheduled job now
                if (model.AssociatedScheduledJobId == null && !string.IsNullOrWhiteSpace(model.PrinterName))
                {
                    var now = DateTime.Now.AddMinutes(5); // slight look ahead tolerance
                    var scheduledJob = await _context.Jobs
                        .Where(j => j.MachineId == model.PrinterName && j.Status == "Scheduled" && j.ScheduledStart <= now)
                        .OrderBy(j => j.ScheduledStart)
                        .FirstOrDefaultAsync();

                    if (scheduledJob != null)
                    {
                        scheduledJob.Status = "Building";
                        scheduledJob.ActualStart = model.ActualStartTime;
                        scheduledJob.LastModifiedDate = DateTime.UtcNow;
                        scheduledJob.LastModifiedBy = User.Identity?.Name ?? "PrintTracking";

                        // Attach to build job if it exists
                        var buildJob = await _context.BuildJobs.FirstOrDefaultAsync(b => b.BuildId == buildId);
                        if (buildJob != null && buildJob.AssociatedScheduledJobId == null)
                        {
                            buildJob.AssociatedScheduledJobId = scheduledJob.Id;
                            if (buildJob.PartId == null && scheduledJob.PartId > 0)
                            {
                                buildJob.PartId = scheduledJob.PartId;
                            }
                        }

                        await _context.SaveChangesAsync();
                    }
                }

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
                    _logger.LogWarning("CompletePrint validation failed. Errors: {Errors}", string.Join(", ", errors));
                    foreach (var kvp in ModelState)
                    {
                        if (kvp.Value.Errors.Any())
                        {
                            _logger.LogWarning("Field '{FieldName}' has errors: {FieldErrors}", kvp.Key, string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage)));
                        }
                    }
                    await PopulatePostPrintViewModelAsync(model);
                    return Partial("_PostPrintModal", model);
                }

                var userId = GetCurrentUserId();

                // Safety: if BuildId not supplied or resolves to no build, attempt inference
                if (model.BuildId <= 0)
                {
                    BuildJob? inferred = null;

                    // 1) If JobId provided, look for build linked to that job
                    if (model.JobId.HasValue)
                    {
                        inferred = await _context.BuildJobs
                            .Where(b => b.AssociatedScheduledJobId == model.JobId.Value && b.Status == "In Progress")
                            .OrderByDescending(b => b.ActualStartTime)
                            .FirstOrDefaultAsync();
                    }

                    // 2) If still not found, try by printer (PrinterName) active build
                    if (inferred == null && !string.IsNullOrWhiteSpace(model.PrinterName))
                    {
                        inferred = await _context.BuildJobs
                            .Where(b => b.PrinterName == model.PrinterName && b.Status == "In Progress")
                            .OrderByDescending(b => b.ActualStartTime)
                            .FirstOrDefaultAsync();
                    }

                    if (inferred != null)
                    {
                        model.BuildId = inferred.BuildId;
                        if (!model.JobId.HasValue && inferred.AssociatedScheduledJobId.HasValue)
                            model.JobId = inferred.AssociatedScheduledJobId;
                        _logger.LogInformation("Inferred BuildId {BuildId} (JobId {JobId}) for completion", model.BuildId, model.JobId);
                    }
                }

                // Attempt to infer JobId from build linkage if not provided AFTER inference
                if (!model.JobId.HasValue && model.BuildId > 0)
                {
                    var build = await _context.BuildJobs.FirstOrDefaultAsync(b => b.BuildId == model.BuildId);
                    if (build?.AssociatedScheduledJobId != null)
                        model.JobId = build.AssociatedScheduledJobId;
                }

                var success = await _printTrackingService.CompletePrintJobAsync(model, userId);

                if (success)
                {
                    _logger.LogInformation("Print completed successfully: BuildId {BuildId}, User {UserId}, ActualHours {ActualHours}",
                        model.BuildId, userId, model.OperatorActualHours);
                    return new JsonResult(new { success = true, message = "Print completed successfully" });
                }
                else
                {
                    _logger.LogWarning("CompletePrint failed - Build job not found or not In Progress. BuildId {BuildId}", model.BuildId);
                    model.Errors = new List<string> { "Error completing print job. Active build job not found." };
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

                // Find an active (or most recent) build associated with this scheduled job
                var build = await _context.BuildJobs
                    .Where(b => b.AssociatedScheduledJobId == job.Id)
                    .OrderByDescending(b => b.ActualStartTime)
                    .FirstOrDefaultAsync();

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
                    actualStart = (job.ActualStart ?? job.ScheduledStart).ToString("yyyy-MM-ddTHH:mm"),
                    buildId = build?.BuildId, // ONLY return real build id (null if none)
                    jobStatus = job.Status
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
                var allMachines = await _machineManagementService.GetActiveMachinesAsync();
                var slsMachines = allMachines
                    .Where(m => GetUnifiedMachineType(m) == "SLS")
                    .OrderBy(m => m.Priority)
                    .ToList();

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

        private List<MachineInfo> CreateFallbackSslMachineInfo()
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
                AvailablePrinters = new List<string> { "TI1", "TI2", "INC" },
                AddPowder = false // default off
            };

            // Resolve machine (by Id, Name, or MachineName) to get canonical MachineId + material
            Machine? machine = null;
            if (!string.IsNullOrWhiteSpace(printerName))
            {
                machine = await _context.Machines
                    .FirstOrDefaultAsync(m => m.MachineId == printerName || m.Name == printerName || m.MachineName == printerName);
                if (machine != null)
                {
                    viewModel.CurrentMachineMaterial = machine.CurrentMaterial;
                    // Normalize printer name to MachineId for downstream queries
                    viewModel.PrinterName = machine.MachineId;
                }
            }

            Job? job = null;

            if (jobId.HasValue)
            {
                job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
            }
            else if (machine != null)
            {
                // Auto-select earliest scheduled (or in-progress but not started) job for this machine
                var now = DateTime.Now;
                job = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machine.MachineId && (j.Status == "Scheduled" || j.Status == "In Progress" || j.Status == "Building"))
                    .OrderBy(j => j.ScheduledStart)
                    .FirstOrDefaultAsync();
            }

            if (job != null)
            {
                viewModel.AssociatedScheduledJobId = job.Id;
                viewModel.PartId = job.PartId;
                viewModel.PartNumber = job.PartNumber;
                viewModel.Quantity = job.Quantity;
                viewModel.EstimatedHours = job.EstimatedHours;
                viewModel.ScheduledStartTime = job.ScheduledStart;
                viewModel.ScheduledEndTime = job.ScheduledEnd;
                viewModel.EstimatedEndTime = job.ScheduledEnd;

                // Safe reflection (underlying property types may be byte/short/int)
                var stackProp = job.GetType().GetProperty("StackLevel");
                if (stackProp != null)
                {
                    var val = stackProp.GetValue(job);
                    if (val != null)
                    {
                        try { viewModel.StackLevel = Convert.ToInt32(val); } catch { }
                    }
                }
                var partsPerBuildProp = job.GetType().GetProperty("PartsPerBuild");
                if (partsPerBuildProp != null)
                {
                    var val = partsPerBuildProp.GetValue(job);
                    if (val != null)
                    {
                        try { viewModel.PartsPerBuild = Convert.ToInt32(val); } catch { }
                    }
                }

                if (job.Part != null)
                {
                    viewModel.PartDescription = job.Part.Description;
                    viewModel.Material = job.Part.SlsMaterial;
                }

                if (viewModel.TotalPartsInBuild <= 1)
                {
                    // Prefer PartsPerBuild if present, else job.Quantity
                    if (viewModel.PartsPerBuild.HasValue && viewModel.PartsPerBuild > 0)
                        viewModel.TotalPartsInBuild = viewModel.PartsPerBuild.Value;
                    else if (job.Quantity > 0)
                        viewModel.TotalPartsInBuild = job.Quantity;
                }
            }
            else
            {
                // Fallback: try to infer part from active build job on machine (if any)
                if (machine != null)
                {
                    var activeJob = await _context.BuildJobs.Include(b => b.Part)
                        .Where(b => b.PrinterName == machine.MachineId && b.Status == "In Progress")
                        .OrderByDescending(b => b.ActualStartTime)
                        .FirstOrDefaultAsync();
                    if (activeJob?.Part != null)
                    {
                        viewModel.PartId = activeJob.PartId;
                        viewModel.PartNumber = activeJob.Part.PartNumber;
                        viewModel.PartDescription = activeJob.Part.Description;
                        viewModel.Material = activeJob.Part.SlsMaterial;
                    }
                }
            }

            return viewModel;
        }

        private async Task<PostPrintViewModel> CreatePostPrintViewModelAsync(int? buildId, string? printerName, int? jobId)
        {
            var viewModel = new PostPrintViewModel
            {
                PrinterName = printerName ?? string.Empty,
                ActualStartTime = DateTime.Now.AddHours(-4), // default guess
                ActualEndTime = DateTime.Now,
                OperatorName = User.Identity?.Name ?? "Unknown",
                UserId = GetCurrentUserId(),
                AvailablePrinters = new List<string>(),
                Parts = new List<PostPrintPartEntry>()
            };

            try
            {
                // Active SLS printers list (fallback to defaults if none)
                var activePrinters = await _context.Machines
                    .Where(m => m.IsActive && (m.MachineType.Contains("SLS") || m.MachineType.Contains("Print") || m.MachineType == ""))
                    .OrderBy(m => m.Priority)
                    .Select(m => m.MachineId)
                    .Distinct()
                    .ToListAsync();
                if (!activePrinters.Any()) activePrinters = new List<string> { "TI1", "TI2", "INC" };
                viewModel.AvailablePrinters = activePrinters;
                if (!string.IsNullOrWhiteSpace(printerName) && !viewModel.AvailablePrinters.Contains(printerName))
                    viewModel.AvailablePrinters.Insert(0, printerName); // ensure selection appears

                // Load list of currently running jobs (Building / In Progress)
                var runningJobsQry = _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.Status == "Building" || j.Status == "In Progress");
                if (!string.IsNullOrWhiteSpace(printerName))
                    runningJobsQry = runningJobsQry.Where(j => j.MachineId == printerName);
                viewModel.AvailableRunningJobs = await runningJobsQry
                    .OrderBy(j => j.MachineId)
                    .ThenByDescending(j => j.ActualStart)
                    .Take(30)
                    .ToListAsync();

                BuildJob? build = null;

                // If a build id was passed explicitly, load it first
                if (buildId.HasValue)
                {
                    build = await _context.BuildJobs
                        .Include(b => b.Part)
                        .FirstOrDefaultAsync(b => b.BuildId == buildId.Value);
                }
                // Otherwise try to resolve active build from printer
                if (build == null && !string.IsNullOrWhiteSpace(printerName))
                {
                    build = await _context.BuildJobs
                        .Include(b => b.Part)
                        .Where(b => b.PrinterName == printerName && b.Status == "In Progress")
                        .OrderByDescending(b => b.ActualStartTime)
                        .FirstOrDefaultAsync();
                }
                // Finally use associated job id
                if (build == null && jobId.HasValue)
                {
                    build = await _context.BuildJobs
                        .Include(b => b.Part)
                        .FirstOrDefaultAsync(b => b.AssociatedScheduledJobId == jobId.Value && b.Status == "In Progress");
                }

                if (build != null)
                {
                    viewModel.BuildId = build.BuildId;
                    viewModel.PrinterName = build.PrinterName; // ensure dropdown auto-select
                    viewModel.ActualStartTime = build.ActualStartTime;
                    viewModel.OperatorEstimatedHours = build.OperatorEstimatedHours;
                    viewModel.JobId = build.AssociatedScheduledJobId; // might be null

                    if (build.Part != null)
                    {
                        viewModel.PartId = build.PartId;
                        viewModel.PartNumber = build.Part.PartNumber;
                        viewModel.PartDescription = build.Part.Description;
                    }
                }
                else if (jobId.HasValue)
                {
                    // Populate from scheduled job if build not found (rare edge case)
                    var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId.Value);
                    if (job != null)
                    {
                        viewModel.JobId = job.Id;
                        viewModel.PrinterName = string.IsNullOrWhiteSpace(printerName) ? job.MachineId : viewModel.PrinterName;
                        viewModel.ActualStartTime = job.ActualStart ?? job.ScheduledStart;
                        if (job.Part != null)
                        {
                            viewModel.PartId = job.PartId;
                            viewModel.PartNumber = job.Part.PartNumber;
                            viewModel.PartDescription = job.Part.Description;
                        }
                        // Estimate missing operator estimated hours
                        if (!viewModel.OperatorEstimatedHours.HasValue && job.ScheduledEnd > job.ScheduledStart)
                            viewModel.OperatorEstimatedHours = (decimal)(job.ScheduledEnd - job.ScheduledStart).TotalHours;
                    }
                }

                // Seed parts list for UI if we have a primary part
                if (!string.IsNullOrWhiteSpace(viewModel.PartNumber) && !viewModel.Parts.Any())
                {
                    viewModel.Parts.Add(new PostPrintPartEntry
                    {
                        PartNumber = viewModel.PartNumber,
                        Quantity = 1,
                        GoodParts = 1,
                        IsPrimary = true,
                        Description = viewModel.PartDescription
                    });
                }

                // Also load available parts (used for adding extra parts in modal)
                viewModel.AvailableParts = await _context.Parts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.PartNumber)
                    .Take(200)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building PostPrintViewModel (buildId={BuildId}, printer={Printer}, jobId={JobId})", buildId, printerName, jobId);
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

        public async Task<IActionResult> OnGetMaintenanceSummaryAsync(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return BadRequest("machineId required");
            var rules = await _maintenanceService.GetMachineStatusAsync(machineId, includeComponents: true);
            var summary = new OpCentrix.ViewModels.Maintenance.MaintenanceSummaryViewModel
            {
                MachineId = machineId,
                Rules = rules,
                OverdueCount = rules.Count(r => r.IsOverdue),
                DueSoonCount = rules.Count(r => r.IsDue && !r.IsOverdue),
                HighestSeverity = rules.Where(r => r.IsOverdue || r.IsDue)
                                        .OrderByDescending(r => r.IsOverdue)
                                        .ThenByDescending(r => r.Severity)
                                        .Select(r => r.Severity)
                                        .DefaultIfEmpty(OpCentrix.Models.Maintenance.MaintenanceSeverity.Info)
                                        .First()
            };
            return Partial("PrintTracking/Partials/_MaintenanceStrip", summary);
        }

        public async Task<IActionResult> OnGetMaintenanceDetailsAsync(string machineId)
        {
            if (string.IsNullOrWhiteSpace(machineId)) return BadRequest("machineId required");
            var rules = await _maintenanceService.GetMachineStatusAsync(machineId, includeComponents: true);
            var vm = new OpCentrix.ViewModels.Maintenance.MaintenanceSummaryViewModel
            {
                MachineId = machineId,
                Rules = rules,
                OverdueCount = rules.Count(r => r.IsOverdue),
                DueSoonCount = rules.Count(r => r.IsDue && !r.IsOverdue),
                HighestSeverity = rules.Where(r => r.IsOverdue || r.IsDue)
                                        .OrderByDescending(r => r.IsOverdue)
                                        .ThenByDescending(r => r.Severity)
                                        .Select(r => r.Severity)
                                        .DefaultIfEmpty(OpCentrix.Models.Maintenance.MaintenanceSeverity.Info)
                                        .First()
            };
            return Partial("PrintTracking/Partials/_MaintenanceDetailsModal", vm);
        }

        public async Task<IActionResult> OnGetMaintenanceFleetSummaryAsync()
        {
            try
            {
                // Load fleet status via service
                var machines = await _context.Machines.Where(m => m.IsActive).Select(m => m.MachineId).ToListAsync();
                var overdueMachines = new List<string>();
                var dueSoonMachines = new List<string>();
                int overdueRules = 0;
                int dueSoonRules = 0;
                foreach (var m in machines)
                {
                    var rules = await _maintenanceService.GetMachineStatusAsync(m);
                    if (!rules.Any()) continue;
                    var machineOverdue = rules.Count(r => r.IsOverdue);
                    var machineDue = rules.Count(r => r.IsDue && !r.IsOverdue);
                    if (machineOverdue > 0)
                    {
                        overdueMachines.Add(m);
                        overdueRules += machineOverdue;
                    }
                    if (machineDue > 0)
                    {
                        dueSoonMachines.Add(m);
                        dueSoonRules += machineDue;
                    }
                }
                var vm = new OpCentrix.ViewModels.Maintenance.MaintenanceFleetSummaryViewModel
                {
                    MachinesWithIssues = overdueMachines.Union(dueSoonMachines).Distinct().Count(),
                    OverdueRules = overdueRules,
                    DueSoonRules = dueSoonRules,
                    MachinesOverdue = overdueMachines.Distinct().OrderBy(x => x).ToList(),
                    MachinesDueSoon = dueSoonMachines.Except(overdueMachines).Distinct().OrderBy(x => x).ToList(),
                    GeneratedAt = DateTime.UtcNow
                };
                return Partial("PrintTracking/Partials/_MaintenanceFleetSummary", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating maintenance fleet summary");
                return Partial("PrintTracking/Partials/_MaintenanceFleetSummary", new OpCentrix.ViewModels.Maintenance.MaintenanceFleetSummaryViewModel { GeneratedAt = DateTime.UtcNow });
            }
        }
    }
}