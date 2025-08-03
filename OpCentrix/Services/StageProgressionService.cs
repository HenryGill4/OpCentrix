using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Models.JobStaging;

namespace OpCentrix.Services
{
    public interface IStageProgressionService
    {
        Task<List<Job>> CreateDownstreamJobsAsync(int buildCohortId);
        Task<Job?> CreateEDMJobAsync(BuildCohort cohort, List<Part> partsRequiringEDM);
        Task<Job?> CreateCNCJobAsync(BuildCohort cohort, List<Part> partsRequiringCNC);
        Task<Job?> CreateLaserEngravingJobAsync(BuildCohort cohort, List<Part> allParts);
        Task<Job?> CreateSandblastingJobAsync(BuildCohort cohort, List<Part> allParts);
        Task<Job?> CreateCoatingJobAsync(BuildCohort cohort, List<Part> partsRequiringCoating);
        Task<Job?> CreateAssemblyJobAsync(BuildCohort cohort, List<Part> partsRequiringAssembly);
        Task<Job?> CreateShippingJobAsync(BuildCohort cohort, List<Part> allParts);
        Task<bool> UpdateScheduleForNewJobsAsync(List<Job> newJobs);
        Task<List<Part>> GetPartsRequiringStageAsync(List<Part> parts, string stageName);
        Task<DateTime> CalculateStageStartTimeAsync(DateTime previousStageEnd, string machineType);
    }

    public class StageProgressionService : IStageProgressionService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageProgressionService> _logger;
        private readonly ICohortManagementService _cohortService;

        public StageProgressionService(
            SchedulerContext context, 
            ILogger<StageProgressionService> logger,
            ICohortManagementService cohortService)
        {
            _context = context;
            _logger = logger;
            _cohortService = cohortService;
        }

        /// <summary>
        /// Create all downstream jobs for a completed build cohort
        /// </summary>
        public async Task<List<Job>> CreateDownstreamJobsAsync(int buildCohortId)
        {
            try
            {
                var cohort = await _context.BuildCohorts
                    .FirstOrDefaultAsync(c => c.Id == buildCohortId);

                if (cohort == null)
                {
                    _logger.LogWarning("Build cohort {CohortId} not found", buildCohortId);
                    return new List<Job>();
                }

                // Get all parts from the cohort's build job
                var parts = await GetCohortPartsAsync(buildCohortId);
                if (!parts.Any())
                {
                    _logger.LogWarning("No parts found for build cohort {CohortId}", buildCohortId);
                    return new List<Job>();
                }

                var createdJobs = new List<Job>();
                DateTime lastJobEnd = DateTime.UtcNow.AddHours(1); // Allow 1 hour for SLS cooldown

                // 1. Create EDM job if any parts require EDM
                var partsRequiringEDM = await GetPartsRequiringStageAsync(parts, "EDM");
                if (partsRequiringEDM.Any())
                {
                    var edmJob = await CreateEDMJobAsync(cohort, partsRequiringEDM);
                    if (edmJob != null)
                    {
                        createdJobs.Add(edmJob);
                        lastJobEnd = edmJob.ScheduledEnd;
                    }
                }

                // 2. Create CNC job if any parts require machining
                var partsRequiringCNC = await GetPartsRequiringStageAsync(parts, "CNC");
                if (partsRequiringCNC.Any())
                {
                    var cncJob = await CreateCNCJobAsync(cohort, partsRequiringCNC);
                    if (cncJob != null)
                    {
                        cncJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "CNC");
                        cncJob.ScheduledEnd = cncJob.ScheduledStart.AddMinutes(partsRequiringCNC.Count * 6); // 6 minutes per part
                        _context.Jobs.Update(cncJob);
                        createdJobs.Add(cncJob);
                        lastJobEnd = cncJob.ScheduledEnd;
                    }
                }

                // 3. Create Laser Engraving job for serial numbers (all parts)
                var laserJob = await CreateLaserEngravingJobAsync(cohort, parts);
                if (laserJob != null)
                {
                    laserJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "LaserEngraving");
                    laserJob.ScheduledEnd = laserJob.ScheduledStart.AddMinutes(parts.Count * 2); // 2 minutes per part
                    _context.Jobs.Update(laserJob);
                    createdJobs.Add(laserJob);
                    lastJobEnd = laserJob.ScheduledEnd;
                }

