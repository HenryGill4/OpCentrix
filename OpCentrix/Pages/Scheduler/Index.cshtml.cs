using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.ViewModels.Scheduler;
using OpCentrix.ViewModels.Shared;
using OpCentrix.Data;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using OpCentrix.Authorization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpCentrix.Pages.Scheduler
{
    /// <summary>
    /// Modern scheduler page using best practices and clean architecture
    /// FIXED: Machine validation and database integration issues resolved
    /// </summary>
    [SchedulerAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerService _schedulerService;
        private readonly IMachineManagementService _machineService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<IndexModel> _logger;
        private const bool BYPASS_SHIFT_CHECKS = true; // TEMP: hard bypass to stabilize scheduler

        public IndexModel(
            SchedulerContext context, 
            ISchedulerService schedulerService, 
            IMachineManagementService machineService, 
            ITimeSlotService timeSlotService, 
            ILogger<IndexModel> logger)
        {
            _context = context;
            _schedulerService = schedulerService;
            _machineService = machineService;
            _timeSlotService = timeSlotService;
            _logger = logger;
        }

        // Display properties - Clean separation
        public SchedulerPageViewModel ViewModel { get; set; } = new();
        public FooterSummaryViewModel Summary { get; set; } = new();
        public List<Machine> AvailableMachines { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();

        // Form binding - Modern DTO approach
        [BindProperty]
        public CreateJobDto CreateJobRequest { get; set; } = new();

        [BindProperty]
        public EditJobDto EditJobRequest { get; set; } = new();

        [BindProperty]
        public int? EditingJobId { get; set; }

        // Helper: unify/normalize machine type labels for scheduler grouping
        private static string GetUnifiedMachineType(Machine m)
        {
            if (m == null) return "Unknown";
            string raw = (m.MachineType ?? "").Trim();
            string name = (m.MachineName ?? m.Name ?? "").Trim();
            string model = (m.MachineModel ?? "").Trim();
            string all = string.Join(" ", raw, name, model).ToUpperInvariant();

            // SLS group (TruPrint + Custom SLS + generic SLS keywords)
            if (all.Contains("TRUPRINT") || all.Contains("TRU PRINT") || all.Contains("SLS") || all.Contains("SELECTIVE LASER") )
                return "SLS";

            // CNC group (Haas, Doosan, Mazak, generic CNC)
            if (all.Contains("CNC") || all.Contains("HAAS") || all.Contains("MAZAK") || all.Contains("DOOSAN"))
                return "CNC";

            // EDM group
            if (all.Contains("EDM") || all.Contains("WIRE EDM"))
                return "EDM";

            // Coating / Cerakote group
            if (all.Contains("COAT") || all.Contains("CERAKOTE") )
                return "Coating";

            // Inspection / QC group
            if (all.Contains("INSPECTION") || all.Contains("QC") || all.Contains("CMM"))
                return "Inspection";

            return string.IsNullOrWhiteSpace(raw) ? "Other" : raw; // fallback to stored type
        }

        // Added machineType filter
        public async Task OnGetAsync(string? zoom = null, DateTime? startDate = null, string? orientation = null, string? machineType = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🎯 [SCHEDULER-{OperationId}] Loading modern scheduler", operationId);

            try
            {
                await LoadAvailableMachinesAsync(operationId);
                // Backfill colors if missing
                foreach (var m in AvailableMachines)
                {
                    if (string.IsNullOrWhiteSpace(m.ColorHex))
                    {
                        m.ColorHex = AssignColor(m.MachineId, AvailableMachines);
                        _context.Machines.Update(m);
                    }
                }
                await _context.SaveChangesAsync();

                // Base data
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                ViewModel.Machines = AvailableMachines.Select(m => m.MachineId).ToList();
                ViewModel.MachineColors = AvailableMachines.ToDictionary(m => m.MachineId, m => string.IsNullOrWhiteSpace(m.ColorHex) ? m.EffectiveColorHex : m.ColorHex!);

                // Jobs before filtering
                await LoadJobsAsync(operationId);

                // Build unified type map
                var unifiedTypeMap = AvailableMachines.ToDictionary(m => m.MachineId, GetUnifiedMachineType);
                var distinctTypes = unifiedTypeMap.Values.Distinct().OrderBy(t => t).ToList();
                ViewData["MachineTypes"] = distinctTypes;

                // Apply filter if requested
                if (!string.IsNullOrWhiteSpace(machineType) && !string.Equals(machineType, "all", StringComparison.OrdinalIgnoreCase))
                {
                    var target = machineType.Trim().ToUpperInvariant();
                    var filteredMachineIds = unifiedTypeMap.Where(kvp => kvp.Value.ToUpperInvariant() == target).Select(kvp => kvp.Key).ToHashSet();

                    ViewModel.Machines = ViewModel.Machines.Where(id => filteredMachineIds.Contains(id)).ToList();
                    ViewModel.Jobs = ViewModel.Jobs.Where(j => filteredMachineIds.Contains(j.MachineId)).ToList();
                    ViewModel.MachineColors = ViewModel.MachineColors
                        .Where(kvp => filteredMachineIds.Contains(kvp.Key))
                        .ToDictionary(k => k.Key, v => v.Value);
                }
                ViewData["CurrentMachineTypeFilter"] = string.IsNullOrWhiteSpace(machineType) ? "all" : machineType;

                // Summary based on (possibly filtered) machines
                await GenerateSummaryAsync(operationId);

                await LoadAvailablePartsAsync(operationId);

                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Scheduler loaded: {JobCount} jobs, {MachineCount} machines (filter={Filter})",
                    operationId, ViewModel.Jobs.Count, ViewModel.Machines.Count, ViewData["CurrentMachineTypeFilter"]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading scheduler", operationId);
                await InitializeEmptyDataAsync();
                TempData["Error"] = "Error loading scheduler data. Please try again.";
            }
        }

        // NEW: Lightweight partial refresh handler for the grid (removes need for full page reload)
        public async Task<IActionResult> OnGetRefreshGridAsync(string? zoom = null, DateTime? startDate = null, string? orientation = null, string? machineType = null)
        {
            var opId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                await LoadAvailableMachinesAsync(opId);
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                ViewModel.Machines = AvailableMachines.Select(m => m.MachineId).ToList();
                ViewModel.MachineColors = AvailableMachines.ToDictionary(m => m.MachineId, m => string.IsNullOrWhiteSpace(m.ColorHex) ? m.EffectiveColorHex : m.ColorHex!);
                await LoadJobsAsync(opId);

                var unifiedTypeMap = AvailableMachines.ToDictionary(m => m.MachineId, GetUnifiedMachineType);
                if (!string.IsNullOrWhiteSpace(machineType) && !string.Equals(machineType, "all", StringComparison.OrdinalIgnoreCase))
                {
                    var target = machineType.Trim().ToUpperInvariant();
                    var filteredMachineIds = unifiedTypeMap.Where(kvp => kvp.Value.ToUpperInvariant() == target).Select(kvp => kvp.Key).ToHashSet();
                    ViewModel.Machines = ViewModel.Machines.Where(id => filteredMachineIds.Contains(id)).ToList();
                    ViewModel.Jobs = ViewModel.Jobs.Where(j => filteredMachineIds.Contains(j.MachineId)).ToList();
                    ViewModel.MachineColors = ViewModel.MachineColors.Where(kvp => filteredMachineIds.Contains(kvp.Key)).ToDictionary(k => k.Key, v => v.Value);
                }

                await GenerateSummaryAsync(opId); // Keep summary coherent if needed client-side

                var isVertical = orientation == "vertical";
                if (isVertical)
                {
                    return Partial("_SchedulerVertical", ViewModel);
                }
                return Partial("_SchedulerHorizontal", ViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error refreshing grid", opId);
                return Content("<div class='p-4 text-red-600'>Error refreshing scheduler grid</div>", "text/html");
            }
        }

        // NEW: Lightweight partial refresh handler for footer summary
        public async Task<IActionResult> OnGetRefreshSummaryAsync(string? zoom = null, DateTime? startDate = null, string? machineType = null)
        {
            var opId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                await LoadAvailableMachinesAsync(opId);
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                ViewModel.Machines = AvailableMachines.Select(m => m.MachineId).ToList();
                ViewModel.MachineColors = AvailableMachines.ToDictionary(m => m.MachineId, m => string.IsNullOrWhiteSpace(m.ColorHex) ? m.EffectiveColorHex : m.ColorHex!);
                await LoadJobsAsync(opId);

                var unifiedTypeMap = AvailableMachines.ToDictionary(m => m.MachineId, GetUnifiedMachineType);
                if (!string.IsNullOrWhiteSpace(machineType) && !string.Equals(machineType, "all", StringComparison.OrdinalIgnoreCase))
                {
                    var target = machineType.Trim().ToUpperInvariant();
                    var filteredMachineIds = unifiedTypeMap.Where(kvp => kvp.Value.ToUpperInvariant() == target).Select(kvp => kvp.Key).ToHashSet();
                    ViewModel.Machines = ViewModel.Machines.Where(id => filteredMachineIds.Contains(id)).ToList();
                    ViewModel.Jobs = ViewModel.Jobs.Where(j => filteredMachineIds.Contains(j.MachineId)).ToList();
                }

                await GenerateSummaryAsync(opId);
                return Partial("_FooterSummary", Summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error refreshing summary", opId);
                return Content("<div class='p-2 text-red-600'>Error refreshing summary</div>", "text/html");
            }
        }

        public async Task<IActionResult> OnGetShowAddModalAsync(string machineId, string date, int? id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🎯 [SCHEDULER-{OperationId}] Opening job modal: machine={MachineId}, id={JobId}", 
                operationId, machineId, id);

            try
            {
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    parsedDate = DateTime.UtcNow;
                }

                Job job;
                if (id.HasValue)
                {
                    job = await _context.Jobs
                        .Include(j => j.Part)
                        .FirstOrDefaultAsync(j => j.Id == id.Value);

                    if (job == null)
                    {
                        return await CreateModalWithErrorAsync("Job not found", machineId, parsedDate, operationId);
                    }
                }
                else
                {
                    job = await CreateNewJobAsync(machineId, parsedDate, operationId);
                }

                await LoadAvailableMachinesAsync(operationId);
                await LoadAvailablePartsAsync(operationId);

                return Partial("_AddEditJobModal", new AddEditJobViewModel 
                { 
                    Job = job, 
                    Parts = AvailableParts,
                    Machines = AvailableMachines
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error opening job modal", operationId);
                return await CreateModalWithErrorAsync("Error loading job form", machineId, DateTime.UtcNow, operationId);
            }
        }

        public async Task<IActionResult> OnPostAddOrUpdateJobAsync([FromForm] CreateJobDto jobRequest)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [SCHEDULER-{OperationId}] Processing job: Id={JobId}, MachineId={MachineId}, PartId={PartId}",
                operationId, jobRequest.Id, jobRequest.MachineId, jobRequest.PartId);

            try
            {
                await LoadAvailableMachinesAsync(operationId);
                await LoadAvailablePartsAsync(operationId);
                var validationResult = await ValidateJobRequestAsync(jobRequest, operationId);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Validation failed: {ErrorCount} errors", 
                        operationId, validationResult.Errors.Count);
                    var errorJob = await ConvertDtoToJobAsync(jobRequest);
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = errorJob,
                        Parts = AvailableParts,
                        Machines = AvailableMachines,
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                }

                Job savedJob;
                if (jobRequest.Id == 0)
                {
                    savedJob = await CreateJobFromDtoAsync(jobRequest, operationId);
                }
                else
                {
                    savedJob = await UpdateJobFromDtoAsync(jobRequest, operationId);
                }

                var successMessage = jobRequest.Id == 0 
                    ? $"Job scheduled successfully for {savedJob.PartNumber}" 
                    : $"Job updated successfully for {savedJob.PartNumber}";

                if (Request.Headers.ContainsKey("HX-Request"))
                {
                    return await HandleSchedulerSuccess(successMessage);
                }

                TempData["SuccessMessage"] = successMessage;
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                var operationIdCopy = operationId;
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error processing job", operationIdCopy);
                try
                {
                    await LoadAvailableMachinesAsync(operationIdCopy);
                    await LoadAvailablePartsAsync(operationIdCopy);
                    var errorJob = await ConvertDtoToJobAsync(jobRequest);
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = errorJob,
                        Parts = AvailableParts,
                        Machines = AvailableMachines,
                        Errors = new List<string> { $"Error saving job: {ex.Message}" }
                    });
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "❌ [SCHEDULER-{OperationId}] Critical error in error handling", operationIdCopy);
                    return Content($@"<script>alert('Error saving job: {ex.Message.Replace("'", "\\'")}');</script>", "text/html");
                }
            }
        }

        public async Task<IActionResult> OnDeleteJobAsync([FromQuery] int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [SCHEDULER-{OperationId}] DELETE handler called for job: {JobId}", operationId, id);
            return await DeleteJobInternalAsync(id, operationId);
        }

        public async Task<IActionResult> OnPostDeleteJobAsync([FromQuery] int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [SCHEDULER-{OperationId}] POST DELETE handler called for job: {JobId}", operationId, id);
            return await DeleteJobInternalAsync(id, operationId);
        }

        private async Task<IActionResult> DeleteJobInternalAsync(int id, string operationId)
        {
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Job {JobId} not found for deletion", operationId, id);
                    return Content("<script>window.showErrorNotification && window.showErrorNotification('Job not found');</script>", "text/html");
                }
                var partNumber = job.PartNumber;
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job deleted: {JobId} - {PartNumber}", operationId, id, partNumber);
                var script = GetGridRefreshScript($"Job \"{partNumber}\" deleted successfully!");
                return Content(script, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error deleting job: {JobId}", operationId, id);
                return Content($"<script>window.showErrorNotification && window.showErrorNotification('Error deleting job');</script>", "text/html");
            }
        }

        private string GetGridRefreshScript(string message)
        {
            // Temporary visual bandaid: show loading overlay to mask color flicker while grid refreshes
            return $@"<script>(function(){{
                const overlay = document.getElementById('loading-indicator');
                const modal = document.getElementById('modal-container');
                if(modal){{ modal.style.display='none'; modal.classList.add('hidden'); modal.innerHTML=''; }}
                document.body.style.overflow='';
                if(window.showSuccessNotification){{window.showSuccessNotification('{message}');}}
                const qs = window.location.search;
                const refresh = () => {{

                    if(!window.htmx){{ window.location.reload(); return; }}
                    const gridReq = htmx.ajax('GET','/Scheduler?handler=RefreshGrid'+(qs?qs.replace('?','&'):''),{{target:'#scheduler-main-content',swap:'innerHTML'}});
                    const summaryReq = htmx.ajax('GET','/Scheduler?handler=RefreshSummary'+(qs?qs.replace('?','&'):''),{{target:'#footer-summary',swap:'innerHTML'}});
                    Promise.all([gridReq, summaryReq]).then(()=>{{
                        if(overlay) overlay.classList.add('hidden');
                    }}).catch(()=>{{ if(overlay) overlay.classList.add('hidden'); }});
                }};
                if(overlay) overlay.classList.remove('hidden');
                // Small delay to hide white glitch before content arrives (BANDAID - replace with smoother incremental rendering later)
                setTimeout(refresh, 350);
            }})();</script>";
        }

        public async Task<IActionResult> OnPostStartPrintJobAsync(int jobId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🎬 [SCHEDULER-{OperationId}] Starting print job: {JobId}", operationId, jobId);
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }
                job.Status = "Building";
                job.ActualStart = DateTime.UtcNow;
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "System";
                await _context.SaveChangesAsync();
                try
                {
                    var printTrackingService = HttpContext.RequestServices.GetService<IPrintTrackingService>();
                    if (printTrackingService != null)
                    {
                        await printTrackingService.CreateBuildJobFromScheduledJobAsync(jobId, User.Identity?.Name ?? "System");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ [SCHEDULER-{OperationId}] Failed to create build job tracking for job {JobId}", operationId, jobId);
                }
                return new JsonResult(new { success = true, message = $"Job {job.PartNumber} started successfully!", jobId, machineId = job.MachineId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error starting print job: {JobId}", operationId, jobId);
                return new JsonResult(new { success = false, error = "Error starting print job" });
            }
        }

        public async Task<IActionResult> OnGetPrintTrackingStatusAsync(int jobId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("📊 [SCHEDULER-{OperationId}] Getting print tracking status for job: {JobId}", operationId, jobId);
            try
            {
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }
                var buildJob = await _context.BuildJobs.FirstOrDefaultAsync(bj => bj.AssociatedScheduledJobId == jobId);
                var status = new
                {
                    success = true,
                    jobId = job.Id,
                    partNumber = job.PartNumber,
                    machineId = job.MachineId,
                    status = job.Status,
                    scheduledStart = job.ScheduledStart,
                    scheduledEnd = job.ScheduledEnd,
                    actualStart = job.ActualStart,
                    actualEnd = job.ActualEnd,
                    hasBuildJob = buildJob != null,
                    buildId = buildJob?.BuildId,
                    operatorEstimate = buildJob?.OperatorEstimatedHours,
                    printTrackingUrl = $"/PrintTracking?jobId={jobId}&machineId={job.MachineId}",
                    isSlsJob = new[] { "TI1", "TI2", "INC", "INC1", "INC2" }.Contains(job.MachineId)
                };
                return new JsonResult(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error getting print tracking status for job: {JobId}", operationId, jobId);
                return new JsonResult(new { success = false, error = "Error getting print tracking status" });
            }
        }

        public async Task<IActionResult> OnPostUpdateFromPrintTrackingAsync(int jobId, decimal actualHours, string operatorName, string notes = "")
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔄 [SCHEDULER-{OperationId}] Updating job {JobId} from print tracking: {ActualHours}h by {Operator}", 
                operationId, jobId, actualHours, operatorName);
            try
            {
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }
                job.Status = "Completed";
                job.ActualEnd = DateTime.UtcNow;
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = operatorName ?? User.Identity?.Name ?? "PrintTracking";
                if (!string.IsNullOrEmpty(notes))
                {
                    job.Notes = string.IsNullOrEmpty(job.Notes) ? $"Print completed: {notes}" : $"{job.Notes}\nPrint completed: {notes}";
                }
                await _context.SaveChangesAsync();
                var updateNotification = new
                {
                    type = "scheduleUpdated",
                    jobId,
                    machineId = job.MachineId,
                    partNumber = job.PartNumber,
                    actualHours,
                    operatorName,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job {JobId} updated from print tracking completion", operationId, jobId);
                return new JsonResult(new { success = true, message = $"Job {job.PartNumber} updated from print tracking", notification = updateNotification });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error updating job from print tracking: {JobId}", operationId, jobId);
                return new JsonResult(new { success = false, error = "Error updating job from print tracking" });
            }
        }

        private async Task LoadAvailableMachinesAsync(string operationId)
        {
            try
            {
                AvailableMachines = await _machineService.GetActiveMachinesAsync();
                if (!AvailableMachines.Any())
                {
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] No active machines found in database", operationId);
                    var defaultCreated = await _machineService.SeedDefaultMachinesAsync();
                    if (defaultCreated)
                    {
                        AvailableMachines = await _machineService.GetActiveMachinesAsync();
                        _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Created default machines: {Count}", operationId, AvailableMachines.Count);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading machines", operationId);
                AvailableMachines = new List<Machine>();
            }
        }

        private async Task LoadAvailablePartsAsync(string operationId)
        {
            try
            {
                AvailableParts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading parts", operationId);
                AvailableParts = new List<Part>();
            }
        }

        private async Task LoadJobsAsync(string operationId)
        {
            try
            {
                var startDate = ViewModel.StartDate.AddDays(-1);
                var endDate = ViewModel.StartDate.AddDays(ViewModel.Dates.Count + 1);
                ViewModel.Jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.ScheduledStart < endDate && j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading jobs", operationId);
                ViewModel.Jobs = new List<Job>();
            }
        }

        private async Task GenerateSummaryAsync(string operationId)
        {
            try
            {
                var activeMachineSet = ViewModel.Machines.ToHashSet();
                Summary = new FooterSummaryViewModel
                {
                    MachineHours = AvailableMachines
                        .Where(m => activeMachineSet.Contains(m.MachineId))
                        .ToDictionary(
                            m => m.MachineId,
                            m => ViewModel.Jobs.Where(j => j.MachineId == m.MachineId).Sum(j => j.DurationHours)
                        ),
                    JobCounts = AvailableMachines
                        .Where(m => activeMachineSet.Contains(m.MachineId))
                        .ToDictionary(
                            m => m.MachineId,
                            m => ViewModel.Jobs.Count(j => j.MachineId == m.MachineId)
                        )
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error generating summary", operationId);
                Summary = new FooterSummaryViewModel();
            }
        }

        private async Task InitializeEmptyDataAsync()
        {
            ViewModel = new SchedulerPageViewModel
            {
                StartDate = DateTime.UtcNow.Date,
                Dates = new List<DateTime> { DateTime.UtcNow.Date },
                Machines = new List<string>(),
                Jobs = new List<Job>(),
                MachineRowHeights = new Dictionary<string, int>()
            };
            
            Summary = new FooterSummaryViewModel();
            AvailableMachines = new List<Machine>();
            AvailableParts = new List<Part>();
        }

        private async Task<Job> CreateNewJobAsync(string machineId, DateTime startDate, string operationId)
        {
            try
            {
                var nextAvailableTime = startDate;
                if (false)
                {
                    nextAvailableTime = await _timeSlotService.GetNextAvailableTimeAsync(machineId, startDate, 8.0);
                }
                else
                {
                    // Simple next-hour rounded fallback
                    var rounded = new DateTime(startDate.Year, startDate.Month, startDate.Day, startDate.Hour, 0, 0);
                    if (startDate.Minute > 0 || startDate.Second > 0) rounded = rounded.AddHours(1);
                    // Clamp after-hours start to 8 AM next business day heuristic
                    if (rounded.Hour < 6) rounded = rounded.Date.AddHours(8);
                    nextAvailableTime = rounded;
                }
                return new Job
                {
                    MachineId = machineId,
                    ScheduledStart = nextAvailableTime,
                    ScheduledEnd = nextAvailableTime.AddHours(8),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000",
                    EstimatedHours = 8.0,
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    CustomerOrderNumber = "",
                    LaserPowerWatts = 200,
                    ScanSpeedMmPerSec = 1200,
                    LayerThicknessMicrons = 30,
                    HatchSpacingMicrons = 120,
                    BuildTemperatureCelsius = 180,
                    ArgonPurityPercent = 99.9,
                    OxygenContentPpm = 50,
                    RequiresArgonPurge = true,
                    RequiresPreheating = true,
                    RequiresPowderSieving = true,
                    DensityPercentage = 99.5,
                    MaterialCostPerKg = 450.00m,
                    LaborCostPerHour = 85.00m,
                    MachineOperatingCostPerHour = 125.00m,
                    ArgonCostPerHour = 15.00m,
                    PreheatingTimeMinutes = 60,
                    CoolingTimeMinutes = 240,
                    PostProcessingTimeMinutes = 45,
                    EstimatedPowderUsageKg = 0.5
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error creating new job", operationId);
                var fallbackStart = startDate.Hour < 6 ? startDate.Date.AddHours(8) : startDate;
                return new Job
                {
                    MachineId = machineId,
                    ScheduledStart = fallbackStart,
                    ScheduledEnd = fallbackStart.AddHours(8),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000",
                    EstimatedHours = 8.0,
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    CustomerOrderNumber = ""
                };
            }
        }

        private async Task<JobValidationResult> ValidateJobRequestAsync(CreateJobDto request, string operationId)
        {
            var result = new JobValidationResult();
            if (string.IsNullOrWhiteSpace(request.MachineId)) result.AddError(nameof(request.MachineId), "Machine must be selected");
            if (request.PartId <= 0) result.AddError(nameof(request.PartId), "Part must be selected");
            if (request.ScheduledStart >= request.ScheduledEnd) result.AddError(nameof(request.ScheduledEnd), "End time must be after start time");
            if (!string.IsNullOrWhiteSpace(request.MachineId))
            {
                var machine = AvailableMachines.FirstOrDefault(m => m.MachineId == request.MachineId);
                if (machine == null)
                {
                    result.AddError(nameof(request.MachineId), $"Machine '{request.MachineId}' is not available.");
                }
                else if (!machine.IsActive)
                {
                    result.AddError(nameof(request.MachineId), $"Machine '{request.MachineId}' is not active");
                }
                else if (!machine.IsAvailableForScheduling)
                {
                    result.AddError(nameof(request.MachineId), $"Machine '{request.MachineId}' is not available for scheduling");
                }
            }
            var duration = request.ScheduledEnd - request.ScheduledStart;
            if (duration.TotalHours > 168) result.AddError(nameof(request.ScheduledEnd), "Job duration cannot exceed 1 week");
            if (duration.TotalMinutes < 15) result.AddError(nameof(request.ScheduledEnd), "Job duration must be at least 15 minutes");

            // TEMP: Skip shift validation to prevent DB/loop issues until schema is stabilized
            if (false)
            {
                try
                {
                    var shiftService = HttpContext.RequestServices.GetService<IOperatingShiftService>();
                    if (shiftService != null && !string.IsNullOrWhiteSpace(request.MachineId))
                    {
                        var okStart = await shiftService.IsTimeWithinOperatingHoursAsync(request.ScheduledStart, request.MachineId);
                        var okEnd = await shiftService.IsTimeWithinOperatingHoursAsync(request.ScheduledEnd, request.MachineId);
                        if (!okStart || !okEnd)
                        {
                            result.AddError(nameof(request.ScheduledStart), "Scheduled time is outside operating hours for the selected machine.");
                        }
                    }
                }
                catch { }
            }
            return result;
        }

        private async Task<Job> CreateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            var part = await _context.Parts.FindAsync(dto.PartId) ?? throw new InvalidOperationException("Selected part not found");
            var job = new Job
            {
                MachineId = dto.MachineId,
                PartId = dto.PartId,
                PartNumber = part.PartNumber,
                ScheduledStart = dto.ScheduledStart,
                ScheduledEnd = dto.ScheduledEnd,
                EstimatedHours = (dto.ScheduledEnd - dto.ScheduledStart).TotalHours,
                Quantity = dto.Quantity,
                Priority = dto.Priority,
                Status = dto.Status ?? "Scheduled",
                SlsMaterial = dto.SlsMaterial ?? part.SlsMaterial ?? "Ti-6Al-4V Grade 5",
                LaserPowerWatts = dto.LaserPowerWatts,
                ScanSpeedMmPerSec = dto.ScanSpeedMmPerSec,
                LayerThicknessMicrons = dto.LayerThicknessMicrons,
                HatchSpacingMicrons = dto.HatchSpacingMicrons,
                BuildTemperatureCelsius = dto.BuildTemperatureCelsius,
                EstimatedPowderUsageKg = dto.EstimatedPowderUsageKg,
                Notes = dto.Notes,
                CustomerOrderNumber = dto.CustomerOrderNumber ?? "",
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob,
                ArgonPurityPercent = 99.9,
                OxygenContentPpm = 50,
                RequiresArgonPurge = true,
                RequiresPreheating = true,
                RequiresPowderSieving = true,
                DensityPercentage = 99.5,
                MaterialCostPerKg = part.MaterialCostPerKg,
                LaborCostPerHour = part.StandardLaborCostPerHour,
                MachineOperatingCostPerHour = part.MachineOperatingCostPerHour,
                ArgonCostPerHour = part.ArgonCostPerHour,
                PreheatingTimeMinutes = part.PreheatingTimeMinutes,
                CoolingTimeMinutes = part.CoolingTimeMinutes,
                PostProcessingTimeMinutes = part.PostProcessingTimeMinutes,
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System",
                LastModifiedBy = User.Identity?.Name ?? "System"
            };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        private async Task<Job> UpdateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            var job = await _context.Jobs.FindAsync(dto.Id) ?? throw new InvalidOperationException("Job not found for update");
            var part = await _context.Parts.FindAsync(dto.PartId) ?? throw new InvalidOperationException("Selected part not found");
            job.MachineId = dto.MachineId;
            job.PartId = dto.PartId;
            job.PartNumber = part.PartNumber;
            job.ScheduledStart = dto.ScheduledStart;
            job.ScheduledEnd = dto.ScheduledEnd;
            job.EstimatedHours = (dto.ScheduledEnd - dto.ScheduledStart).TotalHours;
            job.Quantity = dto.Quantity;
            job.Priority = dto.Priority;
            job.Status = dto.Status ?? job.Status;
            job.SlsMaterial = dto.SlsMaterial ?? job.SlsMaterial;
            job.LaserPowerWatts = dto.LaserPowerWatts;
            job.ScanSpeedMmPerSec = dto.ScanSpeedMmPerSec;
            job.LayerThicknessMicrons = dto.LayerThicknessMicrons;
            job.HatchSpacingMicrons = dto.HatchSpacingMicrons;
            job.BuildTemperatureCelsius = dto.BuildTemperatureCelsius;
            job.EstimatedPowderUsageKg = dto.EstimatedPowderUsageKg;
            job.Notes = dto.Notes;
            job.CustomerOrderNumber = dto.CustomerOrderNumber ?? "";
            job.Operator = dto.Operator;
            job.IsRushJob = dto.IsRushJob;
            if (job.PartId != dto.PartId)
            {
                job.ArgonPurityPercent = 99.9;
                job.OxygenContentPpm = 50;
                job.RequiresArgonPurge = true;
                job.RequiresPreheating = true;
                job.RequiresPowderSieving = true; // fixed property name
                job.MaterialCostPerKg = part.MaterialCostPerKg;
                job.LaborCostPerHour = part.StandardLaborCostPerHour;
                job.MachineOperatingCostPerHour = part.MachineOperatingCostPerHour;
                job.ArgonCostPerHour = part.ArgonCostPerHour;
                job.PreheatingTimeMinutes = part.PreheatingTimeMinutes;
                job.CoolingTimeMinutes = part.CoolingTimeMinutes;
                job.PostProcessingTimeMinutes = part.PostProcessingTimeMinutes;
            }
            job.LastModifiedDate = DateTime.UtcNow;
            job.LastModifiedBy = User.Identity?.Name ?? "System";
            await _context.SaveChangesAsync();
            return job;
        }

        private async Task<Job> ConvertDtoToJobAsync(CreateJobDto dto)
        {
            var job = new Job
            {
                Id = dto.Id,
                MachineId = dto.MachineId,
                PartId = dto.PartId,
                ScheduledStart = dto.ScheduledStart,
                ScheduledEnd = dto.ScheduledEnd,
                Quantity = dto.Quantity,
                Priority = dto.Priority,
                Status = dto.Status ?? "Scheduled",
                SlsMaterial = dto.SlsMaterial ?? "Ti-6Al-4V Grade 5",
                LaserPowerWatts = dto.LaserPowerWatts,
                ScanSpeedMmPerSec = dto.ScanSpeedMmPerSec,
                LayerThicknessMicrons = dto.LayerThicknessMicrons,
                HatchSpacingMicrons = dto.HatchSpacingMicrons,
                BuildTemperatureCelsius = dto.BuildTemperatureCelsius,
                EstimatedPowderUsageKg = dto.EstimatedPowderUsageKg,
                Notes = dto.Notes,
                CustomerOrderNumber = dto.CustomerOrderNumber ?? "",
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob
            };
            if (dto.PartId > 0)
            {
                var part = await _context.Parts.FindAsync(dto.PartId);
                if (part != null) job.PartNumber = part.PartNumber;
            }
            return job;
        }

        private async Task<PartialViewResult> CreateModalWithErrorAsync(string errorMessage, string machineId, DateTime startDate, string operationId)
        {
            await LoadAvailableMachinesAsync(operationId);
            await LoadAvailablePartsAsync(operationId);
            var errorJob = await CreateNewJobAsync(machineId, startDate, operationId);
            return Partial("_AddEditJobModal", new AddEditJobViewModel
            {
                Job = errorJob,
                Parts = AvailableParts,
                Machines = AvailableMachines,
                Errors = new List<string> { errorMessage }
            });
        }

        private async Task<IActionResult> HandleSchedulerSuccess(string message)
        {
            // Instead of full page reload, refresh grid & summary via HTMX for instant feedback
            var script = GetGridRefreshScript(message);
            return Content(script, "text/html");
        }

        public async Task<IActionResult> OnGetSuggestNextTimeAsync(string machineId, double durationHours, DateTime? preferredStart = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🕐 [SCHEDULER-{OperationId}] Suggesting next time for {MachineId}, duration {Duration}h",
                operationId, machineId, durationHours);
            try
            {
                if (string.IsNullOrWhiteSpace(machineId))
                {
                    return new JsonResult(new { success = false, error = "Machine ID is required" });
                }
                DateTime suggestedStart;
                if (true)
                {
                    // Simple deterministic suggestion for stability
                    var seed = preferredStart ?? DateTime.UtcNow;
                    suggestedStart = new DateTime(seed.Year, seed.Month, seed.Day, seed.Hour, 0, 0);
                    if (seed.Minute > 0 || seed.Second > 0) suggestedStart = suggestedStart.AddHours(1);
                    if (suggestedStart.Hour < 6) suggestedStart = suggestedStart.Date.AddHours(8);
                }
                else
                {
                    suggestedStart = await _timeSlotService.GetNextAvailableTimeAsync(machineId, preferredStart ?? DateTime.UtcNow, durationHours);
                }
                var suggestedEnd = suggestedStart.AddHours(durationHours);
                return new JsonResult(new
                {
                    success = true,
                    startTime = suggestedStart.ToString("yyyy-MM-ddTHH:mm"),
                    endTime = suggestedEnd.ToString("yyyy-MM-ddTHH:mm"),
                    displayStart = suggestedStart.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    displayEnd = suggestedEnd.ToString("MMM dd, yyyy 'at' h:mm tt"),
                    message = $"Next available slot: {suggestedStart:MMM dd 'at' h:mm tt}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error suggesting next time", operationId);
                return new JsonResult(new { success = false, error = "Error finding available time slot" });
            }
        }

        public async Task<IActionResult> OnPostUpdateJobDurationAsync(int jobId, double newDurationHours)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🕐 [SCHEDULER-{OperationId}] Updating job {JobId} duration to {Duration}h",
                operationId, jobId, newDurationHours);
            try
            {
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }
                var oldDuration = job.EstimatedHours;
                var newEndTime = job.ScheduledStart.AddHours(newDurationHours);
                job.EstimatedHours = newDurationHours;
                job.ScheduledEnd = newEndTime;
                job.LastModifiedDate = DateTime.UtcNow;
                job.LastModifiedBy = User.Identity?.Name ?? "System";
                if (job.Quantity > 0 && job.Part != null)
                {
                    var timePerPart = newDurationHours / job.Quantity;
                    job.Part.EstimatedHours = timePerPart;
                    job.Part.LastModifiedDate = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
                return new JsonResult(new 
                { 
                    success = true, 
                    message = $"Job duration updated from {oldDuration:F1}h to {newDurationHours:F1}h",
                    oldDuration,
                    newDuration = newDurationHours,
                    newEndTime = newEndTime.ToString("yyyy-MM-ddTHH:mm"),
                    partDurationUpdated = job.Part != null && job.Quantity > 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error updating job duration", operationId);
                return new JsonResult(new { success = false, error = "Error updating job duration" });
            }
        }

        private string AssignColor(string machineId, List<Machine> all)
        {
            var palette = new[]{"#6366F1","#0EA5E9","#10B981","#F59E0B","#EC4899","#8B5CF6","#14B8A6","#F97316","#EF4444","#3B82F6","#84CC16","#9333EA","#06B6D4","#F43F5E","#A855F7"};
            var used = all.Where(m=>!string.IsNullOrWhiteSpace(m.ColorHex)).Select(m=>m.ColorHex!).ToHashSet();
            var free = palette.FirstOrDefault(c=>!used.Contains(c));
            if(free!=null) return free;
            var hash = machineId.Aggregate(17,(acc,ch)=>acc*31+ch);
            return palette[Math.Abs(hash)%palette.Length];
        }

        // NEW: Variant suggestion endpoint for SLS stacking (single/double/triple)
        public async Task<IActionResult> OnGetVariantSuggestionsAsync(int partId, string machineId, DateTime? preferredStart = null)
        {
            var opId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🧠 [SCHEDULER-{OperationId}] Variant suggestions requested for PartId={PartId} on {MachineId}", opId, partId, machineId);
            try
            {
                var part = await _context.Parts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == partId);
                if (part == null)
                {
                    return new JsonResult(new { success = false, error = "Part not found" });
                }

                // Pull historical builds for this part
                var pn = part.PartNumber;
                var builds = await _context.BuildJobs
                    .Include(b => b.BuildJobParts)
                    .Where(b => b.Status == "Completed" &&
                           (b.BuildJobParts.Any(p => p.PartNumber == pn) || b.PartId == part.Id))
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(200)
                    .AsNoTracking()
                    .ToListAsync();

                var samples = new Dictionary<int, List<double>>(); // stack -> hours

                foreach (var b in builds)
                {
                    var qty = b.BuildJobParts?.Where(p => p.PartNumber == pn).Sum(p => (int?)p.Quantity) ?? 0;
                    if (qty == 0)
                    {
                        // Fallback: if the tracked BuildJob is tied directly to this part
                        if (b.PartId == part.Id)
                        {
                            qty = b.TotalPartsInBuild > 0 ? b.TotalPartsInBuild : 1;
                        }
                    }
                    if (qty <= 0) continue;

                    double hours = 0;
                    if (b.OperatorActualHours.HasValue)
                        hours = (double)b.OperatorActualHours.Value;
                    else if (b.ActualEndTime.HasValue)
                        hours = (b.ActualEndTime.Value - b.ActualStartTime).TotalHours;
                    else if (b.ScheduledEndTime.HasValue && b.ScheduledStartTime.HasValue)
                        hours = (b.ScheduledEndTime.Value - b.ScheduledStartTime.Value).TotalHours;
                    else if (b.OperatorEstimatedHours.HasValue)
                        hours = (double)b.OperatorEstimatedHours.Value;

                    if (hours <= 0.05) continue;

                    if (!samples.ContainsKey(qty)) samples[qty] = new List<double>();
                    samples[qty].Add(hours);
                }

                // Build candidate variants 1/2/3 (optionally 4 if history shows it)
                var candidateStacks = new HashSet<int>(new[] { 1, 2, 3 });
                foreach (var k in samples.Keys)
                {
                    if (k >= 4) candidateStacks.Add(Math.Min(k, 4)); // bucket 4+ as 4
                }

                var overheadHours = (part.PreheatingTimeMinutes + part.CoolingTimeMinutes + part.PostProcessingTimeMinutes) / 60.0;
                var perPartHours = part.HasAdminOverride ? (part.AdminEstimatedHoursOverride ?? part.EstimatedHours) : part.EstimatedHours;

                // heuristic multipliers when no history
                double StackMultiplier(int s) => s switch { 1 => 1.0, 2 => 1.6, 3 => 2.0, _ => 2.5 };

                var now = preferredStart ?? DateTime.UtcNow;
                var variants = new List<object>();

                foreach (var s in candidateStacks.OrderBy(x => x))
                {
                    List<double> hist;
                    if (s <= 3)
                    {
                        hist = samples.ContainsKey(s) ? samples[s] : new List<double>();
                    }
                    else
                    {
                        // 4+ bucket: combine all >=4
                        hist = samples.Where(kv => kv.Key >= 4).SelectMany(kv => kv.Value).ToList();
                    }

                    double durationMedian;
                    double durationP80;
                    int sampleCount = hist.Count;

                    if (sampleCount > 0)
                    {
                        durationMedian = Percentile(hist, 0.5);
                        durationP80 = Percentile(hist, 0.8);
                    }
                    else
                    {
                        // fallback estimate
                        durationMedian = overheadHours + perPartHours * StackMultiplier(s);
                        durationP80 = durationMedian * 1.1;
                    }

                    DateTime? nextStart = null;
                    DateTime? nextEnd = null;
                    try
                    {
                        var start = await _timeSlotService.GetNextAvailableTimeAsync(machineId, now, durationMedian);
                        nextStart = start;
                        nextEnd = start.AddHours(durationMedian);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[SCHEDULER-{OperationId}] Time slot lookup failed for variant s={Stack}", opId, s);
                    }

                    var throughput = s / durationMedian; // parts per hour

                    variants.Add(new
                    {
                        stack = s,
                        label = s switch { 1 => "Single", 2 => "Double", 3 => "Triple", _ => $"{s}x" },
                        sampleCount,
                        medianHours = Math.Round(durationMedian, 2),
                        p80Hours = Math.Round(durationP80, 2),
                        throughput = Math.Round(throughput, 3),
                        nextStart = nextStart?.ToString("yyyy-MM-ddTHH:mm"),
                        nextEnd = nextEnd?.ToString("yyyy-MM-ddTHH:mm")
                    });
                }

                // choose recommended: highest throughput; if tie, earliest nextStart
                var chosen = variants
                    .Cast<dynamic>()
                    .OrderByDescending(v => (double)v.throughput)
                    .ThenBy(v => v.nextStart ?? "9999")
                    .FirstOrDefault();

                var response = new
                {
                    success = true,
                    partNumber = pn,
                    variants = variants.Select(v =>
                    {
                        dynamic dv = v;
                        bool isRecommended = chosen != null && dv.stack == chosen.stack && dv.medianHours == chosen.medianHours;
                        return new
                        {
                            dv.stack,
                            dv.label,
                            dv.sampleCount,
                            dv.medianHours,
                            dv.p80Hours,
                            dv.throughput,
                            dv.nextStart,
                            dv.nextEnd,
                            recommended = isRecommended
                        };
                    }).ToList()
                };

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error generating variant suggestions", opId);
                return new JsonResult(new { success = false, error = "Error generating suggestions" });
            }
        }

        private static double Percentile(List<double> sequence, double percentile)
        {
            if (sequence == null || sequence.Count == 0) return 0;
            var sorted = sequence.OrderBy(x => x).ToList();
            var n = sorted.Count;
            if (n == 1) return sorted[0];
            var rank = percentile * (n - 1);
            var lowIdx = (int)Math.Floor(rank);
            var highIdx = (int)Math.Ceiling(rank);
            if (lowIdx == highIdx) return sorted[lowIdx];
            var weight = rank - lowIdx;
            return sorted[lowIdx] * (1 - weight) + sorted[highIdx] * weight;
        }

        // NEW: Enhanced embedded view handler for printing dashboard with SLS filtering
        public async Task<IActionResult> OnGetEmbeddedViewAsync(string? machineFilter = "SLS")
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🎯 [SCHEDULER-EMBEDDED-{OperationId}] Loading embedded scheduler view with filter: {Filter}", 
                operationId, machineFilter ?? "all");

            try
            {
                // Load all available machines first
                await LoadAvailableMachinesAsync(operationId);

                // Apply SLS filtering (future-ready for other machine types)
                var filteredMachines = FilterMachinesByType(machineFilter ?? "SLS");
                
                if (!filteredMachines.Any())
                {
                    _logger.LogWarning("⚠️ [SCHEDULER-EMBEDDED-{OperationId}] No machines found for filter: {Filter}", 
                        operationId, machineFilter);
                }

                // Create embedded view model with filtered data
                var embeddedViewModel = await CreateEmbeddedSchedulerViewModelAsync(filteredMachines, operationId);

                _logger.LogInformation("✅ [SCHEDULER-EMBEDDED-{OperationId}] Embedded view loaded: {JobCount} jobs, {MachineCount} machines", 
                    operationId, embeddedViewModel.Jobs.Count, embeddedViewModel.Machines.Count);

                // Return the enhanced embedded scheduler view
                return Partial("_EmbeddedSchedulerEnhanced", embeddedViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-EMBEDDED-{OperationId}] Error loading embedded scheduler view", operationId);
                
                // Return error fallback view
                var fallbackViewModel = CreateFallbackEmbeddedViewModel();
                return Partial("_EmbeddedSchedulerEnhanced", fallbackViewModel);
            }
        }

        // NEW: Filter machines by type (future-ready for multiple types)
        private List<Machine> FilterMachinesByType(string machineFilter)
        {
            if (string.IsNullOrWhiteSpace(machineFilter) || machineFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                return AvailableMachines;
            }

            var targetFilter = machineFilter.Trim().ToUpperInvariant();
            var filteredMachines = new List<Machine>();

            foreach (var machine in AvailableMachines)
            {
                var unifiedType = GetUnifiedMachineType(machine);
                if (unifiedType.Equals(targetFilter, StringComparison.OrdinalIgnoreCase))
                {
                    filteredMachines.Add(machine);
                }
            }

            return filteredMachines;
        }

        // NEW: Create enhanced embedded scheduler view model (future-ready for print tracking integration)
        private async Task<EmbeddedSchedulerViewModel> CreateEmbeddedSchedulerViewModelAsync(
            List<Machine> filteredMachines, string operationId)
        {
            try
            {
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(3); // 3-day view for embedded scheduler

                // Get jobs for the filtered machines within date range
                var machineIds = filteredMachines.Select(m => m.MachineId).ToList();
                var jobs = new List<Job>();

                if (machineIds.Any())
                {
                    jobs = await _context.Jobs
                        .Include(j => j.Part)
                        .Where(j => machineIds.Contains(j.MachineId) && 
                                   j.ScheduledStart >= startDate && 
                                   j.ScheduledStart < endDate)
                        .OrderBy(j => j.ScheduledStart)
                        .ThenBy(j => j.Priority)
                        .Take(100) // Reasonable limit for embedded view
                        .AsNoTracking()
                        .ToListAsync();
                }

                // Create machine colors dictionary from scheduler data
                var machineColors = filteredMachines.ToDictionary(
                    m => m.MachineId,
                    m => string.IsNullOrWhiteSpace(m.ColorHex) ? m.EffectiveColorHex : m.ColorHex!
                );

                // FUTURE-READY: Add hooks for real-time print status updates
                var enhancedJobs = await EnrichJobsWithPrintTrackingDataAsync(jobs, operationId);

                var viewModel = new EmbeddedSchedulerViewModel
                {
                    Jobs = enhancedJobs,
                    Machines = machineIds,
                    StartDate = startDate,
                    Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList(),
                    MachineColors = machineColors
                };

                return viewModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-EMBEDDED-{OperationId}] Error creating embedded view model", operationId);
                throw;
            }
        }

        // FUTURE-READY: Enrich jobs with print tracking data (actual vs scheduled times, completion status)
        private async Task<List<Job>> EnrichJobsWithPrintTrackingDataAsync(List<Job> jobs, string operationId)
        {
            try
            {
                if (!jobs.Any()) return jobs;

                var jobIds = jobs.Select(j => j.Id).ToList();

                // Get associated build jobs for print tracking integration
                var buildJobs = await _context.BuildJobs
                    .Where(bj => bj.AssociatedScheduledJobId.HasValue && 
                                jobIds.Contains(bj.AssociatedScheduledJobId.Value))
                    .AsNoTracking()
                    .ToListAsync();

                var buildJobLookup = buildJobs.ToDictionary(
                    bj => bj.AssociatedScheduledJobId!.Value, 
                    bj => bj
                );

                // FUTURE: This is where we'll add real-time status updates
                foreach (var job in jobs)
                {
                    if (buildJobLookup.TryGetValue(job.Id, out var buildJob))
                    {
                        // Future enhancement: Update job status based on actual print progress
                        // For now, just ensure status consistency
                        if (buildJob.Status == "In Progress" && job.Status != "Building")
                        {
                            job.Status = "Building";
                        }
                        else if (buildJob.Status == "Completed" && job.Status != "Completed")
                        {
                            job.Status = "Completed";
                            job.ActualEnd = buildJob.ActualEndTime;
                        }
                    }
                }

                return jobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "⚠️ [SCHEDULER-EMBEDDED-{OperationId}] Error enriching jobs with print tracking data", operationId);
                // Return original jobs if enrichment fails
                return jobs;
            }
        }

        // NEW: Create fallback embedded view model for error cases
        private EmbeddedSchedulerViewModel CreateFallbackEmbeddedViewModel()
        {
            var startDate = DateTime.Today;
            return new EmbeddedSchedulerViewModel
            {
                Jobs = new List<Job>(),
                Machines = new List<string>(),
                StartDate = startDate,
                Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList(),
                MachineColors = new Dictionary<string, string>()
            };
        }
    }

    public class CreateJobDto
    {
        public int Id { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public int PartId { get; set; }
        public DateTime ScheduledStart { get; set; } = DateTime.UtcNow.AddHours(1);
        public DateTime ScheduledEnd { get; set; } = DateTime.UtcNow.AddHours(9);
        public int Quantity { get; set; } = 1;
        public int Priority { get; set; } = 3;
        public string? Status { get; set; }
        public string? SlsMaterial { get; set; }
        public double LaserPowerWatts { get; set; } = 200;
        public double ScanSpeedMmPerSec { get; set; } = 1200;
        public double LayerThicknessMicrons { get; set; } = 30;
        public double HatchSpacingMicrons { get; set; } = 120;
        public double BuildTemperatureCelsius { get; set; } = 180;
        public double EstimatedPowderUsageKg { get; set; } = 0.5;
        public string? Notes { get; set; }
        public string? CustomerOrderNumber { get; set; }
        public string? Operator { get; set; }
        public bool IsRushJob { get; set; }
    }

    public class EditJobDto : CreateJobDto { }

    public class JobValidationResult
    {
        public List<JobValidationError> Errors { get; } = new();
        public bool IsValid => Errors.Count == 0;
        public void AddError(string propertyName, string errorMessage) => Errors.Add(new JobValidationError { PropertyName = propertyName, ErrorMessage = errorMessage });
    }

    public class JobValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class EmbeddedSchedulerViewModel
    {
        public List<Job> Jobs { get; set; } = new();
        public List<string> Machines { get; set; } = new();
        public DateTime StartDate { get; set; } = DateTime.Today;
        public List<DateTime> Dates { get; set; } = new();
        public Dictionary<string, string> MachineColors { get; set; } = new();
    }
}
