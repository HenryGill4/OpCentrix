using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    public interface ISchedulerService
    {
        SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null);
        bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors);
        (int maxLayers, int rowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs);
        Task<bool> ValidateSlsJobCompatibilityAsync(Job job);
        Task<double> CalculateOptimalPowderChangeoverTimeAsync(string machineId, string newMaterial);
        Task<List<string>> GetMachineCapabilityIssuesAsync(Job job);
        Task<decimal> CalculateSlsJobCostEstimateAsync(Job job);
        Task<List<Job>> OptimizeBuildPlatformLayoutAsync(string machineId, DateTime buildDate);
        Task<bool> ValidateOperatorAvailabilityAsync(Job job);
        Task<List<string>> ValidateSchedulingConstraintsAsync(Job job, List<Job> existingJobs);
        Task<DateTime> CalculateJobStartTimeWithSettingsAsync(Job job, List<Job> existingJobs);
        Task<DateTime> CalculateJobEndTimeWithSettingsAsync(Job job);
        Task<bool> IsWeekendOperationAllowedAsync(DateTime jobDate);
        Task<DateTime> CalculateNextAvailableStartTimeAsync(string machineId, DateTime preferredDate, double estimatedDurationHours, string material, List<Job> existingJobs);
        Task<(bool canSchedule, string reason, DateTime suggestedTime)> CheckSchedulingConflict(string machineId, DateTime proposedStart, double durationHours, string material, List<Job> existingJobs);
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SchedulerService> _logger;
        private readonly ISchedulerSettingsService _settingsService;

        public SchedulerService(SchedulerContext context, ILogger<SchedulerService> logger, ISchedulerSettingsService settingsService)
        {
            _context = context;
            _logger = logger;
            _settingsService = settingsService;
        }

        public SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null)
        {
            // Validate and set zoom level with new zoom options
            var validZooms = new[] { "day", "12h", "10h", "8h", "6h", "4h", "2h", "hour", "30min", "15min" };
            if (!validZooms.Contains(zoom))
                zoom = "day";

            // UPDATED: Start from UTC today instead of server local time
            var start = startDate ?? DateTime.UtcNow.Date;

            // Define SLS machine configuration
            var machines = new List<string> { "TI1", "TI2", "INC" };

            // Calculate view parameters based on zoom (zoom is now guaranteed non-null)
            var (dateCount, slotsPerDay, slotMinutes) = GetZoomParameters(zoom!);

            // Generate date range starting from UTC today
            var dates = Enumerable.Range(0, dateCount)
                .Select(i => start.AddDays(i))
                .ToList();

            _logger.LogInformation("Generated scheduler data for {MachineCount} machines, {DateCount} days starting {StartDate}, zoom: {Zoom}",
                machines.Count, dateCount, start.ToString("yyyy-MM-dd"), zoom);

            return new SchedulerPageViewModel
            {
                StartDate = start,
                Dates = dates,
                Machines = machines,
                SlotsPerDay = slotsPerDay,
                SlotMinutes = slotMinutes,
                Jobs = new List<Job>(), // Will be populated by controller
                MachineRowHeights = new Dictionary<string, int>()
            };
        }

        public bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                // Basic validation
                if (job.ScheduledEnd <= job.ScheduledStart)
                {
                    errors.Add("Job end time must be after start time");
                }

                if (job.Quantity <= 0)
                {
                    errors.Add("Quantity must be greater than zero");
                }

                // SLS-specific validation
                ValidateSlsParameters(job, errors);

                // Check for time conflicts with existing jobs
                foreach (var existingJob in existingJobs)
                {
                    if (job.OverlapsWith(existingJob))
                    {
                        errors.Add($"Job conflicts with existing job '{existingJob.PartNumber}' scheduled from {existingJob.ScheduledStart:MM/dd HH:mm} to {existingJob.ScheduledEnd:MM/dd HH:mm}");
                    }
                }

                // Material changeover validation with settings
                ValidateMaterialChangeoverAsync(job, existingJobs, errors).Wait();

                // Build platform capacity validation
                ValidateBuildPlatformCapacity(job, existingJobs, errors);

                // Validate scheduling constraints with settings
                var constraintErrors = ValidateSchedulingConstraintsAsync(job, existingJobs).Result;
                errors.AddRange(constraintErrors);

                _logger.LogInformation("Job validation completed for {PartNumber} on {MachineId}: {ErrorCount} errors found",
                    job.PartNumber, job.MachineId, errors.Count);

                return errors.Count == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating job scheduling for {PartNumber}", job.PartNumber);
                errors.Add("An error occurred during validation. Please try again.");
                return false;
            }
        }

        public (int maxLayers, int rowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs)
        {
            if (!jobs.Any())
                return (1, 160); // Default height for empty machines

            try
            {
                // Enhanced layering algorithm for SLS jobs
                var layers = new List<List<Job>>();
                var sortedJobs = jobs.OrderBy(j => j.ScheduledStart).ToList();

                foreach (var job in sortedJobs)
                {
                    var placedInLayer = false;

                    // Try to place in existing layers
                    for (int i = 0; i < layers.Count; i++)
                    {
                        var canPlaceInLayer = layers[i].All(existingJob =>
                            !job.OverlapsWith(existingJob) &&
                            IsCompatibleForSameBuildPlatform(job, existingJob));

                        if (canPlaceInLayer)
                        {
                            layers[i].Add(job);
                            placedInLayer = true;
                            break;
                        }
                    }

                    // Create new layer if couldn't place in existing ones
                    if (!placedInLayer)
                    {
                        layers.Add(new List<Job> { job });
                    }
                }

                // Calculate row height based on layers and job complexity
                var baseHeight = 160;
                var layerHeight = 40;
                var complexityBonus = CalculateComplexityBonus(jobs);

                var calculatedHeight = baseHeight + (layers.Count - 1) * layerHeight + complexityBonus;
                var finalHeight = Math.Max(160, Math.Min(400, calculatedHeight));

                _logger.LogDebug("Calculated layout for {MachineId}: {LayerCount} layers, {Height}px height",
                    machineId, layers.Count, finalHeight);

                return (layers.Count, finalHeight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating machine row layout for {MachineId}", machineId);
                return (1, 160); // Fallback to default
            }
        }

        public async Task<bool> ValidateSlsJobCompatibilityAsync(Job job)
        {
            try
            {
                // Get machine configuration
                var machine = await _context.SlsMachines
                    .FirstOrDefaultAsync(m => m.MachineId == job.MachineId);

                if (machine == null)
                {
                    _logger.LogWarning("Machine {MachineId} not found for job validation", job.MachineId);
                    return false;
                }

                // Check material compatibility
                if (!machine.SupportsMaterial(job.SlsMaterial))
                {
                    _logger.LogWarning("Machine {MachineId} does not support material {Material}",
                        job.MachineId, job.SlsMaterial);
                    return false;
                }

                // Check part dimensions against build envelope
                if (job.Part != null && !machine.CanAccommodatePart(job.Part))
                {
                    _logger.LogWarning("Part {PartNumber} dimensions exceed machine {MachineId} build envelope",
                        job.PartNumber, job.MachineId);
                    return false;
                }

                // Check process parameter compatibility
                if (job.LaserPowerWatts > machine.MaxLaserPowerWatts)
                {
                    _logger.LogWarning("Job {PartNumber} requires laser power {Power}W, machine {MachineId} max is {MaxPower}W",
                        job.PartNumber, job.LaserPowerWatts, job.MachineId, machine.MaxLaserPowerWatts);
                    return false;
                }

                if (job.ScanSpeedMmPerSec > machine.MaxScanSpeedMmPerSec)
                {
                    _logger.LogWarning("Job {PartNumber} requires scan speed {Speed}mm/s, machine {MachineId} max is {MaxSpeed}mm/s",
                        job.PartNumber, job.ScanSpeedMmPerSec, job.MachineId, machine.MaxScanSpeedMmPerSec);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SLS job compatibility for {PartNumber}", job.PartNumber);
                return false;
            }
        }

        public async Task<double> CalculateOptimalPowderChangeoverTimeAsync(string machineId, string newMaterial)
        {
            try
            {
                // Get machine current material
                var machine = await _context.SlsMachines
                    .FirstOrDefaultAsync(m => m.MachineId == machineId);

                if (machine == null)
                    return 0;

                // Get the last job to determine current material
                var lastJob = await _context.Jobs
                    .Where(j => j.MachineId == machineId && j.Status == "Completed")
                    .OrderByDescending(j => j.ActualEnd ?? j.ScheduledEnd)
                    .FirstOrDefaultAsync();

                var currentMaterial = lastJob?.SlsMaterial ?? machine.CurrentMaterial;

                if (string.IsNullOrEmpty(currentMaterial) || currentMaterial == newMaterial)
                    return 0; // No changeover needed

                // Use settings service for changeover time calculation
                var changeoverTime = await _settingsService.GetChangeoverTimeAsync(currentMaterial, newMaterial);

                _logger.LogDebug("Calculated changeover time from {FromMaterial} to {ToMaterial}: {Minutes} minutes",
                    currentMaterial, newMaterial, changeoverTime);

                return changeoverTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating powder changeover time for machine {MachineId}", machineId);
                return 60; // Default fallback
            }
        }

        public async Task<List<string>> GetMachineCapabilityIssuesAsync(Job job)
        {
            var issues = new List<string>();

            try
            {
                var machine = await _context.SlsMachines
                    .FirstOrDefaultAsync(m => m.MachineId == job.MachineId);

                if (machine == null)
                {
                    issues.Add($"Machine {job.MachineId} not found");
                    return issues;
                }

                // Check build envelope
                if (job.Part != null)
                {
                    if (job.Part.LengthMm > machine.BuildLengthMm)
                        issues.Add($"Part length ({job.Part.LengthMm}mm) exceeds machine build length ({machine.BuildLengthMm}mm)");

                    if (job.Part.WidthMm > machine.BuildWidthMm)
                        issues.Add($"Part width ({job.Part.WidthMm}mm) exceeds machine build width ({machine.BuildWidthMm}mm)");

                    if (job.Part.HeightMm > machine.BuildHeightMm)
                        issues.Add($"Part height ({job.Part.HeightMm}mm) exceeds machine build height ({machine.BuildHeightMm}mm)");
                }

                // Check material support
                if (!machine.SupportsMaterial(job.SlsMaterial))
                    issues.Add($"Machine does not support material: {job.SlsMaterial}");

                // Check process parameters
                if (job.LaserPowerWatts > machine.MaxLaserPowerWatts)
                    issues.Add($"Required laser power ({job.LaserPowerWatts}W) exceeds machine maximum ({machine.MaxLaserPowerWatts}W)");

                if (job.ScanSpeedMmPerSec > machine.MaxScanSpeedMmPerSec)
                    issues.Add($"Required scan speed ({job.ScanSpeedMmPerSec}mm/s) exceeds machine maximum ({machine.MaxScanSpeedMmPerSec}mm/s)");

                if (job.LayerThicknessMicrons < machine.MinLayerThicknessMicrons)
                    issues.Add($"Required layer thickness ({job.LayerThicknessMicrons}micrometers) below machine minimum ({machine.MinLayerThicknessMicrons}micrometers)");

                if (job.LayerThicknessMicrons > machine.MaxLayerThicknessMicrons)
                    issues.Add($"Required layer thickness ({job.LayerThicknessMicrons}micrometers) above machine maximum ({machine.MaxLayerThicknessMicrons}micrometers)");

                // Check machine availability
                if (!machine.IsActive)
                    issues.Add("Machine is inactive");

                if (!machine.IsAvailableForScheduling)
                    issues.Add("Machine is not available for scheduling");

                if (machine.RequiresMaintenance)
                    issues.Add("Machine requires maintenance");

                return issues;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking machine capability issues for job {PartNumber}", job.PartNumber);
                issues.Add("Error checking machine compatibility");
                return issues;
            }
        }

        public async Task<decimal> CalculateSlsJobCostEstimateAsync(Job job)
        {
            try
            {
                decimal totalCost = 0;

                // Labor cost
                totalCost += job.EstimatedLaborCost;

                // Material cost (powder)
                totalCost += job.EstimatedMaterialCost;

                // Machine operating cost
                totalCost += job.EstimatedMachineCost;

                // Argon cost
                totalCost += job.EstimatedArgonCost;

                // Add setup costs
                if (job.Part != null)
                {
                    totalCost += job.Part.SetupCost;
                    totalCost += job.Part.PostProcessingCost;
                    totalCost += job.Part.QualityInspectionCost;
                }

                // Add powder changeover cost if needed using settings
                var changeoverTime = await CalculateOptimalPowderChangeoverTimeAsync(job.MachineId, job.SlsMaterial);
                if (changeoverTime > 0)
                {
                    totalCost += (decimal)(changeoverTime / 60.0) * job.LaborCostPerHour; // Convert minutes to hours
                }

                _logger.LogDebug("Calculated cost estimate for job {PartNumber}: {Cost:C}", job.PartNumber, totalCost);

                return totalCost;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating cost estimate for job {PartNumber}", job.PartNumber);
                return 0;
            }
        }

        public async Task<List<Job>> OptimizeBuildPlatformLayoutAsync(string machineId, DateTime buildDate)
        {
            try
            {
                // Get all jobs scheduled for this machine on this date
                var jobs = await _context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId &&
                               j.ScheduledStart.Date == buildDate.Date)
                    .OrderBy(j => j.Priority)
                    .ThenBy(j => j.ScheduledStart)
                    .ToListAsync();

                if (!jobs.Any())
                    return jobs;

                // Get machine build envelope
                var machine = await _context.SlsMachines
                    .FirstOrDefaultAsync(m => m.MachineId == machineId);

                if (machine == null)
                    return jobs;

                // Simple optimization: group by material and priority
                var optimizedJobs = jobs
                    .GroupBy(j => j.SlsMaterial)
                    .SelectMany(group => group.OrderBy(j => j.Priority))
                    .ToList();

                // TODO: Implement more sophisticated 3D packing algorithm
                // For now, just return ordered by material compatibility and priority

                _logger.LogInformation("Optimized {JobCount} jobs for machine {MachineId} on {Date}",
                    optimizedJobs.Count, machineId, buildDate.Date);

                return optimizedJobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error optimizing build platform layout for machine {MachineId}", machineId);
                return new List<Job>();
            }
        }

        /// <summary>
        /// Validate operator availability based on scheduler settings
        /// </summary>
        public async Task<bool> ValidateOperatorAvailabilityAsync(Job job)
        {
            try
            {
                return await _settingsService.IsOperatorAvailableAsync(job.ScheduledStart, job.ScheduledEnd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating operator availability for job {PartNumber}", job.PartNumber);
                return true; // Default to available if check fails
            }
        }

        /// <summary>
        /// Validate scheduling constraints based on current settings
        /// </summary>
        public async Task<List<string>> ValidateSchedulingConstraintsAsync(Job job, List<Job> existingJobs)
        {
            var errors = new List<string>();

            try
            {
                var settings = await _settingsService.GetSettingsAsync();

                // Check weekend operations
                if (!await IsWeekendOperationAllowedAsync(job.ScheduledStart))
                {
                    if (job.ScheduledStart.DayOfWeek == DayOfWeek.Saturday && !settings.SaturdayOperations)
                    {
                        errors.Add("Saturday operations are not enabled in scheduler settings");
                    }
                    if (job.ScheduledStart.DayOfWeek == DayOfWeek.Sunday && !settings.SundayOperations)
                    {
                        errors.Add("Sunday operations are not enabled in scheduler settings");
                    }
                }

                // Check operator availability during shift hours
                if (!await ValidateOperatorAvailabilityAsync(job))
                {
                    errors.Add($"Job scheduled outside operator shift hours ({job.ScheduledStart:HH:mm} - {job.ScheduledEnd:HH:mm})");
                }

                // Check maximum jobs per machine per day
                var sameDay = job.ScheduledStart.Date;
                var jobsOnSameDay = existingJobs.Count(j => j.MachineId == job.MachineId && j.ScheduledStart.Date == sameDay);
                
                if (jobsOnSameDay >= settings.MaxJobsPerMachinePerDay)
                {
                    errors.Add($"Maximum {settings.MaxJobsPerMachinePerDay} jobs per machine per day exceeded");
                }

                // Check minimum time between jobs
                var previousJob = existingJobs
                    .Where(j => j.MachineId == job.MachineId && j.ScheduledEnd <= job.ScheduledStart)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .FirstOrDefault();

                if (previousJob != null)
                {
                    var timeBetween = (job.ScheduledStart - previousJob.ScheduledEnd).TotalMinutes;
                    if (timeBetween < settings.MinimumTimeBetweenJobsMinutes)
                    {
                        errors.Add($"Minimum {settings.MinimumTimeBetweenJobsMinutes} minutes required between jobs (only {timeBetween:F0} minutes available)");
                    }
                }

                // Check quality requirements
                if (settings.QualityCheckRequired && string.IsNullOrEmpty(job.QualityInspector))
                {
                    errors.Add("Quality inspector must be assigned when quality checks are required");
                }

                // Check operator certification requirements
                if (!string.IsNullOrEmpty(settings.RequiredOperatorCertification) && string.IsNullOrEmpty(job.Operator))
                {
                    errors.Add($"Operator with {settings.RequiredOperatorCertification} certification must be assigned");
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating scheduling constraints for job {PartNumber}", job.PartNumber);
                return new List<string> { "Error validating scheduling constraints" };
            }
        }

        /// <summary>
        /// Calculate optimal job start time considering settings and constraints
        /// </summary>
        public async Task<DateTime> CalculateJobStartTimeWithSettingsAsync(Job job, List<Job> existingJobs)
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                var proposedStart = job.ScheduledStart;

                // Get previous job on same machine
                var previousJob = existingJobs
                    .Where(j => j.MachineId == job.MachineId && j.ScheduledEnd <= proposedStart)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .FirstOrDefault();

                if (previousJob != null)
                {
                    // Add changeover time if material is different
                    var changeoverTime = TimeSpan.Zero;
                    if (previousJob.SlsMaterial != job.SlsMaterial)
                    {
                        var changeoverMinutes = await _settingsService.GetChangeoverTimeAsync(previousJob.SlsMaterial, job.SlsMaterial);
                        changeoverTime = TimeSpan.FromMinutes(changeoverMinutes);
                    }

                    // Add minimum time between jobs
                    var minimumGap = TimeSpan.FromMinutes(settings.MinimumTimeBetweenJobsMinutes);
                    
                    // Calculate earliest possible start
                    var earliestStart = previousJob.ScheduledEnd.Add(changeoverTime).Add(minimumGap);
                    
                    if (proposedStart < earliestStart)
                    {
                        proposedStart = earliestStart;
                    }
                }

                // Ensure start time is within shift hours
                proposedStart = AdjustToShiftHours(proposedStart, settings);

                return proposedStart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating job start time for {PartNumber}", job.PartNumber);
                return job.ScheduledStart; // Return original if calculation fails
            }
        }

        /// <summary>
        /// Calculate job end time including all processing steps based on settings
        /// </summary>
        public async Task<DateTime> CalculateJobEndTimeWithSettingsAsync(Job job)
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                var endTime = job.ScheduledStart;

                // Add preheating time if required
                if (job.RequiresPreheating)
                {
                    endTime = endTime.AddMinutes(job.PreheatingTimeMinutes > 0 ? job.PreheatingTimeMinutes : settings.DefaultPreheatingTimeMinutes);
                }

                // Add actual build time
                var buildTime = TimeSpan.FromHours(job.EstimatedHours);
                endTime = endTime.Add(buildTime);

                // Add cooling time
                var coolingMinutes = job.CoolingTimeMinutes > 0 ? job.CoolingTimeMinutes : settings.DefaultCoolingTimeMinutes;
                endTime = endTime.AddMinutes(coolingMinutes);

                // Add post-processing time if required
                if (job.RequiresPostProcessing)
                {
                    var postProcessingMinutes = job.PostProcessingTimeMinutes > 0 ? job.PostProcessingTimeMinutes : settings.DefaultPostProcessingTimeMinutes;
                    endTime = endTime.AddMinutes(postProcessingMinutes);
                }

                // Add setup buffer time
                endTime = endTime.AddMinutes(settings.SetupTimeBufferMinutes);

                return endTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating job end time for {PartNumber}", job.PartNumber);
                return job.ScheduledEnd; // Return original if calculation fails
            }
        }

        /// <summary>
        /// Check if weekend operations are allowed for the given date
        /// </summary>
        public async Task<bool> IsWeekendOperationAllowedAsync(DateTime jobDate)
        {
            try
            {
                // CRITICAL FIX: Use the SchedulerSettingsService method instead of direct settings check
                return await _settingsService.IsWeekendOperationAllowedAsync(jobDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking weekend operations for date {Date}", jobDate);
                return false; // Default to not allowed if check fails
            }
        }

        /// <summary>
        /// Calculate the next available start time for a new job on the specified machine
        /// Automatically finds the earliest possible start time considering existing jobs and constraints
        /// </summary>
        public async Task<DateTime> CalculateNextAvailableStartTimeAsync(string machineId, DateTime preferredDate, double estimatedDurationHours, string material, List<Job> existingJobs)
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                
                // Get the last completed or in-progress job on this machine
                var lastJob = existingJobs
                    .Where(j => j.MachineId == machineId)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .FirstOrDefault();

                var now = DateTime.UtcNow;
                var startTime = preferredDate > now ? preferredDate : now;
                
                // If there's a last job, start after it
                if (lastJob != null && lastJob.ScheduledEnd > startTime)
                {
                    startTime = lastJob.ScheduledEnd;
                    
                    // Add changeover time if materials are different
                    if (lastJob.SlsMaterial != material)
                    {
                        var changeoverTime = await _settingsService.GetChangeoverTimeAsync(lastJob.SlsMaterial, material);
                        startTime = startTime.AddMinutes(changeoverTime);
                    }
                    
                    // Add minimum time between jobs
                    startTime = startTime.AddMinutes(settings.MinimumTimeBetweenJobsMinutes);
                }

                // Ensure we're in a valid operating day
                var maxDaysToCheck = 30;
                var daysChecked = 0;
                
                while (daysChecked < maxDaysToCheck)
                {
                    // Check if this is a valid operating day
                    if (await IsWeekendOperationAllowedAsync(startTime))
                    {
                        // Adjust to nearest shift start
                        startTime = AdjustToShiftHours(startTime, settings);
                        
                        // Check for conflicts with existing jobs
                        var endTime = startTime.AddHours(estimatedDurationHours);
                        var conflicts = existingJobs.Where(j => 
                            j.MachineId == machineId &&
                            j.ScheduledStart < endTime &&
                            j.ScheduledEnd > startTime).ToList();
                        
                        if (!conflicts.Any())
                        {
                            _logger.LogInformation("Found optimal start time for machine {MachineId}: {StartTime} (searched {Days} days)", 
                                machineId, startTime, daysChecked);
                            return startTime;
                        }
                        
                        // Move past all conflicts
                        var latestConflictEnd = conflicts.Max(c => c.ScheduledEnd);
                        startTime = latestConflictEnd.AddMinutes(settings.MinimumTimeBetweenJobsMinutes);
                        
                        // If still same day, try again
                        if (startTime.Date == conflicts.First().ScheduledStart.Date)
                            continue;
                    }
                    
                    // Move to next day
                    startTime = startTime.Date.AddDays(1).Add(settings.StandardShiftStart);
                    daysChecked++;
                }

                // If no slot found, return a fallback time
                var fallbackTime = DateTime.UtcNow.Date.AddDays(maxDaysToCheck).Add(settings.StandardShiftStart);
                _logger.LogWarning("No available slot found for machine {MachineId} within {Days} days, returning fallback time: {FallbackTime}", 
                    machineId, maxDaysToCheck, fallbackTime);
                
                return fallbackTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating next available start time for machine {MachineId}", machineId);
                // Return a safe fallback time
                return DateTime.UtcNow.Date.AddDays(1).AddHours(8); // Tomorrow at 8 AM
            }
        }

        /// <summary>
        /// Get scheduling conflict information for a proposed time slot
        /// </summary>
        public async Task<(bool canSchedule, string reason, DateTime suggestedTime)> CheckSchedulingConflict(
            string machineId, DateTime proposedStart, double durationHours, string material, List<Job> existingJobs)
        {
            try
            {
                var proposedEnd = proposedStart.AddHours(durationHours);
                var settings = await _settingsService.GetSettingsAsync();

                // Check for direct overlaps
                var overlappingJobs = existingJobs
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart < proposedEnd && 
                               j.ScheduledEnd > proposedStart)
                    .ToList();

                if (overlappingJobs.Any())
                {
                    var nextAvailable = await CalculateNextAvailableStartTimeAsync(machineId, proposedStart, durationHours, material, existingJobs);
                    var conflictJob = overlappingJobs.First();
                    return (false, $"Conflicts with job '{conflictJob.PartNumber}' ({conflictJob.ScheduledStart:MM/dd HH:mm} - {conflictJob.ScheduledEnd:MM/dd HH:mm})", nextAvailable);
                }

                // Check material changeover requirements
                var previousJob = existingJobs
                    .Where(j => j.MachineId == machineId && j.ScheduledEnd <= proposedStart)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .FirstOrDefault();

                if (previousJob != null && previousJob.SlsMaterial != material)
                {
                    var changeoverTime = await _settingsService.GetChangeoverTimeAsync(previousJob.SlsMaterial, material);
                    var requiredStart = previousJob.ScheduledEnd.AddMinutes(Math.Max(changeoverTime, settings.MinimumTimeBetweenJobsMinutes));
                    
                    if (proposedStart < requiredStart)
                    {
                        return (false, $"Insufficient time for material changeover (need {changeoverTime} minutes)", requiredStart);
                    }
                }

                // Check weekend restrictions
                if (!await IsWeekendOperationAllowedAsync(proposedStart))
                {
                    if (proposedStart.DayOfWeek == DayOfWeek.Saturday || proposedStart.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var nextMonday = GetNextMonday(proposedStart).Add(settings.StandardShiftStart);
                        return (false, "Weekend operations not allowed", nextMonday);
                    }
                }

                // Check shift hours
                if (!IsTimeInAnyShift(proposedStart.TimeOfDay, settings))
                {
                    var adjustedTime = AdjustToShiftHours(proposedStart, settings);
                    return (false, "Proposed time is outside shift hours", adjustedTime);
                }

                return (true, "Time slot is available", proposedStart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking scheduling conflict for machine {MachineId} at {ProposedStart}", machineId, proposedStart);
                var fallbackTime = proposedStart.Date.AddDays(1).AddHours(8);
                return (false, "Error checking availability", fallbackTime);
            }
        }

        #region Private Helper Methods

        private (int dateCount, int slotsPerDay, int slotMinutes) GetZoomParameters(string zoom)
        {
            return zoom switch
            {
                "day" => (56, 1, 1440),     // Exactly 8 weeks (56 days), 1 slot per day (24 hours)
                "12h" => (28, 2, 720),      // 4 weeks, 2 slots per day (12 hours each)
                "10h" => (21, 2, 600),      // 3 weeks, 2 slots per day (10 hours each)  
                "8h" => (21, 3, 480),       // 3 weeks, 3 slots per day (8 hours each)
                "6h" => (14, 4, 360),       // 2 weeks, 4 slots per day (6 hours each)
                "4h" => (14, 6, 240),       // 2 weeks, 6 slots per day (4 hours each)
                "2h" => (7, 12, 120),       // 1 week, 12 slots per day (2 hours each)
                "hour" => (14, 24, 60),     // 2 weeks, 24 slots per day (1 hour each)
                "30min" => (7, 48, 30),     // 1 week, 48 slots per day (30 minutes each)
                "15min" => (3, 96, 15),     // 3 days, 96 slots per day (15 minutes each)
                _ => (56, 1, 1440)          // Default to 8-week day view
            };
        }

        private void ValidateSlsParameters(Job job, List<string> errors)
        {
            // Validate SLS-specific parameters
            if (job.LaserPowerWatts <= 0 || job.LaserPowerWatts > 2000)
                errors.Add("Laser power must be between 1 and 2000 watts");

            if (job.ScanSpeedMmPerSec <= 0 || job.ScanSpeedMmPerSec > 5000)
                errors.Add("Scan speed must be between 1 and 5000 mm/sec");

            if (job.LayerThicknessMicrons <= 0 || job.LayerThicknessMicrons > 200)
                errors.Add("Layer thickness must be between 1 and 200 microns");

            if (job.HatchSpacingMicrons <= 0 || job.HatchSpacingMicrons > 1000)
                errors.Add("Hatch spacing must be between 1 and 1000 microns");

            if (job.BuildTemperatureCelsius < 0 || job.BuildTemperatureCelsius > 500)
                errors.Add("Build temperature must be between 0 and 500 degrees Celsius");

            if (job.ArgonPurityPercent < 95 || job.ArgonPurityPercent > 100)
                errors.Add("Argon purity must be between 95% and 100%");

            if (job.OxygenContentPpm < 0 || job.OxygenContentPpm > 200)
                errors.Add("Oxygen content must be between 0 and 200 ppm");

            if (string.IsNullOrEmpty(job.SlsMaterial))
                errors.Add("SLS material must be specified");

            if (job.EstimatedPowderUsageKg < 0 || job.EstimatedPowderUsageKg > 50)
                errors.Add("Estimated powder usage must be between 0 and 50 kg");

            // Validate part number format (XX-XXXX)
            if (!System.Text.RegularExpressions.Regex.IsMatch(job.PartNumber, @"^\d{2}-\d{4}$"))
                errors.Add("Part number must be in format XX-XXXX (e.g., 14-5396)");
        }

        private async Task ValidateMaterialChangeoverAsync(Job job, List<Job> existingJobs, List<string> errors)
        {
            // Find the last job before this one on the same machine
            var previousJob = existingJobs
                .Where(j => j.MachineId == job.MachineId && j.ScheduledEnd <= job.ScheduledStart)
                .OrderByDescending(j => j.ScheduledEnd)
                .FirstOrDefault();

            if (previousJob != null && previousJob.SlsMaterial != job.SlsMaterial)
            {
                var changeoverTime = await _settingsService.GetChangeoverTimeAsync(previousJob.SlsMaterial, job.SlsMaterial);
                var availableTime = (job.ScheduledStart - previousJob.ScheduledEnd).TotalMinutes;

                if (availableTime < changeoverTime)
                {
                    errors.Add($"Insufficient time for powder changeover from {previousJob.SlsMaterial} to {job.SlsMaterial}. " +
                              $"Required: {changeoverTime} minutes, Available: {availableTime:F0} minutes");
                }
            }
        }

        private void ValidateBuildPlatformCapacity(Job job, List<Job> existingJobs, List<string> errors)
        {
            // Find concurrent jobs on the same machine (for build platform sharing)
            var concurrentJobs = existingJobs
                .Where(j => j.MachineId == job.MachineId &&
                           j.ScheduledStart < job.ScheduledEnd &&
                           j.ScheduledEnd > job.ScheduledStart)
                .ToList();

            if (concurrentJobs.Any())
            {
                // Check if materials are compatible for same build
                var incompatibleJobs = concurrentJobs
                    .Where(j => !job.IsCompatibleMaterial(j.SlsMaterial))
                    .ToList();

                if (incompatibleJobs.Any())
                {
                    errors.Add($"Material {job.SlsMaterial} is incompatible with concurrent jobs using: " +
                              string.Join(", ", incompatibleJobs.Select(j => j.SlsMaterial).Distinct()));
                }

                // TODO: Add 3D space validation for build platform layout
            }
        }

        private bool IsCompatibleForSameBuildPlatform(Job job1, Job job2)
        {
            // Jobs can share build platform if:
            // 1. They use compatible materials
            // 2. They have similar process parameters
            // 3. They fit within build envelope

            if (!job1.IsCompatibleMaterial(job2.SlsMaterial))
                return false;

            // Check process parameter compatibility (within 10% tolerance)
            var laserPowerDiff = Math.Abs(job1.LaserPowerWatts - job2.LaserPowerWatts) / Math.Max(job1.LaserPowerWatts, job2.LaserPowerWatts);
            var scanSpeedDiff = Math.Abs(job1.ScanSpeedMmPerSec - job2.ScanSpeedMmPerSec) / Math.Max(job1.ScanSpeedMmPerSec, job2.ScanSpeedMmPerSec);
            var tempDiff = Math.Abs(job1.BuildTemperatureCelsius - job2.BuildTemperatureCelsius) / Math.Max(job1.BuildTemperatureCelsius, job2.BuildTemperatureCelsius);

            return laserPowerDiff <= 0.1 && scanSpeedDiff <= 0.1 && tempDiff <= 0.05; // 5% temperature tolerance
        }

        private int CalculateComplexityBonus(List<Job> jobs)
        {
            if (!jobs.Any())
                return 0;

            var complexityScore = 0;

            // Add complexity based on number of different materials
            var materialCount = jobs.Select(j => j.SlsMaterial).Distinct().Count();
            complexityScore += materialCount * 10;

            // Add complexity based on priority spread
            var priorities = jobs.Select(j => j.Priority).ToList();
            var prioritySpread = priorities.Max() - priorities.Min();
            complexityScore += prioritySpread * 5;

            // Add complexity for rush jobs
            var rushJobCount = jobs.Count(j => j.IsRushJob);
            complexityScore += rushJobCount * 15;

            return Math.Min(80, complexityScore); // Cap at 80px bonus
        }

        private DateTime AdjustToShiftHours(DateTime proposedTime, SchedulerSettings settings)
        {
            var timeOfDay = proposedTime.TimeOfDay;
            var date = proposedTime.Date;

            // Check if time falls within any shift
            if (IsTimeInShift(timeOfDay, settings.StandardShiftStart, settings.StandardShiftEnd) ||
                IsTimeInShift(timeOfDay, settings.EveningShiftStart, settings.EveningShiftEnd) ||
                IsTimeInShift(timeOfDay, settings.NightShiftStart, settings.NightShiftEnd))
            {
                return proposedTime; // Already in shift hours
            }

            // Adjust to next available shift start
            var standardStart = date.Add(settings.StandardShiftStart);
            var eveningStart = date.Add(settings.EveningShiftStart);
            var nightStart = date.Add(settings.NightShiftStart);

            // Find the next shift start after proposed time
            var nextShifts = new[] { standardStart, eveningStart, nightStart }
                .Where(shift => shift > proposedTime)
                .OrderBy(shift => shift);

            if (nextShifts.Any())
            {
                return nextShifts.First();
            }

            // If no shift today, use standard shift start tomorrow
            return date.AddDays(1).Add(settings.StandardShiftStart);
        }

        private static bool IsTimeInShift(TimeSpan time, TimeSpan shiftStart, TimeSpan shiftEnd)
        {
            if (shiftStart <= shiftEnd)
            {
                // Normal shift (e.g., 7:00 - 15:00)
                return time >= shiftStart && time <= shiftEnd;
            }
            else
            {
                // Night shift crossing midnight (e.g., 23:00 - 07:00)
                return time >= shiftStart || time <= shiftEnd;
            }
        }

        /// <summary>
        /// Check if a time falls within any configured shift
        /// </summary>
        private bool IsTimeInAnyShift(TimeSpan time, SchedulerSettings settings)
        {
            return IsTimeInShift(time, settings.StandardShiftStart, settings.StandardShiftEnd) ||
                   IsTimeInShift(time, settings.EveningShiftStart, settings.EveningShiftEnd) ||
                   IsTimeInShift(time, settings.NightShiftStart, settings.NightShiftEnd);
        }

        /// <summary>
        /// Get next Monday from a given date
        /// </summary>
        private DateTime GetNextMonday(DateTime date)
        {
            var daysUntilMonday = ((int)DayOfWeek.Monday - (int)date.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && date.DayOfWeek != DayOfWeek.Monday)
                daysUntilMonday = 7;
            
            return date.Date.AddDays(daysUntilMonday);
        }

        #endregion
    }
}