using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services.Admin
{
    /// <summary>
    /// Service for managing prototype tracking through production stages
    /// Captures real time and cost data for accurate production planning
    /// </summary>
    public class PrototypeTrackingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PrototypeTrackingService> _logger;

        public PrototypeTrackingService(SchedulerContext context, ILogger<PrototypeTrackingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Prototype Job Management

        /// <summary>
        /// Creates a new prototype job for tracking a part through production stages
        /// </summary>
        public async Task<PrototypeJob> CreatePrototypeJobAsync(Part part, string requestedBy)
        {
            try
            {
                var prototypeNumber = await GeneratePrototypeNumberAsync();
                
                var prototypeJob = new PrototypeJob
                {
                    PartId = part.Id,
                    PrototypeNumber = prototypeNumber,
                    RequestedBy = requestedBy,
                    RequestDate = DateTime.UtcNow,
                    Priority = "Standard",
                    Status = "InProgress",
                    TotalEstimatedHours = (decimal)part.EstimatedHours,
                    TotalEstimatedCost = CalculateEstimatedCost(part),
                    CreatedBy = requestedBy,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.PrototypeJobs.Add(prototypeJob);
                await _context.SaveChangesAsync();

                // Create stage executions for all active production stages
                await CreateStageExecutionsAsync(prototypeJob.Id);

                _logger.LogInformation("Created prototype job {PrototypeNumber} for part {PartNumber}", 
                    prototypeNumber, part.PartNumber);

                return prototypeJob;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prototype job for part {PartId}", part.Id);
                throw;
            }
        }

        /// <summary>
        /// Gets all active prototype jobs with their progress information
        /// </summary>
        public async Task<List<PrototypeJob>> GetActivePrototypeJobsAsync()
        {
            return await _context.PrototypeJobs
                .Include(pj => pj.Part)
                .Include(pj => pj.StageExecutions)
                    .ThenInclude(se => se.ProductionStage)
                .Where(pj => pj.IsActive && pj.Status != "Completed")
                .OrderBy(pj => pj.RequestDate)
                .ToListAsync();
        }

        /// <summary>
        /// Gets detailed information about a specific prototype job
        /// </summary>
        public async Task<PrototypeJob?> GetPrototypeJobDetailsAsync(int prototypeJobId)
        {
            return await _context.PrototypeJobs
                .Include(pj => pj.Part)
                .Include(pj => pj.StageExecutions)
                    .ThenInclude(se => se.ProductionStage)
                .Include(pj => pj.StageExecutions)
                    .ThenInclude(se => se.TimeLogs)
                .Include(pj => pj.AssemblyComponents)
                .FirstOrDefaultAsync(pj => pj.Id == prototypeJobId);
        }

        /// <summary>
        /// Updates a prototype job
        /// </summary>
        public async Task<bool> UpdatePrototypeJobAsync(PrototypeJob prototypeJob)
        {
            try
            {
                prototypeJob.UpdatedDate = DateTime.UtcNow;
                _context.PrototypeJobs.Update(prototypeJob);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prototype job {Id}", prototypeJob.Id);
                return false;
            }
        }

        #endregion

        #region Stage Execution

        /// <summary>
        /// Starts execution of a production stage
        /// </summary>
        public async Task<bool> StartStageAsync(int prototypeJobId, int stageId, string executor)
        {
            try
            {
                var execution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(se => se.PrototypeJobId == prototypeJobId && se.ProductionStageId == stageId);

                if (execution == null)
                {
                    _logger.LogWarning("Stage execution not found for prototype {PrototypeJobId} stage {StageId}", 
                        prototypeJobId, stageId);
                    return false;
                }

                execution.Status = "InProgress";
                execution.StartDate = DateTime.UtcNow;
                execution.ExecutedBy = executor;
                execution.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Started stage {StageId} for prototype job {PrototypeJobId} by {Executor}", 
                    stageId, prototypeJobId, executor);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting stage {StageId} for prototype job {PrototypeJobId}", 
                    stageId, prototypeJobId);
                return false;
            }
        }

        /// <summary>
        /// Completes execution of a production stage
        /// </summary>
        public async Task<bool> CompleteStageAsync(int stageExecutionId, decimal actualHours, decimal actualCost)
        {
            try
            {
                var execution = await _context.ProductionStageExecutions
                    .Include(se => se.PrototypeJob)
                    .FirstOrDefaultAsync(se => se.Id == stageExecutionId);

                if (execution == null)
                {
                    _logger.LogWarning("Stage execution not found: {StageExecutionId}", stageExecutionId);
                    return false;
                }

                execution.Status = "Completed";
                execution.CompletionDate = DateTime.UtcNow;
                execution.ActualHours = actualHours;
                execution.ActualCost = actualCost;
                execution.UpdatedDate = DateTime.UtcNow;

                // Update prototype job totals
                if (execution.PrototypeJobId.HasValue)
                {
                    await UpdatePrototypeJobTotalsAsync(execution.PrototypeJobId.Value);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Completed stage execution {StageExecutionId} with {ActualHours}h and ${ActualCost}", 
                    stageExecutionId, actualHours, actualCost);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing stage execution {StageExecutionId}", stageExecutionId);
                return false;
            }
        }

        /// <summary>
        /// Skips a production stage with reason
        /// </summary>
        public async Task<bool> SkipStageAsync(int stageExecutionId, string reason)
        {
            try
            {
                var execution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(se => se.Id == stageExecutionId);

                if (execution == null)
                {
                    _logger.LogWarning("Stage execution not found: {StageExecutionId}", stageExecutionId);
                    return false;
                }

                execution.Status = "Skipped";
                execution.Issues = reason;
                execution.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Skipped stage execution {StageExecutionId}: {Reason}", 
                    stageExecutionId, reason);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error skipping stage execution {StageExecutionId}", stageExecutionId);
                return false;
            }
        }

        /// <summary>
        /// Gets all stage executions for a prototype job
        /// </summary>
        public async Task<List<ProductionStageExecution>> GetStageExecutionsAsync(int prototypeJobId)
        {
            return await _context.ProductionStageExecutions
                .Include(se => se.ProductionStage)
                .Include(se => se.TimeLogs)
                .Where(se => se.PrototypeJobId == prototypeJobId)
                .OrderBy(se => se.ProductionStage!.DisplayOrder)
                .ToListAsync();
        }

        #endregion

        #region Time Logging

        /// <summary>
        /// Logs time for a stage execution
        /// </summary>
        public async Task<bool> LogTimeAsync(int stageExecutionId, DateTime startTime, DateTime? endTime, string activity)
        {
            try
            {
                var timeLog = new PrototypeTimeLog
                {
                    ProductionStageExecutionId = stageExecutionId,
                    LogDate = DateTime.UtcNow.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    ElapsedMinutes = endTime.HasValue ? (int)(endTime.Value - startTime).TotalMinutes : null,
                    ActivityType = DetermineActivityType(activity),
                    ActivityDescription = activity,
                    Employee = "System", // TODO: Get from current user context
                    CreatedDate = DateTime.UtcNow
                };

                _context.PrototypeTimeLogs.Add(timeLog);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging time for stage execution {StageExecutionId}", stageExecutionId);
                return false;
            }
        }

        /// <summary>
        /// Gets time logs for a stage execution
        /// </summary>
        public async Task<List<PrototypeTimeLog>> GetTimeLogsAsync(int stageExecutionId)
        {
            return await _context.PrototypeTimeLogs
                .Where(tl => tl.ProductionStageExecutionId == stageExecutionId)
                .OrderBy(tl => tl.StartTime)
                .ToListAsync();
        }

        #endregion

        #region Component Management

        /// <summary>
        /// Adds an assembly component to a prototype job
        /// </summary>
        public async Task<bool> AddAssemblyComponentAsync(int prototypeJobId, AssemblyComponent component)
        {
            try
            {
                component.PrototypeJobId = prototypeJobId;
                component.CreatedDate = DateTime.UtcNow;
                component.IsActive = true;

                _context.AssemblyComponents.Add(component);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding assembly component to prototype job {PrototypeJobId}", prototypeJobId);
                return false;
            }
        }

        /// <summary>
        /// Gets assembly components for a prototype job
        /// </summary>
        public async Task<List<AssemblyComponent>> GetAssemblyComponentsAsync(int prototypeJobId)
        {
            return await _context.AssemblyComponents
                .Where(ac => ac.PrototypeJobId == prototypeJobId && ac.IsActive)
                .OrderBy(ac => ac.ComponentType)
                .ThenBy(ac => ac.ComponentDescription)
                .ToListAsync();
        }

        /// <summary>
        /// Updates component status
        /// </summary>
        public async Task<bool> UpdateComponentStatusAsync(int componentId, string status)
        {
            try
            {
                var component = await _context.AssemblyComponents.FindAsync(componentId);
                if (component == null) return false;

                component.Status = status;
                
                switch (status)
                {
                    case "Ordered":
                        component.OrderDate = DateTime.UtcNow;
                        break;
                    case "Received":
                        component.ReceivedDate = DateTime.UtcNow;
                        break;
                    case "Used":
                        component.UsedDate = DateTime.UtcNow;
                        break;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating component status {ComponentId}", componentId);
                return false;
            }
        }

        #endregion

        #region Admin Review

        /// <summary>
        /// Gets prototype jobs awaiting admin review
        /// </summary>
        public async Task<List<PrototypeJob>> GetJobsAwaitingReviewAsync()
        {
            return await _context.PrototypeJobs
                .Include(pj => pj.Part)
                .Include(pj => pj.StageExecutions)
                    .ThenInclude(se => se.ProductionStage)
                .Where(pj => pj.IsActive && pj.AdminReviewStatus == "Pending" && 
                            pj.StageExecutions.All(se => se.Status == "Completed" || se.Status == "Skipped"))
                .OrderBy(pj => pj.CompletionDate)
                .ToListAsync();
        }

        /// <summary>
        /// Approves a prototype for production
        /// </summary>
        public async Task<bool> ApprovePrototypeAsync(int prototypeJobId, string adminNotes)
        {
            try
            {
                var prototypeJob = await _context.PrototypeJobs
                    .Include(pj => pj.Part)
                    .FirstOrDefaultAsync(pj => pj.Id == prototypeJobId);

                if (prototypeJob == null) return false;

                prototypeJob.AdminReviewStatus = "Approved";
                prototypeJob.AdminReviewBy = "Admin"; // TODO: Get from current user
                prototypeJob.AdminReviewDate = DateTime.UtcNow;
                prototypeJob.AdminReviewNotes = adminNotes;
                prototypeJob.Status = "Approved";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Approved prototype job {PrototypeJobId} for production", prototypeJobId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving prototype job {PrototypeJobId}", prototypeJobId);
                return false;
            }
        }

        /// <summary>
        /// Converts prototype to production part with learned data
        /// </summary>
        public async Task<bool> ConvertToProductionPartAsync(int prototypeJobId)
        {
            try
            {
                var prototypeJob = await _context.PrototypeJobs
                    .Include(pj => pj.Part)
                    .Include(pj => pj.StageExecutions)
                    .FirstOrDefaultAsync(pj => pj.Id == prototypeJobId);

                if (prototypeJob?.Part == null) return false;

                // Update part with actual learned data
                prototypeJob.Part.EstimatedHours = (double)prototypeJob.TotalActualHours;
                prototypeJob.Part.AverageCostPerUnit = prototypeJob.TotalActualCost;
                prototypeJob.Part.PartCategory = "Production";
                prototypeJob.Part.LastModifiedDate = DateTime.UtcNow;
                prototypeJob.Part.LastModifiedBy = "System";

                // Mark prototype as converted
                prototypeJob.Status = "Production";
                prototypeJob.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Converted prototype job {PrototypeJobId} to production part", prototypeJobId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting prototype job {PrototypeJobId} to production", prototypeJobId);
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<string> GeneratePrototypeNumberAsync()
        {
            var lastPrototype = await _context.PrototypeJobs
                .Where(pj => pj.PrototypeNumber.StartsWith("PROTO-"))
                .OrderByDescending(pj => pj.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (lastPrototype != null)
            {
                var numberPart = lastPrototype.PrototypeNumber.Substring(6);
                if (int.TryParse(numberPart, out var currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            return $"PROTO-{nextNumber:D3}";
        }

        private async Task CreateStageExecutionsAsync(int prototypeJobId)
        {
            var productionStages = await _context.ProductionStages
                .Where(ps => ps.IsActive)
                .OrderBy(ps => ps.DisplayOrder)
                .ToListAsync();

            foreach (var stage in productionStages)
            {
                var execution = new ProductionStageExecution
                {
                    PrototypeJobId = prototypeJobId,
                    ProductionStageId = stage.Id,
                    Status = "NotStarted",
                    QualityCheckRequired = stage.RequiresQualityCheck,
                    EstimatedHours = (decimal)(stage.DefaultSetupMinutes / 60.0) + 1.0m, // Rough estimate
                    EstimatedCost = stage.DefaultHourlyRate * ((decimal)(stage.DefaultSetupMinutes / 60.0) + 1.0m),
                    ExecutedBy = "TBD",
                    CreatedDate = DateTime.UtcNow
                };

                _context.ProductionStageExecutions.Add(execution);
            }

            await _context.SaveChangesAsync();
        }

        private decimal CalculateEstimatedCost(Part part)
        {
            // Basic cost calculation based on part properties
            var materialCost = part.MaterialCostPerKg * (decimal)part.PowderRequirementKg;
            var laborCost = part.StandardLaborCostPerHour * (decimal)part.EstimatedHours;
            var overheadCost = part.SetupCost;

            return materialCost + laborCost + overheadCost;
        }

        private async Task UpdatePrototypeJobTotalsAsync(int prototypeJobId)
        {
            var prototypeJob = await _context.PrototypeJobs
                .Include(pj => pj.StageExecutions)
                .FirstOrDefaultAsync(pj => pj.Id == prototypeJobId);

            if (prototypeJob == null) return;

            var completedStages = prototypeJob.StageExecutions.Where(se => se.Status == "Completed").ToList();

            prototypeJob.TotalActualHours = completedStages.Sum(se => se.ActualHours ?? 0);
            prototypeJob.TotalActualCost = completedStages.Sum(se => se.ActualCost ?? 0);

            if (prototypeJob.TotalEstimatedHours > 0)
            {
                prototypeJob.TimeVariancePercent = 
                    ((prototypeJob.TotalActualHours - prototypeJob.TotalEstimatedHours) / prototypeJob.TotalEstimatedHours) * 100;
            }

            if (prototypeJob.TotalEstimatedCost > 0)
            {
                prototypeJob.CostVariancePercent = 
                    ((prototypeJob.TotalActualCost - prototypeJob.TotalEstimatedCost) / prototypeJob.TotalEstimatedCost) * 100;
            }

            // Check if all stages are completed
            if (prototypeJob.StageExecutions.All(se => se.Status == "Completed" || se.Status == "Skipped"))
            {
                prototypeJob.CompletionDate = DateTime.UtcNow;
                prototypeJob.LeadTimeDays = prototypeJob.StartDate.HasValue
                    ? (int)(prototypeJob.CompletionDate.Value - prototypeJob.StartDate.Value).TotalDays
                    : (int?)null;
                prototypeJob.AdminReviewStatus = "Pending";
            }
        }

        private string DetermineActivityType(string activity)
        {
            var activityLower = activity.ToLower();
            
            if (activityLower.Contains("setup")) return "Setup";
            if (activityLower.Contains("quality") || activityLower.Contains("inspection")) return "QualityCheck";
            if (activityLower.Contains("rework") || activityLower.Contains("fix")) return "Rework";
            
            return "Production";
        }

        #endregion

        #region Cost Analysis

        /// <summary>
        /// Get cost analysis for a prototype job
        /// </summary>
        public async Task<object> GetCostAnalysisAsync(int prototypeJobId)
        {
            try
            {
                var prototypeJob = await _context.PrototypeJobs
                    .Include(pj => pj.StageExecutions)
                    .ThenInclude(se => se.ProductionStage)
                    .FirstOrDefaultAsync(pj => pj.Id == prototypeJobId);

                if (prototypeJob == null)
                {
                    return new { };
                }

                return new
                {
                    TotalEstimatedCost = prototypeJob.TotalEstimatedCost,
                    TotalActualCost = prototypeJob.TotalActualCost,
                    CostVariance = prototypeJob.TotalActualCost - prototypeJob.TotalEstimatedCost,
                    CostVariancePercent = prototypeJob.CostVariancePercent,
                    StageBreakdown = prototypeJob.StageExecutions.Select(se => new
                    {
                        StageName = se.ProductionStage?.Name ?? "Unknown",
                        EstimatedCost = se.EstimatedCost ?? 0,
                        ActualCost = se.ActualCost ?? 0,
                        MaterialCost = se.MaterialCost ?? 0,
                        LaborCost = se.LaborCost ?? 0,
                        OverheadCost = se.OverheadCost ?? 0
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cost analysis for prototype job {PrototypeJobId}", prototypeJobId);
                return new { };
            }
        }

        #endregion
    }
}