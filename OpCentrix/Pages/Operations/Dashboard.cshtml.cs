using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using System.Security.Claims;

namespace OpCentrix.Pages.Operations
{
    /// <summary>
    /// Operator-focused dashboard for stage management and punch in/out functionality
    /// Mobile-optimized following PrintTracking patterns
    /// </summary>
    [Authorize(Policy = "OperatorAccess")]
    public class DashboardModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<DashboardModel> _logger;
        private readonly IStageProgressionService _stageProgressionService;
        private readonly ProductionStageService _productionStageService;

        // Operator-specific properties
        public string CurrentOperator { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public List<OperatorActiveStage> MyActiveStages { get; set; } = new();
        public List<OperatorAvailableStage> AvailableStages { get; set; } = new();
        
        // Dashboard statistics
        public int TotalActiveStages { get; set; }
        public int TotalAvailableStages { get; set; }
        public double TotalHoursToday { get; set; }
        public int CompletedStagesThisShift { get; set; }

        public DashboardModel(
            SchedulerContext context,
            ILogger<DashboardModel> logger,
            IStageProgressionService stageProgressionService,
            ProductionStageService productionStageService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _stageProgressionService = stageProgressionService ?? throw new ArgumentNullException(nameof(stageProgressionService));
            _productionStageService = productionStageService ?? throw new ArgumentNullException(nameof(productionStageService));
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get current operator info (following PrintTracking pattern)
                CurrentOperator = User.Identity?.Name ?? "Unknown";
                UserRole = GetCurrentUserRole();

                await LoadOperatorStagesAsync();
                await LoadAvailableStagesAsync();
                await CalculateDashboardStatisticsAsync();

                _logger.LogInformation("Operator dashboard loaded for {Operator} with {ActiveStages} active and {AvailableStages} available stages",
                    CurrentOperator, MyActiveStages.Count, AvailableStages.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading operator dashboard for {Operator}", CurrentOperator);
                TempData["Error"] = "Error loading dashboard. Please refresh the page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId)
        {
            try
            {
                // Validate job and stage exist
                var job = await _context.Jobs.FindAsync(jobId);
                var stage = await _context.ProductionStages.FindAsync(stageId);

                if (job == null || stage == null)
                {
                    return new JsonResult(new { success = false, message = "Job or stage not found" });
                }

                // Check if operator is already working on another stage (business rule)
                var currentlyWorking = await _context.ProductionStageExecutions
                    .Where(pse => pse.ExecutedBy == CurrentOperator && pse.Status == "InProgress")
                    .CountAsync();

                if (currentlyWorking >= 1) // Limit one active stage per operator
                {
                    return new JsonResult(new { success = false, message = "You can only work on one stage at a time. Please punch out of your current stage first." });
                }

                // Check if this stage is already being worked on
                var stageInProgress = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(pse => pse.JobId == jobId && 
                                              pse.ProductionStageId == stageId && 
                                              pse.Status == "InProgress");

                if (stageInProgress != null)
                {
                    return new JsonResult(new { success = false, message = "This stage is already being worked on by another operator" });
                }

                // Create new stage execution
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,
                    ProductionStageId = stageId,
                    ExecutedBy = CurrentOperator,
                    OperatorName = CurrentOperator,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = stage.DefaultSetupMinutes / 60.0m,
                    CreatedBy = CurrentOperator,
                    CreatedDate = DateTime.UtcNow
                };

                _context.ProductionStageExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {Operator} punched in to stage {StageId} ({StageName}) for job {JobId} ({PartNumber})",
                    CurrentOperator, stageId, stage.Name, jobId, job.PartNumber);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Punched in to {stage.Name} for {job.PartNumber}",
                    executionId = execution.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching in to stage {StageId} for job {JobId} by operator {Operator}", 
                    stageId, jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error punching in. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostPunchOutAsync(int jobId, int stageId)
        {
            try
            {
                // Find the operator's active execution
                var execution = await _context.ProductionStageExecutions
                    .Include(pse => pse.ProductionStage)
                    .Include(pse => pse.Job)
                    .FirstOrDefaultAsync(pse => pse.JobId == jobId && 
                                              pse.ProductionStageId == stageId && 
                                              pse.ExecutedBy == CurrentOperator &&
                                              pse.Status == "InProgress");

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active execution found for this stage" });
                }

                // Complete the execution
                execution.Status = "Completed";
                execution.CompletionDate = DateTime.UtcNow;
                execution.ActualEndTime = DateTime.UtcNow;
                execution.LastModifiedBy = CurrentOperator;
                execution.LastModifiedDate = DateTime.UtcNow;

                // Calculate actual hours
                if (execution.StartDate.HasValue)
                {
                    execution.ActualHours = (decimal)(DateTime.UtcNow - execution.StartDate.Value).TotalHours;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {Operator} completed stage {StageId} ({StageName}) for job {JobId} ({PartNumber}) in {ActualHours:F2} hours",
                    CurrentOperator, stageId, execution.ProductionStage?.Name, jobId, execution.Job?.PartNumber, execution.ActualHours);

                // Check if this triggers automatic stage progression
                if (_stageProgressionService != null && execution.Job?.BuildCohortId.HasValue == true)
                {
                    try
                    {
                        // Check if all stages for this job's cohort are complete
                        // If so, this might trigger creation of downstream jobs
                        var cohortId = execution.Job.BuildCohortId.Value;
                        var allCohortJobs = await _context.Jobs
                            .Where(j => j.BuildCohortId == cohortId)
                            .ToListAsync();

                        var allJobsComplete = allCohortJobs.All(j => j.Status == "Completed");

                        if (allJobsComplete)
                        {
                            // This was the final job in the cohort - create downstream jobs
                            var downstreamJobs = await _stageProgressionService.CreateDownstreamJobsAsync(cohortId);
                            if (downstreamJobs.Any())
                            {
                                _logger.LogInformation("Created {JobCount} downstream jobs for completed cohort {CohortId}", 
                                    downstreamJobs.Count, cohortId);
                            }
                        }
                    }
                    catch (Exception progEx)
                    {
                        _logger.LogWarning(progEx, "Stage progression check failed for job {JobId} after completing stage {StageId}", 
                            jobId, stageId);
                        // Don't fail the punch-out operation
                    }
                }

                return new JsonResult(new { 
                    success = true, 
                    message = $"Completed {execution.ProductionStage?.Name} for {execution.Job?.PartNumber}",
                    actualHours = execution.ActualHours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching out of stage {StageId} for job {JobId} by operator {Operator}", 
                    stageId, jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error punching out. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetRefreshDashboardAsync()
        {
            try
            {
                CurrentOperator = User.Identity?.Name ?? "Unknown";
                UserRole = GetCurrentUserRole();

                await LoadOperatorStagesAsync();
                await LoadAvailableStagesAsync();
                await CalculateDashboardStatisticsAsync();

                return Partial("_DashboardContent", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing operator dashboard for {Operator}", CurrentOperator);
                return StatusCode(500, "Error refreshing dashboard");
            }
        }

        #region Private Methods

        /// <summary>
        /// Load stages the operator is currently working on
        /// </summary>
        private async Task LoadOperatorStagesAsync()
        {
            try
            {
                var activeExecutions = await _context.ProductionStageExecutions
                    .Include(pse => pse.Job)
                    .Include(pse => pse.ProductionStage)
                    .Where(pse => pse.ExecutedBy == CurrentOperator && pse.Status == "InProgress")
                    .OrderBy(pse => pse.StartDate)
                    .ToListAsync();

                MyActiveStages = activeExecutions.Select(ex => new OperatorActiveStage
                {
                    JobId = ex.JobId.GetValueOrDefault(),
                    StageId = ex.ProductionStageId,
                    JobPartNumber = ex.Job?.PartNumber ?? "Unknown",
                    StageName = ex.ProductionStage?.Name ?? "Unknown",
                    StartTime = ex.StartDate ?? DateTime.UtcNow,
                    EstimatedHours = (double)(ex.EstimatedHours ?? 0),
                    ExecutionId = ex.Id
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading active stages for operator {Operator}", CurrentOperator);
                MyActiveStages = new List<OperatorActiveStage>();
            }
        }

        /// <summary>
        /// Load stages available for the operator to work on
        /// </summary>
        private async Task LoadAvailableStagesAsync()
        {
            try
            {
                var today = DateTime.Today;
                var endDate = today.AddDays(7);

                // Get jobs scheduled for this week that aren't currently being worked on
                var availableJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .Where(j => j.ScheduledStart >= today && 
                               j.ScheduledStart <= endDate &&
                               j.Status.ToLower() == "scheduled")
                    .OrderBy(j => j.ScheduledStart)
                    .Take(20) // Limit to 20 for performance
                    .ToListAsync();

                var availableStagesList = new List<OperatorAvailableStage>();

                foreach (var job in availableJobs)
                {
                    if (job.Part?.PartStageRequirements == null) continue;

                    foreach (var stageReq in job.Part.PartStageRequirements.Where(psr => psr.IsRequired))
                    {
                        // Check if this stage is already being worked on
                        var inProgress = await _context.ProductionStageExecutions
                            .AnyAsync(pse => pse.JobId == job.Id && 
                                           pse.ProductionStageId == stageReq.ProductionStageId && 
                                           pse.Status == "InProgress");

                        if (!inProgress && stageReq.ProductionStage != null)
                        {
                            availableStagesList.Add(new OperatorAvailableStage
                            {
                                JobId = job.Id,
                                StageId = stageReq.ProductionStageId,
                                JobPartNumber = job.PartNumber,
                                StageName = stageReq.ProductionStage.Name,
                                MachineId = job.MachineId ?? "TBD",
                                EstimatedHours = stageReq.ProductionStage.DefaultSetupMinutes / 60.0,
                                Priority = job.Priority.ToString(), // Fixed: Convert int to string
                                Status = job.Status,
                                ScheduledStart = job.ScheduledStart
                            });
                        }
                    }
                }

                AvailableStages = availableStagesList
                    .OrderBy(s => s.ScheduledStart)
                    .ThenBy(s => s.Priority)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading available stages for operator {Operator}", CurrentOperator);
                AvailableStages = new List<OperatorAvailableStage>();
            }
        }

        /// <summary>
        /// Calculate dashboard statistics
        /// </summary>
        private async Task CalculateDashboardStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;

                TotalActiveStages = MyActiveStages.Count;
                TotalAvailableStages = AvailableStages.Count;

                // Calculate hours worked today
                var completedToday = await _context.ProductionStageExecutions
                    .Where(pse => pse.ExecutedBy == CurrentOperator && 
                                 pse.Status == "Completed" &&
                                 pse.CompletionDate.HasValue &&
                                 pse.CompletionDate.Value.Date == today)
                    .ToListAsync();

                TotalHoursToday = completedToday.Sum(ex => (double)(ex.ActualHours ?? 0));
                CompletedStagesThisShift = completedToday.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating dashboard statistics for operator {Operator}", CurrentOperator);
            }
        }

        /// <summary>
        /// Get current user role (following PrintTracking pattern)
        /// </summary>
        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ??
                   User.FindFirst("Role")?.Value ?? "Operator";
        }

        #endregion
    }

    #region Supporting View Models

    /// <summary>
    /// Represents a stage the operator is currently working on
    /// </summary>
    public class OperatorActiveStage
    {
        public int JobId { get; set; }
        public int StageId { get; set; }
        public string JobPartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public double EstimatedHours { get; set; }
        public int ExecutionId { get; set; }
    }

    /// <summary>
    /// Represents a stage available for the operator to work on
    /// </summary>
    public class OperatorAvailableStage
    {
        public int JobId { get; set; }
        public int StageId { get; set; }
        public string JobPartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public double EstimatedHours { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
    }

    #endregion
}