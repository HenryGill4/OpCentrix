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
    /// </summary>
    [SchedulerAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerService _schedulerService;
        private readonly IMachineManagementService _machineService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<IndexModel> _logger;
        private readonly IOperatingShiftService _shiftService;
        private readonly IScheduleCompressionService _compressionService; // NEW
        // Single authoritative flag (allow end outside shift but still validate start)
        private const bool BYPASS_SHIFT_CHECKS = false;

        public IndexModel(
            SchedulerContext context,
            ISchedulerService schedulerService,
            IMachineManagementService machineService,
            ITimeSlotService timeSlotService,
            ILogger<IndexModel> logger,
            IOperatingShiftService shiftService,
            IScheduleCompressionService compressionService) // NEW
        {
            _context = context;
            _schedulerService = schedulerService;
            _machineService = machineService;
            _timeSlotService = timeSlotService;
            _logger = logger;
            _shiftService = shiftService;
            _compressionService = compressionService; // NEW
        }

        // Display properties - Clean separation
        public SchedulerPageViewModel ViewModel { get; set; } = new();
        public FooterSummaryViewModel Summary { get; set; } = new();
        public List<Machine> AvailableMachines { get; set; } = new();
        public List<Part> AvailableParts { get; set; } = new();
        public List<MasterPart> AvailableMasterParts { get; set; } = new();

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
                await LoadAvailableMasterPartsAsync(operationId);

                return Partial("_AddEditJobModal", new AddEditJobViewModel 
                { 
                    Job = job, 
                    Parts = AvailableParts,
                    MasterParts = AvailableMasterParts,
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
            _logger.LogInformation("🔧 [SCHEDULER-{OperationId}] Processing job: Id={JobId}, MachineId={MachineId}, PartId={PartId}, MasterPartId={MasterPartId}",
                operationId, jobRequest.Id, jobRequest.MachineId, jobRequest.PartId, jobRequest.MasterPartId);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    var formSnapshot = new Dictionary<string, string?>();
                    foreach (var kvp in Request.Form)
                    {
                        if (kvp.Key.Equals("__RequestVerificationToken", StringComparison.OrdinalIgnoreCase))
                            continue;
                        formSnapshot[kvp.Key] = kvp.Value.ToString();
                    }
                    _logger.LogDebug("🧾 [SCHEDULER-{OperationId}] Raw form payload: {Payload}", operationId, System.Text.Json.JsonSerializer.Serialize(formSnapshot));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ [SCHEDULER-{OperationId}] Failed to serialize form payload for debug", operationId);
                }
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                await LoadAvailableMachinesAsync(operationId);
                await LoadAvailablePartsAsync(operationId);
                await LoadAvailableMasterPartsAsync(operationId);

                // Hydrate stacking-related fields (StackLevel, PartsPerBuild, PlannedStackDurationHours) from MasterPart if missing or incomplete
                await HydrateStackFieldsAsync(jobRequest, operationId);

                if (jobRequest.PartId <= 0 && jobRequest.MasterPartId.HasValue)
                {
                    var resolved = await TryResolveLegacyPartAsync(jobRequest, operationId);
                    if (resolved)
                    {
                        _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Resolved legacy PartId {PartId} from MasterPartId {MasterPartId}", operationId, jobRequest.PartId, jobRequest.MasterPartId);
                    }
                }

                // NEW: Auto non-overlapping slot assignment for NEW jobs only
                if (jobRequest.Id == 0 && !string.IsNullOrWhiteSpace(jobRequest.MachineId))
                {
                    double durationHours;
                    if (jobRequest.PlannedStackDurationHours.HasValue && jobRequest.PlannedStackDurationHours.Value > 0)
                    {
                        durationHours = jobRequest.PlannedStackDurationHours.Value;
                    }
                    else
                    {
                        durationHours = (jobRequest.ScheduledEnd - jobRequest.ScheduledStart).TotalHours;
                        if (durationHours <= 0.05 || double.IsNaN(durationHours) || double.IsInfinity(durationHours))
                            durationHours = 8.0; // fallback
                    }
                    var desiredStart = jobRequest.ScheduledStart;
                    var (autoStart, autoEnd) = await FindNextAvailableSlotAsync(jobRequest.MachineId, desiredStart, durationHours, null, operationId);

                    var (shiftStart, shiftEnd) = await AdjustToOperatingShiftsAsync(jobRequest.MachineId, autoStart, durationHours, operationId);
                    if (shiftStart > autoStart)
                    {
                        (autoStart, autoEnd) = await FindNextAvailableSlotAsync(jobRequest.MachineId, shiftStart, durationHours, null, operationId);
                        (autoStart, autoEnd) = await AdjustToOperatingShiftsAsync(jobRequest.MachineId, autoStart, durationHours, operationId);
                    }
                    else
                    {
                        autoStart = shiftStart;
                        autoEnd = shiftEnd;
                    }

                    if (autoStart != desiredStart)
                    {
                        _logger.LogInformation("🕓 [SCHEDULER-{OperationId}] Adjusted start to avoid overlap & shift bounds. Was {OldStart} now {NewStart}", operationId, desiredStart, autoStart);
                    }
                    jobRequest.ScheduledStart = autoStart;
                    if (jobRequest.PlannedStackDurationHours.HasValue)
                    {
                        jobRequest.ScheduledEnd = autoStart.AddHours(jobRequest.PlannedStackDurationHours.Value);
                    }
                    else
                    {
                        jobRequest.ScheduledEnd = autoEnd;
                    }
                }

                var validationResult = await ValidateJobRequestAsync(jobRequest, operationId);
                if (!validationResult.IsValid)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        foreach (var err in validationResult.Errors)
                        {
                            _logger.LogDebug("❌ [SCHEDULER-{OperationId}] Validation error: Field={Field} Message={Message}", operationId, err.PropertyName, err.ErrorMessage);
                        }
                    }

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
                        MasterParts = AvailableMasterParts,
                        Machines = AvailableMachines,
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                }

                Job savedJob;
                if (jobRequest.Id == 0)
                {
                    _logger.LogDebug("🆕 [SCHEDULER-{OperationId}] Creating new job record", operationId);
                    savedJob = await CreateJobFromDtoAsync(jobRequest, operationId);
                }
                else
                {
                    _logger.LogDebug("✏️ [SCHEDULER-{OperationId}] Updating existing job {JobId}", operationId, jobRequest.Id);
                    savedJob = await UpdateJobFromDtoAsync(jobRequest, operationId);
                }

                stopwatch.Stop();
                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job persisted (Id={SavedId}) in {Elapsed}ms", operationId, savedJob.Id, stopwatch.ElapsedMilliseconds);

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
                stopwatch.Stop();
                var operationIdCopy = operationId;
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error processing job after {Elapsed}ms", operationIdCopy, stopwatch.ElapsedMilliseconds);
                try
                {
                    await LoadAvailableMachinesAsync(operationIdCopy);
                    await LoadAvailablePartsAsync(operationIdCopy);
                    await LoadAvailableMasterPartsAsync(operationIdCopy);
                    var errorJob = await ConvertDtoToJobAsync(jobRequest);
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = errorJob,
                        Parts = AvailableParts,
                        MasterParts = AvailableMasterParts,
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

        // Hydrate stack related fields from MasterPart if missing/incomplete
        private async Task HydrateStackFieldsAsync(CreateJobDto dto, string operationId)
        {
            try
            {
                if (!dto.MasterPartId.HasValue || dto.MasterPartId.Value <= 0)
                    return;

                // Use already loaded list where possible to avoid extra query
                MasterPart? mp = AvailableMasterParts.FirstOrDefault(m => m.Id == dto.MasterPartId.Value);
                if (mp == null)
                {
                    mp = await _context.MasterParts.AsNoTracking().FirstOrDefaultAsync(m => m.Id == dto.MasterPartId.Value);
                }
                if (mp == null) return;

                // Determine effective stack level
                if (!dto.StackLevel.HasValue || dto.StackLevel.Value < 1)
                {
                    // If quantity posted, recommend; else default 1
                    dto.StackLevel = (byte)mp.GetRecommendedStackLevel(dto.Quantity > 0 ? dto.Quantity : mp.PartsPerBuildSingle);
                }

                // Parts per build
                if (!dto.PartsPerBuild.HasValue || dto.PartsPerBuild.Value <= 0)
                {
                    var ppb = mp.GetPartsPerBuild(dto.StackLevel.Value);
                    if (ppb.HasValue && ppb.Value > 0)
                        dto.PartsPerBuild = ppb.Value;
                }

                // If quantity not specified (or <=0) fall back to parts per build
                if (dto.Quantity <= 0 && dto.PartsPerBuild.HasValue)
                {
                    dto.Quantity = dto.PartsPerBuild.Value;
                }

                // Duration hours
                if (!dto.PlannedStackDurationHours.HasValue || dto.PlannedStackDurationHours.Value <= 0)
                {
                    var dur = mp.GetStackDuration(dto.StackLevel.Value);
                    if (dur.HasValue && dur.Value > 0)
                        dto.PlannedStackDurationHours = Math.Round(dur.Value, 2);
                }

                // Adjust ScheduledEnd if we now have a stack-based duration
                if (dto.PlannedStackDurationHours.HasValue && dto.PlannedStackDurationHours.Value > 0)
                {
                    dto.ScheduledEnd = dto.ScheduledStart.AddHours(dto.PlannedStackDurationHours.Value);
                }

                _logger.LogDebug("🧪 [SCHEDULER-{OperationId}] Hydrated stack fields -> StackLevel={StackLevel}, PartsPerBuild={PPB}, PlannedDuration={Dur}h", operationId, dto.StackLevel, dto.PartsPerBuild, dto.PlannedStackDurationHours);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ [SCHEDULER-{OperationId}] HydrateStackFieldsAsync failed", operationId);
            }
        }

        // NEW: Find next available non-overlapping slot on a machine
        private async Task<(DateTime start, DateTime end)> FindNextAvailableSlotAsync(string machineId, DateTime desiredStartUtc, double durationHours, int? excludeJobId, string operationId)
        {
            if (durationHours <= 0) durationHours = 0.25; // minimum slice
            // Pull future jobs on this machine (including those currently in progress that extend beyond desired start)
            var futureJobs = await _context.Jobs
                .Where(j => j.MachineId == machineId && j.ScheduledEnd > desiredStartUtc && (excludeJobId == null || j.Id != excludeJobId))
                .OrderBy(j => j.ScheduledStart)
                .Select(j => new { j.ScheduledStart, j.ScheduledEnd })
                .AsNoTracking()
                .ToListAsync();

            var candidateStart = desiredStartUtc;
            if (candidateStart < DateTime.UtcNow.AddMinutes(-5))
            {
                candidateStart = DateTime.UtcNow; // do not schedule far in the past
            }
            var candidateEnd = candidateStart.AddHours(durationHours);

            foreach (var job in futureJobs)
            {
                // If this job ends before our candidate starts, continue
                if (job.ScheduledEnd <= candidateStart)
                    continue;
                // If this job starts after our candidate ends, we have a free gap and can break
                if (job.ScheduledStart >= candidateEnd)
                    break; // gap found
                // Overlap detected -> move candidateStart to end of this job and recalc end
                candidateStart = job.ScheduledEnd;
                candidateEnd = candidateStart.AddHours(durationHours);
            }

            _logger.LogDebug("🧩 [SCHEDULER-{OperationId}] Slot resolution machine={MachineId} desired={Desired} resolvedStart={Start} resolvedEnd={End} duration={Duration}h (futureJobs={Count})", 
                operationId, machineId, desiredStartUtc, candidateStart, candidateEnd, durationHours, futureJobs.Count);
            return (candidateStart, candidateEnd);
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
                return Content("<script>window.showErrorNotification && window.showErrorNotification('Error deleting job');</script>", "text/html");
            }
        }

        private string GetGridRefreshScript(string message)
        {
            // Updated: Use unified overlay helpers so it appears as soon as modal closes
            // and hides ONLY after both grid + summary partials finish rendering.
            return $@"<script>(function(){{
                const overlay = document.getElementById('loading-indicator');
                const modal = document.getElementById('modal-container');
                // Close modal immediately
                if(modal){{ modal.style.display='none'; modal.classList.add('hidden'); modal.innerHTML=''; }}
                document.body.style.overflow='';
                // Toast / notification
                if(window.showSuccessNotification){{ window.showSuccessNotification('{message}'); }}
                // Show overlay using new API (falls back if helper missing)
                if(window.showSchedulerOverlay){{ window.showSchedulerOverlay('Refreshing schedule...'); }}
                else if(overlay){{ overlay.classList.add('active'); overlay.removeAttribute('hidden'); }}
                const qs = window.location.search;
                const refresh = () => {{
                    if(!window.htmx){{
                        // Full reload fallback
                        window.location.reload();
                        return;
                    }}
                    const gridReq = htmx.ajax('GET','/Scheduler?handler=RefreshGrid'+(qs?qs.replace('?','&'):'') ,{{ target:'#scheduler-main-content', swap:'innerHTML' }});
                    const summaryReq = htmx.ajax('GET','/Scheduler?handler=RefreshSummary'+(qs?qs.replace('?','&'):'') ,{{ target:'#footer-summary', swap:'innerHTML' }});
                    Promise.all([gridReq, summaryReq])
                        .then(()=>{{ if(window.hideSchedulerOverlay) window.hideSchedulerOverlay(); else if(overlay) overlay.classList.remove('active'); }})
                        .catch(()=>{{ if(window.hideSchedulerOverlay) window.hideSchedulerOverlay(); else if(overlay) overlay.classList.remove('active'); }});
                }};
                // Small delay to mitigate layout flash before content swap
                setTimeout(refresh, 150);
            }})();</script>";
        }

        // ================== NEW: Batch delete handler ==================
        public async Task<IActionResult> OnPostDeleteJobsAsync(string jobIds)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [SCHEDULER-{OperationId}] Batch delete request: {Ids}", operationId, jobIds);
            if (string.IsNullOrWhiteSpace(jobIds))
            {
                return Content("<script>window.showErrorNotification && window.showErrorNotification('No jobs selected');</script>", "text/html");
            }
            try
            {
                var ids = jobIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var v) ? v : 0)
                    .Where(v => v > 0)
                    .Distinct()
                    .ToList();
                if (ids.Count == 0)
                {
                    return Content("<script>window.showErrorNotification && window.showErrorNotification('No valid job ids');</script>", "text/html");
                }
                var jobs = await _context.Jobs.Where(j => ids.Contains(j.Id)).ToListAsync();
                if (jobs.Count == 0)
                {
                    return Content("<script>window.showErrorNotification && window.showErrorNotification('Jobs not found');</script>", "text/html");
                }
                _context.Jobs.RemoveRange(jobs);
                await _context.SaveChangesAsync();
                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Deleted {Count} jobs (ids: {Ids})", operationId, jobs.Count, string.Join(',', jobs.Select(j=>j.Id)));
                var script = GetGridRefreshScript($"Deleted {jobs.Count} job(s)");
                return Content(script, "text/html");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Batch delete failed", operationId);
                return Content("<script>window.showErrorNotification && window.showErrorNotification('Batch delete failed');</script>", "text/html");
            }
        }
        // ================== END NEW ==================

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
                // Flag for UI broadcast / client polling
                TempData["PrintStartedJobId"] = job.Id;
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

        private async Task LoadAvailableMasterPartsAsync(string operationId)
        {
            try
            {
                AvailableMasterParts = await _context.MasterParts
                    .Where(mp => mp.IsActive)
                    .OrderBy(mp => mp.PartNumber)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading master parts", operationId);
                AvailableMasterParts = new List<MasterPart>();
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
            AvailableMasterParts = new List<MasterPart>();
        }

        /// <summary>
        /// Create a new job with defaulted start time logic:
        /// 1. Base = last job end on machine (any status) OR now, whichever is later
        /// 2. Add 3 hour changeover buffer (operator setup)
        /// 3. If user clicked future slot that is later, prefer that
        /// 4. Align forward into an operating shift. If alignment lands inside a shift BEFORE its 3h setup window is satisfied (shiftStart + 3h), push to shiftStart + 3h.
        /// 5. Round to nearest 15 minutes (floor)
        /// </summary>
        private async Task<Job> CreateNewJobAsync(string machineId, DateTime requestedStart, string operationId)
        {
            const double setupBufferHours = 3.0; // required operator changeover inside a shift
            const double defaultDurationHours = 8.0;
            try
            {
                var nowUtc = DateTime.UtcNow;

                // 1 + 2: base candidate from last job + setup buffer
                var lastJobEnd = await _context.Jobs
                    .Where(j => j.MachineId == machineId)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .Select(j => (DateTime?)j.ScheduledEnd)
                    .FirstOrDefaultAsync();

                DateTime candidate = (lastJobEnd.HasValue && lastJobEnd.Value > nowUtc) ? lastJobEnd.Value : nowUtc;
                candidate = candidate.AddHours(setupBufferHours);

                // 3: honor user click if later
                if (requestedStart > candidate)
                    candidate = requestedStart;

                if (candidate < nowUtc)
                    candidate = nowUtc.AddMinutes(5);

                // 4: shift alignment with setup requirement relative to shift start
                // We will iterate (safety cap) advancing in 15 min increments until inside an acceptable shift window.
                int guard = 0;
                while (guard < 96) // up to 24h search
                {
                    guard++;
                    bool insideAnyShift = false;
                    DateTime? shiftStartForCandidate = null;
                    DateTime? shiftEndForCandidate = null;

                    // Fetch shifts for candidate day and previous day (to catch cross-midnight shifts)
                    var dayShifts = await _shiftService.GetShiftsForDayAsync(candidate.DayOfWeek, machineId) ?? new List<OperatingShift>();
                    var prevDayShifts = await _shiftService.GetShiftsForDayAsync(candidate.AddDays(-1).DayOfWeek, machineId) ?? new List<OperatingShift>();

                    IEnumerable<(DateTime start, DateTime end)> materialized = EnumerateShiftWindows(candidate.Date, dayShifts)
                        .Concat(EnumerateShiftWindows(candidate.AddDays(-1).Date, prevDayShifts));

                    foreach (var (sStart, sEnd) in materialized.OrderBy(w => w.start))
                    {
                        // If candidate before this shift window start, jump to its start (still need setup buffer afterwards)
                        if (candidate < sStart)
                        {
                            candidate = sStart; // move to shift start then apply buffer rule below
                        }
                        if (candidate >= sStart && candidate < sEnd)
                        {
                            insideAnyShift = true;
                            shiftStartForCandidate = sStart;
                            shiftEndForCandidate = sEnd;
                            break;
                        }
                    }

                    if (!insideAnyShift)
                    {
                        // Advance 15 minutes and continue searching
                        candidate = candidate.AddMinutes(15);
                        continue;
                    }

                    // Enforce setup buffer AFTER shift start
                    var minOperationalStart = shiftStartForCandidate.Value.AddHours(setupBufferHours);
                    if (candidate < minOperationalStart)
                    {
                        candidate = minOperationalStart;
                        // If pushing past shift end, we need to move to next shift, so continue loop
                        if (candidate >= shiftEndForCandidate.Value)
                        {
                            continue;
                        }
                    }

                    // We are inside a shift and after setup buffer; exit loop
                    break;
                }

                // 5: floor to nearest 15 minutes
                candidate = new DateTime(candidate.Year, candidate.Month, candidate.Day, candidate.Hour, candidate.Minute - (candidate.Minute % 15), 0, DateTimeKind.Utc);

                return new Job
                {
                    MachineId = machineId,
                    ScheduledStart = candidate,
                    ScheduledEnd = candidate.AddHours(defaultDurationHours),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000",
                    EstimatedHours = defaultDurationHours,
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    CustomerOrderNumber = string.Empty,
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
                _logger.LogError(ex, "[SCHEDULER-{OperationId}] Failed default start computation (fallback applied)", operationId);
                var fallback = requestedStart < DateTime.UtcNow ? DateTime.UtcNow.AddHours(1) : requestedStart;
                return new Job
                {
                    MachineId = machineId,
                    ScheduledStart = fallback,
                    ScheduledEnd = fallback.AddHours(defaultDurationHours),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000",
                    EstimatedHours = defaultDurationHours,
                    SlsMaterial = "Ti-6Al-4V Grade 5",
                    CustomerOrderNumber = string.Empty
                };
            }

            // Local iterator to expand shift definitions into absolute windows
            IEnumerable<(DateTime start, DateTime end)> EnumerateShiftWindows(DateTime day, IEnumerable<OperatingShift> shifts)
            {
                foreach (var sh in shifts)
                {
                    var start = day + sh.StartTime;
                    var end = day + sh.EndTime;
                    if (sh.EndTime < sh.StartTime) // crosses midnight
                        end = end.AddDays(1);
                    yield return (start, end);
                }
            }
        }

        private async Task<JobValidationResult> ValidateJobRequestAsync(CreateJobDto request, string operationId)
        {
            var result = new JobValidationResult();

            // Scheduler now operates ONLY with Master Parts. MasterPartId is required.
            if (!request.MasterPartId.HasValue || request.MasterPartId.Value <= 0)
            {
                result.AddError(nameof(request.MasterPartId), "Master part must be selected");
                return result; // no need to continue if missing
            }

            // Attempt to resolve legacy part mapping (still required by current Job schema) BEFORE enforcing PartId error.
            if (request.PartId <= 0)
            {
                var resolved = await TryResolveLegacyPartAsync(request, operationId);
                if (!resolved || request.PartId <= 0)
                {
                    result.AddError(nameof(request.PartId), "Selected master part has no legacy part mapping (Part record not found).");
                }
            }

            if (string.IsNullOrWhiteSpace(request.MachineId)) result.AddError(nameof(request.MachineId), "Machine must be selected");
            if (request.ScheduledStart >= request.ScheduledEnd && !request.PlannedStackDurationHours.HasValue) result.AddError(nameof(request.ScheduledEnd), "End time must be after start time");
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
            if (!request.PlannedStackDurationHours.HasValue)
            {
                if (duration.TotalHours > 168) result.AddError(nameof(request.ScheduledEnd), "Job duration cannot exceed 1 week");
                if (duration.TotalMinutes < 15) result.AddError(nameof(request.ScheduledEnd), "Job duration must be at least 15 minutes");
            }
            // Operating shift validation (machine-aware)
            if (!BYPASS_SHIFT_CHECKS && string.IsNullOrWhiteSpace(request.MachineId) == false && result.IsValid)
            {
                try
                {
                    var startOk = await _shiftService.IsTimeWithinOperatingHoursAsync(request.ScheduledStart, request.MachineId);
                    if (!startOk)
                        result.AddError(nameof(request.ScheduledStart), "Start time is outside active operating shifts");
                    // Allow end outside shift: no error, optional future warning handled client-side
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[SCHEDULER-{OperationId}] Shift validation failed (continuing)", operationId);
                }
            }
            return result;
        }

        // UPDATED: Shift analysis now focuses only on end-of-run alignment (idle before staffed shift) + setup buffer
        public async Task<IActionResult> OnGetShiftAnalysisAsync(string machineId, DateTime start, double durationHours)
        {
            var opId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                if (string.IsNullOrWhiteSpace(machineId) || durationHours <= 0)
                    return new JsonResult(new { success = false, error = "Machine and positive duration required" });

                var end = start.AddHours(durationHours);
                var setupWindowStart = start.AddHours(-3);
                var setupWindowEnd = start; // exclusive

                async Task<double> GetShiftCoveredHoursAsync(DateTime windowStart, DateTime windowEnd)
                {
                    double covered = 0;
                    var cursor = windowStart.Date.AddDays(-1); // include previous day for cross-midnight shifts
                    var lastDay = windowEnd.Date.AddDays(1);
                    while (cursor <= lastDay)
                    {
                        try
                        {
                            var shifts = await _shiftService.GetShiftsForDayAsync(cursor.DayOfWeek, machineId) ?? new List<OperatingShift>();
                            foreach (var s in shifts)
                            {
                                var ws = cursor + s.StartTime;
                                var crosses = s.EndTime < s.StartTime;
                                var we = crosses ? cursor.AddDays(1) + s.EndTime : cursor + s.EndTime;
                                var ovStart = ws > windowStart ? ws : windowStart;
                                var ovEnd = we < windowEnd ? we : windowEnd;
                                if (ovEnd > ovStart)
                                    covered += (ovEnd - ovStart).TotalHours;
                            }
                        }
                        catch { }
                        cursor = cursor.AddDays(1);
                    }
                    return covered;
                }

                // We still compute setup coverage; runtime coverage kept for potential later metrics but not shown in message
                var setupCovered = await GetShiftCoveredHoursAsync(setupWindowStart, setupWindowEnd);
                var setupMissing = Math.Max(0, 3.0 - setupCovered);
                bool setupSatisfied = setupMissing <= 0.01;

                // Determine end alignment vs next staffed shift
                double postRunIdleHours = 0;
                DateTime? nextShiftStart = null;
                bool endsInsideShift = false;

                DateTime searchCursor = end.Date.AddDays(-1);
                var searchLimit = end.Date.AddDays(5);
                while (searchCursor <= searchLimit && nextShiftStart == null)
                {
                    List<OperatingShift>? shifts;
                    try { shifts = await _shiftService.GetShiftsForDayAsync(searchCursor.DayOfWeek, machineId); } catch { shifts = null; }
                    if (shifts != null)
                    {
                        foreach (var s in shifts)
                        {
                            var ws = searchCursor + s.StartTime;
                            var crosses = s.EndTime < s.StartTime;
                            var we = crosses ? searchCursor.AddDays(1) + s.EndTime : searchCursor + s.EndTime;
                            if (end >= ws && end < we)
                            {
                                endsInsideShift = true;
                                nextShiftStart = end; // no idle
                                postRunIdleHours = 0;
                                break;
                            }
                            if (end < ws && nextShiftStart == null)
                            {
                                nextShiftStart = ws;
                                postRunIdleHours = (ws - end).TotalHours;
                                break;
                            }
                        }
                    }
                    searchCursor = searchCursor.AddDays(1);
                }
                if (nextShiftStart == null)
                {
                    nextShiftStart = end; // fail-open
                    postRunIdleHours = 0;
                }

                // User-facing concise message (no runtime outside staffed details)
                string message = endsInsideShift
                    ? (setupSatisfied ? "Ends during staffed shift – 3h setup satisfied" : $"Ends during staffed shift – setup short {setupMissing:F1}h")
                    : (setupSatisfied
                        ? $"Ends {postRunIdleHours:F1}h before next staffed shift (idle window)"
                        : $"Ends {postRunIdleHours:F1}h before next staffed shift; setup short {setupMissing:F1}h");

                return new JsonResult(new
                {
                    success = true,
                    machineId,
                    startUtc = start.ToUniversalTime(),
                    endUtc = end.ToUniversalTime(),
                    postRunIdleHours = Math.Round(postRunIdleHours, 2),
                    nextShiftStartUtc = nextShiftStart.Value.ToUniversalTime(),
                    endsInsideShift,
                    setupSatisfied,
                    setupMissingHours = Math.Round(setupMissing, 2),
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SCHEDULER-{OperationId}] Shift analysis failed", opId);
                return new JsonResult(new { success = false, error = "Shift analysis failed" });
            }
        }

        public async Task<IActionResult> OnGetEmbeddedViewAsync(string? machineFilter = "SLS")
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                await LoadAvailableMachinesAsync(operationId);
                var filtered = FilterMachinesByType(machineFilter ?? "SLS");
                var vm = await CreateEmbeddedSchedulerViewModelAsync(filtered, operationId);
                return Partial("_EmbeddedSchedulerEnhanced", vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-EMBEDDED-{OperationId}] Error loading embedded scheduler view", operationId);
                return Partial("_EmbeddedSchedulerEnhanced", CreateFallbackEmbeddedViewModel());
            }
        }

        private List<Machine> FilterMachinesByType(string machineFilter)
        {
            if (string.IsNullOrWhiteSpace(machineFilter) || machineFilter.Equals("all", StringComparison.OrdinalIgnoreCase))
                return AvailableMachines;
            var target = machineFilter.Trim().ToUpperInvariant();
            return AvailableMachines.Where(m => GetUnifiedMachineType(m).Equals(target, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private async Task<EmbeddedSchedulerViewModel> CreateEmbeddedSchedulerViewModelAsync(List<Machine> machines, string operationId)
        {
            var startDate = DateTime.Today;
            var endDate = startDate.AddDays(3);
            var ids = machines.Select(m => m.MachineId).ToList();
            var jobs = await _context.Jobs.Include(j => j.Part)
                .Where(j => ids.Contains(j.MachineId) && j.ScheduledStart >= startDate && j.ScheduledStart < endDate)
                .OrderBy(j => j.ScheduledStart)
                .Take(100)
                .AsNoTracking()
                .ToListAsync();
            var vm = new EmbeddedSchedulerViewModel
            {
                Machines = ids,
                Jobs = jobs,
                StartDate = startDate,
                Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList(),
                MachineColors = machines.ToDictionary(m => m.MachineId, m => string.IsNullOrWhiteSpace(m.ColorHex) ? m.EffectiveColorHex : m.ColorHex!)
            };
            return vm;
        }

        private EmbeddedSchedulerViewModel CreateFallbackEmbeddedViewModel() => new()
        {
            StartDate = DateTime.Today,
            Dates = Enumerable.Range(0, 3).Select(i => DateTime.Today.AddDays(i)).ToList()
        };

        // DTOs and helper classes
        public class CreateJobDto
        {
            public int Id { get; set; }
            public string MachineId { get; set; } = string.Empty;
            public int PartId { get; set; }
            public int? MasterPartId { get; set; }
            public byte? StackLevel { get; set; }
            public int? PartsPerBuild { get; set; }
            public double? PlannedStackDurationHours { get; set; }
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
        // ===== Added back missing helper methods =====
        private string AssignColor(string machineId, List<Machine> all)
        {
            var palette = new[] {"#6366F1","#0EA5E9","#10B981","#F59E0B","#EC4899","#8B5CF6","#14B8A6","#F97316","#EF4444","#3B82F6","#84CC16","#9333EA","#06B6D4","#F43F5E","#A855F7"};
            var used = all.Where(m => !string.IsNullOrWhiteSpace(m.ColorHex)).Select(m => m.ColorHex!).ToHashSet();
            var free = palette.FirstOrDefault(c => !used.Contains(c));
            if (free != null) return free;
            var hash = machineId.Aggregate(17, (acc, ch) => acc * 31 + ch);
            return palette[Math.Abs(hash) % palette.Length];
        }

        private async Task<(DateTime start, DateTime end)> AdjustToOperatingShiftsAsync(string machineId, DateTime desiredStartUtc, double durationHours, string operationId)
        {
            if (durationHours <= 0) durationHours = 0.25;
            var attempt = desiredStartUtc;
            for (int dayOffset = 0; dayOffset < 14; dayOffset++)
            {
                var day = attempt.Date;
                try
                {
                    var shifts = await _shiftService.GetShiftsForDayAsync(day.DayOfWeek, machineId);
                    if (shifts == null || shifts.Count == 0)
                    {
                        attempt = day.AddDays(1).AddHours(6); // skip to next day 6AM
                        continue;
                    }
                    foreach (var shift in shifts.OrderBy(s => s.StartTime))
                    {
                        var windowStart = day + shift.StartTime;
                        var crosses = shift.EndTime < shift.StartTime;
                        var windowEnd = crosses ? day.AddDays(1) + shift.EndTime : day + shift.EndTime;
                        if (attempt < windowStart) attempt = windowStart;
                        if (attempt >= windowStart && attempt < windowEnd)
                        {
                            var end = attempt.AddHours(durationHours);
                            return (attempt, end);
                        }
                    }
                    // after all shifts, move to next day 6AM
                    attempt = day.AddDays(1).AddHours(6);
                }
                catch
                {
                    // Fail-open: return original request
                    return (desiredStartUtc, desiredStartUtc.AddHours(durationHours));
                }
            }
            _logger.LogWarning("[SCHEDULER-{OperationId}] Could not align start within 14 days of shifts, using desired", operationId);
            return (desiredStartUtc, desiredStartUtc.AddHours(durationHours));
        }

        private async Task<bool> TryResolveLegacyPartAsync(CreateJobDto dto, string operationId)
        {
            try
            {
                if (!dto.MasterPartId.HasValue) return false;
                var master = await _context.MasterParts.AsNoTracking().FirstOrDefaultAsync(mp => mp.Id == dto.MasterPartId.Value);
                if (master == null) return false;
                var pn = master.PartNumber?.Trim();
                if (string.IsNullOrWhiteSpace(pn)) return false;
                // direct match
                var legacy = await _context.Parts.AsNoTracking().FirstOrDefaultAsync(p => p.PartNumber == pn);
                if (legacy == null)
                {
                    var digits = new string(pn.Where(char.IsDigit).ToArray());
                    if (digits.Length == 6)
                    {
                        var formatted = digits.Substring(0, 2) + "-" + digits.Substring(2);
                        legacy = await _context.Parts.AsNoTracking().FirstOrDefaultAsync(p => p.PartNumber == formatted);
                    }
                }
                if (legacy != null)
                {
                    dto.PartId = legacy.Id;
                    if (dto.PlannedStackDurationHours.HasValue)
                        dto.ScheduledEnd = dto.ScheduledStart.AddHours(dto.PlannedStackDurationHours.Value);
                    return true;
                }
                // Create shadow part
                var shadow = new Part
                {
                    PartNumber = pn,
                    Name = master.Name,
                    Description = string.IsNullOrWhiteSpace(master.Description) ? master.Name : master.Description,
                    Material = master.Material,
                    SlsMaterial = master.Material,
                    EstimatedHours = master.SingleStackDurationHours ?? master.StageEstimateSingle ?? 8.0,
                    AdminOverrideBy = "System",
                    Industry = "General",
                    Application = "MasterPartBridge",
                    CustomerPartNumber = pn,
                    PartCategory = "MasterBridge",
                    PartClass = "B",
                    Dimensions = string.Empty,
                    BuildFileTemplate = string.Empty,
                    CadFilePath = string.Empty,
                    CadFileVersion = "v1",
                    CreatedBy = User.Identity?.Name ?? "System",
                    LastModifiedBy = User.Identity?.Name ?? "System",
                    WorkflowTemplate = "MasterPart_Auto",
                    AvgDuration = "8h 0m",
                    IsLegacyForm = false,
                    AllowStacking = master.AllowStacking,
                    SingleStackDurationHours = master.SingleStackDurationHours,
                    DoubleStackDurationHours = master.DoubleStackDurationHours,
                    TripleStackDurationHours = master.TripleStackDurationHours
                };
                _context.Parts.Add(shadow);
                await _context.SaveChangesAsync();
                dto.PartId = shadow.Id;
                if (dto.PlannedStackDurationHours.HasValue)
                    dto.ScheduledEnd = dto.ScheduledStart.AddHours(dto.PlannedStackDurationHours.Value);
                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Shadow Part created Id={PartId} for MasterPart {MasterPartId}", operationId, shadow.Id, dto.MasterPartId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error resolving legacy part from master part {MasterPartId}", operationId, dto.MasterPartId);
                return false;
            }
        }

        private async Task<Job> CreateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            if (dto.PartId <= 0 && dto.MasterPartId.HasValue)
                await TryResolveLegacyPartAsync(dto, operationId);
            var part = await _context.Parts.FindAsync(dto.PartId) ?? throw new InvalidOperationException("Selected part not found");
            var job = new Job
            {
                MachineId = dto.MachineId,
                PartId = dto.PartId,
                PartNumber = part.PartNumber,
                ScheduledStart = dto.ScheduledStart,
                ScheduledEnd = dto.PlannedStackDurationHours.HasValue ? dto.ScheduledStart.AddHours(dto.PlannedStackDurationHours.Value) : dto.ScheduledEnd,
                EstimatedHours = dto.PlannedStackDurationHours ?? (dto.ScheduledEnd - dto.ScheduledStart).TotalHours,
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
                CustomerOrderNumber = dto.CustomerOrderNumber ?? string.Empty,
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob,
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
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? "System",
                LastModifiedBy = User.Identity?.Name ?? "System",
                MasterPartId = dto.MasterPartId,
                StackLevel = dto.StackLevel,
                PartsPerBuild = dto.PartsPerBuild,
                PlannedStackDurationHours = dto.PlannedStackDurationHours
            };
            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        private async Task<Job> UpdateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            if (dto.PartId <= 0 && dto.MasterPartId.HasValue)
                await TryResolveLegacyPartAsync(dto, operationId);
            var job = await _context.Jobs.FindAsync(dto.Id) ?? throw new InvalidOperationException("Job not found for update");
            var part = await _context.Parts.FindAsync(dto.PartId) ?? throw new InvalidOperationException("Selected part not found");
            job.MachineId = dto.MachineId;
            job.PartId = dto.PartId;
            job.PartNumber = part.PartNumber;
            job.ScheduledStart = dto.ScheduledStart;
            if (dto.PlannedStackDurationHours.HasValue)
            {
                job.ScheduledEnd = dto.ScheduledStart.AddHours(dto.PlannedStackDurationHours.Value);
                job.EstimatedHours = dto.PlannedStackDurationHours.Value;
            }
            else
            {
                job.ScheduledEnd = dto.ScheduledEnd;
                job.EstimatedHours = (dto.ScheduledEnd - dto.ScheduledStart).TotalHours;
            }
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
            job.CustomerOrderNumber = dto.CustomerOrderNumber ?? string.Empty;
            job.Operator = dto.Operator;
            job.IsRushJob = dto.IsRushJob;
            if (job.PartId != dto.PartId)
            {
                job.MaterialCostPerKg = part.MaterialCostPerKg;
                job.LaborCostPerHour = part.StandardLaborCostPerHour;
                job.MachineOperatingCostPerHour = part.MachineOperatingCostPerHour;
                job.ArgonCostPerHour = part.ArgonCostPerHour;
                job.PreheatingTimeMinutes = part.PreheatingTimeMinutes;
                job.CoolingTimeMinutes = part.CoolingTimeMinutes;
                job.PostProcessingTimeMinutes = part.PostProcessingTimeMinutes;
            }
            job.MasterPartId = dto.MasterPartId;
            job.StackLevel = dto.StackLevel;
            job.PartsPerBuild = dto.PartsPerBuild;
            job.PlannedStackDurationHours = dto.PlannedStackDurationHours;
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
                CustomerOrderNumber = dto.CustomerOrderNumber ?? string.Empty,
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob,
                MasterPartId = dto.MasterPartId,
                StackLevel = dto.StackLevel,
                PartsPerBuild = dto.PartsPerBuild,
                PlannedStackDurationHours = dto.PlannedStackDurationHours
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
            await LoadAvailableMasterPartsAsync(operationId);
            var errorJob = await CreateNewJobAsync(machineId, startDate, operationId);
            return Partial("_AddEditJobModal", new AddEditJobViewModel
            {
                Job = errorJob,
                Parts = AvailableParts,
                MasterParts = AvailableMasterParts,
                Machines = AvailableMachines,
                Errors = new List<string> { errorMessage }
            });
        }

        private async Task<IActionResult> HandleSchedulerSuccess(string message)
        {
            var script = GetGridRefreshScript(message);
            return Content(script, "text/html");
        }
        // ===== end helper methods =====

        // NEW: Compress machine schedule handler (MVP)
        public async Task<IActionResult> OnPostCompressMachineAsync(string machineId)
        {
            var opId = Guid.NewGuid().ToString("N")[..8];
            try
            {
                if (string.IsNullOrWhiteSpace(machineId))
                    return new JsonResult(new { success = false, error = "Machine id required" });

                var machineExists = await _machineService.GetMachineByMachineIdAsync(machineId) != null;
                if (!machineExists)
                    return new JsonResult(new { success = false, error = "Machine not found" });

                var userName = User?.Identity?.Name ?? "System";
                var compression = await _compressionService.CompressMachineAsync(machineId, DateTime.UtcNow, null, null, userName);

                return new JsonResult(new
                {
                    success = compression.JobsMoved >= 0,
                    machineId,
                    moved = compression.JobsMoved,
                    considered = compression.JobsConsidered,
                    pulledMinutes = (int)compression.TotalMinutesPulledForward,
                    changedIds = compression.ChangedIds.ToArray(),
                    warnings = compression.Warnings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[COMPRESS-ERR] Compression failure for machine {MachineId}", machineId);
                return new JsonResult(new { success = false, error = "Compression failed" });
            }
        }
    }
}
