using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Service for managing build cohorts in Option A workflow enhancement
    /// Handles SLS build completion ? CNC ? EDM ? Assembly ? QC workflow
    /// </summary>
    public interface ICohortManagementService
    {
        Task<BuildCohort> CreateCohortAsync(int buildJobId, string buildNumber, int partCount, string material);
        Task<BuildCohort?> GetCohortAsync(int cohortId);
        Task<BuildCohort?> GetCohortByBuildJobAsync(int buildJobId);
        Task<List<BuildCohort>> GetActiveCohorts();
        Task<bool> UpdateCohortStatusAsync(int cohortId, string status);
        Task<List<Job>> GetCohortJobsAsync(int cohortId);
        Task<bool> AssignJobToCohortAsync(int jobId, int cohortId, string workflowStage, int stageOrder, int totalStages);
        Task<Dictionary<string, int>> GetCohortStageStatusAsync(int cohortId);
    }

    public class CohortManagementService : ICohortManagementService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<CohortManagementService> _logger;

        public CohortManagementService(SchedulerContext context, ILogger<CohortManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create a new cohort when SLS build is completed
        /// </summary>
        public async Task<BuildCohort> CreateCohortAsync(int buildJobId, string buildNumber, int partCount, string material)
        {
            var cohort = new BuildCohort
            {
                BuildJobId = buildJobId,
                BuildNumber = buildNumber,
                PartCount = partCount,
                Material = material,
                Status = "Complete", // SLS is complete when cohort is created
                CompletedDate = DateTime.UtcNow,
                CreatedBy = "PrintTrackingSystem",
                CreatedDate = DateTime.UtcNow,
                LastModifiedDate = DateTime.UtcNow,
                Notes = $"Cohort created from SLS build {buildNumber} with {partCount} parts"
            };

            _context.BuildCohorts.Add(cohort);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new cohort {BuildNumber} with {PartCount} parts from BuildJob {BuildJobId}", 
                buildNumber, partCount, buildJobId);

            return cohort;
        }

        /// <summary>
        /// Get cohort by ID with jobs loaded
        /// </summary>
        public async Task<BuildCohort?> GetCohortAsync(int cohortId)
        {
            return await _context.BuildCohorts
                .Include(c => c.BuildJob)
                .Include(c => c.Jobs)
                .FirstOrDefaultAsync(c => c.Id == cohortId);
        }

        /// <summary>
        /// Get cohort by BuildJobId
        /// </summary>
        public async Task<BuildCohort?> GetCohortByBuildJobAsync(int buildJobId)
        {
            return await _context.BuildCohorts
                .Include(c => c.BuildJob)
                .Include(c => c.Jobs)
                .FirstOrDefaultAsync(c => c.BuildJobId == buildJobId);
        }

        /// <summary>
        /// Get all active cohorts (not completed workflow)
        /// </summary>
        public async Task<List<BuildCohort>> GetActiveCohorts()
        {
            return await _context.BuildCohorts
                .Include(c => c.BuildJob)
                .Include(c => c.Jobs)
                .Where(c => c.Status != "Shipped" && c.Status != "Archived")
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Update cohort status as it progresses through workflow
        /// </summary>
        public async Task<bool> UpdateCohortStatusAsync(int cohortId, string status)
        {
            var cohort = await _context.BuildCohorts.FindAsync(cohortId);
            if (cohort == null) return false;

            cohort.Status = status;
            cohort.LastModifiedDate = DateTime.UtcNow;

            if (status == "Complete")
            {
                cohort.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated cohort {CohortId} status to {Status}", cohortId, status);

            return true;
        }

        /// <summary>
        /// Get all jobs associated with a cohort
        /// </summary>
        public async Task<List<Job>> GetCohortJobsAsync(int cohortId)
        {
            return await _context.Jobs
                .Include(j => j.Part)
                .Where(j => j.BuildCohortId == cohortId)
                .OrderBy(j => j.StageOrder)
                .ThenBy(j => j.ScheduledStart)
                .ToListAsync();
        }

        /// <summary>
        /// Assign a job to a cohort with workflow stage information
        /// </summary>
        public async Task<bool> AssignJobToCohortAsync(int jobId, int cohortId, string workflowStage, int stageOrder, int totalStages)
        {
            var job = await _context.Jobs.FindAsync(jobId);
            if (job == null) return false;

            job.BuildCohortId = cohortId;
            job.WorkflowStage = workflowStage;
            job.StageOrder = stageOrder;
            job.TotalStages = totalStages;
            job.LastModifiedDate = DateTime.UtcNow;
            job.LastModifiedBy = "CohortManagementSystem";

            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned job {JobId} to cohort {CohortId} as stage {WorkflowStage} ({StageOrder}/{TotalStages})", 
                jobId, cohortId, workflowStage, stageOrder, totalStages);

            return true;
        }

        /// <summary>
        /// Get stage status summary for a cohort (how many jobs in each stage)
        /// </summary>
        public async Task<Dictionary<string, int>> GetCohortStageStatusAsync(int cohortId)
        {
            var stageStatus = await _context.Jobs
                .Where(j => j.BuildCohortId == cohortId)
                .GroupBy(j => j.WorkflowStage ?? "Unknown")
                .Select(g => new { Stage = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Stage, x => x.Count);

            return stageStatus;
        }

        /// <summary>
        /// Auto-create cohort from completed SLS build
        /// Called by PrintTrackingService when SLS build completes
        /// </summary>
        public async Task<BuildCohort?> AutoCreateCohortFromBuildAsync(BuildJob buildJob)
        {
            try
            {
                // Generate cohort build number
                var buildNumber = $"BUILD-{DateTime.UtcNow:yyyy}-{buildJob.BuildId:D3}";
                
                // Get part count from BuildJobParts
                var partCount = await _context.BuildJobParts
                    .Where(bjp => bjp.BuildId == buildJob.BuildId)
                    .SumAsync(bjp => bjp.Quantity);

                // Determine material from parts or use default
                var material = await _context.BuildJobParts
                    .Where(bjp => bjp.BuildId == buildJob.BuildId)
                    .Select(bjp => bjp.Material)
                    .FirstOrDefaultAsync() ?? "Ti-6Al-4V Grade 5";

                var cohort = await CreateCohortAsync(buildJob.BuildId, buildNumber, partCount, material);

                _logger.LogInformation("Auto-created cohort {BuildNumber} from completed SLS BuildJob {BuildJobId}", 
                    buildNumber, buildJob.BuildId);

                return cohort;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-create cohort from BuildJob {BuildJobId}", buildJob.BuildId);
                return null;
            }
        }
    }
}