using OpCentrix.Models;
using OpCentrix.Models.ViewModels;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Services.Admin;

namespace OpCentrix.Services
{
    public interface ISchedulerService
    {
        SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null);
        Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom = null, DateTime? startDate = null);
        bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors);
        (int maxLayers, int rowHeight) CalculateMachineRowLayout(string machineId, List<Job> jobs);
        Task<bool> ValidateSlsJobCompatibilityAsync(Job job, SchedulerContext context);
        Task<double> CalculateOptimalPowderChangeoverTimeAsync(string machineId, string newMaterial, SchedulerContext context);
        Task<List<string>> GetMachineCapabilityIssuesAsync(Job job, SchedulerContext context);
        Task<decimal> CalculateSlsJobCostEstimateAsync(Job job, SchedulerContext context);
        Task<List<Job>> OptimizeBuildPlatformLayoutAsync(string machineId, DateTime buildDate, SchedulerContext context);
    }

    public class SchedulerService : ISchedulerService
    {
        private readonly ILogger<SchedulerService> _logger;
        private readonly IOperatingShiftService _operatingShiftService;
        
        public SchedulerService(ILogger<SchedulerService> logger, IOperatingShiftService operatingShiftService)
        {
            _logger = logger;
            _operatingShiftService = operatingShiftService;
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

        public async Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom = null, DateTime? startDate = null)
        {
            // For now, delegate to the synchronous version
            // In a real implementation, this would use async database calls
            return await Task.FromResult(GetSchedulerData(zoom, startDate));
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

                // Material changeover validation
                ValidateMaterialChangeover(job, existingJobs, errors);

                // Build platform capacity validation
                ValidateBuildPlatformCapacity(job, existingJobs, errors);

                // Task 8: Basic operating hours validation (simplified for sync method)
                ValidateBasicOperatingHours(job, errors);

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

        public async Task<bool> ValidateSlsJobCompatibilityAsync(Job job, SchedulerContext context)
        {
            try
            {
                // Get machine configuration
                var machine = await context.SlsMachines
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

        public async Task<double> CalculateOptimalPowderChangeoverTimeAsync(string machineId, string newMaterial, SchedulerContext context)
        {
            try
            {
                // Get machine current material
                var machine = await context.SlsMachines
                    .FirstOrDefaultAsync(m => m.MachineId == machineId);

                if (machine == null)
                    return 0;

                // Get the last job to determine current material
                var lastJob = await context.Jobs
                    .Where(j => j.MachineId == machineId && j.Status == "Completed")
                    .OrderByDescending(j => j.ActualEnd ?? j.ScheduledEnd)
                    .FirstOrDefaultAsync();

                var currentMaterial = lastJob?.SlsMaterial ?? machine.CurrentMaterial;

                if (string.IsNullOrEmpty(currentMaterial) || currentMaterial == newMaterial)
                    return 0; // No changeover needed

                // Calculate changeover time based on material compatibility
                var changeoverMatrix = new Dictionary<(string from, string to), double>
                {
                    // Same material family - quick changeover
                    [("Ti-6Al-4V Grade 5", "Ti-6Al-4V ELI Grade 23")] = 30,
                    [("Ti-6Al-4V ELI Grade 23", "Ti-6Al-4V Grade 5")] = 30,
                    [("Inconel 718", "Inconel 625")] = 45,
                    [("Inconel 625", "Inconel 718")] = 45,
                    
                    // Different material families - full cleaning required
                    [("Ti-6Al-4V Grade 5", "Inconel 718")] = 120,
                    [("Ti-6Al-4V Grade 5", "Inconel 625")] = 120,
                    [("Ti-6Al-4V ELI Grade 23", "Inconel 718")] = 120,
                    [("Ti-6Al-4V ELI Grade 23", "Inconel 625")] = 120,
                    [("Inconel 718", "Ti-6Al-4V Grade 5")] = 120,
                    [("Inconel 718", "Ti-6Al-4V ELI Grade 23")] = 120,
                    [("Inconel 625", "Ti-6Al-4V Grade 5")] = 120,
                    [("Inconel 625", "Ti-6Al-4V ELI Grade 23")] = 120
                };

                return changeoverMatrix.GetValueOrDefault((currentMaterial, newMaterial), 60); // Default 1 hour
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating powder changeover time for machine {MachineId}", machineId);
                return 60; // Default fallback
            }
        }

        public async Task<List<string>> GetMachineCapabilityIssuesAsync(Job job, SchedulerContext context)
        {
            var issues = new List<string>();

            try
            {
                var machine = await context.SlsMachines
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
                    issues.Add($"Required layer thickness ({job.LayerThicknessMicrons}?m) below machine minimum ({machine.MinLayerThicknessMicrons}?m)");

                if (job.LayerThicknessMicrons > machine.MaxLayerThicknessMicrons)
                    issues.Add($"Required layer thickness ({job.LayerThicknessMicrons}?m) above machine maximum ({machine.MaxLayerThicknessMicrons}?m)");

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

        public async Task<decimal> CalculateSlsJobCostEstimateAsync(Job job, SchedulerContext context)
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

                // Add powder changeover cost if needed
                var changeoverTime = await CalculateOptimalPowderChangeoverTimeAsync(job.MachineId, job.SlsMaterial, context);
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

        public async Task<List<Job>> OptimizeBuildPlatformLayoutAsync(string machineId, DateTime buildDate, SchedulerContext context)
        {
            try
            {
                // Get all jobs scheduled for this machine on this date
                var jobs = await context.Jobs
                    .Include(j => j.Part)
                    .Where(j => j.MachineId == machineId && 
                               j.ScheduledStart.Date == buildDate.Date)
                    .OrderBy(j => j.Priority)
                    .ThenBy(j => j.ScheduledStart)
                    .ToListAsync();

                if (!jobs.Any())
                    return jobs;

                // Get machine build envelope
                var machine = await context.SlsMachines
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
                "day" => (7, 1, 1440),      // 7 days, 1 slot per day (24 hours)
                "hour" => (3, 24, 60),      // 3 days, 24 slots per day (1 hour each)
                "30min" => (2, 48, 30),     // 2 days, 48 slots per day (30 minutes each)
                "15min" => (1, 96, 15),     // 1 day, 96 slots per day (15 minutes each)
                _ => (7, 1, 1440)           // Default to day view
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
                errors.Add("Build temperature must be between 0 and 500°C");

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

        private void ValidateMaterialChangeover(Job job, List<Job> existingJobs, List<string> errors)
        {
            // Find the last job before this one on the same machine
            var previousJob = existingJobs
                .Where(j => j.MachineId == job.MachineId && j.ScheduledEnd <= job.ScheduledStart)
                .OrderByDescending(j => j.ScheduledEnd)
                .FirstOrDefault();

            if (previousJob != null && previousJob.SlsMaterial != job.SlsMaterial)
            {
                var changeoverTime = job.CalculatePowderChangeoverTime(previousJob.SlsMaterial);
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

        private void ValidateBasicOperatingHours(Job job, List<string> errors)
        {
            try
            {
                // Basic business hours validation (8 AM to 5 PM, Monday-Friday)
                // TODO: This will be enhanced to use IOperatingShiftService in async context
                
                // Check job start time
                if (!IsWithinBasicBusinessHours(job.ScheduledStart))
                {
                    errors.Add($"Job start time {job.ScheduledStart:MM/dd/yyyy HH:mm} is outside standard operating hours (Mon-Fri, 8 AM - 5 PM)");
                }
                
                // Check job end time
                if (!IsWithinBasicBusinessHours(job.ScheduledEnd))
                {
                    errors.Add($"Job end time {job.ScheduledEnd:MM/dd/yyyy HH:mm} is outside standard operating hours (Mon-Fri, 8 AM - 5 PM)");
                }
                
                // Note: For full operating shift validation, use ValidateOperatingHoursAsync in async contexts
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating basic operating hours for job {PartNumber}", job.PartNumber);
                errors.Add("Unable to validate operating hours");
            }
        }

        private bool IsWithinBasicBusinessHours(DateTime dateTime)
        {
            // Basic business hours validation (8 AM to 5 PM, Monday-Friday)
            var dayOfWeek = dateTime.DayOfWeek;
            var timeOfDay = dateTime.TimeOfDay;
            
            // Weekend check
            if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }
            
            // Business hours check (8 AM to 5 PM)
            var startTime = new TimeSpan(8, 0, 0);   // 8:00 AM
            var endTime = new TimeSpan(17, 0, 0);    // 5:00 PM
            
            return timeOfDay >= startTime && timeOfDay <= endTime;
        }
        #endregion
    }
}