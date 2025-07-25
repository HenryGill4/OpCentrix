using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;
using OpCentrix.Services;
using OpCentrix.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Pages.Scheduler
{
    [SchedulerAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerService _schedulerService;
        private readonly ILogger<IndexModel> _logger;

        public SchedulerPageViewModel ViewModel { get; set; } = new();
        public FooterSummaryViewModel Summary { get; set; } = new();

        public IndexModel(SchedulerContext context, ISchedulerService schedulerService, ILogger<IndexModel> logger)
        {
            _context = context;
            _schedulerService = schedulerService;
            _logger = logger;
        }

        public async Task OnGetAsync(string? zoom = null, DateTime? startDate = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("?? [SCHEDULER-{OperationId}] Starting OnGetAsync with zoom: {Zoom}, startDate: {StartDate}",
                operationId, zoom, startDate);

            try
            {
                // Get scheduler configuration with optimized date range
                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Getting scheduler data from service", operationId);
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);

                // PERFORMANCE OPTIMIZATION: Load only jobs within visible date range + buffer
                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Loading optimized job data", operationId);
                await LoadOptimizedJobDataAsync(operationId);

                // Calculate machine row heights efficiently
                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Calculating machine row heights", operationId);
                CalculateMachineRowHeights(operationId);

                // Generate efficient summary data
                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Generating optimized summary", operationId);
                await GenerateOptimizedSummaryAsync(operationId);

                _logger.LogInformation("? [SCHEDULER-{OperationId}] Scheduler loaded successfully: {JobCount} jobs across {MachineCount} machines",
                    operationId, ViewModel.Jobs.Count, ViewModel.Machines.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Critical error loading scheduler page: {ErrorMessage}",
                    operationId, ex.Message);
                await HandleLoadErrorAsync(zoom, startDate, operationId, ex);
            }
        }

        public async Task<IActionResult> OnGetEmbeddedViewAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Loading embedded scheduler view for admin panel", operationId);

            try
            {
                // Load current jobs for today and next 2 days (mini view)
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(3);

                _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Querying jobs from {StartDate} to {EndDate}",
                    operationId, startDate, endDate);

                var jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.ScheduledStart < endDate && j.ScheduledEnd > startDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ThenBy(j => j.MachineId)
                    .AsNoTracking()
                    .Take(20) // Limit for mini view
                    .ToListAsync();

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Found {JobCount} jobs for embedded view", operationId, jobs.Count);

                // Get available machines
                var machines = await _context.SlsMachines
                    .Where(m => m.IsActive)
                    .Select(m => m.MachineId)
                    .OrderBy(m => m)
                    .ToListAsync();

                // Fallback to default machines if none in database
                if (!machines.Any())
                {
                    machines = new List<string> { "TI1", "TI2", "INC" };
                    _logger.LogWarning("🔧 [SCHEDULER-{OperationId}] No machines found in database, using defaults", operationId);
                }

                // Create proper EmbeddedSchedulerViewModel instead of anonymous object
                var miniViewModel = new EmbeddedSchedulerViewModel
                {
                    Jobs = jobs ?? new List<Job>(),
                    Machines = machines,
                    Dates = Enumerable.Range(0, 3).Select(i => startDate.AddDays(i)).ToList(),
                    StartDate = startDate
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Returning embedded view with {JobCount} jobs and {MachineCount} machines",
                    operationId, miniViewModel.Jobs.Count, miniViewModel.Machines.Count);

                return Partial("_EmbeddedScheduler", miniViewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading embedded scheduler view: {ErrorMessage}",
                    operationId, ex.Message);

                // Return a robust fallback view with error details using proper model
                var errorViewModel = new EmbeddedSchedulerViewModel
                {
                    Jobs = new List<Job>(),
                    Machines = new List<string> { "TI1", "TI2", "INC" },
                    Dates = Enumerable.Range(0, 3).Select(i => DateTime.Today.AddDays(i)).ToList(),
                    StartDate = DateTime.Today
                };

                return Partial("_EmbeddedScheduler", errorViewModel);
            }
        }

        private async Task LoadOptimizedJobDataAsync(string operationId)
        {
            try
            {
                // Calculate optimal date range for loading with buffer
                var bufferDays = 1; // Small buffer for edge cases
                var queryStartDate = ViewModel.StartDate.AddDays(-bufferDays);
                var queryEndDate = ViewModel.StartDate.AddDays(ViewModel.Dates.Count + bufferDays);

                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Querying jobs from {QueryStart} to {QueryEnd}",
                    operationId, queryStartDate, queryEndDate);

                // PERFORMANCE: Load only jobs within visible timeframe
                var jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.ScheduledStart < queryEndDate && j.ScheduledEnd > queryStartDate)
                    .OrderBy(j => j.ScheduledStart)
                    .ThenBy(j => j.MachineId)
                    .AsNoTracking() // PERFORMANCE: Read-only for display
                    .ToListAsync();

                ViewModel.Jobs = jobs;
                _logger.LogDebug("? [SCHEDULER-{OperationId}] Loaded {JobCount} jobs within date range", operationId, jobs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error loading optimized job data: {ErrorMessage}",
                    operationId, ex.Message);

                // Fallback to empty job list
                ViewModel.Jobs = new List<Job>();
                throw; // Re-throw to be handled by caller
            }
        }

        private void CalculateMachineRowHeights(string operationId)
        {
            try
            {
                foreach (var machine in ViewModel.Machines)
                {
                    var machineJobs = ViewModel.Jobs.Where(j => j.MachineId == machine).ToList();
                    var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machine, machineJobs);
                    ViewModel.MachineRowHeights[machine] = rowHeight;

                    _logger.LogDebug("?? [SCHEDULER-{OperationId}] Machine {Machine}: {JobCount} jobs, height {Height}px",
                        operationId, machine, machineJobs.Count, rowHeight);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error calculating machine row heights: {ErrorMessage}",
                    operationId, ex.Message);

                // Fallback to default heights
                foreach (var machine in ViewModel.Machines)
                {
                    ViewModel.MachineRowHeights[machine] = 160; // Default height
                }
                throw; // Re-throw to be handled by caller
            }
        }

        private async Task GenerateOptimizedSummaryAsync(string operationId)
        {
            try
            {
                // PERFORMANCE: Calculate summary from already loaded jobs instead of new query
                Summary = new FooterSummaryViewModel
                {
                    MachineHours = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => ViewModel.Jobs.Where(j => j.MachineId == m).Sum(j => j.DurationHours)
                    ),
                    JobCounts = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => ViewModel.Jobs.Count(j => j.MachineId == m)
                    )
                };

                _logger.LogDebug("? [SCHEDULER-{OperationId}] Generated summary: {TotalJobs} total jobs, {TotalHours:F1} total hours",
                    operationId, Summary.TotalJobs, Summary.TotalHours);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error generating optimized summary: {ErrorMessage}",
                    operationId, ex.Message);

                // Fallback to empty summary
                Summary = new FooterSummaryViewModel();
                throw; // Re-throw to be handled by caller
            }
        }

        private async Task HandleLoadErrorAsync(string? zoom, DateTime? startDate, string operationId, Exception originalError)
        {
            _logger.LogWarning("?? [SCHEDULER-{OperationId}] Providing fallback data due to load error: {ErrorMessage}",
                operationId, originalError.Message);

            try
            {
                // Provide minimal fallback data to prevent crashes
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                Summary = new FooterSummaryViewModel();

                // Try to load at least basic machine data
                var machines = await _context.Jobs
                    .Select(j => j.MachineId)
                    .Distinct()
                    .Take(10) // Limit for safety
                    .ToListAsync();

                if (machines.Any())
                {
                    ViewModel.Machines = machines;
                    _logger.LogDebug("? [SCHEDULER-{OperationId}] Loaded {MachineCount} machines for fallback",
                        operationId, machines.Count);
                }
            }
            catch (Exception fallbackError)
            {
                _logger.LogError(fallbackError, "? [SCHEDULER-{OperationId}] Failed to load fallback machine data: {ErrorMessage}",
                    operationId, fallbackError.Message);

                // Last resort fallback
                ViewModel = new SchedulerPageViewModel
                {
                    StartDate = startDate ?? DateTime.UtcNow.Date,
                    Dates = new List<DateTime> { DateTime.UtcNow.Date },
                    Machines = new List<string> { "TI1", "TI2", "INC" },
                    Jobs = new List<Job>(),
                    MachineRowHeights = new Dictionary<string, int>
                    {
                        { "TI1", 160 },
                        { "TI2", 160 },
                        { "INC", 160 }
                    }
                };
                Summary = new FooterSummaryViewModel();
            }
        }

        public async Task<IActionResult> OnGetShowAddModalAsync(string machineId, string date, int? id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Opening modal for machine: {MachineId}, date: {Date}, jobId: {JobId}",
                operationId, machineId, date, id);

            try
            {
                // Parse and validate date
                if (!DateTime.TryParse(date, out var parsedDate))
                {
                    parsedDate = DateTime.UtcNow;
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Invalid date provided: {Date}, using current time", operationId, date);
                }

                Job job;
                if (id.HasValue)
                {
                    // Load existing job with related data
                    _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Loading existing job with ID: {JobId}", operationId, id.Value);

                    job = await _context.Jobs
                        .Include(j => j.Part)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(j => j.Id == id.Value);

                    if (job == null)
                    {
                        _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Job with ID {JobId} not found", operationId, id.Value);
                        return await CreateJobNotFoundModalAsync(machineId, parsedDate, operationId);
                    }

                    _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Found existing job: {PartNumber}", operationId, job.PartNumber);
                }
                else
                {
                    // Create new job with sensible defaults
                    _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Creating new job with defaults", operationId);
                    job = CreateNewJobWithDefaults(machineId, parsedDate, operationId);
                    
                    // 🚀 AUTOMATIC TIMING CALCULATION FOR NEW JOBS
                    _logger.LogDebug("🕒 [SCHEDULER-{OperationId}] Calculating automatic start time for new job", operationId);
                    job = await CalculateOptimalJobTimingAsync(job, operationId);
                }

                // Load active parts efficiently
                _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Loading active parts for modal", operationId);
                var parts = await _context.Parts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.PartNumber)
                    .AsNoTracking()
                    .ToListAsync();

                // Check for scheduling conflicts and provide helpful information
                var conflictInfo = "";
                if (!id.HasValue) // Only check conflicts for new jobs
                {
                    var existingJobs = await _context.Jobs
                        .Where(j => j.MachineId == job.MachineId)
                        .AsNoTracking()
                        .ToListAsync();

                    var (canSchedule, reason, suggestedTime) = await _schedulerService.CheckSchedulingConflict(
                        job.MachineId, job.ScheduledStart, job.EstimatedHours, job.SlsMaterial, existingJobs);

                    if (!canSchedule)
                    {
                        conflictInfo = $"Note: {reason}. Suggested time: {suggestedTime:MMM dd, yyyy HH:mm}";
                        _logger.LogInformation("ℹ️ [SCHEDULER-{OperationId}] Scheduling conflict detected: {Reason}", operationId, reason);
                    }
                    else
                    {
                        _logger.LogInformation("✅ [SCHEDULER-{OperationId}] No scheduling conflicts detected for new job", operationId);
                    }
                }

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Modal data prepared: {PartCount} parts available",
                    operationId, parts.Count);

                var viewModel = new AddEditJobViewModel 
                { 
                    Job = job, 
                    Parts = parts 
                };

                // Add scheduling info to the view model if there's conflict information
                if (!string.IsNullOrEmpty(conflictInfo))
                {
                    viewModel.Errors = new List<string> { conflictInfo };
                }

                return Partial("_AddEditJobModal", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error opening add/edit modal: {ErrorMessage}",
                    operationId, ex.Message);
                return await CreateErrorModalAsync(machineId, DateTime.TryParse(date, out var d) ? d : DateTime.UtcNow, operationId, ex);
            }
        }

        private Job CreateNewJobWithDefaults(string machineId, DateTime startDate, string operationId)
        {
            try
            {
                // Use UTC for all default date/time values
                var now = DateTime.UtcNow;
                
                // CRITICAL FIX: Use a more reasonable default start time (next business day at 7 AM)
                var defaultStart = now.Date.AddDays(1);
                if (defaultStart.DayOfWeek == DayOfWeek.Saturday)
                    defaultStart = defaultStart.AddDays(2); // Skip to Monday
                else if (defaultStart.DayOfWeek == DayOfWeek.Sunday)
                    defaultStart = defaultStart.AddDays(1); // Skip to Monday
                
                defaultStart = defaultStart.AddHours(7); // 7 AM start
                
                // Use the provided startDate if it's reasonable, otherwise use default
                var jobStartTime = startDate > now ? startDate : defaultStart;
                
                // Create initial job with sensible values
                var job = new Job
                {
                    MachineId = machineId,
                    ScheduledStart = jobStartTime,
                    ScheduledEnd = jobStartTime.AddHours(8), // This will be recalculated
                    CreatedDate = now,
                    LastModifiedDate = now,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000", // TEMP: Valid format, will be overwritten when part is selected
                    EstimatedHours = 8.0, // Default 8-hour job
                    
                    // CRITICAL FIX: Set required fields for validation
                    Operator = "John Doe", // Default operator name
                    QualityInspector = "Jane Smith", // Default quality inspector name
                    Supervisor = User.Identity?.Name ?? "System", // Default to current user
                    
                    // SLS-specific defaults
                    SlsMaterial = "Ti-6Al-4V Grade 5",
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
                    RequiresPostProcessing = true,
                    DensityPercentage = 99.5
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Created new job defaults for machine {MachineId} starting {StartTime}",
                    operationId, machineId, jobStartTime);

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error creating new job defaults: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate and set optimal start and end times for a new job
        /// </summary>
        private async Task<Job> CalculateOptimalJobTimingAsync(Job job, string operationId)
        {
            try
            {
                _logger.LogDebug("🕒 [SCHEDULER-{OperationId}] Calculating optimal timing for job on machine {MachineId}",
                    operationId, job.MachineId);

                // Get all existing jobs for conflict checking (but limit to near-term)
                var searchStartDate = DateTime.UtcNow.Date;
                var searchEndDate = searchStartDate.AddDays(30); // Only look 30 days ahead for better scheduling
                
                var existingJobs = await _context.Jobs
                    .Where(j => j.MachineId == job.MachineId && 
                               j.ScheduledStart >= searchStartDate &&
                               j.ScheduledStart <= searchEndDate)
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate next available start time within a reasonable timeframe
                var preferredStart = job.ScheduledStart > DateTime.UtcNow ? job.ScheduledStart : DateTime.UtcNow.Date.AddDays(1).AddHours(7);
                
                var optimalStartTime = await _schedulerService.CalculateNextAvailableStartTimeAsync(
                    job.MachineId, 
                    preferredStart, 
                    job.EstimatedHours, 
                    job.SlsMaterial, 
                    existingJobs);

                // If the calculated time is too far in the future, use a nearer time
                if ((optimalStartTime - DateTime.UtcNow).TotalDays > 14)
                {
                    _logger.LogWarning("🕒 [SCHEDULER-{OperationId}] Calculated time too far out ({OptimalTime}), using nearer time", 
                        operationId, optimalStartTime);
                    
                    // Find next available business day within 7 days
                    var nearerStart = DateTime.UtcNow.Date.AddDays(1).AddHours(7);
                    while (nearerStart.DayOfWeek == DayOfWeek.Saturday || nearerStart.DayOfWeek == DayOfWeek.Sunday)
                    {
                        nearerStart = nearerStart.AddDays(1);
                    }
                    optimalStartTime = nearerStart;
                }

                // Update job timing
                job.ScheduledStart = optimalStartTime;
                
                // Calculate optimal end time based on all processing requirements
                job.ScheduledEnd = await _schedulerService.CalculateJobEndTimeWithSettingsAsync(job);

                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Optimal timing calculated - Start: {Start}, End: {End}, Duration: {Duration}h",
                    operationId, job.ScheduledStart, job.ScheduledEnd, job.DurationHours);

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error calculating optimal job timing: {ErrorMessage}",
                    operationId, ex.Message);
                
                // Fallback to reasonable default timing if calculation fails
                var fallbackStart = DateTime.UtcNow.Date.AddDays(1).AddHours(7);
                job.ScheduledStart = fallbackStart;
                job.ScheduledEnd = fallbackStart.AddHours(job.EstimatedHours);
                
                return job;
            }
        }

        private async Task<PartialViewResult> CreateJobNotFoundModalAsync(string machineId, DateTime startDate, string operationId)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                _logger.LogDebug("? [SCHEDULER-{OperationId}] Created job not found modal with {PartCount} parts",
                    operationId, parts.Count);

                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = CreateNewJobWithDefaults(machineId, startDate, operationId),
                    Parts = parts,
                    Errors = new List<string> { "The requested job was not found. Creating a new job instead." }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error creating job not found modal: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<PartialViewResult> CreateErrorModalAsync(string machineId, DateTime startDate, string operationId, Exception originalError)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).Take(10).ToListAsync(); // Limit for safety
                _logger.LogDebug("? [SCHEDULER-{OperationId}] Created error modal with {PartCount} parts",
                    operationId, parts.Count);

                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = CreateNewJobWithDefaults(machineId, startDate, operationId),
                    Parts = parts,
                    Errors = new List<string> { $"Error loading job form (ID: {operationId}): {originalError.Message}" }
                });
            }
            catch (Exception fallbackError)
            {
                _logger.LogError(fallbackError, "? [SCHEDULER-{OperationId}] Error creating error modal fallback: {ErrorMessage}",
                    operationId, fallbackError.Message);

                // Absolute fallback
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = CreateNewJobWithDefaults(machineId, startDate, operationId),
                    Parts = new List<Part>(),
                    Errors = new List<string> {
                        $"System error (ID: {operationId}): {originalError.Message}",
                        "Please refresh the page and try again."
                    }
                });
            }
        }

        public async Task<IActionResult> OnPostAddOrUpdateJobAsync([FromForm] Job job)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🔧 [SCHEDULER-{OperationId}] Processing job submission: Id={JobId}, MachineId={MachineId}, PartId={PartId}",
                operationId, job.Id, job.MachineId, job.PartId);

            try
            {
                // DEFENSIVE VALIDATION: Fix potential format/argument exceptions
                await ValidateAndSanitizeJobDataAsync(job, operationId);

                // Enhanced validation with detailed logging
                var validationErrors = new List<string>();

                if (string.IsNullOrEmpty(job.MachineId))
                {
                    validationErrors.Add("Machine must be selected");
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Validation failed: Missing machine ID", operationId);
                }

                if (job.PartId <= 0)
                {
                    validationErrors.Add("Part must be selected");
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Validation failed: Missing part ID (PartId: {PartId})",
                        operationId, job.PartId);
                }

                if (validationErrors.Any())
                {
                    _logger.LogWarning("❌ [SCHEDULER-{OperationId}] Job submission failed validation: {ErrorCount} errors",
                        operationId, validationErrors.Count);
                    return await CreateValidationErrorAsync(job, string.Join("; ", validationErrors), operationId);
                }

                // Get part information with detailed logging
                _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Looking up part with ID: {PartId}", operationId, job.PartId);
                var part = await _context.Parts.FindAsync(job.PartId);
                if (part == null)
                {
                    _logger.LogWarning("❌ [SCHEDULER-{OperationId}] Part not found: {PartId}", operationId, job.PartId);
                    return await CreatePartNotFoundErrorAsync(job, operationId);
                }

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Found part: {PartNumber} - {Description}",
                    operationId, part.PartNumber, part.Description);

                // CRITICAL FIX: Set PartNumber from selected Part BEFORE validation
                job.PartNumber = part.PartNumber;
                job.EstimatedHours = part.EstimatedHours; // Update duration based on part
                
                // 🚀 AUTOMATIC TIMING RECALCULATION FOR NEW JOBS
                if (job.Id == 0) // Only for new jobs
                {
                    _logger.LogDebug("🕒 [SCHEDULER-{OperationId}] Recalculating timing based on selected part", operationId);
                    job = await CalculateOptimalJobTimingAsync(job, operationId);
                }

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Set job PartNumber to: {PartNumber}, Duration: {Hours}h",
                    operationId, job.PartNumber, job.EstimatedHours);

                // Basic time validation after automatic calculation
                if (job.ScheduledStart >= job.ScheduledEnd)
                {
                    validationErrors.Add("End time must be after start time");
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Validation failed: Invalid time range (Start: {Start}, End: {End})",
                        operationId, job.ScheduledStart, job.ScheduledEnd);
                }

                if (validationErrors.Any())
                {
                    _logger.LogWarning("❌ [SCHEDULER-{OperationId}] Job submission failed post-calculation validation: {ErrorCount} errors",
                        operationId, validationErrors.Count);
                    return await CreateValidationErrorAsync(job, string.Join("; ", validationErrors), operationId);
                }

                // Validate job timing with optimized query
                _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Checking for scheduling conflicts...", operationId);
                if (!await ValidateJobTimingAsync(job, operationId))
                {
                    _logger.LogWarning("❌ [SCHEDULER-{OperationId}] Scheduling conflict detected", operationId);
                    return await CreateTimingConflictErrorAsync(job, operationId);
                }

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] No scheduling conflicts found", operationId);

                // ENHANCED DB OPERATION: Use transaction to prevent context disposal issues
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Process job based on whether it's new or existing
                    string logAction;
                    if (job.Id == 0)
                    {
                        _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Creating new job...", operationId);
                        var createResult = await CreateNewJobAsync(job, part, operationId);
                        if (!createResult.success)
                        {
                            _logger.LogError("❌ [SCHEDULER-{OperationId}] Failed to create job: {Error}",
                                operationId, "Job creation failed");
                            await transaction.RollbackAsync();
                            return await CreateSaveErrorAsync(job, "Job creation failed", operationId);
                        }
                        job = createResult.job; // Get the job with assigned ID
                        logAction = "Created";
                    }
                    else
                    {
                        _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Updating existing job {JobId}...", operationId, job.Id);
                        var updateResult = await UpdateExistingJobAsync(job, part, operationId);
                        if (!updateResult.success)
                        {
                            _logger.LogError("❌ [SCHEDULER-{OperationId}] Failed to update job: {Error}",
                                operationId, "Job update failed");
                            await transaction.RollbackAsync();
                            return await CreateSaveErrorAsync(job, "Job update failed", operationId);
                        }
                        job = updateResult.job; // Get the updated job
                        logAction = "Updated";
                    }

                    // Create audit log
                    await CreateAuditLogAsync(job, logAction, operationId);

                    // Commit transaction
                    await transaction.CommitAsync();

                    _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job {Action} successfully: {JobId} - {PartNumber}, Scheduled: {Start} - {End}",
                        operationId, logAction, job.Id, job.PartNumber, job.ScheduledStart, job.ScheduledEnd);

                    // **CRITICAL FIX: Return proper HTMX response instead of JavaScript**
                    
                    // Add success message to TempData for notification
                    TempData["SuccessMessage"] = $"Job {logAction.ToLower()} successfully! {job.PartNumber} scheduled for {job.ScheduledStart:MMM dd, HH:mm}";
                    
                    // **HTMX RESPONSE: Return combined response that closes modal and refreshes machine row**
                    var responseHtml = $@"
                        <!-- Close modal via HTMX -->
                        <div id=""modal-close-trigger"" 
                             hx-get=""/Scheduler?handler=CloseModal""
                             hx-target=""#job-modal-container""
                             hx-swap=""outerHTML""
                             hx-trigger=""load"">
                        </div>
                        
                        <!-- Show success notification -->
                        <div id=""success-notification"" 
                             hx-get=""/Scheduler?handler=ShowSuccessNotification&message={Uri.EscapeDataString($"Job {logAction.ToLower()} successfully!")}&operationId={operationId}""
                             hx-target=""#notification-container""
                             hx-swap=""innerHTML""
                             hx-trigger=""load delay:200ms"">
                        </div>
                        
                        <!-- Refresh the specific machine row -->
                        <div id=""machine-row-refresh-trigger""
                             hx-get=""/Scheduler?handler=MachineRow&machineId={job.MachineId}""
                             hx-target=""#machine-row-{job.MachineId}""
                             hx-swap=""outerHTML""
                             hx-trigger=""load delay:400ms"">
                        </div>
                        
                        <!-- Refresh footer summary -->
                        <div id=""footer-refresh-trigger""
                             hx-get=""/Scheduler?handler=FooterSummary""
                             hx-target=""#footer-summary""
                             hx-swap=""outerHTML""
                             hx-trigger=""load delay:600ms"">
                        </div>";

                    return Content(responseHtml, "text/html");
                }
                catch (Exception transactionEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(transactionEx, "🚨 [SCHEDULER-{OperationId}] Transaction error: {ErrorMessage}",
                        operationId, transactionEx.Message);
                    throw; // Re-throw to be handled by outer catch
                }
            }
            catch (ObjectDisposedException odEx)
            {
                _logger.LogError(odEx, "🚨 [SCHEDULER-{OperationId}] DbContext disposal error: {ErrorMessage}",
                    operationId, odEx.Message);
                return await CreateDbContextErrorAsync(job, operationId);
            }
            catch (FormatException fEx)
            {
                _logger.LogError(fEx, "🚨 [SCHEDULER-{OperationId}] Format conversion error: {ErrorMessage}",
                    operationId, fEx.Message);
                return await CreateFormatErrorAsync(job, operationId, fEx);
            }
            catch (ArgumentException aEx)
            {
                _logger.LogError(aEx, "🚨 [SCHEDULER-{OperationId}] Argument error: {ErrorMessage}",
                    operationId, aEx.Message);
                return await CreateArgumentErrorAsync(job, operationId, aEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 [SCHEDULER-{OperationId}] Critical error in job submission processing: {ErrorMessage}",
                    operationId, ex.Message);
                return await CreateGenericErrorAsync(job, operationId, ex);
            }
        }

        private async Task<PartialViewResult> CreatePartNotFoundErrorAsync(Job job, string operationId)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Part not found (ID: {operationId}). Please select a valid part." }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error creating part not found error modal: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<PartialViewResult> CreateTimingConflictErrorAsync(Job job, string operationId)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Scheduling conflict detected (ID: {operationId}). Please choose a different time slot." }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error creating timing conflict error modal: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<PartialViewResult> CreateSaveErrorAsync(Job job, string errorMessage, string operationId)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Save error (ID: {operationId}): {errorMessage}" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error creating save error modal: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<(bool success, Job job)> CreateNewJobAsync(Job job, Part part, string operationId)
        {
            try
            {
                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Creating new job for part: {PartNumber}",
                    operationId, part.PartNumber);

                var now = DateTime.UtcNow;
                job.CreatedDate = now;
                job.LastModifiedDate = now;
                job.CreatedBy = User.Identity?.Name ?? "System";
                job.LastModifiedBy = User.Identity?.Name ?? "System";

                // CRITICAL FIX: Set part-specific data from the Part entity
                job.PartNumber = part.PartNumber; // Fix for part number validation
                job.EstimatedHours = part.EstimatedHours;
                job.SlsMaterial = part.SlsMaterial;
                job.LaserPowerWatts = part.RecommendedLaserPower;
                job.ScanSpeedMmPerSec = part.RecommendedScanSpeed;
                job.BuildTemperatureCelsius = part.RecommendedBuildTemperature;
                job.LayerThicknessMicrons = part.RecommendedLayerThickness;
                job.HatchSpacingMicrons = part.RecommendedHatchSpacing;
                job.EstimatedPowderUsageKg = part.PowderRequirementKg;
                job.PreheatingTimeMinutes = part.PreheatingTimeMinutes;
                job.CoolingTimeMinutes = part.CoolingTimeMinutes;
                job.PostProcessingTimeMinutes = part.PostProcessingTimeMinutes;

                // Set cost data from part
                job.MaterialCostPerKg = part.MaterialCostPerKg;
                job.LaborCostPerHour = part.StandardLaborCostPerHour;
                job.MachineOperatingCostPerHour = part.MachineOperatingCostPerHour;
                job.ArgonCostPerHour = part.ArgonCostPerHour;

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Set job properties from part - PartNumber: {PartNumber}, Material: {Material}",
                    operationId, job.PartNumber, job.SlsMaterial);

                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] New job created successfully: {JobId}",
                    operationId, job.Id);

                return (true, job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error creating new job: {ErrorMessage}",
                    operationId, ex.Message);
                return (false, job);
            }
        }

        private async Task<(bool success, Job job)> UpdateExistingJobAsync(Job job, Part part, string operationId)
        {
            try
            {
                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Updating existing job: {JobId}",
                    operationId, job.Id);

                var existingJob = await _context.Jobs.FindAsync(job.Id);
                if (existingJob == null)
                {
                    _logger.LogWarning("?? [SCHEDULER-{OperationId}] Job not found for update: {JobId}",
                        operationId, job.Id);
                    return (false, job);
                }

                // Update job properties
                UpdateJobFromForm(existingJob, job);
                existingJob.LastModifiedDate = DateTime.UtcNow;
                existingJob.LastModifiedBy = User.Identity?.Name ?? "System";

                await _context.SaveChangesAsync();

                _logger.LogInformation("? [SCHEDULER-{OperationId}] Job updated successfully: {JobId}",
                    operationId, job.Id);

                return (true, existingJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error updating job: {ErrorMessage}",
                    operationId, ex.Message);
                return (false, job);
            }
        }

        private void UpdateJobFromForm(Job existingJob, Job formJob)
        {
            existingJob.MachineId = formJob.MachineId;
            existingJob.PartId = formJob.PartId;
            existingJob.PartNumber = formJob.PartNumber;
            existingJob.ScheduledStart = formJob.ScheduledStart;
            existingJob.ScheduledEnd = formJob.ScheduledEnd;
            existingJob.EstimatedHours = formJob.EstimatedHours;
            existingJob.Quantity = formJob.Quantity;
            existingJob.Status = formJob.Status;
            existingJob.Priority = formJob.Priority;
            existingJob.SlsMaterial = formJob.SlsMaterial;
            existingJob.LaserPowerWatts = formJob.LaserPowerWatts;
            existingJob.ScanSpeedMmPerSec = formJob.ScanSpeedMmPerSec;
            existingJob.LayerThicknessMicrons = formJob.LayerThicknessMicrons;
            existingJob.HatchSpacingMicrons = formJob.HatchSpacingMicrons;
            existingJob.BuildTemperatureCelsius = formJob.BuildTemperatureCelsius;
            existingJob.ArgonPurityPercent = formJob.ArgonPurityPercent;
            existingJob.OxygenContentPpm = formJob.OxygenContentPpm;
            existingJob.RequiresArgonPurge = formJob.RequiresArgonPurge;
            existingJob.RequiresPreheating = formJob.RequiresPreheating;
            existingJob.RequiresPowderSieving = formJob.RequiresPowderSieving;
            existingJob.DensityPercentage = formJob.DensityPercentage;
            
            // CRITICAL FIX: Include the new required fields
            existingJob.Operator = formJob.Operator;
            existingJob.QualityInspector = formJob.QualityInspector;
            existingJob.Supervisor = formJob.Supervisor;
            existingJob.CustomerOrderNumber = formJob.CustomerOrderNumber;
            existingJob.Notes = formJob.Notes;
            existingJob.IsRushJob = formJob.IsRushJob;
        }

        private async Task<PartialViewResult> CreateValidationErrorAsync(Job job, string errorMessage, string operationId)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                _logger.LogDebug("? [SCHEDULER-{OperationId}] Created validation error modal with {PartCount} parts",
                    operationId, parts.Count);

                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> { $"Validation Error (ID: {operationId}): {errorMessage}" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error creating validation error modal: {ErrorMessage}",
                    operationId, ex.Message);
                throw;
            }
        }

        private async Task<bool> ValidateJobTimingAsync(Job job, string operationId)
        {
            try
            {
                // PERFORMANCE: Only check relevant jobs for conflicts
                var conflictBuffer = TimeSpan.FromHours(24); // 24-hour buffer for overlap detection

                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Querying for potential conflicts on machine {Machine} from {Start} to {End}",
                    operationId, job.MachineId, job.ScheduledStart.Subtract(conflictBuffer), job.ScheduledEnd.Add(conflictBuffer));

                var existingJobs = await _context.Jobs
                    .Where(j => j.Id != job.Id &&
                               j.MachineId == job.MachineId &&
                               j.ScheduledStart < job.ScheduledEnd.Add(conflictBuffer) &&
                               j.ScheduledEnd > job.ScheduledStart.Subtract(conflictBuffer))
                    .AsNoTracking()
                    .ToListAsync();

                _logger.LogDebug("?? [SCHEDULER-{OperationId}] Found {ConflictJobCount} potential conflicting jobs",
                    operationId, existingJobs.Count);

                var isValid = _schedulerService.ValidateJobScheduling(job, existingJobs, out var errors);

                if (!isValid)
                {
                    _logger.LogWarning("?? [SCHEDULER-{OperationId}] Job validation failed: {ErrorCount} conflicts - {Errors}",
                        operationId, errors.Count, string.Join("; ", errors));
                }

                return isValid;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Error validating job timing: {ErrorMessage}",
                    operationId, ex.Message);
                return false; // Fail safe - reject if validation throws
            }
        }

        private async Task CreateAuditLogAsync(Job job, string action, string operationId)
        {
            try
            {
                _context.JobLogEntries.Add(new JobLogEntry
                {
                    MachineId = job.MachineId,
                    PartNumber = job.PartNumber ?? string.Empty,
                    Action = action,
                    Operator = User.Identity?.Name ?? "System",
                    Notes = $"Job {action.ToLower()} via scheduler interface (Operation ID: {operationId})",
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                _logger.LogDebug("? [SCHEDULER-{OperationId}] Audit log created for {Action} action", operationId, action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [SCHEDULER-{OperationId}] Failed to create audit log for job {JobId}: {ErrorMessage}",
                    operationId, job.Id, ex.Message);
                // Don't fail the main operation for audit log issues
            }
        }

        private async Task ValidateAndSanitizeJobDataAsync(Job job, string operationId)
        {
            try
            {
                _logger.LogDebug("🔧 [SCHEDULER-{OperationId}] Validating and sanitizing job data", operationId);

                // Ensure numeric values are within valid ranges
                job.LaserPowerWatts = Math.Max(50, Math.Min(400, job.LaserPowerWatts));
                job.ScanSpeedMmPerSec = Math.Max(100, Math.Min(5000, job.ScanSpeedMmPerSec));
                job.LayerThicknessMicrons = Math.Max(20, Math.Min(60, job.LayerThicknessMicrons));
                job.HatchSpacingMicrons = Math.Max(50, Math.Min(200, job.HatchSpacingMicrons));
                job.BuildTemperatureCelsius = Math.Max(100, Math.Min(300, job.BuildTemperatureCelsius));
                job.Quantity = Math.Max(1, Math.Min(1000, job.Quantity));
                job.Priority = Math.Max(1, Math.Min(5, job.Priority));

                // Ensure estimated powder usage is reasonable
                if (job.EstimatedPowderUsageKg <= 0 || job.EstimatedPowderUsageKg > 50)
                {
                    job.EstimatedPowderUsageKg = 0.5; // Default value
                }

                // Ensure material selection is valid
                var validMaterials = new[] { "Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23", "Inconel 718", "Inconel 625" };
                if (!validMaterials.Contains(job.SlsMaterial))
                {
                    job.SlsMaterial = "Ti-6Al-4V Grade 5"; // Default
                }

                // Ensure machine ID is valid
                var validMachines = new[] { "TI1", "TI2", "INC" };
                if (!validMachines.Contains(job.MachineId))
                {
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Invalid machine ID: {MachineId}, using default", operationId, job.MachineId);
                    job.MachineId = "TI1"; // Default
                }

                // Calculate EstimatedHours from time difference
                var timeDiff = job.ScheduledEnd - job.ScheduledStart;
                if (timeDiff.TotalHours > 0 && timeDiff.TotalHours <= 168) // Max 1 week
                {
                    job.EstimatedHours = timeDiff.TotalHours;
                }
                else
                {
                    job.EstimatedHours = 8; // Default 8 hours
                    job.ScheduledEnd = job.ScheduledStart.AddHours(8);
                }

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Job data validated and sanitized", operationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error validating job data: {ErrorMessage}", operationId, ex.Message);
                throw;
            }
        }

        private async Task<PartialViewResult> CreateDbContextErrorAsync(Job job, string operationId)
        {
            try
            {
                // Create new context instance if original is disposed
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).Take(10).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> {
                        $"Database connection error (ID: {operationId}). Please try again.",
                        "If this error persists, please refresh the page."
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error creating DB context error modal: {ErrorMessage}",
                    operationId, ex.Message);

                // Absolute fallback
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = new List<Part>(),
                    Errors = new List<string> {
                        $"Critical database error (ID: {operationId})",
                        ">Please refresh the page and try again."
                    }
                });
            }
        }

        private async Task<PartialViewResult> CreateFormatErrorAsync(Job job, string operationId, FormatException ex)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> {
                        $"Data format error (ID: {operationId}): Please check numeric values.",
                        "Ensure all numbers are entered correctly (no letters or special characters)."
                    }
                });
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "❌ [SCHEDULER-{OperationId}] Error creating format error modal: {ErrorMessage}",
                    operationId, dbEx.Message);

                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = new List<Part>(),
                    Errors = new List<string> {
                        $"Format error (ID: {operationId}): Invalid data format",
                        "Please check all numeric fields and try again."
                    }
                });
            }
        }

        private async Task<PartialViewResult> CreateArgumentErrorAsync(Job job, string operationId, ArgumentException ex)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).OrderBy(p => p.PartNumber).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> {
                        $"Validation error (ID: {operationId}): {ex.Message}",
                        "Please check all fields and ensure they contain valid values."
                    }
                });
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "❌ [SCHEDULER-{OperationId}] Error creating argument error modal: {ErrorMessage}",
                    operationId, dbEx.Message);

                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = new List<Part>(),
                    Errors = new List<string> {
                        $"Argument error (ID: {operationId}): Invalid parameter",
                        "Please check all form fields and try again."
                    }
                });
            }
        }

        private async Task<PartialViewResult> CreateGenericErrorAsync(Job job, string operationId, Exception originalError)
        {
            try
            {
                var parts = await _context.Parts.Where(p => p.IsActive).Take(10).ToListAsync();
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = parts,
                    Errors = new List<string> {
                        $"Unexpected error (ID: {operationId}): {originalError.Message}",
                        "Please try again or contact support if this continues."
                    }
                });
            }
            catch (Exception fallbackError)
            {
                _logger.LogError(fallbackError, "❌ [SCHEDULER-{OperationId}] Error creating generic error modal fallback: {ErrorMessage}",
                    operationId, fallbackError.Message);

                // Absolute fallback
                return Partial("_AddEditJobModal", new AddEditJobViewModel
                {
                    Job = job,
                    Parts = new List<Part>(),
                    Errors = new List<string> {
                        $"Critical system error (ID: {operationId})",
                        "Please refresh the page and try again."
                    }
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteJobAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [SCHEDULER-{OperationId}] Processing job deletion request: JobId={JobId}",
                operationId, id);

            try
            {
                // Find the job to delete
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Job not found for deletion: {JobId}", operationId, id);

                    // Return empty content to close modal
                    return Content("", "text/html");
                }

                var machineId = job.MachineId;
                var jobInfo = $"{job.PartNumber} (ID: {job.Id})";

                _logger.LogDebug("🗑️ [SCHEDULER-{OperationId}] Deleting job: {JobInfo} from machine: {MachineId}",
                    operationId, jobInfo, machineId);

                // Use transaction for safe deletion
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Create audit log entry before deletion
                    var auditEntry = new JobLogEntry
                    {
                        MachineId = job.MachineId,
                        PartNumber = job.PartNumber ?? string.Empty,
                        Action = "Deleted",
                        Operator = User.Identity?.Name ?? "System",
                        Notes = $"Job deleted via scheduler interface (Operation ID: {operationId}). " +
                               $"Original schedule: {job.ScheduledStart:yyyy-MM-dd HH:mm} - {job.ScheduledEnd:yyyy-MM-dd HH:mm}",
                        Timestamp = DateTime.UtcNow
                    };

                    _context.JobLogEntries.Add(auditEntry);

                    // Remove the job
                    _context.Jobs.Remove(job);

                    // Save changes
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job deleted successfully: {JobInfo}",
                        operationId, jobInfo);

                    // Return simple success response that will close modal and trigger refresh
                    var successScript = "<div id=\"delete-success\" hx-trigger=\"load\">" +
                        "<script>" +
                        "if (window.closeJobModal) { window.closeJobModal(); } " +
                        "else { const modal = document.getElementById('modal-container'); " +
                        "if (modal) { modal.classList.add('hidden'); modal.classList.remove('flex'); " +
                        "document.body.style.overflow = ''; modal.innerHTML = ''; } } " +
                        "if (window.showSuccessNotification) { " +
                        "window.showSuccessNotification('Job deleted successfully!'); } " +
                        "setTimeout(() => { window.location.reload(); }, 500);" +
                        "</script></div>";

                    return Content(successScript, "text/html");
                }
                catch (Exception transactionEx)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(transactionEx, "🚨 [SCHEDULER-{OperationId}] Transaction error during job deletion: {ErrorMessage}",
                        operationId, transactionEx.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🚨 [SCHEDULER-{OperationId}] Critical error deleting job {JobId}: {ErrorMessage}",
                    operationId, id, ex.Message);

                var errorScript = "<div id=\"delete-error\">" +
                    "<script>" +
                    "if (window.showErrorNotification) { " +
                    "window.showErrorNotification('Error deleting job. Please try again.'); }" +
                    "</script></div>";

                return Content(errorScript, "text/html");
            }
        }

        /// <summary>
        /// Get human-readable scheduling availability information for a machine
        /// </summary>
        public async Task<IActionResult> OnGetSchedulingAvailabilityAsync(string machineId, string date)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("📅 [SCHEDULER-{OperationId}] Checking availability for machine {MachineId} on {Date}",
                operationId, machineId, date);

            try
            {
                if (!DateTime.TryParse(date, out var checkDate))
                {
                    checkDate = DateTime.UtcNow.Date;
                }

                // Get existing jobs for this machine
                var existingJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart >= checkDate.Date &&
                               j.ScheduledStart < checkDate.Date.AddDays(1))
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate next available time for a standard 8-hour job
                var nextAvailable = await _schedulerService.CalculateNextAvailableStartTimeAsync(
                    machineId, checkDate, 8.0, "Ti-6Al-4V Grade 5", existingJobs);

                var availabilityInfo = new
                {
                    machineId = machineId,
                    requestedDate = checkDate.ToString("yyyy-MM-dd"),
                    nextAvailableTime = nextAvailable.ToString("yyyy-MM-dd HH:mm"),
                    isToday = nextAvailable.Date == DateTime.UtcNow.Date,
                    isSameDay = nextAvailable.Date == checkDate.Date,
                    daysOut = (nextAvailable.Date - checkDate.Date).Days,
                    jobsScheduled = existingJobs.Count,
                    message = GenerateAvailabilityMessage(checkDate, nextAvailable, existingJobs.Count)
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Availability calculated: Next available {NextAvailable}",
                    operationId, nextAvailable);

                return new JsonResult(availabilityInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error checking scheduling availability: {ErrorMessage}",
                    operationId, ex.Message);

                return new JsonResult(new
                {
                    machineId = machineId,
                    error = "Unable to check availability",
                    message = "Please try again or contact support"
                });
            }
        }

        /// <summary>
        /// Generate a user-friendly availability message
        /// </summary>
        private string GenerateAvailabilityMessage(DateTime requestedDate, DateTime nextAvailable, int existingJobCount)
        {
            if (nextAvailable.Date == requestedDate.Date)
            {
                if (existingJobCount == 0)
                {
                    return $"✅ Machine is available! You can schedule a job starting at {nextAvailable:HH:mm} today.";
                }
                else
                {
                    return $"⏰ Machine has {existingJobCount} job(s) scheduled today. Next available time: {nextAvailable:HH:mm}.";
                }
            }
            else if (nextAvailable.Date == requestedDate.Date.AddDays(1))
            {
                return $"📅 Machine is fully booked today. Next available: Tomorrow at {nextAvailable:HH:mm}.";
            }
            else
            {
                var daysOut = (nextAvailable.Date - requestedDate.Date).Days;
                return $"📆 Machine is busy for the next {daysOut} day(s). Next available: {nextAvailable:MMM dd} at {nextAvailable:HH:mm}.";
            }
        }

        /// <summary>
        /// Get optimal timing for a job based on part selection
        /// </summary>
        public async Task<IActionResult> OnGetOptimalTimingAsync(string machineId, int partId, string preferredStart)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🕒 [SCHEDULER-{OperationId}] Calculating optimal timing for part {PartId} on machine {MachineId}",
                operationId, partId, machineId);

            try
            {
                // Parse preferred start time
                if (!DateTime.TryParse(preferredStart, out var startDate))
                {
                    startDate = DateTime.UtcNow.Date.AddHours(8); // Default to 8 AM today
                }

                // Get part information
                var part = await _context.Parts.FindAsync(partId);
                if (part == null)
                {
                    return new JsonResult(new { error = "Part not found" });
                }

                // Get existing jobs for conflict checking
                var existingJobs = await _context.Jobs
                    .Where(j => j.MachineId == machineId)
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate optimal start time
                var optimalStart = await _schedulerService.CalculateNextAvailableStartTimeAsync(
                    machineId, startDate, part.EstimatedHours, part.SlsMaterial, existingJobs);

                // Calculate end time with all processing steps
                var tempJob = new Job
                {
                    MachineId = machineId,
                    PartId = partId,
                    ScheduledStart = optimalStart,
                    EstimatedHours = part.EstimatedHours,
                    SlsMaterial = part.SlsMaterial,
                    RequiresPreheating = true, // Default to true since Part model doesn't have this field
                    RequiresPostProcessing = true, // Default to true since Part model doesn't have this field
                    PreheatingTimeMinutes = part.PreheatingTimeMinutes,
                    CoolingTimeMinutes = part.CoolingTimeMinutes,
                    PostProcessingTimeMinutes = part.PostProcessingTimeMinutes
                };

                var optimalEnd = await _schedulerService.CalculateJobEndTimeWithSettingsAsync(tempJob);

                // Check for conflicts at the calculated time
                var (canSchedule, reason, suggestedTime) = await _schedulerService.CheckSchedulingConflict(
                    machineId, optimalStart, part.EstimatedHours, part.SlsMaterial, existingJobs);

                var timingInfo = new
                {
                    partId = partId,
                    partNumber = part.PartNumber,
                    machineId = machineId,
                    preferredStart = preferredStart,
                    optimalStart = optimalStart.ToString("yyyy-MM-ddTHH:mm"),
                    optimalEnd = optimalEnd.ToString("yyyy-MM-ddTHH:mm"),
                    duration = part.EstimatedHours,
                    material = part.SlsMaterial,
                    canSchedule = canSchedule,
                    conflictReason = canSchedule ? null : reason,
                    suggestedTime = canSchedule ? null : suggestedTime.ToString("yyyy-MM-ddTHH:mm"),
                    message = canSchedule 
                        ? $"✅ Optimal time calculated: {optimalStart:MMM dd, HH:mm} - {optimalEnd:MMM dd, HH:mm}"
                        : $"⚠️ Conflict detected: {reason}. Suggested: {suggestedTime:MMM dd, HH:mm}"
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Optimal timing calculated: {OptimalStart} - {OptimalEnd}",
                    operationId, optimalStart, optimalEnd);

                return new JsonResult(timingInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error calculating optimal timing: {ErrorMessage}",
                    operationId, ex.Message);

                return new JsonResult(new
                {
                    error = "Unable to calculate optimal timing",
                    message = "Please try again or contact support"
                });
            }
        }

        /// <summary>
        /// Get updated machine row HTML after job changes
        /// </summary>
        public async Task<IActionResult> OnGetMachineRowAsync(string machineId)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔄 [SCHEDULER-{OperationId}] Getting updated machine row for {MachineId}", operationId, machineId);

            try
            {
                // Get the current scheduler data
                ViewModel = _schedulerService.GetSchedulerData();
                
                // Load jobs for this machine
                var queryStartDate = ViewModel.StartDate.AddDays(-1);
                var queryEndDate = ViewModel.StartDate.AddDays(ViewModel.Dates.Count + 1);

                var machineJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart < queryEndDate && 
                               j.ScheduledEnd > queryStartDate)
                    .OrderBy(j => j.ScheduledStart)
                    .AsNoTracking()
                    .ToListAsync();

                // Calculate row height
                var (_, rowHeight) = _schedulerService.CalculateMachineRowLayout(machineId, machineJobs);

                // Create partial model for the machine row
                var machineRowModel = new {
                    MachineId = machineId,
                    Dates = ViewModel.Dates,
                    Jobs = machineJobs,
                    RowHeight = rowHeight,
                    SlotsPerDay = ViewModel.SlotsPerDay,
                    SlotMinutes = ViewModel.SlotMinutes,
                    JobCount = machineJobs.Count,
                    TotalHours = machineJobs.Sum(j => j.DurationHours)
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Machine row updated: {JobCount} jobs, {Height}px",
                    operationId, machineJobs.Count, rowHeight);

                return Partial("_FullMachineRow", machineRowModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error getting machine row for {MachineId}: {ErrorMessage}",
                    operationId, machineId, ex.Message);
                
                // Return error message in machine row format
                return Content($"<div class='error-message'>Error loading machine {machineId}: {ex.Message}</div>", "text/html");
            }
        }

        /// <summary>
        /// Get updated footer summary after job changes
        /// </summary>
        public async Task<IActionResult> OnGetFooterSummaryAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔄 [SCHEDULER-{OperationId}] Getting updated footer summary", operationId);

            try
            {
                // Get the current scheduler data
                ViewModel = _schedulerService.GetSchedulerData();

                // Load all current jobs for summary
                var queryStartDate = ViewModel.StartDate.AddDays(-1);
                var queryEndDate = ViewModel.StartDate.AddDays(ViewModel.Dates.Count + 1);

                var allJobs = await _context.Jobs
                    .Where(j => j.ScheduledStart < queryEndDate && j.ScheduledEnd > queryStartDate)
                    .AsNoTracking()
                    .ToListAsync();

                // Generate updated summary
                Summary = new FooterSummaryViewModel
                {
                    MachineHours = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => allJobs.Where(j => j.MachineId == m).Sum(j => j.DurationHours)
                    ),
                    JobCounts = ViewModel.Machines.ToDictionary(
                        m => m,
                        m => allJobs.Count(j => j.MachineId == m)
                    )
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Footer summary updated: {TotalJobs} total jobs",
                    operationId, Summary.TotalJobs);

                return Partial("_FooterSummary", Summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error getting footer summary: {ErrorMessage}",
                    operationId, ex.Message);
                
                // Return minimal error summary
                return Partial("_FooterSummary", new FooterSummaryViewModel());
            }
        }

        /// <summary>
        /// **CRITICAL FIX: Close modal via HTMX response**
        /// </summary>
        public IActionResult OnGetCloseModal()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogDebug("🔄 [SCHEDULER-{OperationId}] Closing modal via HTMX", operationId);

            // Return empty content to remove modal from DOM
            return Content("", "text/html");
        }

        /// <summary>
        /// **CRITICAL FIX: Show success notification via HTMX**
        /// </summary>
        public IActionResult OnGetShowSuccessNotification(string message, string operationId)
        {
            _logger.LogDebug("🔄 [SCHEDULER-{OperationId}] Showing success notification: {Message}", operationId, message);

            // Return notification HTML
            var notificationHtml = $@"
                <div class=""notification notification-success fixed top-4 right-4 bg-green-500 text-white px-6 py-3 rounded-lg shadow-lg z-50 animate-fade-in"">
                    <div class=""flex items-center"">
                        <svg class=""w-5 h-5 mr-2"" fill=""none"" stroke=""currentColor"" viewBox=""0 0 24 24"">
                            <path stroke-linecap=""round"" stroke-linejoin=""round"" stroke-width=""2"" d=""M5 13l4 4L19 7""></path>
                        </svg>
                        <span>{message}</span>
                    </div>
                    <script>
                        setTimeout(() => {{
                            const notification = document.querySelector('.notification-success');
                            if (notification) {{
                                notification.style.opacity = '0';
                                setTimeout(() => notification.remove(), 300);
                            }}
                        }}, 3000);
                    </script>
                </div>";

            return Content(notificationHtml, "text/html");
        }
    }
}
