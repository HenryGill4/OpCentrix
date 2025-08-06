using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using OpCentrix.Extensions;
using System.Security.Claims;

namespace OpCentrix.Pages.Operations
{
    [Authorize]
    public class StageDashboardModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly IStageProgressionService _stageProgressionService;
        private readonly ProductionStageService _productionStageService;
        private readonly ILogger<StageDashboardModel> _logger;

        public StageDashboardModel(
            SchedulerContext context,
            IStageProgressionService stageProgressionService,
            ProductionStageService productionStageService,
            ILogger<StageDashboardModel> logger)
        {
            _context = context;
            _stageProgressionService = stageProgressionService;
            _productionStageService = productionStageService;
            _logger = logger;
        }

        public List<StageAwareJob> StageJobs { get; set; } = new();
        public List<ProductionStage> ProductionStages { get; set; } = new();
        public string ViewMode { get; set; } = "stage"; // stage, machine, operator

        public async Task OnGetAsync(string? view = "stage")
        {
            ViewMode = view ?? "stage";
            
            try
            {
                // Load production stages (reuse existing service)
                ProductionStages = await _productionStageService.GetAllStagesAsync();
                
                // Load jobs for the next 7 days using extension method
                var startDate = DateTime.Today;
                var endDate = DateTime.Today.AddDays(7);
                
                StageJobs = await _stageProgressionService.GetStageAwareJobsAsync(_context, startDate, endDate);
                
                _logger.LogInformation("Stage dashboard loaded with {JobCount} jobs and {StageCount} stages", 
                    StageJobs.Count, ProductionStages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading stage dashboard");
                StageJobs = new List<StageAwareJob>();
                ProductionStages = new List<ProductionStage>();
            }
        }

        /// <summary>
        /// Handle operator punch-in to a stage (updated to use direct JobId linking)
        /// </summary>
        public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId, string operatorName)
        {
            try
            {
                var job = await _context.Jobs.Include(j => j.Part).FirstOrDefaultAsync(j => j.Id == jobId);
                var stage = await _context.ProductionStages.FindAsync(stageId);
                
                if (job == null || stage == null)
                {
                    return new JsonResult(new { success = false, message = "Job or stage not found" });
                }

                // Check if already punched into this stage
                var existingExecution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(se => se.JobId == jobId && 
                                             se.ProductionStageId == stageId && 
                                             se.Status == "InProgress");

                if (existingExecution != null)
                {
                    return new JsonResult(new { success = false, message = "Already punched into this stage" });
                }

                // Create stage execution using direct JobId link
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,  // Direct link to regular job
                    ProductionStageId = stageId,
                    ExecutedBy = operatorName,
                    OperatorName = operatorName,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = stage.DefaultSetupMinutes / 60.0m,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = operatorName
                };

                _context.ProductionStageExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {Operator} punched into stage {StageId} for job {JobId}", 
                    operatorName, stageId, jobId);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Punched into {stage.Name} successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching into stage {StageId} for job {JobId}", stageId, jobId);
                return new JsonResult(new { 
                    success = false, 
                    message = "Error punching in. Please try again." 
                });
            }
        }

        /// <summary>
        /// Handle operator punch-out from a stage (updated to use direct JobId linking)
        /// </summary>
        public async Task<IActionResult> OnPostPunchOutAsync(int jobId, int stageId)
        {
            try
            {
                var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
                if (job == null)
                {
                    return new JsonResult(new { success = false, message = "Job not found" });
                }

                // Find the active stage execution using direct JobId link
                var execution = await _context.ProductionStageExecutions
                    .Include(se => se.ProductionStage)
                    .FirstOrDefaultAsync(se => se.JobId == jobId && 
                                             se.ProductionStageId == stageId && 
                                             se.Status == "InProgress");

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active punch-in found" });
                }

                // Complete the stage execution
                execution.Status = "Completed";
                execution.CompletionDate = DateTime.UtcNow;
                execution.ActualEndTime = DateTime.UtcNow;
                
                if (execution.ActualStartTime.HasValue)
                {
                    var duration = DateTime.UtcNow - execution.ActualStartTime.Value;
                    execution.ActualHours = (decimal)duration.TotalHours;
                }

                execution.UpdatedDate = DateTime.UtcNow;
                execution.LastModifiedDate = DateTime.UtcNow;
                execution.LastModifiedBy = User.Identity?.Name ?? "Unknown";

                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator punched out of stage {StageId} for job {JobId}", stageId, jobId);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Punched out of {execution.ProductionStage?.Name} successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching out of stage {StageId} for job {JobId}", stageId, jobId);
                return new JsonResult(new { 
                    success = false, 
                    message = "Error punching out. Please try again." 
                });
            }
        }

        /// <summary>
        /// Refresh stages content for HTMX
        /// </summary>
        public async Task<IActionResult> OnGetRefreshStagesAsync()
        {
            await OnGetAsync();
            return Page();
        }

        #region Helper Methods

        /// <summary>
        /// Get count of active jobs for a stage
        /// </summary>
        public int GetActiveJobsCount(int stageId)
        {
            return StageJobs.Count(sj => sj.StageExecutions.Any(se => 
                se.ProductionStageId == stageId && se.Status == "InProgress"));
        }

        /// <summary>
        /// Get count of queued jobs for a stage
        /// </summary>
        public int GetQueuedJobsCount(int stageId)
        {
            return StageJobs.Count(sj => sj.RequiredStages.Any(rs => 
                rs.Id == stageId && sj.GetStageStatus(rs) == "Pending"));
        }

        /// <summary>
        /// Get current operator name
        /// </summary>
        public string GetCurrentOperator()
        {
            return User.Identity?.Name ?? "Unknown";
        }

        /// <summary>
        /// Check if user is in admin role
        /// </summary>
        public bool IsAdminUser()
        {
            return User.IsInRole("Admin") || User.IsInRole("Manager");
        }

        /// <summary>
        /// Get CSS class for job status badge
        /// </summary>
        public string GetJobStatusClass(string status)
        {
            return status?.ToLower() switch
            {
                "scheduled" => "badge-secondary",
                "inprogress" => "badge-primary", 
                "in progress" => "badge-primary",
                "completed" => "badge-success",
                "cancelled" => "badge-danger",
                "on hold" => "badge-warning",
                _ => "badge-secondary"
            };
        }

        #endregion
    }
}