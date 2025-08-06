using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;
using OpCentrix.Services.Admin;
using System.Security.Claims;

namespace OpCentrix.Pages.Operations.Stages
{
    /// <summary>
    /// SLS Operations Dashboard - Individual stage interface for 3D Printing department
    /// Mobile-optimized following PrintTracking patterns
    /// Part of Phase 2: Individual Stage Dashboards implementation
    /// </summary>
    [Authorize(Policy = "OperatorAccess")]
    public class SLSModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<SLSModel> _logger;
        private readonly IStageProgressionService _stageProgressionService;
        private readonly ProductionStageService _productionStageService;

        // SLS-specific properties
        public string CurrentOperator { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;

        // SLS stage data
        public ProductionStage SLSStage { get; set; } = new();
        public List<SLSJobExecution> ActiveSLSJobs { get; set; } = new();
        public List<SLSJobExecution> QueuedSLSJobs { get; set; } = new();
        public List<SLSJobExecution> CompletedSLSJobs { get; set; } = new();

        // SLS statistics
        public int TotalActiveSLSJobs { get; set; }
        public int TotalQueuedSLSJobs { get; set; }
        public double TotalSLSHoursToday { get; set; }
        public int CompletedSLSJobsToday { get; set; }
        public List<SLSMachineStatus> SLSMachines { get; set; } = new();

        public SLSModel(
            SchedulerContext context,
            ILogger<SLSModel> logger,
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
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";

                await LoadSLSStageAsync();
                await LoadSLSJobsAsync();
                await LoadSLSMachinesAsync();
                await CalculateSLSStatisticsAsync();

                _logger.LogInformation("SLS Operations dashboard loaded for {Operator} ({UserRole}) with {ActiveJobs} active and {QueuedJobs} queued SLS jobs",
                    CurrentOperator, UserRole, ActiveSLSJobs.Count, QueuedSLSJobs.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLS Operations dashboard for {Operator}", CurrentOperator);
                TempData["Error"] = "Error loading SLS dashboard. Please refresh the page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStartSLSJobAsync(int jobId)
        {
            try
            {
                // Validate job exists and is for SLS stage
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return new JsonResult(new { success = false, message = "Job not found" });
                }

                // Check if this is an SLS job
                var slsStageReq = job.Part?.PartStageRequirements
                    ?.FirstOrDefault(psr => psr.ProductionStage?.Name.Contains("SLS") == true);

                if (slsStageReq == null)
                {
                    return new JsonResult(new { success = false, message = "This job does not require SLS processing" });
                }

                // Check if operator is already working on another job (business rule)
                var currentlyWorking = await _context.ProductionStageExecutions
                    .Where(pse => pse.ExecutedBy == CurrentOperator && pse.Status == "InProgress")
                    .CountAsync();

                if (currentlyWorking >= 1)
                {
                    return new JsonResult(new { success = false, message = "You can only work on one job at a time. Please complete your current job first." });
                }

                // Create new stage execution for SLS
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,
                    ProductionStageId = slsStageReq.ProductionStageId,
                    ExecutedBy = CurrentOperator,
                    OperatorName = CurrentOperator,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = slsStageReq.ProductionStage?.DefaultSetupMinutes / 60.0m ?? 4.0m,
                    CreatedBy = CurrentOperator,
                    CreatedDate = DateTime.UtcNow
                };

                _context.ProductionStageExecutions.Add(execution);

                // Update job status
                job.Status = "InProgress";
                job.ActualStart = DateTime.UtcNow;
                job.LastModifiedBy = CurrentOperator;
                job.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("SLS job {JobId} ({PartNumber}) started by operator {Operator}",
                    jobId, job.PartNumber, CurrentOperator);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Started SLS job for {job.PartNumber}",
                    executionId = execution.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting SLS job {JobId} by operator {Operator}", 
                    jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error starting SLS job. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostCompleteSLSJobAsync(int jobId)
        {
            try
            {
                // Find the operator's active execution for this job
                var execution = await _context.ProductionStageExecutions
                    .Include(pse => pse.ProductionStage)
                    .Include(pse => pse.Job)
                    .FirstOrDefaultAsync(pse => pse.JobId == jobId && 
                                              pse.ExecutedBy == CurrentOperator &&
                                              pse.Status == "InProgress" &&
                                              pse.ProductionStage.Name.Contains("SLS"));

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active SLS execution found for this job" });
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

                // Update job status
                if (execution.Job != null)
                {
                    execution.Job.Status = "Completed";
                    execution.Job.ActualEnd = DateTime.UtcNow;
                    execution.Job.LastModifiedBy = CurrentOperator;
                    execution.Job.LastModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("SLS job {JobId} ({PartNumber}) completed by operator {Operator} in {ActualHours:F2} hours",
                    jobId, execution.Job?.PartNumber, CurrentOperator, execution.ActualHours);

                // Check if this triggers automatic stage progression
                if (execution.Job?.BuildCohortId.HasValue == true)
                {
                    try
                    {
                        var cohortId = execution.Job.BuildCohortId.Value;
                        var downstreamJobs = await _stageProgressionService.CreateDownstreamJobsAsync(cohortId);
                        if (downstreamJobs.Any())
                        {
                            _logger.LogInformation("Created {JobCount} downstream jobs for completed SLS cohort {CohortId}", 
                                downstreamJobs.Count, cohortId);
                        }
                    }
                    catch (Exception progEx)
                    {
                        _logger.LogWarning(progEx, "Stage progression failed after SLS completion for job {JobId}", jobId);
                        // Don't fail the completion operation
                    }
                }

                return new JsonResult(new { 
                    success = true, 
                    message = $"Completed SLS job for {execution.Job?.PartNumber}",
                    actualHours = execution.ActualHours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing SLS job {JobId} by operator {Operator}", 
                    jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error completing SLS job. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetRefreshSLSDashboardAsync()
        {
            try
            {
                CurrentOperator = User.Identity?.Name ?? "Unknown";
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";

                await LoadSLSStageAsync();
                await LoadSLSJobsAsync();
                await LoadSLSMachinesAsync();
                await CalculateSLSStatisticsAsync();

                return Partial("_SLSContent", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing SLS dashboard for operator {Operator}", CurrentOperator);
                return StatusCode(500, "Error refreshing SLS dashboard");
            }
        }

        #region Private Methods

        /// <summary>
        /// Load SLS stage information
        /// </summary>
        private async Task LoadSLSStageAsync()
        {
            try
            {
                SLSStage = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Name.Contains("SLS") && ps.IsActive) 
                    ?? new ProductionStage 
                    { 
                        Name = "3D Printing (SLS)", 
                        StageColor = "#007bff", 
                        Department = "3D Printing" 
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLS stage information");
                SLSStage = new ProductionStage { Name = "3D Printing (SLS)", StageColor = "#007bff", Department = "3D Printing" };
            }
        }

        /// <summary>
        /// Load SLS jobs based on current status
        /// </summary>
        private async Task LoadSLSJobsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var nextWeek = today.AddDays(7);

                // Get jobs that require SLS processing
                var allSLSJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .Where(j => j.Part.PartStageRequirements.Any(psr => 
                        psr.ProductionStage.Name.Contains("SLS") && psr.IsRequired))
                    .Where(j => j.ScheduledStart >= today && j.ScheduledStart <= nextWeek)
                    .OrderBy(j => j.ScheduledStart)
                    .Take(50) // Limit for performance
                    .ToListAsync();

                var slsStageExecutions = await _context.ProductionStageExecutions
                    .Include(pse => pse.Job)
                    .Include(pse => pse.ProductionStage)
                    .Where(pse => pse.ProductionStage.Name.Contains("SLS"))
                    .Where(pse => allSLSJobs.Select(j => j.Id).Contains(pse.JobId.GetValueOrDefault()))
                    .ToListAsync();

                // Create SLS job executions
                var slsJobs = allSLSJobs.Select(job => 
                {
                    var execution = slsStageExecutions.FirstOrDefault(se => se.JobId == job.Id);
                    return new SLSJobExecution
                    {
                        Job = job,
                        StageExecution = execution,
                        PartNumber = job.PartNumber,
                        Quantity = job.Quantity,
                        EstimatedHours = job.EstimatedHours,
                        MachineId = job.MachineId ?? "TBD",
                        Status = execution?.Status ?? "Pending",
                        StartTime = execution?.StartDate,
                        EndTime = execution?.CompletionDate,
                        ActualHours = execution?.ActualHours,
                        OperatorName = execution?.OperatorName ?? "",
                        ScheduledStart = job.ScheduledStart
                    };
                }).ToList();

                // Categorize jobs
                ActiveSLSJobs = slsJobs.Where(j => j.Status == "InProgress").ToList();
                QueuedSLSJobs = slsJobs.Where(j => j.Status == "Pending" || j.Status == "Scheduled").ToList();
                CompletedSLSJobs = slsJobs.Where(j => j.Status == "Completed").Take(10).ToList(); // Last 10 completed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLS jobs");
                ActiveSLSJobs = new List<SLSJobExecution>();
                QueuedSLSJobs = new List<SLSJobExecution>();
                CompletedSLSJobs = new List<SLSJobExecution>();
            }
        }

        /// <summary>
        /// Load SLS machine status information
        /// </summary>
        private async Task LoadSLSMachinesAsync()
        {
            try
            {
                var slsMachines = await _context.Machines
                    .Where(m => m.IsActive && m.MachineType.ToUpper() == "SLS")
                    .OrderBy(m => m.Priority)
                    .ToListAsync();

                SLSMachines = slsMachines.Select(m => new SLSMachineStatus
                {
                    MachineId = m.MachineId,
                    MachineName = m.MachineName,
                    Status = m.Status,
                    CurrentMaterial = m.CurrentMaterial ?? "None",
                    Location = m.Location ?? "Print Floor",
                    ActiveJobs = ActiveSLSJobs.Count(j => j.MachineId == m.MachineId),
                    QueuedJobs = QueuedSLSJobs.Count(j => j.MachineId == m.MachineId),
                    BuildVolume = $"{m.BuildLengthMm}×{m.BuildWidthMm}×{m.BuildHeightMm}mm",
                    UtilizationPercent = CalculateMachineUtilization(m.MachineId)
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading SLS machines");
                SLSMachines = new List<SLSMachineStatus>();
            }
        }

        /// <summary>
        /// Calculate SLS dashboard statistics
        /// </summary>
        private async Task CalculateSLSStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;

                TotalActiveSLSJobs = ActiveSLSJobs.Count;
                TotalQueuedSLSJobs = QueuedSLSJobs.Count;

                // Calculate hours worked today in SLS
                var completedTodayExecutions = await _context.ProductionStageExecutions
                    .Include(pse => pse.ProductionStage)
                    .Where(pse => pse.ExecutedBy == CurrentOperator && 
                                 pse.Status == "Completed" &&
                                 pse.CompletionDate.HasValue &&
                                 pse.CompletionDate.Value.Date == today &&
                                 pse.ProductionStage.Name.Contains("SLS"))
                    .ToListAsync();

                TotalSLSHoursToday = completedTodayExecutions.Sum(ex => (double)(ex.ActualHours ?? 0));
                CompletedSLSJobsToday = completedTodayExecutions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating SLS statistics");
            }
        }

        /// <summary>
        /// Calculate machine utilization percentage
        /// </summary>
        private double CalculateMachineUtilization(string machineId)
        {
            try
            {
                var activeTime = ActiveSLSJobs
                    .Where(j => j.MachineId == machineId && j.StartTime.HasValue)
                    .Sum(j => (DateTime.Now - j.StartTime.Value).TotalHours);

                var scheduledTime = QueuedSLSJobs
                    .Where(j => j.MachineId == machineId)
                    .Sum(j => j.EstimatedHours);

                var totalAvailableHours = 24.0; // 24 hours in a day
                return Math.Min(100.0, ((activeTime + scheduledTime) / totalAvailableHours) * 100.0);
            }
            catch
            {
                return 0.0;
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

    #region Supporting Models

    /// <summary>
    /// SLS-specific job execution model
    /// </summary>
    public class SLSJobExecution
    {
        public Job Job { get; set; } = new();
        public ProductionStageExecution? StageExecution { get; set; }
        public string PartNumber { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double EstimatedHours { get; set; }
        public string MachineId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? ActualHours { get; set; }
        public string OperatorName { get; set; } = string.Empty;
        public DateTime ScheduledStart { get; set; }
    }

    /// <summary>
    /// SLS machine status model
    /// </summary>
    public class SLSMachineStatus
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CurrentMaterial { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ActiveJobs { get; set; }
        public int QueuedJobs { get; set; }
        public string BuildVolume { get; set; } = string.Empty;
        public double UtilizationPercent { get; set; }
    }

    #endregion
}