                // 4. Create Sandblasting job (surface prep for all parts)
                var sandblastJob = await CreateSandblastingJobAsync(cohort, parts);
                if (sandblastJob != null)
                {
                    sandblastJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "Sandblasting");
                    sandblastJob.ScheduledEnd = sandblastJob.ScheduledStart.AddHours(2); // 2 hours for batch
                    _context.Jobs.Update(sandblastJob);
                    createdJobs.Add(sandblastJob);
                    lastJobEnd = sandblastJob.ScheduledEnd;
                }

                // 5. Create Coating job if any parts require coating
                var partsRequiringCoating = await GetPartsRequiringStageAsync(parts, "Coating");
                if (partsRequiringCoating.Any())
                {
                    var coatingJob = await CreateCoatingJobAsync(cohort, partsRequiringCoating);
                    if (coatingJob != null)
                    {
                        coatingJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "Coating");
                        coatingJob.ScheduledEnd = coatingJob.ScheduledStart.AddHours(4); // 4 hours for coating process
                        _context.Jobs.Update(coatingJob);
                        createdJobs.Add(coatingJob);
                        lastJobEnd = coatingJob.ScheduledEnd;
                    }
                }

                // 6. Create Assembly job if any parts require assembly
                var partsRequiringAssembly = await GetPartsRequiringStageAsync(parts, "Assembly");
                if (partsRequiringAssembly.Any())
                {
                    var assemblyJob = await CreateAssemblyJobAsync(cohort, partsRequiringAssembly);
                    if (assemblyJob != null)
                    {
                        assemblyJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "Assembly");
                        assemblyJob.ScheduledEnd = assemblyJob.ScheduledStart.AddHours(1); // 1 hour for assembly
                        _context.Jobs.Update(assemblyJob);
                        createdJobs.Add(assemblyJob);
                        lastJobEnd = assemblyJob.ScheduledEnd;
                    }
                }

                // 7. Create Shipping job (all parts)
                var shippingJob = await CreateShippingJobAsync(cohort, parts);
                if (shippingJob != null)
                {
                    shippingJob.ScheduledStart = await CalculateStageStartTimeAsync(lastJobEnd, "Shipping");
                    shippingJob.ScheduledEnd = shippingJob.ScheduledStart.AddHours(1); // 1 hour for packaging
                    _context.Jobs.Update(shippingJob);
                    createdJobs.Add(shippingJob);
                }

                // Save all jobs
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created {JobCount} downstream jobs for cohort {CohortId}", 
                    createdJobs.Count, buildCohortId);

                return createdJobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating downstream jobs for cohort {CohortId}", buildCohortId);
                return new List<Job>();
            }
        }

        /// <summary>
        /// Create EDM job for parts requiring EDM operations
        /// Groups all parts together as per specifications (30 suppressors stay together)
        /// </summary>
        public async Task<Job?> CreateEDMJobAsync(BuildCohort cohort, List<Part> partsRequiringEDM)
        {
            try
            {
                if (!partsRequiringEDM.Any()) return null;

                // Estimate 1-4 hours based on part complexity
                var totalComplexity = partsRequiringEDM.Sum(p => GetPartComplexityScore(p));
                var estimatedHours = Math.Max(1, Math.Min(4, totalComplexity / 10.0)); // Scale complexity

                var edmJob = new Job
                {
                    MachineId = "EDM1", // Default EDM machine
                    PartNumber = $"EDM-{cohort.BuildNumber}",
                    PartId = partsRequiringEDM.First().Id, // Use first part as reference
                    ScheduledStart = DateTime.UtcNow.AddHours(1), // After SLS cooldown
                    ScheduledEnd = DateTime.UtcNow.AddHours(1 + estimatedHours),
                    Status = "Scheduled",
                    Quantity = partsRequiringEDM.Sum(p => cohort.PartCount), // Total parts in cohort
                    Priority = 2, // High priority for downstream processing
                    EstimatedHours = estimatedHours,
                    EstimatedDuration = TimeSpan.FromHours(estimatedHours),
                    Notes = $"EDM operations for {partsRequiringEDM.Count} part types from build {cohort.BuildNumber}. " +
                           $"Parts stay together in crates during processing.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "EDM",
                    StageOrder = 2,
                    TotalStages = 7 // SLS -> EDM -> CNC -> Engraving -> Sandblast -> Coating -> Shipping
                };

                _context.Jobs.Add(edmJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created EDM job {JobId} for cohort {CohortId} with {PartCount} parts", 
                    edmJob.Id, cohort.Id, partsRequiringEDM.Count);

                return edmJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating EDM job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create CNC job for parts requiring machining (6 minutes per suppressor)
        /// </summary>
        public async Task<Job?> CreateCNCJobAsync(BuildCohort cohort, List<Part> partsRequiringCNC)
        {
            try
            {
                if (!partsRequiringCNC.Any()) return null;

                var totalParts = cohort.PartCount;
                var estimatedMinutes = totalParts * 6; // 6 minutes per suppressor as specified
                var estimatedHours = estimatedMinutes / 60.0;

                var cncJob = new Job
                {
                    MachineId = "CNC1", // Default CNC machine
                    PartNumber = $"CNC-{cohort.BuildNumber}",
                    PartId = partsRequiringCNC.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(2), // After EDM completion
                    ScheduledEnd = DateTime.UtcNow.AddHours(2 + estimatedHours),
                    Status = "Scheduled",
                    Quantity = totalParts,
                    Priority = 2,
                    EstimatedHours = estimatedHours,
                    EstimatedDuration = TimeSpan.FromMinutes(estimatedMinutes),
                    Notes = $"CNC machining for {totalParts} parts from build {cohort.BuildNumber}. " +
                           $"Estimated {estimatedMinutes} minutes total ({estimatedMinutes/totalParts} min per part).",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "CNC",
                    StageOrder = 3,
                    TotalStages = 7
                };

                _context.Jobs.Add(cncJob);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created CNC job {JobId} for cohort {CohortId} with {TotalMinutes} minutes estimated", 
                    cncJob.Id, cohort.Id, estimatedMinutes);

                return cncJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating CNC job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create Laser Engraving job for serial numbers (all parts)
        /// </summary>
        public async Task<Job?> CreateLaserEngravingJobAsync(BuildCohort cohort, List<Part> allParts)
        {
            try
            {
                var totalParts = cohort.PartCount;
                var estimatedMinutes = totalParts * 2; // 2 minutes per part for serial number engraving
                var estimatedHours = estimatedMinutes / 60.0;

                var laserJob = new Job
                {
                    MachineId = "LASER1", // Default laser engraving machine
                    PartNumber = $"LASER-{cohort.BuildNumber}",
                    PartId = allParts.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(4), // After CNC
                    ScheduledEnd = DateTime.UtcNow.AddHours(4 + estimatedHours),
                    Status = "Scheduled",
                    Quantity = totalParts,
                    Priority = 2,
                    EstimatedHours = estimatedHours,
                    EstimatedDuration = TimeSpan.FromMinutes(estimatedMinutes),
                    Notes = $"Laser engraving serial numbers for {totalParts} parts from build {cohort.BuildNumber}.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "LaserEngraving",
                    StageOrder = 4,
                    TotalStages = 7
                };

                _context.Jobs.Add(laserJob);
                return laserJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Laser Engraving job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create Sandblasting job for surface preparation (all parts)
        /// </summary>
        public async Task<Job?> CreateSandblastingJobAsync(BuildCohort cohort, List<Part> allParts)
        {
            try
            {
                var sandblastJob = new Job
                {
                    MachineId = "SANDBLAST1", 
                    PartNumber = $"SANDBLAST-{cohort.BuildNumber}",
                    PartId = allParts.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(5), // After laser engraving
                    ScheduledEnd = DateTime.UtcNow.AddHours(7), // 2 hours for batch processing
                    Status = "Scheduled",
                    Quantity = cohort.PartCount,
                    Priority = 2,
                    EstimatedHours = 2.0,
                    EstimatedDuration = TimeSpan.FromHours(2),
                    Notes = $"Sandblasting surface preparation for {cohort.PartCount} parts from build {cohort.BuildNumber}.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "Sandblasting",
                    StageOrder = 5,
                    TotalStages = 7
                };

                _context.Jobs.Add(sandblastJob);
                return sandblastJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Sandblasting job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create Coating job for parts requiring surface treatment
        /// </summary>
        public async Task<Job?> CreateCoatingJobAsync(BuildCohort cohort, List<Part> partsRequiringCoating)
        {
            try
            {
                if (!partsRequiringCoating.Any()) return null;

                var coatingJob = new Job
                {
                    MachineId = "COATING1", 
                    PartNumber = $"COATING-{cohort.BuildNumber}",
                    PartId = partsRequiringCoating.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(7), // After sandblasting
                    ScheduledEnd = DateTime.UtcNow.AddHours(11), // 4 hours for coating process
                    Status = "Scheduled",
                    Quantity = partsRequiringCoating.Sum(p => cohort.PartCount / cohort.PartCount), // Parts requiring coating
                    Priority = 2,
                    EstimatedHours = 4.0,
                    EstimatedDuration = TimeSpan.FromHours(4),
                    Notes = $"Coating operations for {partsRequiringCoating.Count} part types from build {cohort.BuildNumber}.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "Coating",
                    StageOrder = 6,
                    TotalStages = 7
                };

                _context.Jobs.Add(coatingJob);
                return coatingJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Coating job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create Assembly job for final assembly
        /// </summary>
        public async Task<Job?> CreateAssemblyJobAsync(BuildCohort cohort, List<Part> partsRequiringAssembly)
        {
            try
            {
                if (!partsRequiringAssembly.Any()) return null;

                var assemblyJob = new Job
                {
                    MachineId = "ASSEMBLY1", 
                    PartNumber = $"ASSEMBLY-{cohort.BuildNumber}",
                    PartId = partsRequiringAssembly.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(11), // After coating
                    ScheduledEnd = DateTime.UtcNow.AddHours(12), // 1 hour for assembly
                    Status = "Scheduled",
                    Quantity = partsRequiringAssembly.Count,
                    Priority = 2,
                    EstimatedHours = 1.0,
                    EstimatedDuration = TimeSpan.FromHours(1),
                    Notes = $"Final assembly for {partsRequiringAssembly.Count} part types from build {cohort.BuildNumber}.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "Assembly",
                    StageOrder = 7,
                    TotalStages = 7
                };

                _context.Jobs.Add(assemblyJob);
                return assemblyJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Assembly job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Create Shipping job for packaging and delivery
        /// </summary>
        public async Task<Job?> CreateShippingJobAsync(BuildCohort cohort, List<Part> allParts)
        {
            try
            {
                var shippingJob = new Job
                {
                    MachineId = "SHIPPING1", 
                    PartNumber = $"SHIPPING-{cohort.BuildNumber}",
                    PartId = allParts.First().Id,
                    ScheduledStart = DateTime.UtcNow.AddHours(12), // After all processing
                    ScheduledEnd = DateTime.UtcNow.AddHours(13), // 1 hour for packaging
                    Status = "Scheduled",
                    Quantity = cohort.PartCount,
                    Priority = 1,
                    EstimatedHours = 1.0,
                    EstimatedDuration = TimeSpan.FromHours(1),
                    Notes = $"Final packaging and shipping for {cohort.PartCount} parts from build {cohort.BuildNumber}.",
                    CreatedBy = "StageProgressionService",
                    LastModifiedBy = "StageProgressionService",
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow,
                    
                    // Link to cohort
                    BuildCohortId = cohort.Id,
                    WorkflowStage = "Shipping",
                    StageOrder = 8,
                    TotalStages = 7
                };

                _context.Jobs.Add(shippingJob);
                return shippingJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Shipping job for cohort {CohortId}", cohort.Id);
                return null;
            }
        }

        /// <summary>
        /// Update schedule to accommodate new jobs
        /// </summary>
        public async Task<bool> UpdateScheduleForNewJobsAsync(List<Job> newJobs)
        {
            try
            {
                // For now, just ensure no scheduling conflicts
                // In a more advanced implementation, this would push back conflicting jobs
                
                foreach (var newJob in newJobs)
                {
                    var conflictingJobs = await _context.Jobs
                        .Where(j => j.MachineId == newJob.MachineId &&
                                   j.Status == "Scheduled" &&
                                   j.Id != newJob.Id &&
                                   ((j.ScheduledStart >= newJob.ScheduledStart && j.ScheduledStart < newJob.ScheduledEnd) ||
                                    (j.ScheduledEnd > newJob.ScheduledStart && j.ScheduledEnd <= newJob.ScheduledEnd)))
                        .ToListAsync();

                    if (conflictingJobs.Any())
                    {
                        _logger.LogWarning("Scheduling conflict detected for job {JobId} on machine {MachineId}", 
                            newJob.Id, newJob.MachineId);
                        
                        // Simple resolution: push conflicting jobs back
                        foreach (var conflictJob in conflictingJobs)
                        {
                            var duration = conflictJob.ScheduledEnd - conflictJob.ScheduledStart;
                            conflictJob.ScheduledStart = newJob.ScheduledEnd;
                            conflictJob.ScheduledEnd = conflictJob.ScheduledStart + duration;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating schedule for new jobs");
                return false;
            }
        }

        /// <summary>
        /// Get parts that require a specific stage
        /// </summary>
        public async Task<List<Part>> GetPartsRequiringStageAsync(List<Part> parts, string stageName)
        {
            var partIds = parts.Select(p => p.Id).ToList();
            
            var partsWithStage = await _context.PartStageRequirements
                .Include(psr => psr.Part)
                .Include(psr => psr.ProductionStage)
                .Where(psr => partIds.Contains(psr.PartId) && 
                             psr.ProductionStage.Name.Contains(stageName) &&
                             psr.IsRequired)
                .Select(psr => psr.Part)
                .Distinct()
                .ToListAsync();

            return partsWithStage;
        }

        /// <summary>
        /// Calculate stage start time based on previous stage completion
        /// </summary>
        public async Task<DateTime> CalculateStageStartTimeAsync(DateTime previousStageEnd, string machineType)
        {
            // Add buffer time based on machine type
            var bufferHours = machineType switch
            {
                "EDM" => 0.5, // 30 minutes setup
                "CNC" => 0.25, // 15 minutes setup  
                "LaserEngraving" => 0.1, // 6 minutes setup
                "Sandblasting" => 0.5, // 30 minutes setup
                "Coating" => 1.0, // 1 hour setup and cure time
                "Assembly" => 0.25, // 15 minutes setup
                "Shipping" => 0.1, // 6 minutes setup
                _ => 0.5 // Default 30 minutes
            };

            return previousStageEnd.AddHours(bufferHours);
        }

        #region Private Helper Methods

        /// <summary>
        /// Get parts associated with a build cohort
        /// </summary>
        private async Task<List<Part>> GetCohortPartsAsync(int buildCohortId)
        {
            // Get parts from build jobs associated with this cohort
            var parts = await _context.BuildJobs
                .Where(bj => bj.BuildJobParts.Any()) // Has parts associated
                .SelectMany(bj => bj.BuildJobParts)
                .Where(bjp => _context.BuildJobs.Any(bj => bj.BuildId == bjp.BuildId && 
                                                          bj.PartId.HasValue))
                .Select(bjp => _context.Parts.FirstOrDefault(p => p.PartNumber == bjp.PartNumber))
                .Where(p => p != null)
                .Distinct()
                .ToListAsync();

            // Fallback: try to get parts directly from jobs with the cohort ID
            if (!parts.Any())
            {
                parts = await _context.Jobs
                    .Where(j => j.BuildCohortId == buildCohortId)
                    .Include(j => j.Part)
                    .Select(j => j.Part)
                    .Where(p => p != null)
                    .Distinct()
                    .ToListAsync();
            }

            return parts;
        }

        /// <summary>
        /// Get complexity score for a part to estimate EDM time
        /// </summary>
        private double GetPartComplexityScore(Part part)
        {
            // Simple complexity scoring based on part characteristics
            var score = 1.0; // Base score

            // Add complexity based on part name/type
            if (part.Name.ToLower().Contains("suppressor")) score += 2.0;
            if (part.Name.ToLower().Contains("complex")) score += 1.5;
            if (part.Name.ToLower().Contains("simple")) score += 0.5;

            // Add complexity based on estimated hours
            if (part.EstimatedHours > 0)
            {
                score += part.EstimatedHours / 10.0; // Scale hours to complexity
            }

            return Math.Max(1.0, Math.Min(10.0, score)); // Clamp between 1 and 10
        }

        #endregion
    }
}