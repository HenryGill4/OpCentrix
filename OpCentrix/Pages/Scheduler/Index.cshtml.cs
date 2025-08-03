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

namespace OpCentrix.Pages.Scheduler
{
    /// <summary>
    /// Modern scheduler page using best practices and clean architecture
    /// - Separate DTOs for different operations
    /// - Clean validation without conflicts
    /// - Proper separation of concerns with machine management
    /// </summary>
    [SchedulerAccess]
    public class IndexModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ISchedulerService _schedulerService;
        private readonly IMachineManagementService _machineService;
        private readonly ITimeSlotService _timeSlotService;
        private readonly ILogger<IndexModel> _logger;

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

        public async Task OnGetAsync(string? zoom = null, DateTime? startDate = null, string? orientation = null)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🎯 [SCHEDULER-{OperationId}] Loading modern scheduler", operationId);

            try
            {
                // Load scheduler data
                ViewModel = _schedulerService.GetSchedulerData(zoom, startDate);
                
                // Load available machines from database
                await LoadAvailableMachinesAsync(operationId);
                
                // Load available parts for job creation
                await LoadAvailablePartsAsync(operationId);
                
                // Load jobs within date range
                await LoadJobsAsync(operationId);
                
                // Calculate summary
                await GenerateSummaryAsync(operationId);

                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Scheduler loaded: {JobCount} jobs, {MachineCount} machines", 
                    operationId, ViewModel.Jobs.Count, AvailableMachines.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading scheduler", operationId);
                await InitializeEmptyDataAsync();
                TempData["Error"] = "Error loading scheduler data. Please try again.";
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
                    // Load existing job
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
                    // Create new job with modern approach
                    job = await CreateNewJobAsync(machineId, parsedDate, operationId);
                }

