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
    /// CNC Operations Dashboard - Individual stage interface for CNC Machining department
    /// Mobile-optimized following PrintTracking patterns
    /// Part of Phase 2: Individual Stage Dashboards implementation
    /// </summary>
    [Authorize(Policy = "OperatorAccess")]
    public class CNCModel : PageModel
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<CNCModel> _logger;
        private readonly IStageProgressionService _stageProgressionService;
        private readonly ProductionStageService _productionStageService;

        // CNC-specific properties
        public string CurrentOperator { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
        public bool IsAdminView { get; set; }
        public bool IsOperatorView => !IsAdminView;

        // CNC stage data
        public ProductionStage CNCStage { get; set; } = new();
        public List<CNCJobExecution> ActiveCNCJobs { get; set; } = new();
        public List<CNCJobExecution> QueuedCNCJobs { get; set; } = new();
        public List<CNCJobExecution> CompletedCNCJobs { get; set; } = new();

        // CNC statistics
        public int TotalActiveCNCJobs { get; set; }
        public int TotalQueuedCNCJobs { get; set; }
        public double TotalCNCHoursToday { get; set; }
        public int CompletedCNCJobsToday { get; set; }
        public List<CNCMachineStatus> CNCMachines { get; set; } = new();

        public CNCModel(
            SchedulerContext context,
            ILogger<CNCModel> logger,
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

                await LoadCNCStageAsync();
                await LoadCNCJobsAsync();
                await LoadCNCMachinesAsync();
                await CalculateCNCStatisticsAsync();

                _logger.LogInformation("CNC Operations dashboard loaded for {Operator} ({UserRole}) with {ActiveJobs} active and {QueuedJobs} queued CNC jobs",
                    CurrentOperator, UserRole, ActiveCNCJobs.Count, QueuedCNCJobs.Count);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CNC Operations dashboard for {Operator}", CurrentOperator);
                TempData["Error"] = "Error loading CNC dashboard. Please refresh the page.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStartCNCJobAsync(int jobId)
        {
            try
            {
                // Validate job exists and is for CNC stage
                var job = await _context.Jobs
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .FirstOrDefaultAsync(j => j.Id == jobId);

                if (job == null)
                {
                    return new JsonResult(new { success = false, message = "Job not found" });
                }

                // Check if this is a CNC job
                var cncStageReq = job.Part?.PartStageRequirements
                    ?.FirstOrDefault(psr => psr.ProductionStage?.Name.Contains("CNC") == true);

                if (cncStageReq == null)
                {
                    return new JsonResult(new { success = false, message = "This job does not require CNC machining" });
                }

                // Check if operator is already working on another job (business rule)
                var currentlyWorking = await _context.ProductionStageExecutions
                    .Where(pse => pse.ExecutedBy == CurrentOperator && pse.Status == "InProgress")
                    .CountAsync();

                if (currentlyWorking >= 1)
                {
                    return new JsonResult(new { success = false, message = "You can only work on one job at a time. Please complete your current job first." });
                }

                // Create new stage execution for CNC
                var execution = new ProductionStageExecution
                {
                    JobId = jobId,
                    ProductionStageId = cncStageReq.ProductionStageId,
                    ExecutedBy = CurrentOperator,
                    OperatorName = CurrentOperator,
                    Status = "InProgress",
                    StartDate = DateTime.UtcNow,
                    ActualStartTime = DateTime.UtcNow,
                    EstimatedHours = cncStageReq.ProductionStage?.DefaultSetupMinutes / 60.0m ?? 2.0m, // CNC typically faster
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

                _logger.LogInformation("CNC job {JobId} ({PartNumber}) started by operator {Operator}",
                    jobId, job.PartNumber, CurrentOperator);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Started CNC machining for {job.PartNumber}",
                    executionId = execution.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting CNC job {JobId} by operator {Operator}", 
                    jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error starting CNC job. Please try again." });
            }
        }

        public async Task<IActionResult> OnPostCompleteCNCJobAsync(int jobId)
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
                                              pse.ProductionStage.Name.Contains("CNC"));

                if (execution == null)
                {
                    return new JsonResult(new { success = false, message = "No active CNC execution found for this job" });
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

                _logger.LogInformation("CNC job {JobId} ({PartNumber}) completed by operator {Operator} in {ActualHours:F2} hours",
                    jobId, execution.Job?.PartNumber, CurrentOperator, execution.ActualHours);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Completed CNC machining for {execution.Job?.PartNumber}",
                    actualHours = execution.ActualHours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing CNC job {JobId} by operator {Operator}", 
                    jobId, CurrentOperator);
                return new JsonResult(new { success = false, message = "Error completing CNC job. Please try again." });
            }
        }

        public async Task<IActionResult> OnGetRefreshCNCDashboardAsync()
        {
            try
            {
                CurrentOperator = User.Identity?.Name ?? "Unknown";
                UserRole = GetCurrentUserRole();
                IsAdminView = UserRole == "Admin" || UserRole == "Manager";

                await LoadCNCStageAsync();
                await LoadCNCJobsAsync();
                await LoadCNCMachinesAsync();
                await CalculateCNCStatisticsAsync();

                return Partial("_CNCContent", this);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing CNC dashboard for operator {Operator}", CurrentOperator);
                return StatusCode(500, "Error refreshing CNC dashboard");
            }
        }

        #region Private Methods

        /// <summary>
        /// Load CNC stage information
        /// </summary>
        private async Task LoadCNCStageAsync()
        {
            try
            {
                CNCStage = await _context.ProductionStages
                    .FirstOrDefaultAsync(ps => ps.Name.Contains("CNC") && ps.IsActive) 
                    ?? new ProductionStage 
                    { 
                        Name = "CNC Machining", 
                        StageColor = "#28a745", 
                        Department = "CNC Machining" 
                    };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CNC stage information");
                CNCStage = new ProductionStage { Name = "CNC Machining", StageColor = "#28a745", Department = "CNC Machining" };
            }
        }

        /// <summary>
        /// Load CNC jobs based on current status
        /// </summary>
        private async Task LoadCNCJobsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var nextWeek = today.AddDays(7);

                // Get jobs that require CNC machining
                var allCNCJobs = await _context.Jobs
                    .Include(j => j.Part)
                    .ThenInclude(p => p.PartStageRequirements)
                    .ThenInclude(psr => psr.ProductionStage)
                    .Where(j => j.Part.PartStageRequirements.Any(psr => 
                        psr.ProductionStage.Name.Contains("CNC") && psr.IsRequired))
                    .Where(j => j.ScheduledStart >= today && j.ScheduledStart <= nextWeek)
                    .OrderBy(j => j.ScheduledStart)
                    .Take(50) // Limit for performance
                    .ToListAsync();

                var cncStageExecutions = await _context.ProductionStageExecutions
                    .Include(pse => pse.Job)
                    .Include(pse => pse.ProductionStage)
                    .Where(pse => pse.ProductionStage.Name.Contains("CNC"))
                    .Where(pse => allCNCJobs.Select(j => j.Id).Contains(pse.JobId.GetValueOrDefault()))
                    .ToListAsync();

                // Create CNC job executions
                var cncJobs = allCNCJobs.Select(job => 
                {
                    var execution = cncStageExecutions.FirstOrDefault(se => se.JobId == job.Id);
                    var cncStageReq = job.Part?.PartStageRequirements?.FirstOrDefault(psr => 
                        psr.ProductionStage?.Name.Contains("CNC") == true);
                    
                    return new CNCJobExecution
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
                        ScheduledStart = job.ScheduledStart,
                        EstimatedMachiningTime = CalculateEstimatedMachiningTime(job.Quantity), // CNC-specific
                        MaterialType = job.Part?.SlsMaterial ?? "Unknown"
                    };
                }).ToList();

                // Categorize jobs
                ActiveCNCJobs = cncJobs.Where(j => j.Status == "InProgress").ToList();
                QueuedCNCJobs = cncJobs.Where(j => j.Status == "Pending" || j.Status == "Scheduled").ToList();
                CompletedCNCJobs = cncJobs.Where(j => j.Status == "Completed").Take(10).ToList(); // Last 10 completed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CNC jobs");
                ActiveCNCJobs = new List<CNCJobExecution>();
                QueuedCNCJobs = new List<CNCJobExecution>();
                CompletedCNCJobs = new List<CNCJobExecution>();
            }
        }

        /// <summary>
        /// Load CNC machine status information
        /// </summary>
        private async Task LoadCNCMachinesAsync()
        {
            try
            {
                var cncMachines = await _context.Machines
                    .Where(m => m.IsActive && m.MachineType.ToUpper().Contains("CNC"))
                    .OrderBy(m => m.Priority)
                    .ToListAsync();

                CNCMachines = cncMachines.Select(m => new CNCMachineStatus
                {
                    MachineId = m.MachineId,
                    MachineName = m.MachineName,
                    Status = m.Status,
                    Location = m.Location ?? "CNC Shop",
                    ActiveJobs = ActiveCNCJobs.Count(j => j.MachineId == m.MachineId),
                    QueuedJobs = QueuedCNCJobs.Count(j => j.MachineId == m.MachineId),
                    UtilizationPercent = CalculateMachineUtilization(m.MachineId),
                    ToolingSetup = GetCurrentToolingSetup(m.MachineId),
                    MaintenanceStatus = m.RequiresMaintenance ? "Due" : "OK"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading CNC machines");
                CNCMachines = new List<CNCMachineStatus>();
            }
        }

        /// <summary>
        /// Calculate CNC dashboard statistics
        /// </summary>
        private async Task CalculateCNCStatisticsAsync()
        {
            try
            {
                var today = DateTime.Today;

                TotalActiveCNCJobs = ActiveCNCJobs.Count;
                TotalQueuedCNCJobs = QueuedCNCJobs.Count;

                // Calculate hours worked today in CNC
                var completedTodayExecutions = await _context.ProductionStageExecutions
                    .Include(pse => pse.ProductionStage)
                    .Where(pse => pse.ExecutedBy == CurrentOperator && 
                                 pse.Status == "Completed" &&
                                 pse.CompletionDate.HasValue &&
                                 pse.CompletionDate.Value.Date == today &&
                                 pse.ProductionStage.Name.Contains("CNC"))
                    .ToListAsync();

                TotalCNCHoursToday = completedTodayExecutions.Sum(ex => (double)(ex.ActualHours ?? 0));
                CompletedCNCJobsToday = completedTodayExecutions.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating CNC statistics");
            }
        }

        /// <summary>
        /// Calculate estimated machining time based on quantity (CNC-specific calculation)
        /// </summary>
        private double CalculateEstimatedMachiningTime(int quantity)
        {
            // CNC-specific: 6 minutes per part as per business requirements
            return (quantity * 6.0) / 60.0; // Convert minutes to hours
        }

        /// <summary>
        /// Calculate machine utilization percentage
        /// </summary>
        private double CalculateMachineUtilization(string machineId)
        {
            try
            {
                var activeTime = ActiveCNCJobs
                    .Where(j => j.MachineId == machineId && j.StartTime.HasValue)
                    .Sum(j => (DateTime.Now - j.StartTime.Value).TotalHours);

                var scheduledTime = QueuedCNCJobs
                    .Where(j => j.MachineId == machineId)
                    .Sum(j => j.EstimatedMachiningTime);

                var totalAvailableHours = 16.0; // 16 hours typical CNC operation day
                return Math.Min(100.0, ((activeTime + scheduledTime) / totalAvailableHours) * 100.0);
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Get current tooling setup for CNC machine
        /// </summary>
        private string GetCurrentToolingSetup(string machineId)
        {
            // This could be enhanced to track actual tooling setups
            var activeJob = ActiveCNCJobs.FirstOrDefault(j => j.MachineId == machineId);
            if (activeJob != null)
            {
                return $"Setup for {activeJob.PartNumber}";
            }
            return "Standard Tooling";
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
    /// CNC-specific job execution model
    /// </summary>
    public class CNCJobExecution
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
        public double EstimatedMachiningTime { get; set; } // CNC-specific
        public string MaterialType { get; set; } = string.Empty;
    }

    /// <summary>
    /// CNC machine status model
    /// </summary>
    public class CNCMachineStatus
    {
        public string MachineId { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ActiveJobs { get; set; }
        public int QueuedJobs { get; set; }
        public double UtilizationPercent { get; set; }
        public string ToolingSetup { get; set; } = string.Empty;
        public string MaintenanceStatus { get; set; } = string.Empty;
    }

    #endregion
}