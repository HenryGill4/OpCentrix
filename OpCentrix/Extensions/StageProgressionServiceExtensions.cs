using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;

namespace OpCentrix.Extensions
{
    /// <summary>
    /// Extension methods for StageProgressionService to support stage dashboard functionality
    /// Following the comprehensive master plan strategy of extending rather than replacing
    /// </summary>
    public static class StageProgressionServiceExtensions
    {
        /// <summary>
        /// Get stage-aware jobs for the dashboard view
        /// Uses existing database structure and services
        /// Updated to use direct JobId linking instead of bridge pattern
        /// </summary>
        public static async Task<List<StageAwareJob>> GetStageAwareJobsAsync(
            this IStageProgressionService service, 
            SchedulerContext context,
            DateTime start, 
            DateTime end)
        {
            // Extension method that builds on existing service
            // Uses existing database tables and relationships
            var jobs = await context.Jobs
                .Where(j => j.ScheduledStart >= start && j.ScheduledStart <= end)
                .Include(j => j.Part)
                .ToListAsync();
                
            var stageAwareJobs = new List<StageAwareJob>();

            foreach (var job in jobs)
            {
                var stageJob = new StageAwareJob
                {
                    BaseJob = job,
                    RequiredStages = await GetJobStagesAsync(context, job),
                    StageExecutions = await GetJobStageExecutionsAsync(context, job.Id)
                };

                stageAwareJobs.Add(stageJob);
            }

            return stageAwareJobs;
        }

        /// <summary>
        /// Get stages required for a specific job
        /// </summary>
        private static async Task<List<ProductionStage>> GetJobStagesAsync(SchedulerContext context, Job job)
        {
            if (job.Part == null) return new List<ProductionStage>();

            // Get stages from part stage requirements
            var stages = await context.PartStageRequirements
                .Where(psr => psr.PartId == job.Part.Id && psr.IsRequired)
                .Include(psr => psr.ProductionStage)
                .Select(psr => psr.ProductionStage)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            return stages;
        }

        /// <summary>
        /// Get stage executions for a specific job (using direct JobId link)
        /// </summary>
        private static async Task<List<ProductionStageExecution>> GetJobStageExecutionsAsync(SchedulerContext context, int jobId)
        {
            return await context.ProductionStageExecutions
                .Where(pse => pse.JobId == jobId)
                .Include(pse => pse.ProductionStage)
                .ToListAsync();
        }
    }

    /// <summary>
    /// Stage-aware job model for dashboard display
    /// </summary>
    public class StageAwareJob
    {
        public Job BaseJob { get; set; } = new();
        public List<ProductionStage> RequiredStages { get; set; } = new();
        public List<ProductionStageExecution> StageExecutions { get; set; } = new();
        
        /// <summary>
        /// Get status for a specific stage
        /// </summary>
        public string GetStageStatus(ProductionStage stage)
        {
            var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
            return execution?.Status ?? "Pending";
        }
        
        /// <summary>
        /// Get progress percentage for a specific stage
        /// </summary>
        public double GetStageProgress(ProductionStage stage)
        {
            var execution = StageExecutions.FirstOrDefault(se => se.ProductionStageId == stage.Id);
            if (execution?.Status == "Completed") return 100.0;
            if (execution?.Status == "InProgress") return 50.0;
            return 0.0;
        }

        /// <summary>
        /// Check if operator can punch into this stage
        /// </summary>
        public bool CanOperatorPunch(ProductionStage stage)
        {
            // Can punch if stage is pending and previous stages are complete
            var stageOrder = RequiredStages.FindIndex(s => s.Id == stage.Id);
            if (stageOrder == -1) return false;

            // First stage can always be punched if pending
            if (stageOrder == 0) return GetStageStatus(stage) == "Pending";

            // Other stages require previous stage to be complete
            for (int i = 0; i < stageOrder; i++)
            {
                if (GetStageStatus(RequiredStages[i]) != "Completed")
                    return false;
            }

            return GetStageStatus(stage) == "Pending";
        }
    }

    /// <summary>
    /// Operator stage view for the operator dashboard
    /// </summary>
    public class OperatorStageView
    {
        public string OperatorName { get; set; } = string.Empty;
        public List<ProductionStageExecution> ActiveStages { get; set; } = new();
        public List<AvailableStage> AvailableStages { get; set; } = new();
    }

    /// <summary>
    /// Available stage for operator punch-in
    /// </summary>
    public class AvailableStage
    {
        public int JobId { get; set; }
        public int StageId { get; set; }
        public string JobPartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public double EstimatedHours { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}