                // Load available data for modal
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
                // Modern validation approach
                var validationResult = await ValidateJobRequestAsync(jobRequest, operationId);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
                    }
                    
                    _logger.LogWarning("⚠️ [SCHEDULER-{OperationId}] Validation failed: {ErrorCount} errors", 
                        operationId, validationResult.Errors.Count);
                    
                    await LoadAvailableMachinesAsync(operationId);
                    await LoadAvailablePartsAsync(operationId);
                    
                    var errorJob = await ConvertDtoToJobAsync(jobRequest);
                    return Partial("_AddEditJobModal", new AddEditJobViewModel
                    {
                        Job = errorJob,
                        Parts = AvailableParts,
                        Machines = AvailableMachines,
                        Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                    });
                }

                // Create or update job
                Job savedJob;
                if (jobRequest.Id == 0)
                {
                    savedJob = await CreateJobFromDtoAsync(jobRequest, operationId);
                    _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job created: {JobId} - {PartNumber}",
                        operationId, savedJob.Id, savedJob.PartNumber);
                }
                else
                {
                    savedJob = await UpdateJobFromDtoAsync(jobRequest, operationId);
                    _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job updated: {JobId} - {PartNumber}",
                        operationId, savedJob.Id, savedJob.PartNumber);
                }

                // CRITICAL FIX: Return JSON response for successful operations
                var successMessage = jobRequest.Id == 0 
                    ? $"Job scheduled successfully for {savedJob.PartNumber}" 
                    : $"Job updated successfully for {savedJob.PartNumber}";

                return new JsonResult(new 
                { 
                    success = true, 
                    jobId = savedJob.Id,
                    partNumber = savedJob.PartNumber,
                    machineId = savedJob.MachineId,
                    message = successMessage
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error processing job", operationId);
                
                // For exceptions, return JSON error response
                return new JsonResult(new 
                { 
                    success = false, 
                    error = "An error occurred while saving the job. Please try again.",
                    details = ex.Message 
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> OnPostDeleteJobAsync(int id)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogInformation("🗑️ [SCHEDULER-{OperationId}] Deleting job: {JobId}", operationId, id);

            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return new JsonResult(new { success = false, error = "Job not found" });
                }

                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Job deleted: {JobId}", operationId, id);
                return new JsonResult(new { success = true, message = "Job deleted successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error deleting job: {JobId}", operationId, id);
                return new JsonResult(new { success = false, error = "Error deleting job" });
            }
        }

        // Helper methods with modern architecture
        private async Task LoadAvailableMachinesAsync(string operationId)
        {
            try
            {
                AvailableMachines = await _machineService.GetActiveMachinesAsync();
                ViewModel.Machines = AvailableMachines.Select(m => m.MachineId).ToList();
                
                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Loaded {MachineCount} machines: {Machines}",
                    operationId, AvailableMachines.Count, string.Join(", ", AvailableMachines.Select(m => m.MachineId)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error loading machines", operationId);
                AvailableMachines = new List<Machine>();
                ViewModel.Machines = new List<string>();
            }
        }

        private async Task LoadAvailablePartsAsync(string operationId)
        {
            try
            {
                AvailableParts = await _context.Parts
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.PartNumber)
                    .AsNoTracking()
                    .ToListAsync();
                
                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Loaded {PartCount} parts", operationId, AvailableParts.Count);
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

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Loaded {JobCount} jobs", operationId, ViewModel.Jobs.Count);
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
                Summary = new FooterSummaryViewModel
                {
                    MachineHours = AvailableMachines.ToDictionary(
                        m => m.MachineId,
                        m => ViewModel.Jobs.Where(j => j.MachineId == m.MachineId).Sum(j => j.DurationHours)
                    ),
                    JobCounts = AvailableMachines.ToDictionary(
                        m => m.MachineId,
                        m => ViewModel.Jobs.Count(j => j.MachineId == m.MachineId)
                    )
                };

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Generated summary: {TotalJobs} jobs, {TotalHours:F1} hours",
                    operationId, Summary.TotalJobs, Summary.TotalHours);
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
                // Get next available time slot
                var nextAvailableTime = await _timeSlotService.GetNextAvailableTimeAsync(machineId, startDate, 8.0);
                
                var job = new Job
                {
                    MachineId = machineId,
                    ScheduledStart = nextAvailableTime,
                    ScheduledEnd = nextAvailableTime.AddHours(8),
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    Status = "Scheduled",
                    Priority = 3,
                    Quantity = 1,
                    PartNumber = "00-0000", // Temporary, will be set when part is selected
                    EstimatedHours = 8.0,
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

                _logger.LogDebug("✅ [SCHEDULER-{OperationId}] Created new job for {MachineId} at {StartTime}",
                    operationId, machineId, nextAvailableTime);

                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [SCHEDULER-{OperationId}] Error creating new job", operationId);
                
                // Fallback with basic defaults
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
                    SlsMaterial = "Ti-6Al-4V Grade 5"
                };
            }
        }

        private async Task<JobValidationResult> ValidateJobRequestAsync(CreateJobDto request, string operationId)
        {
            var result = new JobValidationResult();
            
            // Required field validation
            if (string.IsNullOrWhiteSpace(request.MachineId))
                result.AddError(nameof(request.MachineId), "Machine must be selected");
                
            if (request.PartId <= 0)
                result.AddError(nameof(request.PartId), "Part must be selected");
                
            if (request.ScheduledStart >= request.ScheduledEnd)
                result.AddError(nameof(request.ScheduledEnd), "End time must be after start time");

            // Business logic validation
            if (!string.IsNullOrWhiteSpace(request.MachineId))
            {
                var machine = AvailableMachines.FirstOrDefault(m => m.MachineId == request.MachineId);
                if (machine == null)
                {
                    result.AddError(nameof(request.MachineId), "Selected machine is not available");
                }
            }

            // IMPROVED: More lenient operating hours validation
            var duration = request.ScheduledEnd - request.ScheduledStart;
            if (duration.TotalHours > 168) // Max 1 week
            {
                result.AddError(nameof(request.ScheduledEnd), "Job duration cannot exceed 1 week");
            }

            _logger.LogDebug("🔍 [SCHEDULER-{OperationId}] Validation result: {IsValid}, {ErrorCount} errors", 
                operationId, result.IsValid, result.Errors.Count);

            return result;
        }

        private async Task<Job> CreateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            var part = await _context.Parts.FindAsync(dto.PartId);
            if (part == null)
            {
                throw new InvalidOperationException("Selected part not found");
            }

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
                CustomerOrderNumber = dto.CustomerOrderNumber,
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob,
                
                // Set additional required fields with defaults since Part doesn't have these properties
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

            _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Created job: {JobId} - {PartNumber} on {MachineId}", 
                operationId, job.Id, job.PartNumber, job.MachineId);

            return job;
        }

        private async Task<Job> UpdateJobFromDtoAsync(CreateJobDto dto, string operationId)
        {
            var job = await _context.Jobs.FindAsync(dto.Id);
            if (job == null)
            {
                throw new InvalidOperationException("Job not found for update");
            }

            var part = await _context.Parts.FindAsync(dto.PartId);
            if (part == null)
            {
                throw new InvalidOperationException("Selected part not found");
            }

            // Update job properties
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
            job.CustomerOrderNumber = dto.CustomerOrderNumber;
            job.Operator = dto.Operator;
            job.IsRushJob = dto.IsRushJob;
            
            // Update part-related fields if part changed - use defaults since Part model may not have these properties
            if (job.PartId != dto.PartId)
            {
                job.ArgonPurityPercent = 99.9;
                job.OxygenContentPpm = 50;
                job.RequiresArgonPurge = true;
                job.RequiresPreheating = true;
                job.RequiresPowderSieving = true;
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

            _logger.LogInformation("✅ [SCHEDULER-{OperationId}] Updated job: {JobId} - {PartNumber} on {MachineId}", 
                operationId, job.Id, job.PartNumber, job.MachineId);

            return job;
        }

        private async Task<Job> ConvertDtoToJobAsync(CreateJobDto dto)
        {
            // Convert DTO to Job for display purposes (e.g., validation errors)
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
                CustomerOrderNumber = dto.CustomerOrderNumber,
                Operator = dto.Operator,
                IsRushJob = dto.IsRushJob
            };

            // Try to get part number if PartId is provided
            if (dto.PartId > 0)
            {
                var part = await _context.Parts.FindAsync(dto.PartId);
                if (part != null)
                {
                    job.PartNumber = part.PartNumber;
                }
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

                // Use the time slot service to find next available time
                var suggestedStart = await _timeSlotService.GetNextAvailableTimeAsync(machineId, preferredStart ?? DateTime.UtcNow, durationHours);
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
    }

    // Modern DTOs for clean separation of concerns
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

    public class EditJobDto : CreateJobDto
    {
        // Inherits all properties from CreateJobDto
        // Can add edit-specific properties if needed
    }

    // Simple validation result class
    public class JobValidationResult
    {
        public List<JobValidationError> Errors { get; } = new();
        public bool IsValid => Errors.Count == 0;

        public void AddError(string propertyName, string errorMessage)
        {
            Errors.Add(new JobValidationError { PropertyName = propertyName, ErrorMessage = errorMessage });
        }
    }

    public class JobValidationError
    {
        public string PropertyName { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
