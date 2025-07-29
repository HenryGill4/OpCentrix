using OpCentrix.Models;
using OpCentrix.ViewModels.Scheduler;
using OpCentrix.Data;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Services.Admin;

namespace OpCentrix.Services
{
    public interface ISchedulerService
    {
        // ADDED: Synchronous method for backward compatibility
        SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null);
        Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom = null, DateTime? startDate = null);
        bool ValidateJobScheduling(Job job, List<Job> existingJobs, out List<string> errors);
        Task<(bool IsValid, List<string> Errors)> ValidateJobSchedulingAsync(Job job, List<Job> existingJobs);
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
        private readonly IMachineManagementService _machineManagementService;
        
        public SchedulerService(
            ILogger<SchedulerService> logger, 
            IOperatingShiftService operatingShiftService,
            IMachineManagementService machineManagementService)
        {
            _logger = logger;
            _operatingShiftService = operatingShiftService;
            _machineManagementService = machineManagementService;
        }

        // ADDED: Synchronous method for backward compatibility
        public SchedulerPageViewModel GetSchedulerData(string? zoom = null, DateTime? startDate = null)
        {
            try
            {
                // Task 9: Enhanced zoom validation with new levels
                var validZooms = new[] { "2month", "month", "week", "day", "12h", "6h", "4h", "2h", "1h", "hour", "30min", "15min" };
                if (!validZooms.Contains(zoom))
                    zoom = "week"; // Default to week view

                // Start from UTC today instead of server local time
                var start = startDate ?? DateTime.UtcNow.Date;

                // Calculate view parameters based on zoom
                var (dateCount, slotsPerDay, slotMinutes) = GetZoomParameters(zoom);

                // Generate date range starting from UTC today
                var dates = Enumerable.Range(0, dateCount)
                    .Select(i => start.AddDays(i))
                    .ToList();

                // For synchronous method, return basic structure with empty machines list
                // This will be populated by the async version or controller
                _logger.LogInformation("Generated basic scheduler data for {DateCount} days starting {StartDate}, zoom: {Zoom}", 
                    dateCount, start.ToString("yyyy-MM-dd"), zoom);

                return new SchedulerPageViewModel
                {
                    StartDate = start,
                    Dates = dates,
                    Machines = new List<string>(), // Will be populated by async method or controller
                    SlotsPerDay = slotsPerDay,
                    SlotMinutes = slotMinutes,
                    Jobs = new List<Job>(), // Will be populated by controller
                    MachineRowHeights = new Dictionary<string, int>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating basic scheduler data");
                
                // Return fallback data structure
                return new SchedulerPageViewModel
                {
                    StartDate = DateTime.UtcNow.Date,
                    Dates = new List<DateTime> { DateTime.UtcNow.Date },
                    Machines = new List<string>(),
                    SlotsPerDay = 1,
                    SlotMinutes = 1440,
                    Jobs = new List<Job>(),
                    MachineRowHeights = new Dictionary<string, int>()
                };
            }
        }

        public async Task<SchedulerPageViewModel> GetSchedulerDataAsync(string? zoom = null, DateTime? startDate = null)
        {
            try
            {
                // Task 9: Enhanced zoom validation with new levels
                var validZooms = new[] { "2month", "month", "week", "day", "12h", "6h", "4h", "2h", "1h", "hour", "30min", "15min" };
                if (!validZooms.Contains(zoom))
                    zoom = "week"; // Default to week view

                // Start from UTC today instead of server local time
                var start = startDate ?? DateTime.UtcNow.Date;

                // UPDATED: Get machines dynamically from database instead of hardcoded list
                var activeMachines = await _machineManagementService.GetActiveMachinesAsync();
                var machineIds = activeMachines.Select(m => m.MachineId).ToList();

                // Fallback to empty list if no machines configured yet
                if (!machineIds.Any())
                {
                    _logger.LogWarning("No active machines found in database");
                    machineIds = new List<string>();
                }

                // Calculate view parameters based on zoom
                var (dateCount, slotsPerDay, slotMinutes) = GetZoomParameters(zoom);

                // Generate date range starting from UTC today
                var dates = Enumerable.Range(0, dateCount)
                    .Select(i => start.AddDays(i))
                    .ToList();

                _logger.LogInformation("Generated scheduler data for {MachineCount} machines, {DateCount} days starting {StartDate}, zoom: {Zoom}", 
                    machineIds.Count, dateCount, start.ToString("yyyy-MM-dd"), zoom);

                return new SchedulerPageViewModel
                {
                    StartDate = start,
                    Dates = dates,
                    Machines = machineIds,
                    SlotsPerDay = slotsPerDay,
                    SlotMinutes = slotMinutes,
                    Jobs = new List<Job>(), // Will be populated by controller
                    MachineRowHeights = new Dictionary<string, int>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating scheduler data");
                
                // Return fallback data structure
                return new SchedulerPageViewModel
                {
                    StartDate = DateTime.UtcNow.Date,
                    Dates = new List<DateTime> { DateTime.UtcNow.Date },
                    Machines = new List<string>(),
                    SlotsPerDay = 1,
                    SlotMinutes = 1440,
                    Jobs = new List<Job>(),
                    MachineRowHeights = new Dictionary<string, int>()
                };
            }
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

        public async Task<(bool IsValid, List<string> Errors)> ValidateJobSchedulingAsync(Job job, List<Job> existingJobs)
        {
            var errors = new List<string>();

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

                // Validate machine exists and is available
                var machine = await _machineManagementService.GetMachineByMachineIdAsync(job.MachineId);
                if (machine == null)
                {
                    errors.Add($"Machine '{job.MachineId}' not found");
                }
                else if (!machine.IsActive)
                {
                    errors.Add($"Machine '{job.MachineId}' is not active");
                }
                else if (!machine.IsAvailableForScheduling)
                {
                    errors.Add($"Machine '{job.MachineId}' is not available for scheduling");
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

                // Task 8: Full operating hours validation using OperatingShiftService
                var operatingHoursErrors = await ValidateOperatingHoursAsync(job);
                errors.AddRange(operatingHoursErrors);

                _logger.LogInformation("Async job validation completed for {PartNumber} on {MachineId}: {ErrorCount} errors found", 
                    job.PartNumber, job.MachineId, errors.Count);

                return (errors.Count == 0, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating job scheduling for {PartNumber}", job.PartNumber);
                errors.Add("An error occurred during validation. Please try again.");
                return (false, errors);
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
                // Get machine configuration using the machine management service
                var machine = await _machineManagementService.GetMachineByMachineIdAsync(job.MachineId);

                if (machine == null)
                {
                    _logger.LogWarning("Machine {MachineId} not found for job validation", job.MachineId);
                    return false;
                }

                // Check material compatibility
                if (!machine.SupportedMaterials.Contains(job.SlsMaterial, StringComparison.OrdinalIgnoreCase))
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

                // Check machine capabilities for SLS-specific parameters
                var capabilities = await _machineManagementService.GetMachineCapabilitiesAsync(machine.Id);
                var laserPowerCapability = capabilities.FirstOrDefault(c => c.CapabilityType == "LaserPower");
                var scanSpeedCapability = capabilities.FirstOrDefault(c => c.CapabilityType == "ScanSpeed");

                if (laserPowerCapability != null && !laserPowerCapability.IsValueInRange(job.LaserPowerWatts))
                {
                    _logger.LogWarning("Job {PartNumber} laser power {Power}W outside machine {MachineId} capability range", 
                        job.PartNumber, job.LaserPowerWatts, job.MachineId);
                    return false;
                }

                if (scanSpeedCapability != null && !scanSpeedCapability.IsValueInRange(job.ScanSpeedMmPerSec))
                {
                    _logger.LogWarning("Job {PartNumber} scan speed {Speed}mm/s outside machine {MachineId} capability range", 
                        job.PartNumber, job.ScanSpeedMmPerSec, job.MachineId);
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
                // Get machine current material using machine management service
                var machine = await _machineManagementService.GetMachineByMachineIdAsync(machineId);

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
                var machine = await _machineManagementService.GetMachineByMachineIdAsync(job.MachineId);

                if (machine == null)
                {
                    issues.Add($"Machine {job.MachineId} not found");
                    return issues;
                }

                // Check part accommodation
                if (job.Part != null && !machine.CanAccommodatePart(job.Part))
                {
                    issues.Add($"Part dimensions exceed machine build envelope");
                }

                // Check material support
                if (!machine.SupportedMaterials.Contains(job.SlsMaterial, StringComparison.OrdinalIgnoreCase))
                    issues.Add($"Machine does not support material: {job.SlsMaterial}");

                // Check machine capabilities
                var capabilities = await _machineManagementService.GetMachineCapabilitiesAsync(machine.Id);
                
                foreach (var capability in capabilities)
                {
                    switch (capability.CapabilityType.ToLower())
                    {
                        case "laserpower":
                            if (!capability.IsValueInRange(job.LaserPowerWatts))
                                issues.Add($"Laser power ({job.LaserPowerWatts}W) outside machine range {capability.RangeDisplay}");
                            break;
                        case "scanspeed":
                            if (!capability.IsValueInRange(job.ScanSpeedMmPerSec))
                                issues.Add($"Scan speed ({job.ScanSpeedMmPerSec}mm/s) outside machine range {capability.RangeDisplay}");
                            break;
                        case "layerthickness":
                            if (!capability.IsValueInRange(job.LayerThicknessMicrons))
                                issues.Add($"Layer thickness ({job.LayerThicknessMicrons}?m) outside machine range {capability.RangeDisplay}");
                            break;
                    }
                }

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
                var machine = await _machineManagementService.GetMachineByMachineIdAsync(machineId);

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

        private (int dateCount, int slotsPerDay, int slotMinutes) GetZoomParameters(string zoom)
        {
            return zoom switch
            {
                // Task 9: Extended zoom levels from 12 hours down to 1 hour
                "2month" => (60, 1, 1440),    // 60 days (2 months), 1 slot per day (24 hours)
                "month" => (30, 1, 1440),     // 30 days (1 month), 1 slot per day (24 hours)
                "week" => (7, 1, 1440),       // 7 days, 1 slot per day (24 hours)
                "day" => (7, 1, 1440),        // 7 days, 1 slot per day (24 hours) - kept for compatibility
                "12h" => (5, 2, 720),         // 5 days, 2 slots per day (12 hours each)
                "6h" => (4, 4, 360),          // 4 days, 4 slots per day (6 hours each)
                "4h" => (3, 6, 240),          // 3 days, 6 slots per day (4 hours each)
                "2h" => (2, 12, 120),         // 2 days, 12 slots per day (2 hours each)
                "1h" => (1, 24, 60),          // 1 day, 24 slots per day (1 hour each)
                "hour" => (1, 24, 60),        // Alias for 1h - kept for compatibility
                "30min" => (1, 48, 30),       // 1 day, 48 slots per day (30 minutes each)
                "15min" => (1, 96, 15),       // 1 day, 96 slots per day (15 minutes each)
                _ => (7, 1, 1440)             // Default to week view
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

        private async Task<List<string>> ValidateOperatingHoursAsync(Job job)
        {
            var errors = new List<string>();
            
            try
            {
                if (_operatingShiftService == null)
                {
                    errors.Add("Operating shift service not available");
                    return errors;
                }

                // Check job start time against operating hours
                var isStartTimeValid = await _operatingShiftService.IsTimeWithinOperatingHoursAsync(job.ScheduledStart);
                if (!isStartTimeValid)
                {
                    errors.Add($"Job start time {job.ScheduledStart:MM/dd/yyyy HH:mm} is outside operating hours");
                }

                // Check job end time against operating hours
                var isEndTimeValid = await _operatingShiftService.IsTimeWithinOperatingHoursAsync(job.ScheduledEnd);
                if (!isEndTimeValid)
                {
                    errors.Add($"Job end time {job.ScheduledEnd:MM/dd/yyyy HH:mm} is outside operating hours");
                }

                // Check if the entire job duration spans operating hours
                if (isStartTimeValid && isEndTimeValid)
                {
                    // For longer jobs, check intermediate times to ensure they don't span non-operating periods
                    var jobDuration = job.ScheduledEnd - job.ScheduledStart;
                    if (jobDuration.TotalHours > 24)
                    {
                        // Check daily intervals for multi-day jobs
                        for (var checkTime = job.ScheduledStart.AddDays(1).Date; 
                             checkTime < job.ScheduledEnd; 
                             checkTime = checkTime.AddDays(1))
                        {
                            var isTimeValid = await _operatingShiftService.IsTimeWithinOperatingHoursAsync(checkTime.AddHours(12)); // Check midday
                            if (!isTimeValid)
                            {
                                errors.Add($"Job spans non-operating day: {checkTime:MM/dd/yyyy}");
                                break; // Only report the first non-operating day to avoid spam
                            }
                        }
                    }
                }

                return errors;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating operating hours for job {PartNumber}", job.PartNumber);
                errors.Add("Unable to validate operating hours");
                return errors;
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