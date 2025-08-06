using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Extensions;

namespace OpCentrix.Pages.Operations
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(SchedulerContext context, ILogger<DashboardModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public string CurrentOperator { get; set; } = string.Empty;
        public List<OperatorActiveStage> MyActiveStages { get; set; } = new();
        public List<AvailableStage> AvailableStages { get; set; } = new();

        public async Task OnGetAsync()
        {
            CurrentOperator = User.Identity?.Name ?? "Unknown";
            
            try
            {
                // Load active stages for current operator
                MyActiveStages = await GetActiveStagesForOperatorAsync(CurrentOperator);
                
                // Load available stages for operator
                AvailableStages = await GetAvailableStagesForOperatorAsync(CurrentOperator);
                
                _logger.LogInformation("Operator dashboard loaded for {Operator}: {ActiveCount} active, {AvailableCount} available", 
                    CurrentOperator, MyActiveStages.Count, AvailableStages.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading operator dashboard for {Operator}", CurrentOperator);
            }
        }

        /// <summary>
        /// Handle operator punch-in
        /// </summary>
        public async Task<IActionResult> OnPostPunchInAsync(int jobId, int stageId)
        {
            try
            {
                var operatorName = User.Identity?.Name ?? "Unknown";
                
                // Validate job and stage
                var job = await _context.Jobs.FindAsync(jobId);
                var stage = await _context.ProductionStages.FindAsync(stageId);
                
                if (job == null || stage == null)
                {
                    return new JsonResult(new { success = false, message = "Job or stage not found" });
                }

                // Check if already punched in
                var existingExecution = await _context.ProductionStageExecutions
                    .FirstOrDefaultAsync(se => se.JobId == jobId && 
                                             se.ProductionStageId == stageId && 
                                             se.Status == "In Progress");

                if (existingExecution != null)
                {
                    return new JsonResult(new { success = false, message = "Already punched into this stage" });
                }

                // Create stage execution
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,
                    ProductionStageId = stageId,
                    OperatorName = operatorName,
                    Status = "In Progress",
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = stage.DefaultSetupMinutes / 60.0m,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = operatorName
                };

                _context.ProductionStageExecutions.Add(execution);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {Operator} punched into {Stage} for job {JobId}", 
                    operatorName, stage.Name, jobId);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Successfully started work on {stage.Name}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching in for job {JobId}, stage {StageId}", jobId, stageId);
                return new JsonResult(new { 
                    success = false, 
                    message = "Error starting work. Please try again." 
                });
            }
        }

        /// <summary>
        /// Handle operator punch-out
        /// </summary>
        public async Task<IActionResult> OnPostPunchOutAsync(int jobId, int stageId)
        {
            try
            {
                var operatorName = User.Identity?.Name ?? "Unknown";
                
                var execution = await _context.ProductionStageExecutions
                    .Include(se => se.ProductionStage)
                    .FirstOrDefaultAsync(se => se.JobId == jobId && 
                                             se.ProductionStageId == stageId && 
                                             se.OperatorName == operatorName &&
                                             se.Status == "In Progress");

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active work found for this stage" });
                }

                // Complete the execution
                execution.Status = "Completed";
                execution.ActualEndTime = DateTime.UtcNow;
                
                if (execution.ActualStartTime.HasValue)
                {
                    var duration = DateTime.UtcNow - execution.ActualStartTime.Value;
                    execution.ActualHours = (decimal)duration.TotalHours;
                }

                execution.LastModifiedDate = DateTime.UtcNow;
                execution.LastModifiedBy = operatorName;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Operator {Operator} completed {Stage} for job {JobId}", 
                    operatorName, execution.ProductionStage?.Name, jobId);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Successfully completed {execution.ProductionStage?.Name}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error punching out for job {JobId}, stage {StageId}", jobId, stageId);
                return new JsonResult(new { 
                    success = false, 
                    message = "Error completing work. Please try again." 
                });
            }
        }

        #region Private Methods

        private async Task<List<OperatorActiveStage>> GetActiveStagesForOperatorAsync(string operatorName)
        {
            return await _context.ProductionStageExecutions
                .Where(se => se.OperatorName == operatorName && se.Status == "In Progress")
                .Include(se => se.Job)
                .ThenInclude(j => j.Part)
                .Include(se => se.ProductionStage)
                .Select(se => new OperatorActiveStage
                {
                    JobId = se.JobId ?? 0,
                    StageId = se.ProductionStageId,
                    JobPartNumber = se.Job.PartNumber,
                    StageName = se.ProductionStage.Name,
                    MachineId = se.Job.MachineId,
                    StartTime = se.ActualStartTime ?? DateTime.UtcNow,
                    EstimatedHours = se.EstimatedHours ?? 0
                })
                .ToListAsync();
        }

        private async Task<List<AvailableStage>> GetAvailableStagesForOperatorAsync(string operatorName)
        {
            // Get jobs that have stages ready for work
            var availableStages = new List<AvailableStage>();

            // Get jobs scheduled for today and next few days
            var startDate = DateTime.Today;
            var endDate = DateTime.Today.AddDays(3);

            var jobs = await _context.Jobs
                .Where(j => j.ScheduledStart >= startDate && 
                           j.ScheduledStart <= endDate &&
                           j.Status == "Scheduled")
                .Include(j => j.Part)
                .ThenInclude(p => p.PartStageRequirements)
                .ThenInclude(psr => psr.ProductionStage)
                .ToListAsync();

            foreach (var job in jobs)
            {
                if (job.Part?.PartStageRequirements == null) continue;

                var requiredStages = job.Part.PartStageRequirements
                    .Where(psr => psr.IsRequired)
                    .OrderBy(psr => psr.ProductionStage.DisplayOrder)
                    .ToList();

                foreach (var stageReq in requiredStages)
                {
                    var stage = stageReq.ProductionStage;
                    
                    // Check if this stage is available (not already in progress or completed)
                    var existingExecution = await _context.ProductionStageExecutions
                        .FirstOrDefaultAsync(se => se.JobId == job.Id && 
                                                  se.ProductionStageId == stage.Id);

                    if (existingExecution?.Status == "Completed" || existingExecution?.Status == "In Progress")
                        continue;

                    // Check if previous stages are complete (if not the first stage)
                    var stageOrder = requiredStages.FindIndex(rs => rs.ProductionStage.Id == stage.Id);
                    bool canStart = true;

                    if (stageOrder > 0)
                    {
                        for (int i = 0; i < stageOrder; i++)
                        {
                            var prevStage = requiredStages[i].ProductionStage;
                            var prevExecution = await _context.ProductionStageExecutions
                                .FirstOrDefaultAsync(se => se.JobId == job.Id && 
                                                          se.ProductionStageId == prevStage.Id);

                            if (prevExecution?.Status != "Completed")
                            {
                                canStart = false;
                                break;
                            }
                        }
                    }

                    if (canStart)
                    {
                        availableStages.Add(new AvailableStage
                        {
                            JobId = job.Id,
                            StageId = stage.Id,
                            JobPartNumber = job.PartNumber,
                            StageName = stage.Name,
                            MachineId = job.MachineId,
                            EstimatedHours = (decimal)(stage.DefaultSetupMinutes / 60.0),
                            Priority = job.Priority.ToString(),
                            Status = job.Status,
                            Notes = !string.IsNullOrEmpty(job.Notes) ? job.Notes : ""
                        });
                    }
                }
            }

            return availableStages.OrderBy(s => s.JobPartNumber).ToList();
        }

        #endregion
    }

    /// <summary>
    /// Active stage for an operator
    /// </summary>
    public class OperatorActiveStage
    {
        public int JobId { get; set; }
        public int StageId { get; set; }
        public string JobPartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal EstimatedHours { get; set; }
    }

    /// <summary>
    /// Available stage for an operator to work on
    /// </summary>
    public class AvailableStage
    {
        public int JobId { get; set; }
        public int StageId { get; set; }
        public string JobPartNumber { get; set; } = string.Empty;
        public string StageName { get; set; } = string.Empty;
        public string MachineId { get; set; } = string.Empty;
        public decimal EstimatedHours { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}