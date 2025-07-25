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
            // Validate and set zoom level
            var validZooms = new[] { "day", "hour", "30min", "15min" };
            if (!validZooms.Contains(zoom))
                zoom = "day";

            // UPDATED: Start from UTC today instead of server local time
            var start = startDate ?? DateTime.UtcNow.Date;

            // Define SLS machine configuration
            var machines = new List<string> { "TI1", "TI2", "INC" };

            // Calculate view parameters based on zoom
            var (dateCount, slotsPerDay, slotMinutes) = GetZoomParameters(zoom);

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
                var settings = await _settingsService.GetSettingsAsync();
                
                if (!settings.EnableWeekendOperations)
                    return jobDate.DayOfWeek != DayOfWeek.Saturday && jobDate.DayOfWeek != DayOfWeek.Sunday;

                return (jobDate.DayOfWeek != DayOfWeek.Saturday || settings.SaturdayOperations) &&
                       (jobDate.DayOfWeek != DayOfWeek.Sunday || settings.SundayOperations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking weekend operations for date {Date}", jobDate);
                return false; // Default to not allowed if check fails
            }
        }

        #region Private Helper Methods

        private DateTime GetMondayOfCurrentWeek()
        {
            // UPDATED: Start from UTC today instead of Monday of current week
            return DateTime.UtcNow.Date;
        }

        private (int dateCount, int slotsPerDay, int slotMinutes) GetZoomParameters(string zoom)
        {
            return zoom switch
            {
                "day" => (56, 1, 1440),     // Exactly 8 weeks (56 days), 1 slot per day (24 hours)
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
        /// Calculate the next available start time for a new job on the specified machine
        /// Automatically finds the earliest possible start time considering existing jobs and constraints
        /// </summary>
        public async Task<DateTime> CalculateNextAvailableStartTimeAsync(string machineId, DateTime preferredDate, double estimatedDurationHours, string material, List<Job> existingJobs)
        {
            try
            {
                var settings = await _settingsService.GetSettingsAsync();
                
                // Step 1: Determine the earliest possible start time based on multiple factors
                var earliestPossibleStart = await DetermineEarliestPossibleStartTime(machineId, preferredDate, material, existingJobs, settings);
                
                var currentSearchTime = earliestPossibleStart;
                var maxSearchDays = 90; // Search up to 90 days ahead
                var searchedDays = 0;
                
                _logger.LogDebug("Enhanced search for next available start time for machine {MachineId} starting from {StartTime}", 
                    machineId, currentSearchTime);

                while (searchedDays < maxSearchDays)
                {
                    // Check if weekend operations are allowed for this date
                    if (!await IsValidOperatingDay(currentSearchTime, settings))
                    {
                        currentSearchTime = GetNextValidOperatingDay(currentSearchTime, settings);
                        searchedDays += GetDaysToNextValidDay(currentSearchTime);
                        continue;
                    }

                    // Adjust to valid shift hours if needed
                    currentSearchTime = AdjustToNearestShiftStart(currentSearchTime, settings);

                    // Get jobs that could conflict with our search window
                    var potentialConflicts = GetRelevantConflictingJobs(machineId, currentSearchTime, estimatedDurationHours, existingJobs);

                    // Try to find a slot in the current day, considering all constraints
                    var availableSlot = await FindOptimalSlotInDay(currentSearchTime, estimatedDurationHours, material, potentialConflicts, settings);
                    
                    if (availableSlot.HasValue)
                    {
                        _logger.LogInformation("Found optimal start time for machine {MachineId}: {StartTime} (searched {Days} days)", 
                            machineId, availableSlot.Value, searchedDays);
                        return availableSlot.Value;
                    }

                    // Move to next day
                    currentSearchTime = GetNextOperatingDayStart(currentSearchTime, settings);
                    searchedDays++;
                }

                // If no slot found within search range, return a time far in the future
                var fallbackTime = DateTime.UtcNow.Date.AddDays(maxSearchDays).Add(settings.StandardShiftStart);
                _logger.LogWarning("No available slot found for machine {MachineId} within {Days} days, returning fallback time: {FallbackTime}", 
                    machineId, maxSearchDays, fallbackTime);
                
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
        /// Determine the earliest possible start time considering last job, current time, and preferences
        /// </summary>
        private async Task<DateTime> DetermineEarliestPossibleStartTime(string machineId, DateTime preferredDate, string material, List<Job> existingJobs, SchedulerSettings settings)
        {
            var now = DateTime.UtcNow;
            var preferredStart = preferredDate > now ? preferredDate : now;

            // Get the last completed or in-progress job on this machine
            var lastJob = existingJobs
                .Where(j => j.MachineId == machineId)
                .OrderByDescending(j => j.ScheduledEnd)
                .FirstOrDefault();

            if (lastJob == null)
            {
                // No previous jobs, can start at preferred time or next shift
                return preferredStart.Date < now.Date 
                    ? now.Date.Add(settings.StandardShiftStart)
                    : AdjustToNearestShiftStart(preferredStart, settings);
            }

            // Calculate minimum start time based on last job end time
            var minimumStartAfterLastJob = await CalculateMinimumStartTimeAfterJob(lastJob, material, settings);

            // If the last job ends after our preferred start, we must wait
            if (minimumStartAfterLastJob > preferredStart)
            {
                _logger.LogDebug("Adjusting start time from {PreferredStart} to {MinimumStart} due to last job ending at {LastJobEnd}",
                    preferredStart, minimumStartAfterLastJob, lastJob.ScheduledEnd);
                return minimumStartAfterLastJob;
            }

            // If we're scheduling for today and it's past the preferred time, start from now + buffer
            if (preferredStart.Date == now.Date && preferredStart < now)
            {
                var nowPlusBuffer = now.AddMinutes(settings.SetupTimeBufferMinutes);
                return Math.Max(minimumStartAfterLastJob.Ticks, nowPlusBuffer.Ticks) > preferredStart.Ticks
                    ? new DateTime(Math.Max(minimumStartAfterLastJob.Ticks, nowPlusBuffer.Ticks))
                    : preferredStart;
            }

            return preferredStart;
        }

        /// <summary>
        /// Calculate the minimum start time after a previous job, including changeover and gap requirements
        /// </summary>
        private async Task<DateTime> CalculateMinimumStartTimeAfterJob(Job previousJob, string newMaterial, SchedulerSettings settings)
        {
            var minimumStart = previousJob.ScheduledEnd;

            // Add minimum time between jobs
            minimumStart = minimumStart.AddMinutes(settings.MinimumTimeBetweenJobsMinutes);

            // Add material changeover time if materials are different
            if (previousJob.SlsMaterial != newMaterial)
            {
                var changeoverTime = await _settingsService.GetChangeoverTimeAsync(previousJob.SlsMaterial, newMaterial);
                minimumStart = minimumStart.AddMinutes(changeoverTime);
                
                _logger.LogDebug("Added changeover time from {FromMaterial} to {ToMaterial}: {ChangeoverMinutes} minutes",
                    previousJob.SlsMaterial, newMaterial, changeoverTime);
            }

            // Ensure the start time falls within shift hours
            return AdjustToNearestShiftStart(minimumStart, settings);
        }

        /// <summary>
        /// Check if a given date allows operations based on weekend settings
        /// </summary>
        private async Task<bool> IsValidOperatingDay(DateTime date, SchedulerSettings settings)
        {
            if (date.DayOfWeek == DayOfWeek.Saturday)
                return settings.EnableWeekendOperations && settings.SaturdayOperations;
            
            if (date.DayOfWeek == DayOfWeek.Sunday)
                return settings.EnableWeekendOperations && settings.SundayOperations;
            
            return true; // Weekdays are always valid
        }

        /// <summary>
        /// Get the next valid operating day based on weekend settings
        /// </summary>
        private DateTime GetNextValidOperatingDay(DateTime currentDate, SchedulerSettings settings)
        {
            var nextDay = currentDate.Date.AddDays(1);
            
            while (!IsValidOperatingDay(nextDay, settings).Result)
            {
                nextDay = nextDay.AddDays(1);
            }
            
            return nextDay.Add(settings.StandardShiftStart);
        }

        /// <summary>
        /// Calculate how many days to skip to reach the next valid operating day
        /// </summary>
        private int GetDaysToNextValidDay(DateTime currentDate)
        {
            if (currentDate.DayOfWeek == DayOfWeek.Saturday)
                return 2; // Skip to Monday
            if (currentDate.DayOfWeek == DayOfWeek.Sunday)
                return 1; // Skip to Monday
            return 1; // Normal next day
        }

        /// <summary>
        /// Adjust time to the nearest shift start, considering all three shifts
        /// </summary>
        private DateTime AdjustToNearestShiftStart(DateTime proposedTime, SchedulerSettings settings)
        {
            var timeOfDay = proposedTime.TimeOfDay;
            var date = proposedTime.Date;

            // Check if already within a shift (with small buffer)
            if (IsTimeInAnyShift(timeOfDay, settings))
            {
                return proposedTime;
            }

            // Find the next shift start
            var shiftStarts = new[]
            {
                new { Time = settings.StandardShiftStart, Name = "Standard" },
                new { Time = settings.EveningShiftStart, Name = "Evening" },
                new { Time = settings.NightShiftStart, Name = "Night" }
            }.OrderBy(s => s.Time).ToList();

            // Find next shift start on the same day
            var nextShiftToday = shiftStarts
                .Where(s => s.Time > timeOfDay)
                .FirstOrDefault();

            if (nextShiftToday != null)
            {
                var result = date.Add(nextShiftToday.Time);
                _logger.LogDebug("Adjusted time from {Original} to {Adjusted} ({ShiftName} shift start)",
                    proposedTime, result, nextShiftToday.Name);
                return result;
            }

            // No more shifts today, go to first shift tomorrow
            var tomorrowStart = date.AddDays(1).Add(settings.StandardShiftStart);
            _logger.LogDebug("Adjusted time from {Original} to {Adjusted} (next day standard shift)",
                proposedTime, tomorrowStart);
            return tomorrowStart;
        }

        /// <summary>
        /// Get the start of the next operating day
        /// </summary>
        private DateTime GetNextOperatingDayStart(DateTime currentDate, SchedulerSettings settings)
        {
            var nextDay = currentDate.Date.AddDays(1);
            
            // Skip weekends if not allowed
            while (!IsValidOperatingDay(nextDay, settings).Result)
            {
                nextDay = nextDay.AddDays(1);
            }
            
            return nextDay.Add(settings.StandardShiftStart);
        }

        /// <summary>
        /// Get jobs that could potentially conflict with our time window (optimized query)
        /// </summary>
        private List<Job> GetRelevantConflictingJobs(string machineId, DateTime searchStart, double durationHours, List<Job> existingJobs)
        {
            var searchEnd = searchStart.AddHours(durationHours);
            var bufferHours = 4; // Look 4 hours before and after for context
            
            return existingJobs
                .Where(j => j.MachineId == machineId &&
                           j.ScheduledEnd >= searchStart.AddHours(-bufferHours) &&
                           j.ScheduledStart <= searchEnd.AddHours(bufferHours))
                .OrderBy(j => j.ScheduledStart)
                .ToList();
        }

        /// <summary>
        /// Find an optimal time slot within a specific day with enhanced logic
        /// </summary>
        private async Task<DateTime?> FindOptimalSlotInDay(DateTime dayStart, double durationHours, string material, 
            List<Job> existingJobs, SchedulerSettings settings)
        {
            try
            {
                var shifts = GetDayShifts(dayStart, settings);
                var durationTimeSpan = TimeSpan.FromHours(durationHours);

                // Try each shift in order of preference
                foreach (var shift in shifts)
                {
                    var slotInShift = await FindSlotInShift(shift.Start, shift.End, durationTimeSpan, material, existingJobs, settings);
                    if (slotInShift.HasValue)
                    {
                        _logger.LogDebug("Found slot in {ShiftName} shift: {StartTime}", shift.Name, slotInShift.Value);
                        return slotInShift.Value;
                    }
                }

                return null; // No slot available in any shift this day
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding optimal slot in day starting {DayStart}", dayStart);
                return null;
            }
        }

        /// <summary>
        /// Get all shifts for a given day
        /// </summary>
        private List<(DateTime Start, DateTime End, string Name)> GetDayShifts(DateTime date, SchedulerSettings settings)
        {
            var day = date.Date;
            var shifts = new List<(DateTime Start, DateTime End, string Name)>();

            // Standard shift
            shifts.Add((
                day.Add(settings.StandardShiftStart),
                day.Add(settings.StandardShiftEnd),
                "Standard"
            ));

            // Evening shift
            shifts.Add((
                day.Add(settings.EveningShiftStart),
                day.Add(settings.EveningShiftEnd),
                "Evening"
            ));

            // Night shift (may cross midnight)
            var nightStart = day.Add(settings.NightShiftStart);
            var nightEnd = settings.NightShiftEnd < settings.NightShiftStart
                ? day.AddDays(1).Add(settings.NightShiftEnd)  // Crosses midnight
                : day.Add(settings.NightShiftEnd);
            
            shifts.Add((nightStart, nightEnd, "Night"));

            return shifts.Where(s => s.Start < s.End).ToList(); // Filter out invalid shifts
        }

        /// <summary>
        /// Find a slot within a specific shift
        /// </summary>
        private async Task<DateTime?> FindSlotInShift(DateTime shiftStart, DateTime shiftEnd, TimeSpan duration, 
            string material, List<Job> existingJobs, SchedulerSettings settings)
        {
            var currentTime = shiftStart;
            var timeIncrement = TimeSpan.FromMinutes(15); // Check every 15 minutes for better precision
            
            while (currentTime.Add(duration) <= shiftEnd)
            {
                if (await IsTimeSlotAvailableEnhanced(currentTime, duration, material, existingJobs, settings))
                {
                    return currentTime;
                }
                
                currentTime = currentTime.Add(timeIncrement);
            }
            
            return null;
        }

        /// <summary>
        /// Enhanced time slot availability check with better conflict resolution
        /// </summary>
        private async Task<bool> IsTimeSlotAvailableEnhanced(DateTime startTime, TimeSpan duration, string material, 
            List<Job> existingJobs, SchedulerSettings settings)
        {
            try
            {
                var endTime = startTime.Add(duration);

                // Check for direct overlaps
                var directOverlaps = existingJobs.Where(job => 
                    job.ScheduledStart < endTime && job.ScheduledEnd > startTime).ToList();

                if (directOverlaps.Any())
                {
                    _logger.LogTrace("Direct overlap found at {StartTime} with jobs: {JobList}",
                        startTime, string.Join(", ", directOverlaps.Select(j => j.PartNumber)));
                    return false;
                }

                // Check previous job for changeover requirements
                var previousJob = existingJobs
                    .Where(j => j.ScheduledEnd <= startTime)
                    .OrderByDescending(j => j.ScheduledEnd)
                    .FirstOrDefault();

                if (previousJob != null)
                {
                    var requiredGap = TimeSpan.FromMinutes(settings.MinimumTimeBetweenJobsMinutes);
                    
                    if (previousJob.SlsMaterial != material)
                    {
                        var changeoverTime = await _settingsService.GetChangeoverTimeAsync(previousJob.SlsMaterial, material);
                        requiredGap = TimeSpan.FromMinutes(Math.Max(changeoverTime, settings.MinimumTimeBetweenJobsMinutes));
                    }
                    
                    if (startTime < previousJob.ScheduledEnd.Add(requiredGap))
                    {
                        _logger.LogTrace("Insufficient gap after previous job at {StartTime}. Required: {RequiredGap}, Available: {AvailableGap}",
                            startTime, requiredGap, startTime - previousJob.ScheduledEnd);
                        return false;
                    }
                }

                // Check next job for changeover requirements
                var nextJob = existingJobs
                    .Where(j => j.ScheduledStart >= endTime)
                    .OrderBy(j => j.ScheduledStart)
                    .FirstOrDefault();

                if (nextJob != null)
                {
                    var requiredGap = TimeSpan.FromMinutes(settings.MinimumTimeBetweenJobsMinutes);
                    
                    if (nextJob.SlsMaterial != material)
                    {
                        var changeoverTime = await _settingsService.GetChangeoverTimeAsync(material, nextJob.SlsMaterial);
                        requiredGap = TimeSpan.FromMinutes(Math.Max(changeoverTime, settings.MinimumTimeBetweenJobsMinutes));
                    }
                    
                    if (nextJob.ScheduledStart < endTime.Add(requiredGap))
                    {
                        _logger.LogTrace("Insufficient gap before next job at {StartTime}. Required: {RequiredGap}, Available: {AvailableGap}",
                            startTime, requiredGap, nextJob.ScheduledStart - endTime);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enhanced time slot availability at {StartTime}", startTime);
                return false;
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